using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;

namespace GPC.Converter.CivilNx
{
    /// <summary>Version extension point. Profiles must match observed schemas; no fallback based on a guessed release number.</summary>
    public interface ICivilNxGeometryProfile
    {
        string Id { get; }
        IReadOnlyDictionary<string, CapabilityStatus> Capabilities { get; }
        ImportBatch Read(CivilNxSnapshot snapshot, AnalysisSource identity, CancellationToken cancellationToken);
    }

    public static class CivilNxGeometryReader
    {
        public const string InputBindingKind = "Civil NX input binding";

        public static ImportReport Import(CivilNxSnapshot snapshot, AnalysisSource identity, ICivilNxGeometryProfile profile = null, CancellationToken cancellationToken = default,
            MappingOptions options = null)
        {
            if (snapshot == null || identity == null) throw new ArgumentNullException();
            var rejected = new ImportReport { Status = ImportStatus.Rejected };
            foreach (var response in snapshot.Responses.Values) rejected.Preserved.Add(Preserve(response));
            try
            {
                profile = profile ?? new CivilNxCommonGeometryProfile();
                var batch = profile.Read(snapshot, identity, cancellationToken);
                if (batch.Program != "MIDAS Civil NX") throw new ArgumentException("CivilNxProfileProgramMismatch");
                batch.Uninterpreted.AddRange(rejected.Preserved);
                batch.SourceHash = snapshot.Hash;
                var report = ModelMapper.Map(batch, cancellationToken, null, options);
                report.Diagnostics.Add(new ModelDiagnostic { Code = "CivilNxGeometryProfile", Severity = DiagnosticSeverity.Information, Message = profile.Id });
                if (report.Model != null)
                {
                    // Exact input fingerprint for binding results read later from the same, unchanged Civil NX model.
                    var binding = new PreservedAssignment { Kind = InputBindingKind, SourceRecord = batch.SourceHash, RawData = report.Model.AnalysisFingerprint(),
                        UnsupportedReason = "Imported Model input fingerprint; results require the same source snapshot and unchanged inputs." };
                    report.Model.PreservedSourceData.Add(binding); report.Preserved.Add(binding);
                }
                return report;
            }
            catch (OperationCanceledException) { rejected.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (ex is SerializationException || ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException || ex is InvalidDataException)
            { rejected.Diagnostics.Add(ModelDiagnostic.Error("CivilNxSchemaRejected", message: ex.Message)); }
            return rejected;
        }
        private static PreservedAssignment Preserve(CivilNxResponse response) => new PreservedAssignment { Kind = "MIDAS Civil NX JSON",
            SourceRecord = response.Endpoint + " SHA256=" + response.Sha256, RawData = response.Json, UnsupportedReason = "Retained original API response; only explicitly supported fields are interpreted." };
    }

    /// <summary>Documented NODE/ELEM/UNIT common schema. Accepts additional fields and preserves the whole response; missing required data is rejected.</summary>
    public sealed class CivilNxCommonGeometryProfile : ICivilNxGeometryProfile
    {
        public string Id => "civil-nx/common-node-elem-unit/v1";
        public IReadOnlyDictionary<string, CapabilityStatus> Capabilities { get; } = new System.Collections.ObjectModel.ReadOnlyDictionary<string, CapabilityStatus>(new Dictionary<string, CapabilityStatus>
        {
            ["NodeCoordinates"] = CapabilityStatus.InterpretedWithoutRealFileTest,
            ["BeamPlateConnectivity"] = CapabilityStatus.InterpretedWithoutRealFileTest,
            ["AdditionalFields"] = CapabilityStatus.PreservedOnly,
            ["AssignmentsAndResultConventions"] = CapabilityStatus.NotSupported
        });
        public ImportBatch Read(CivilNxSnapshot snapshot, AnalysisSource identity, CancellationToken cancellationToken)
        {
            if (identity.Program != "MIDAS Civil NX" || string.IsNullOrWhiteSpace(identity.ModelRevision)) throw new ArgumentException("ExplicitCivilNxModelIdentityRequired");
            foreach (var key in new[] { "UNIT", "NODE", "ELEM" }) if (!snapshot.Responses.ContainsKey(key)) throw new InvalidDataException("MissingCivilNxDatabase: " + key);
            var units = CivilNxJson.Read<UnitResponse>(snapshot.Responses["UNIT"].Json)?.Units;
            var nodes = CivilNxJson.Read<NodeResponse>(snapshot.Responses["NODE"].Json)?.Nodes;
            var elements = CivilNxJson.Read<ElementResponse>(snapshot.Responses["ELEM"].Json)?.Elements;
            if (units == null || !units.TryGetValue("1", out var unit) || unit == null || nodes == null || elements == null) throw new NotSupportedException("UnknownCivilNxGeometrySchema");
            double length;
            switch (unit.Distance?.ToUpperInvariant()) { case "MM": length = 1; break; case "CM": length = 10; break; case "M": length = 1000; break;
                case "IN": length = 25.4; break; case "FT": length = 304.8; break; default: throw new NotSupportedException("UnknownCivilNxLengthUnit"); }
            var conversion = new ResultUnits(1, length, 1);
            var batch = new ImportBatch { Program = identity.Program, SolverVersion = identity.SolverVersion, ModelRevision = identity.ModelRevision, AnalysisId = identity.AnalysisId };
            foreach (var pair in nodes)
            {
                cancellationToken.ThrowIfCancellationRequested(); var node = pair.Value;
                if (node?.X == null || node.Y == null || node.Z == null) throw new InvalidDataException("MissingCivilNxCoordinate: " + pair.Key);
                batch.Nodes.Add(new NodeRecord { Id = pair.Key, GlobalPosition = new Point3d(conversion.Length(node.X.Value), conversion.Length(node.Y.Value), conversion.Length(node.Z.Value)), Record = "NODE/" + pair.Key });
            }
            foreach (var pair in elements)
            {
                cancellationToken.ThrowIfCancellationRequested(); var element = pair.Value;
                if (element?.Nodes == null) throw new InvalidDataException("MissingCivilNxConnectivity: " + pair.Key);
                var ids = element.Nodes.Reverse().SkipWhile(n => n == 0).Reverse().ToArray(); // The API pads NODE to eight entries with zeros.
                // Documented 2/3/4-node arrays only; an unrecognized padding/higher-order layout needs a separate profile.
                if (element.Type == "BEAM" && ids.Length == 2 && ids.All(n => n > 0))
                    batch.Beams.Add(new BeamRecord { Id = pair.Key, I = ids[0].ToString(CultureInfo.InvariantCulture), J = ids[1].ToString(CultureInfo.InvariantCulture), Record = "ELEM/" + pair.Key });
                else if (element.Type == "PLATE" && (ids.Length == 3 || ids.Length == 4) && ids.All(n => n > 0))
                    batch.Shells.Add(new ShellRecord { Id = pair.Key, Nodes = ids.Select(n => n.ToString(CultureInfo.InvariantCulture)).ToArray(), Record = "ELEM/" + pair.Key });
                else throw new NotSupportedException("UnsupportedCivilNxElementOrConnectivity: " + pair.Key + " (" + element.Type + ")");
            }
            batch.Diagnostics.Add(new ModelDiagnostic { Code = "CivilNxAssignmentsPending", Severity = DiagnosticSeverity.Warning,
                Message = "Only node coordinates and beam/plate connectivity mapped; use CivilNxModelProfile for axes, properties, groups, supports and loads." });
            return batch;
        }
        [DataContract] private sealed class UnitResponse { [DataMember(Name = "UNIT")] public Dictionary<string, UnitData> Units { get; set; } }
        [DataContract] private sealed class UnitData { [DataMember(Name = "DIST")] public string Distance { get; set; } }
        [DataContract] private sealed class NodeResponse { [DataMember(Name = "NODE")] public Dictionary<string, NodeData> Nodes { get; set; } }
        [DataContract] private sealed class NodeData
        { [DataMember] public double? X { get; set; } [DataMember] public double? Y { get; set; } [DataMember] public double? Z { get; set; } }
        [DataContract] private sealed class ElementResponse { [DataMember(Name = "ELEM")] public Dictionary<string, ElementData> Elements { get; set; } }
        [DataContract] private sealed class ElementData
        { [DataMember(Name = "TYPE")] public string Type { get; set; } [DataMember(Name = "NODE")] public int[] Nodes { get; set; } }
    }
}
