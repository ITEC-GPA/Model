using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results;
using GPC.Model.Results.ElementResults;
using GPC.Model.Results.ResultLocations;

namespace GPC.Converter
{
    /// <summary>Validate and stage a complete dataset before attaching it. Call with exclusive ownership of the model.</summary>
    public static class ResultMapper
    {
        public static ResultImportReport Import(GPC.Model.Models.Model model, ResultImportBatch batch, CancellationToken cancellationToken = default)
        {
            if (model == null || batch == null) throw new ArgumentNullException();
            var report = new ResultImportReport { Status = ImportStatus.Rejected };
            string record = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var source = batch.Source; var geometry = model.AnalysisSource;
                if (source == null || geometry == null || string.IsNullOrWhiteSpace(source.Program) || string.IsNullOrWhiteSpace(source.SolverVersion)
                    || string.IsNullOrWhiteSpace(source.ModelRevision) || string.IsNullOrWhiteSpace(source.AnalysisId)
                    || source.Program != geometry.Program || source.SolverVersion != geometry.SolverVersion
                    || source.ModelRevision != geometry.ModelRevision || source.AnalysisId != geometry.AnalysisId)
                    throw new ArgumentException("AnalysisSourceMismatch");
                var fingerprint = model.AnalysisFingerprint();
                if (batch.ExpectedInputFingerprint != fingerprint) throw new ArgumentException("InputFingerprintMismatch");
                if (string.IsNullOrWhiteSpace(batch.DatasetId) || model.Datasets.ContainsKey(batch.DatasetId)
                    || model.AllElements.SelectMany(e => e.Results).SelectMany(r => r.Results).Any(r => r.State?.DatasetId == batch.DatasetId))
                    throw new ArgumentException("MissingOrDuplicateDataset");
                if (batch.Units == null || string.IsNullOrWhiteSpace(batch.SourceHash) || string.IsNullOrWhiteSpace(batch.ReaderVersion)
                    || string.IsNullOrWhiteSpace(batch.ResolvedConvention)) throw new ArgumentException("MissingResultProvenance");
                var issues = model.ValidateTopology().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
                if (issues.Length != 0) { report.Diagnostics.AddRange(issues); return report; }
                var staged = new List<Tuple<Element, ElementResult>>(); var keys = new HashSet<string>();
                // One lookup per sample; same result as Model.FindBySource, which scans all elements and requires a unique match.
                var bySource = model.AllElements.Where(e => e.Source != null).ToLookup(e => e.Source);
                Element Find(SourceIdentity identity)
                {
                    var matches = bySource[identity].Take(2).ToArray();
                    if (matches.Length > 1) throw new InvalidOperationException("DuplicateSourceIdentity");
                    return matches.FirstOrDefault();
                }
                foreach (ResultRecord row in batch.Beams.Cast<ResultRecord>().Concat(batch.Shells).Concat(batch.NodeForces).Concat(batch.NodeDisplacements))
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = row?.Record;
                    if (row == null || string.IsNullOrWhiteSpace(row.Case) || string.IsNullOrWhiteSpace(row.Record) || row.State == null)
                        throw new ArgumentException("MissingResultStateOrRecord");
                    // Results never create load definitions, since that would silently change the analysis fingerprint.
                    ILoadCase loadCase = model.LoadCases.TryGetValue(row.Case, out var lc) ? (ILoadCase)lc :
                        model.Combinations.TryGetValue(row.Case, out var combination) ? combination : throw new ArgumentException("MissingOrUnmappedCase");
                    var family = row is BeamForceRecord ? EntityFamily.Beam : row is ShellForceRecord ? EntityFamily.Shell : EntityFamily.Node;
                    var element = Find(new SourceIdentity(source.Program, source.ModelRevision, family, row.ElementId));
                    if (element == null) throw new ArgumentException("MissingSourceElement");
                    var count = family == EntityFamily.Shell ? 8 : 6;
                    if (row.Values == null || row.Values.Length != count || row.State.Components == null || row.State.Components.Length != count)
                        throw new ArgumentException("InvalidComponentCount");
                    Axes.Validate(row.Axes);
                    var values = new double[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (!Enum.IsDefined(typeof(ComponentAvailability), row.State.Components[i])) throw new ArgumentException("InvalidComponentFlag");
                        if (row.State.Components[i] == ComponentAvailability.Available && !row.Values[i].HasValue) throw new ArgumentException("AvailableComponentMissing");
                        if (row.Values[i].HasValue && (double.IsNaN(row.Values[i].Value) || double.IsInfinity(row.Values[i].Value))) throw new ArgumentException("NonFiniteSourceValue");
                        values[i] = row.Values[i].HasValue ? row is NodeDisplacementRecord
                            ? (i < 3 ? batch.Units.Length(row.Values[i].Value) : batch.Units.Angle(row.Values[i].Value)) : family != EntityFamily.Shell
                            ? (i < 3 ? batch.Units.Force(row.Values[i].Value) : batch.Units.BeamMoment(row.Values[i].Value))
                            : (i < 5 ? batch.Units.ForcePerLength(row.Values[i].Value, batch.ShellDenominatorLengthToMm) : batch.Units.ShellMoment(row.Values[i].Value, batch.ShellDenominatorLengthToMm))
                            : double.NaN;
                    }
                    var state = row.State.Copy(); state.DatasetId = batch.DatasetId; state.InputFingerprint = fingerprint; state.ModelRevision = source.ModelRevision;
                    if (state.Semantics != batch.Semantics) throw new ArgumentException("InconsistentAnalysisSemantics");
                    state.IsSynthetic = batch.IsSynthetic; state.SourceHash = batch.SourceHash; state.SourceRecord = record;
                    state.Normalization = "N,mm,rad"; state.Transformation = "Explicit source units to N,mm,rad; axes and signs resolved by " + batch.ReaderVersion;
                    state.Original = new OriginalResultData { Values = (double?[])row.Values.Clone(), Axes = CopyAxes(row.Axes, null),
                        ComponentOrder = row is NodeForceRecord ? "Fx,Fy,Fz,Mx,My,Mz" : row is NodeDisplacementRecord ? "Dx,Dy,Dz,Rx,Ry,Rz"
                            : count == 6 ? "N,V1,V2,T,M1,M2" : "Fxx,Fyy,Fxy,Fxz,Fyz,Mxx,Myy,Mxy",
                        Convention = batch.ResolvedConvention, ReaderVersion = batch.ReaderVersion, ForceToN = batch.Units.ForceToN,
                        LengthToMm = batch.Units.LengthToMm, MomentToNmm = batch.Units.MomentToNmm, AngleToRad = batch.Units.AngleToRad,
                        DenominatorLengthToMm = batch.ShellDenominatorLengthToMm };
                    var axes = CopyAxes(row.Axes, batch.Units);
                    ResultLocation sample;
                    if (row is BeamForceRecord beam)
                    {
                        sample = new StationResultBeamForces(loadCase, new ResultBeamForces(values[0], values[1], values[2], values[3], values[4], values[5], axes), beam.Station)
                        { Side = beam.Side, Body = beam.Body, StationDomain = beam.StationDomain, PhysicalDistance = beam.PhysicalDistance.HasValue ? batch.Units.Length(beam.PhysicalDistance.Value) : (double?)null, State = state };
                        var key = ModelArchive.Fingerprint(new object[] { family, element.Id, row.Case, state.Phase, state.Step, state.Mode, state.MovingLoadPosition,
                            state.ConcomitantStateId, beam.Station, beam.Side });
                        if (!keys.Add(key)) throw new ArgumentException("DuplicateResultStateAndLocation");
                        staged.Add(Tuple.Create(element, (ElementResult)new BeamResult(new[] { (IBeamResultLocation)sample })));
                    }
                    else if (row is ShellForceRecord shell)
                    {
                        if (shell.Location == null || double.IsNaN(shell.Location.X) || double.IsNaN(shell.Location.Y)
                            || double.IsInfinity(shell.Location.X) || double.IsInfinity(shell.Location.Y)) throw new ArgumentException("InvalidShellLocation");
                        var location = shell.CoordinateKind == ResultCoordinateKind.LocalPhysical ? new Point2d(batch.Units.Length(shell.Location.X), batch.Units.Length(shell.Location.Y))
                            : new Point2d(shell.Location.X, shell.Location.Y);
                        if (shell.CoordinateKind == ResultCoordinateKind.Global) throw new NotSupportedException("UseLocalPhysicalOrNaturalShellPoint");
                        int? nodeId = null;
                        if (shell.SourceNodeId != null)
                        {
                            var node = Find(new SourceIdentity(source.Program, source.ModelRevision, EntityFamily.Node, shell.SourceNodeId)) as NodeElement;
                            if (node == null || !((AreaElement)element).Nodes.Any(n => ReferenceEquals(n, node))) throw new ArgumentException("InvalidShellPointNode");
                            nodeId = node.Id;
                        }
                        sample = new PointResultPlateForces(loadCase, new ResultPlateForces(axes, values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]), location, shell.PointKind.ToString())
                        { PointKind = shell.PointKind, CoordinateKind = shell.CoordinateKind, SourceNodeId = nodeId, AveragingRegion = shell.AveragingRegion, State = state };
                        var key = ModelArchive.Fingerprint(new object[] { family, element.Id, row.Case, state.Phase, state.Step, state.Mode, state.MovingLoadPosition,
                            state.ConcomitantStateId, location, shell.PointKind, shell.CoordinateKind, nodeId, shell.AveragingRegion });
                        if (!keys.Add(key)) throw new ArgumentException("DuplicateResultStateAndLocation");
                        staged.Add(Tuple.Create(element, (ElementResult)new PlateElementResult(new List<IPlateResultLocation> { (IPlateResultLocation)sample })));
                    }
                    else
                    {
                        var node = (NodeElement)element;
                        if (Axes.Length(axes.Origin - node.Position) > 1e-7) throw new ArgumentException("NodalResultReductionPointMustBeNode");
                        if (row is NodeForceRecord nodal)
                        {
                            var owner = nodal.OwnerSource == null ? null : Find(nodal.OwnerSource);
                            if (nodal.OwnerSource != null && owner == null) throw new ArgumentException("MissingNodalForceOwner");
                            var forces = new NodeResultForces(loadCase, new ResultBeamForces(values[2], values[0], values[1], values[5], values[3], values[4], axes))
                            {
                                Kind = nodal.Kind, Body = nodal.Body, OwnerElementId = owner?.Id,
                                OwnerElementFamily = owner == null ? (EntityFamily?)null : GPC.Model.Models.Model.FamilyOf(owner),
                                ElementEnd = nodal.ElementEnd, AggregationSet = nodal.AggregationSet, State = state
                            };
                            NodalActions.Validate(model, node, forces); sample = forces;
                        }
                        else sample = new NodeResultDisplacement(loadCase, new ResultDisplacement(axes, values[0], values[1], values[2], values[3], values[4], values[5])) { State = state };
                        var forceSample = sample as NodeResultForces;
                        var key = ModelArchive.Fingerprint(new object[] { family, element.Id, row.Case, state.Phase, state.Step, state.Mode, state.MovingLoadPosition,
                            state.ConcomitantStateId, row is NodeDisplacementRecord, forceSample?.Kind, forceSample?.Body,
                            forceSample?.OwnerElementFamily, forceSample?.OwnerElementId, forceSample?.ElementEnd, forceSample?.AggregationSet });
                        if (!keys.Add(key)) throw new ArgumentException("DuplicateResultStateAndLocation");
                        staged.Add(Tuple.Create(element, (ElementResult)new NodeResult(new List<INodeResultLocation> { (INodeResultLocation)sample })));
                    }
                    if (state.Components.Any(c => c != ComponentAvailability.Available))
                        report.Diagnostics.Add(new ModelDiagnostic { Code = "IncompleteImportedSample", Severity = DiagnosticSeverity.Warning, Family = family, ElementId = element.Id, Record = record });
                }
                if (staged.Count == 0) throw new ArgumentException("EmptyResultDataset");
                cancellationToken.ThrowIfCancellationRequested();
                if (model.AnalysisFingerprint() != fingerprint) throw new InvalidOperationException("ModelChangedDuringImport");
                var dataset = new AnalysisDataset { Id = batch.DatasetId, Program = source.Program, SolverVersion = source.SolverVersion,
                    ModelRevision = source.ModelRevision, AnalysisId = source.AnalysisId, InputFingerprint = fingerprint,
                    NormalizedUnits = "N,mm,rad", Semantics = batch.Semantics, IsSynthetic = batch.IsSynthetic };
                dataset.SourceHashes.Add("results", batch.SourceHash);
                if (!string.IsNullOrEmpty(geometry.GeometryHash)) dataset.SourceHashes.Add("geometry", geometry.GeometryHash);
                model.Datasets.Add(dataset.Id, dataset);
                foreach (var item in staged) item.Item1.AddResult(item.Item2);
                report.ImportedSamples = staged.Count;
                report.Status = report.Diagnostics.Count == 0 ? ImportStatus.Completed : ImportStatus.Partial;
            }
            catch (OperationCanceledException) { report.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { report.Diagnostics.Add(new ModelDiagnostic { Code = "ResultImportRejected", Severity = DiagnosticSeverity.Error, Record = record, Message = ex.Message }); }
            return report;
        }
        private static CoordinateSystem CopyAxes(CoordinateSystem axes, ResultUnits units) => new CoordinateSystem(
            new Point3d(units == null ? axes.Origin.X : units.Length(axes.Origin.X), units == null ? axes.Origin.Y : units.Length(axes.Origin.Y), units == null ? axes.Origin.Z : units.Length(axes.Origin.Z)),
            new Vector3d(axes.V1.X, axes.V1.Y, axes.V1.Z), new Vector3d(axes.V2.X, axes.V2.Y, axes.V2.Z), new Vector3d(axes.V3.X, axes.V3.Y, axes.V3.Z));
    }
}
