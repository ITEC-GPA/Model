using GPC.Model.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Core.Units;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.CivilNx
{
    /// <summary>Linear static results of a Civil NX model imported with <see cref="CivilNxModelProfile"/>, read through post/TABLE.
    /// Conventions verified on a real Civil NX model through the API (October 2026):
    /// beam Axial, Shear-y, Shear-z, Torsion, Moment-z are right-handed actions on the positive section face, Moment-y has the opposite sign
    /// (checked against the support reactions at the end of a rigid link, two load cases, and dM/dx along the element);
    /// plate Mxx, Myy, Mxy are minus the first moment of the stresses (checked against top/bottom stresses), in the plate axes rotated by ANGLE.</summary>
    public static class CivilNxResults
    {
        public const string ReaderVersion = "GPC.CivilNx.LinearStatic/1";
        private const string Convention = "Beam (ECS x,y,z = GPC V3,V1,V2): N=Axial, V1=Shear-y, V2=Shear-z, T=Torsion, M1=-Moment-y, M2=Moment-z on the positive face. "
            + "Plate centre, per unit width, plate ECS rotated by ANGLE: Fxx, Fyy, Fxy, Fxz=Vxx, Fyz=Vyy, Mxx=-Mxx, Myy=-Myy, Mxy=-Mxy. "
            + "Nodes: GCS reactions on the node, GCS displacements and rotations in radians.";
        private static readonly Regex QuarterPart = new Regex(@"^\s*([123])\s*/\s*4\s*$", RegexOptions.CultureInvariant);

        /// <summary>Requests the result tables of the given static cases (STLD names) for every imported beam, plate and node,
        /// and reactions for every restrained node; one request per case and table.</summary>
        public static Task<IReadOnlyList<CivilNxResponse>> ReadAsync(CivilNxApiClient client, GPC.Model.Models.Model model, IEnumerable<string> staticCases,
            CancellationToken cancellationToken = default)
        {
            if (client == null || model == null || staticCases == null) throw new ArgumentNullException();
            return ReadAsync(client, model, ResultFilter.All(model, staticCases), cancellationToken);
        }

        /// <summary>Requests the result tables of a plan resolved on the imported model (<see cref="ResultFilter"/>): beam forces of its beams,
        /// plate forces of its plates, displacements of its nodes and reactions of its supports, for its static cases only.</summary>
        public static async Task<IReadOnlyList<CivilNxResponse>> ReadAsync(CivilNxApiClient client, GPC.Model.Models.Model model, ResultReadPlan plan,
            CancellationToken cancellationToken = default)
        {
            if (client == null || model == null || plan == null) throw new ArgumentNullException();
            var beams = Ids(plan.Beams); var plates = Ids(plan.Shells); var nodes = Ids(plan.Nodes); var supports = Ids(plan.Supports);
            var responses = new List<CivilNxResponse>(); var names = Cases(model, plan.StaticCases);
            // Cases per request bounded so that a plate table stays well under the client's 32 MiB response limit.
            int perRequest = Math.Max(1, Math.Min(10, 20000 / Math.Max(1, plates.Length)));
            for (int at = 0; at < names.Length; at += perRequest)
            {
                var cases = names.Skip(at).Take(perRequest).Select(name => name + "(ST)").ToArray();
                if (beams.Length != 0) responses.Add(await client.ReadResultTableAsync(new CivilNxTableRequest { Table = CivilNxResultTable.BeamForce, ElementIds = beams, LoadCases = cases }, cancellationToken).ConfigureAwait(false));
                if (plates.Length != 0) responses.Add(await client.ReadResultTableAsync(new CivilNxTableRequest { Table = CivilNxResultTable.PlateForcePerUnitLength, ElementIds = plates, LoadCases = cases }, cancellationToken).ConfigureAwait(false));
                if (nodes.Length != 0) responses.Add(await client.ReadResultTableAsync(new CivilNxTableRequest { Table = CivilNxResultTable.DisplacementGlobal, ElementIds = nodes, LoadCases = cases }, cancellationToken).ConfigureAwait(false));
                if (supports.Length != 0) responses.Add(await client.ReadResultTableAsync(new CivilNxTableRequest { Table = CivilNxResultTable.ReactionGlobal, ElementIds = supports, LoadCases = cases }, cancellationToken).ConfigureAwait(false));
            }
            return responses;
        }

        private static int[] Ids(IEnumerable<Element> elements) => elements.Where(e => e.Source?.Program == CivilNxModelProfile.Program)
            .Select(e => int.Parse(e.Source.OriginalId, NumberStyles.None, CultureInfo.InvariantCulture)).Distinct().OrderBy(i => i).ToArray();
        private static string[] Cases(GPC.Model.Models.Model model, IEnumerable<string> staticCases)
        {
            var names = staticCases.Distinct(StringComparer.Ordinal).ToArray();
            foreach (var name in names) if (!model.LoadCases.ContainsKey(name)) throw new ArgumentException("UnknownStaticCase: " + name);
            return names;
        }

        /// <summary>Separate request for the unaveraged plate values at the element nodes (extrapolated by the solver from the integration
        /// points), for the given static cases; import them with <see cref="Import"/>, usually as their own dataset.</summary>
        public static Task<IReadOnlyList<CivilNxResponse>> ReadPlateNodesAsync(CivilNxApiClient client, GPC.Model.Models.Model model, IEnumerable<string> staticCases,
            CancellationToken cancellationToken = default)
        {
            if (client == null || model == null || staticCases == null) throw new ArgumentNullException();
            return ReadPlateNodesAsync(client, model, ResultFilter.All(model, staticCases), cancellationToken);
        }

        /// <summary>The element-node plate values of the plates and static cases of a plan.</summary>
        public static async Task<IReadOnlyList<CivilNxResponse>> ReadPlateNodesAsync(CivilNxApiClient client, GPC.Model.Models.Model model, ResultReadPlan plan,
            CancellationToken cancellationToken = default)
        {
            if (client == null || model == null || plan == null) throw new ArgumentNullException();
            var plates = Ids(plan.Shells); var names = Cases(model, plan.StaticCases); var responses = new List<CivilNxResponse>();
            int perRequest = Math.Max(1, Math.Min(10, 5000 / Math.Max(1, plates.Length)));
            for (int at = 0; at < names.Length && plates.Length != 0; at += perRequest)
                responses.Add(await client.ReadResultTableAsync(new CivilNxTableRequest { Table = CivilNxResultTable.PlateForcePerUnitLength, ElementIds = plates,
                    LoadCases = names.Skip(at).Take(perRequest).Select(n => n + "(ST)").ToArray(), PlateNodes = true }, cancellationToken).ConfigureAwait(false));
            return responses;
        }

        /// <summary>Binds the tables to the model: <paramref name="current"/> must be a snapshot of the same unchanged Civil NX model,
        /// taken when the tables were read, and the Model inputs must be unchanged since the import.</summary>
        public static ResultImportReport Import(GPC.Model.Models.Model model, CivilNxSnapshot current, IReadOnlyList<CivilNxResponse> tables, string datasetId,
            bool isSynthetic = false, CancellationToken cancellationToken = default)
        {
            var rejected = new ResultImportReport { Status = ImportStatus.Rejected };
            var skipped = new List<ModelDiagnostic>();
            try
            {
                if (model == null || current == null || tables == null) throw new ArgumentNullException();
                var source = model.AnalysisSource;
                if (source?.Program != CivilNxModelProfile.Program || source.GeometryHash != current.Hash)
                    throw new InvalidDataException("The Civil NX model changed or is not the imported one: re-import it before reading results.");
                var changed = SourceBinding.Check(model, CivilNxGeometryReader.InputBindingKind, source.GeometryHash, "Civil NX");
                if (changed != null) skipped.Add(changed);
                ResultImportBatch batch = null; double force = 0, length = 0; var rejectedBeams = new HashSet<string>(StringComparer.Ordinal);
                var index = model.AllElements.Where(e => e.Source != null).ToDictionary(e => e.Source);
                foreach (var response in tables)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var parsed = CivilNxJson.Read<Dictionary<string, CivilNxResultTableData>>(response.Json);
                    if (parsed == null || parsed.Count != 1) throw new InvalidDataException("UnknownCivilNxResultSchema");
                    var name = parsed.Keys.Single(); var table = CivilNxResultTableData.Read(response, name);
                    double f = MidasConventions.ForceFactor(table.ForceUnit) ?? throw new NotSupportedException("UnknownResultForceUnit: " + table.ForceUnit);
                    double l = MidasConventions.LengthFactor(table.LengthUnit) ?? throw new NotSupportedException("UnknownResultLengthUnit: " + table.LengthUnit);
                    if (batch == null)
                    {
                        force = f; length = l;
                        batch = new ResultImportBatch { Source = source, DatasetId = datasetId, SourceHash = SourceHash(tables), ExpectedInputFingerprint = model.AnalysisFingerprint(),
                            AnalysisInputFingerprint = SourceBinding.AnalysisFingerprint(model, CivilNxGeometryReader.InputBindingKind, source.GeometryHash),
                            Units = new ResultUnits(f, l, f * l), ShellDenominatorLengthToMm = l, ResolvedConvention = Convention, ReaderVersion = ReaderVersion,
                            Semantics = AnalysisSemantics.LinearStatic, IsSynthetic = isSynthetic };
                    }
                    else if (f != force || l != length) throw new InvalidDataException("All result tables must use the same units.");
                    if (table.Headers.Contains("Stage") || table.Headers.Contains("Step")) throw new NotSupportedException("Construction stage results are not supported.");
                    bool beams = table.Headers.Contains("Axial"), plates = table.Headers.Contains("Fxx"), reactions = table.Headers.Contains("FX"), displacements = table.Headers.Contains("DX");
                    if ((beams ? 1 : 0) + (plates ? 1 : 0) + (reactions ? 1 : 0) + (displacements ? 1 : 0) != 1) throw new NotSupportedException("UnknownCivilNxResultTable: " + name);
                    for (int r = 0; r < table.Rows.Length; r++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var loadCase = table.Get(r, "Load"); var record = "post/TABLE " + name + " row " + table.Get(r, "Index");
                        if (beams) Beam(index, batch, table, r, loadCase, record, l, rejectedBeams, skipped);
                        else if (plates) Plate(index, batch, table, r, loadCase, record, l);
                        else Node(index, batch, table, r, loadCase, record, l, reactions);
                    }
                }
                if (batch == null) throw new ArgumentException("No result tables supplied.");
                var report = ResultMapper.Import(model, batch, cancellationToken);
                report.Diagnostics.AddRange(skipped);
                report.Diagnostics.Add(new ModelDiagnostic { Code = "CivilNxResultConvention", Severity = DiagnosticSeverity.Information, Message = Convention });
                if (report.Status == ImportStatus.Completed || report.Status == ImportStatus.Partial)
                    model.PreservedSourceData.Add(new PreservedAssignment { Kind = "Civil NX result tables", SourceRecord = batch.SourceHash,
                        RawData = string.Join("\n", tables.Select(t => t.Endpoint + " SHA256=" + t.Sha256)), UnsupportedReason = "Hashes of the post/TABLE responses of dataset " + datasetId + "." });
                return report;
            }
            catch (OperationCanceledException) { rejected.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException || ex is InvalidOperationException || ex is NotSupportedException
                || ex is FormatException || ex is OverflowException || ex is System.Runtime.Serialization.SerializationException)
            { rejected.Diagnostics.Add(ModelDiagnostic.Error("CivilNxResultsRejected", message: ex.Message)); }
            return rejected;
        }

        private static void Beam(Dictionary<SourceIdentity, Element> index, ResultImportBatch batch, CivilNxResultTableData table, int r, string loadCase, string record, double length,
            HashSet<string> rejectedBeams, List<ModelDiagnostic> skipped)
        {
            var id = table.Get(r, "Elem");
            var beam = Find(index, batch, EntityFamily.Beam, id) as BeamElement
                ?? throw new InvalidDataException("Result for an absent beam: " + id);
            var a = beam.Assignments;
            if (a.RigidLengthI != 0 || a.RigidLengthJ != 0 || a.OffsetI != null || a.OffsetJ != null)
            {
                // The station domain of beams with end offsets has not been verified against Civil NX.
                if (rejectedBeams.Add(id)) skipped.Add(new ModelDiagnostic { Code = "CivilNxOffsetBeamResultsSkipped", Severity = DiagnosticSeverity.Warning, ElementId = beam.Id,
                    Family = EntityFamily.Beam, Record = record, Message = "Beam " + id + " has end offsets: its results are not imported until the station domain is verified." });
                return;
            }
            var part = table.Get(r, "Part"); double station;
            var quarter = QuarterPart.Match(part);
            if (part.StartsWith("I[", StringComparison.Ordinal) && part.EndsWith("]", StringComparison.Ordinal) && part.Substring(2, part.Length - 3) == beam.NodeI.Source?.OriginalId) station = 0;
            else if (part.StartsWith("J[", StringComparison.Ordinal) && part.EndsWith("]", StringComparison.Ordinal) && part.Substring(2, part.Length - 3) == beam.NodeJ.Source?.OriginalId) station = 1;
            else if (quarter.Success) station = int.Parse(quarter.Groups[1].Value, CultureInfo.InvariantCulture) / 4.0;
            else throw new InvalidDataException("Unknown or mismatched beam part '" + part + "' at " + record);
            var axes = a.SectionAxes; Axes.Validate(axes);
            var point = beam.StartPoint + (beam.EndPoint - beam.StartPoint) * station;
            batch.Beams.Add(new BeamForceRecord { ElementId = id, Case = loadCase, State = State(loadCase, 6), Axes = SourceAxes(axes, point, length), Record = record,
                Values = new double?[] { V(table, r, "Axial"), V(table, r, "Shear-y"), V(table, r, "Shear-z"), V(table, r, "Torsion"), -V(table, r, "Moment-y"), V(table, r, "Moment-z") },
                Station = station, PhysicalDistance = beam.Length * station / length, StationDomain = "NodeToNode", Body = ActionBody.PositiveSectionFace });
        }

        private static void Plate(Dictionary<SourceIdentity, Element> index, ResultImportBatch batch, CivilNxResultTableData table, int r, string loadCase, string record, double length)
        {
            var id = table.Get(r, "Elem"); var node = table.Get(r, "Node");
            var shell = Find(index, batch, EntityFamily.Shell, id) as AreaElement
                ?? throw new InvalidDataException("Result for an absent plate: " + id);
            var axes = shell.CoordinateSystem; Axes.Validate(axes);
            var values = new double?[] { V(table, r, "Fxx"), V(table, r, "Fyy"), V(table, r, "Fxy"), V(table, r, "Vxx"), V(table, r, "Vyy"), -V(table, r, "Mxx"), -V(table, r, "Myy"), -V(table, r, "Mxy") };
            if (node == "Cent")
                batch.Shells.Add(new ShellForceRecord { ElementId = id, Case = loadCase, State = State(loadCase, 8), Record = record, Values = values,
                    Axes = SourceAxes(axes, axes.Origin, length), Location = new Point2d(0, 0), PointKind = ShellResultPointKind.Centroid, CoordinateKind = ResultCoordinateKind.LocalPhysical });
            else
            {
                // Unaveraged value at an element node, extrapolated by the solver: same plate axes, point in local physical coordinates.
                var vertex = shell.Nodes.FirstOrDefault(n => n.Source?.OriginalId == node) ?? throw new InvalidDataException("Plate " + id + " has no node " + node + " at " + record);
                Vector3d d = vertex.Position - axes.Origin;
                batch.Shells.Add(new ShellForceRecord { ElementId = id, Case = loadCase, State = State(loadCase, 8), Record = record, Values = values,
                    Axes = SourceAxes(axes, axes.Origin, length), Location = new Point2d(Axes.Dot(d, axes.V1) / length, Axes.Dot(d, axes.V2) / length),
                    PointKind = ShellResultPointKind.ElementNodeExtrapolated, CoordinateKind = ResultCoordinateKind.LocalPhysical, SourceNodeId = node });
            }
        }

        private static void Node(Dictionary<SourceIdentity, Element> index, ResultImportBatch batch, CivilNxResultTableData table, int r, string loadCase, string record, double length, bool reaction)
        {
            var id = table.Get(r, "Node");
            var node = Find(index, batch, EntityFamily.Node, id) as NodeElement
                ?? throw new InvalidDataException("Result for an absent node: " + id);
            var axes = SourceAxes(CoordinateSystem.Global, node.Position, length);
            if (reaction)
                batch.NodeForces.Add(new NodeForceRecord { ElementId = id, Case = loadCase, State = State(loadCase, 6), Axes = axes, Record = record,
                    Values = new double?[] { V(table, r, "FX"), V(table, r, "FY"), V(table, r, "FZ"), V(table, r, "MX"), V(table, r, "MY"), V(table, r, "MZ") },
                    Kind = NodalForceKind.SupportReaction, Body = ActionBody.OnNode });
            else
                batch.NodeDisplacements.Add(new NodeDisplacementRecord { ElementId = id, Case = loadCase, State = State(loadCase, 6), Axes = axes, Record = record,
                    Values = new double?[] { V(table, r, "DX"), V(table, r, "DY"), V(table, r, "DZ"), V(table, r, "RX"), V(table, r, "RY"), V(table, r, "RZ") } });
        }

        private static Element Find(Dictionary<SourceIdentity, Element> index, ResultImportBatch batch, EntityFamily family, string id) =>
            index.TryGetValue(new SourceIdentity(CivilNxModelProfile.Program, batch.Source.ModelRevision, family, id), out var element) ? element : null;
        private static double V(CivilNxResultTableData table, int row, string column)
        {
            var text = table.Get(row, column);
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || double.IsNaN(value) || double.IsInfinity(value))
                throw new InvalidDataException("Non-numeric " + column + " '" + text + "' at row " + row);
            return value;
        }
        private static CoordinateSystem SourceAxes(CoordinateSystem axes, Point3d pointMm, double lengthToMm) =>
            Frames.At(axes, new Point3d(pointMm.X / lengthToMm, pointMm.Y / lengthToMm, pointMm.Z / lengthToMm));
        private static ResultState State(string loadCase, int count) => new ResultState { Semantics = AnalysisSemantics.LinearStatic,
            ConcomitantStateId = "Civil NX static case " + loadCase, IsCombined = false, IsCumulative = true,
            Components = Enumerable.Repeat(ComponentAvailability.Available, count).ToArray(), Coverage = "Explicit post/TABLE selection; beams at I, 1/4, 2/4, 3/4, J; plates at the centre." };
        private static string SourceHash(IReadOnlyList<CivilNxResponse> tables) =>
            GPC.Model.Core.ModelValues.Fingerprint(tables.Select(t => (object)t.Sha256));
    }
}
