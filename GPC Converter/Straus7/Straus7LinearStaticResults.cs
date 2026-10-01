using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.PostProcessing;

namespace GPC.Converter.Straus7
{
    /// <summary>Documented R3 linear-static force convention. Requires an unchanged API-imported model,
    /// explicit case mapping, initial axes and unstaged primary cases. Does not attest section-centroid offsets.</summary>
    public static class Straus7LinearStaticResults
    {
        public static ResultImportReport Import(GPC.Model.Models.Model model, Straus7ResultsReport native,
            string datasetId, IReadOnlyDictionary<int, string> caseMap, bool isSynthetic = false, CancellationToken cancellationToken = default)
        {
            var rejected = new ResultImportReport { Status = ImportStatus.Rejected };
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (model == null || native == null || caseMap == null) throw new ArgumentNullException();
                if (native.Status != ImportStatus.Completed || native.File == null || native.File.ValidationFlags != 0)
                    throw new InvalidDataException("Only successfully acquired, natively validated results can be normalized.");
                if (native.File.Solver != 1 || native.ModelStageCount != 0 || native.Cases.Any(c => c.Stage != 0))
                    throw new NotSupportedException("This profile supports unstaged linear-static primary cases only.");
                var source = model.AnalysisSource;
                if (source == null || source.Program != "Straus7" || source.GeometryHash != native.ModelHash || source.SolverVersion != native.ApiVersion
                    || string.IsNullOrWhiteSpace(source.AnalysisId) || string.IsNullOrWhiteSpace(native.ResultHash)) throw new InvalidDataException("Model/result source mismatch or missing analysis identity.");
                var bindings = model.PreservedSourceData.Where(p => p.Kind == "Straus7 input binding" && p.SourceRecord == native.ModelHash).ToArray();
                if (bindings.Length != 1 || bindings[0].RawData != model.AnalysisFingerprint()) throw new InvalidDataException("Model inputs changed since API import; re-establish the source binding before importing results.");
                if (native.Cases.Select(c => c.Number).Distinct().Count() != native.Cases.Count) throw new InvalidDataException("Duplicate result case metadata.");
                var caseNumbers = new HashSet<int>(native.Cases.Select(c => c.Number));
                foreach (var c in native.Cases)
                    if (c.Number < 1 || c.Number > native.File.PrimaryCases || !caseMap.TryGetValue(c.Number, out var name) || !model.LoadCases.ContainsKey(name))
                        throw new InvalidDataException("Explicit mapping to an existing Model load case is required for case " + c.Number);
                if (native.Cases.Select(c => caseMap[c.Number]).Distinct(StringComparer.Ordinal).Count() != native.Cases.Count)
                    throw new InvalidDataException("Different native result cases must not be collapsed into the same Model case.");
                var units = Straus7ApiConverter.Units(native.Units);
                var batch = new ResultImportBatch { Source = source, SourceHash = native.ResultHash, DatasetId = datasetId,
                    ExpectedInputFingerprint = model.AnalysisFingerprint(), ReaderVersion = "GPC.Straus7.R3.LinearStatic/2",
                    ResolvedConvention = "Beam: negative of global equilibrating actions on End-2 cut, projected onto GPC V1/V2/V3; N tension-positive. Plate: local stress-resultant tensor; positive Mxx/Myy tension on +z; per native width. Nodal reactions: XYZ forces/moments on node. Element node forces: XYZ forces/moments on owning element. Nodal displacements: XYZ translations and rotations in radians.",
                    Units = units, ShellDenominatorLengthToMm = units.LengthToMm, Semantics = AnalysisSemantics.LinearStatic, IsSynthetic = isSynthetic };
                var keys = new HashSet<string>();
                foreach (var table in native.Tables)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (table == null || !caseNumbers.Contains(table.CaseNumber) || table.Number < 1 || table.Columns != 6 || table.Rows <= 0)
                        throw new InvalidDataException("Unsupported native table shape or case.");
                    if (table.Entity != Straus7Entity.Node && table.Entity != Straus7Entity.Beam && table.Entity != Straus7Entity.Plate)
                        throw new NotSupportedException("Unsupported native result entity.");
                    Straus7ApiConverter.Finite(table.Values, checked(table.Rows * table.Columns), table.Quantity);
                    if (!keys.Add(table.Entity + "/" + table.Number + "/" + table.CaseNumber + "/" + table.Quantity)) throw new InvalidDataException("Duplicate native table.");
                    if (table.Entity == Straus7Entity.Node)
                    {
                        var node = model.FindBySource(new SourceIdentity("Straus7", source.ModelRevision, EntityFamily.Node, Straus7ApiConverter.Id(table.Number))) as NodeElement;
                        if (node == null || table.Rows != 1) throw new InvalidDataException("Nodal result has no matching node or invalid row count.");
                        var axes = SourceAxes(CoordinateSystem.Global, node.Position, units.LengthToMm);
                        var values = table.Values.Select(v => (double?)v).ToArray(); var record = "St7GetNodeResult/" + table.Quantity + "/" + table.Number + "/case/" + table.CaseNumber;
                        if (table.Quantity == "NodeReactionGlobal") batch.NodeForces.Add(new NodeForceRecord { ElementId = node.Source.OriginalId,
                            Case = caseMap[table.CaseNumber], State = State(table.CaseNumber, 6), Axes = axes, Values = values, Record = record,
                            Kind = NodalForceKind.SupportReaction, Body = ActionBody.OnNode });
                        else if (table.Quantity == "NodeDisplacementGlobal") batch.NodeDisplacements.Add(new NodeDisplacementRecord { ElementId = node.Source.OriginalId,
                            Case = caseMap[table.CaseNumber], State = State(table.CaseNumber, 6), Axes = axes, Values = values, Record = record });
                        else throw new NotSupportedException("Unsupported nodal quantity: " + table.Quantity);
                        continue;
                    }
                    // R3.1.5 reports birth stage 1 even for an unstaged linear-static model (case stage 0).
                    // Construction stages are excluded above using the model and result-case metadata.
                    if (table.ElementState == null || table.ElementState.Length != 3 || table.ElementState[0] != 1 || table.ElementState[1] != 1
                        || table.ElementState[2] < 0 || table.ElementState[2] > 1)
                        throw new NotSupportedException("Inactive, missing or staged element result.");
                    string id = Straus7ApiConverter.Id(table.Number); var family = table.Entity == Straus7Entity.Beam ? EntityFamily.Beam : EntityFamily.Shell;
                    var element = model.FindBySource(new SourceIdentity("Straus7", source.ModelRevision, family, id));
                    if (element == null) throw new InvalidDataException("Native result refers to an absent element.");
                    if (table.Quantity == "ElementNodeForceGlobal")
                    {
                        var nodes = element is BeamElement ownerBeam ? new[] { ownerBeam.NodeI, ownerBeam.NodeJ }
                            : element is AreaElement ownerPlate ? ownerPlate.Nodes.ToArray() : throw new NotSupportedException("Element node force family.");
                        if (table.NodeNumbers == null || table.Rows != nodes.Length || !table.NodeNumbers.Select(Straus7ApiConverter.Id).SequenceEqual(nodes.Select(n => n.Source?.OriginalId)))
                            throw new InvalidDataException("Element node force rows do not match the owner's ordered connectivity.");
                        for (int row = 0; row < nodes.Length; row++)
                        {
                            var node = nodes[row];
                            batch.NodeForces.Add(new NodeForceRecord { ElementId = node.Source.OriginalId, Case = caseMap[table.CaseNumber], State = State(table.CaseNumber, 6),
                                Axes = SourceAxes(CoordinateSystem.Global, node.Position, units.LengthToMm), Values = table.Values.Skip(row * 6).Take(6).Select(v => (double?)v).ToArray(),
                                Kind = element is BeamElement ? NodalForceKind.ElementEndForce : NodalForceKind.ElementNodeForce, Body = ActionBody.OnElement,
                                OwnerSource = element.Source, ElementEnd = element is BeamElement ? (row == 0 ? "I" : "J") : null,
                                Record = "Straus7 ElementNodeForceGlobal/" + table.Entity + "/" + id + "/node/" + node.Source.OriginalId + "/case/" + table.CaseNumber });
                        }
                    }
                    else if (table.Entity == Straus7Entity.Beam && table.Quantity == "BeamForceGlobal")
                    {
                        var beam = (BeamElement)element; var axes = beam.Assignments.SectionAxes; Axes.Validate(axes);
                        Straus7ApiConverter.Finite(table.Positions, table.Rows, "Beam stations");
                        if (table.Positions.Any(p => p < 0 || p > 1) || table.Positions.Distinct().Count() != table.Rows
                            || !table.Positions.SequenceEqual(table.Positions.OrderBy(p => p)))
                            throw new NotSupportedException("Ambiguous beam stations/discontinuity sides require an explicit profile; they cannot be merged or averaged.");
                        for (int row = 0; row < table.Rows; row++)
                        {
                            int at = row * 6; var v = table.Values;
                            var force = new Vector3d(-v[at], -v[at + 2], -v[at + 4]);
                            var moment = new Vector3d(-v[at + 1], -v[at + 3], -v[at + 5]);
                            double station = table.Positions[row];
                            Vector3d delta = beam.EndPoint - beam.StartPoint;
                            var p = beam.StartPoint + delta * station;
                            batch.Beams.Add(new BeamForceRecord { ElementId = id, Case = caseMap[table.CaseNumber], State = State(table.CaseNumber, 6),
                                Axes = SourceAxes(axes, p, units.LengthToMm), Values = new double?[] { Axes.Dot(force, axes.V3), Axes.Dot(force, axes.V1), Axes.Dot(force, axes.V2),
                                    Axes.Dot(moment, axes.V3), Axes.Dot(moment, axes.V1), Axes.Dot(moment, axes.V2) },
                                Station = station, PhysicalDistance = beam.Length * station / units.LengthToMm, Body = ActionBody.PositiveSectionFace,
                                StationDomain = "Straus7 bpParam, reference element length; offsets not resolved", Record = "St7GetBeamResultArray Beam/" + id + "/case/" + table.CaseNumber + "/row/" + row });
                        }
                    }
                    else if (table.Entity == Straus7Entity.Plate && table.Quantity == "PlateForceLocalCentroid")
                    {
                        var shell = (AreaElement)element;
                        var moments = native.Tables.Where(t => t.Entity == Straus7Entity.Plate && t.Number == table.Number && t.CaseNumber == table.CaseNumber && t.Quantity == "PlateMomentLocalCentroid").ToArray();
                        if (table.Rows != 1 || moments.Length != 1 || moments[0].Rows != 1 || moments[0].Columns != 6) throw new InvalidDataException("Matching centroid force and moment tables are required.");
                        Straus7ApiConverter.Finite(moments[0].Values, 6, "Plate moments");
                        var f = table.Values; var m = moments[0].Values;
                        if (f[2] != 0 || m[2] != 0 || m[4] != 0 || m[5] != 0) throw new NotSupportedException("Nonzero out-of-plane tensor terms are not represented by the shell resultant contract.");
                        Axes.Validate(shell.CoordinateSystem);
                        batch.Shells.Add(new ShellForceRecord { ElementId = id, Case = caseMap[table.CaseNumber], State = State(table.CaseNumber, 8),
                            Axes = SourceAxes(shell.CoordinateSystem, shell.CoordinateSystem.Origin, units.LengthToMm), Values = new double?[] { f[0], f[1], f[3], f[5], f[4], m[0], m[1], m[3] },
                            Location = new Point2d(0, 0), // Origin is the native centroid, not an assumed UV coordinate of an irregular quad.
                            CoordinateKind = ResultCoordinateKind.LocalPhysical, PointKind = ShellResultPointKind.Centroid,
                            Record = "St7GetPlateResultArray Plate/" + id + "/case/" + table.CaseNumber + "/centroid (no averaging)" });
                    }
                    else if (table.Entity != Straus7Entity.Plate || table.Quantity != "PlateMomentLocalCentroid") throw new NotSupportedException("Unsupported native quantity: " + table.Quantity);
                }
                foreach (var moments in native.Tables.Where(t => t.Quantity == "PlateMomentLocalCentroid"))
                    if (!native.Tables.Any(t => t.Quantity == "PlateForceLocalCentroid" && t.Entity == moments.Entity && t.Number == moments.Number && t.CaseNumber == moments.CaseNumber))
                        throw new InvalidDataException("Plate moments without matching forces.");
                var evidence = new[]
                {
                    Straus7ApiConverter.Evidence("ResultFile/" + native.ResultHash, native.Tables,
                        "Native arrays before permutation/rotation/unit conversion, including nodal and element node forces."),
                    Straus7ApiConverter.Evidence("ResultUnits/" + native.ResultHash, native.Units),
                    Straus7ApiConverter.Evidence("ResultCases/" + native.ResultHash, native.Cases),
                    Straus7ApiConverter.Evidence("ResultValidation/" + native.ResultHash, native.File)
                };
                var result = ResultMapper.Import(model, batch, cancellationToken);
                if (result.Status == ImportStatus.Completed || result.Status == ImportStatus.Partial) model.PreservedSourceData.AddRange(evidence);
                return result;
            }
            catch (OperationCanceledException) { rejected.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException || ex is InvalidOperationException || ex is NotSupportedException || ex is OverflowException)
            { rejected.Diagnostics.Add(ModelDiagnostic.Error("Straus7NormalizationRejected", message: ex.Message)); }
            return rejected;
        }
        private static CoordinateSystem SourceAxes(CoordinateSystem axes, Point3d pointMm, double lengthToMm) =>
            new CoordinateSystem(new Point3d(pointMm.X / lengthToMm, pointMm.Y / lengthToMm, pointMm.Z / lengthToMm), axes.V1, axes.V2, axes.V3);
        private static ResultState State(int resultCase, int count) => new ResultState { Semantics = AnalysisSemantics.LinearStatic,
            ConcomitantStateId = "Straus7 primary case " + resultCase, IsCombined = false, IsCumulative = true,
            Components = Enumerable.Repeat(ComponentAvailability.Available, count).ToArray(), Coverage = "Explicit API selection only; no continuous-maximum or full-model coverage assertion." };
    }
}
