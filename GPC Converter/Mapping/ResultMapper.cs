using GPC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Core;
using GPC.Model.Results;
using GPC.Model.Results.Storage;
using GPC.Model.Results.Locations;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Core.Units;
using GPC.Model.Results.Processing;
using GPC.Model.Results.State;

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
                var analysisFingerprint = batch.AnalysisInputFingerprint ?? model.Analysis?.InputFingerprint ?? fingerprint;
                if (model.Analysis != null && (analysisFingerprint != model.Analysis.InputFingerprint
                    || model.SolverBindingFingerprint() != model.Analysis.SolverBindingFingerprint()))
                    throw new ArgumentException("AnalysisSnapshotMismatch");
                if (string.IsNullOrWhiteSpace(batch.DatasetId) || model.Datasets.ContainsKey(batch.DatasetId) || model.ContainsResultsOf(batch.DatasetId))
                    throw new ArgumentException("MissingOrDuplicateDataset");
                if (batch.Units == null || string.IsNullOrWhiteSpace(batch.SourceHash) || string.IsNullOrWhiteSpace(batch.ReaderVersion)
                    || string.IsNullOrWhiteSpace(batch.ResolvedConvention)) throw new ArgumentException("MissingResultProvenance");
                var issues = model.ValidateTopology().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
                if (issues.Length != 0) { report.Diagnostics.AddRange(issues); return report; }
                var staged = new List<Tuple<Element, ElementResult>>(); var keys = new HashSet<SampleKey>();
                // One lookup per sample; same result as Model.FindBySource, which scans all elements and requires a unique match.
                var bySource = model.AllElements.Where(e => e.Source != null).ToLookup(e => e.Source);
                Element Find(SourceIdentity identity)
                {
                    var matches = bySource[identity].Take(2).ToArray();
                    if (matches.Length > 1) throw new InvalidOperationException("DuplicateSourceIdentity");
                    return matches.FirstOrDefault();
                }
                // Combinations of the Models module (1/11/2026): every row is read, checked and turned into its sample on its own (read-only
                // lookups in the model and the batch), in parallel; then, in the order of the rows, its exception is raised, its key checked
                // against the previous ones and its sample staged. The same samples, diagnostics and first rejection as one row at a time.
                var rows = batch.Beams.Cast<ResultRecord>().Concat(batch.Shells).Concat(batch.NodeForces).Concat(batch.NodeDisplacements).ToArray();
                var mapped = new MappedRow[rows.Length];
                var transformation = "Explicit source units to N,mm,rad; axes and signs resolved by " + batch.ReaderVersion;
                Parallel.For(0, rows.Length, new ParallelOptions { CancellationToken = cancellationToken }, i =>
                {
                    try { mapped[i] = Map(model, batch, rows[i], source, analysisFingerprint, transformation, Find, cancellationToken); }
                    catch (Exception ex) { mapped[i] = new MappedRow { Error = ExceptionDispatchInfo.Capture(ex) }; }
                });
                for (int i = 0; i < rows.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested(); var row = rows[i]; record = row?.Record;
                    var item = mapped[i];
                    if (item.Error != null) item.Error.Throw();
                    if (!keys.Add(item.Key)) throw new ArgumentException("DuplicateResultStateAndLocation");
                    staged.Add(Tuple.Create(item.Element, item.Result));
                    if (item.Incomplete)
                        report.Diagnostics.Add(new ModelDiagnostic { Code = "IncompleteImportedSample", Severity = DiagnosticSeverity.Warning, Family = item.Family, ElementId = item.Element.Id, Record = record });
                }
                if (staged.Count == 0) throw new ArgumentException("EmptyResultDataset");
                cancellationToken.ThrowIfCancellationRequested();
                if (model.AnalysisFingerprint() != fingerprint) throw new InvalidOperationException("ModelChangedDuringImport");
                var dataset = new AnalysisDataset { Id = batch.DatasetId, Program = source.Program, SolverVersion = source.SolverVersion,
                    ModelRevision = source.ModelRevision, AnalysisId = source.AnalysisId, InputFingerprint = analysisFingerprint, AnalysisSnapshotId = model.Analysis?.Id,
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
        /// <summary>A row read and checked, with its sample, or the exception that rejects it.</summary>
        private sealed class MappedRow
        {
            public ExceptionDispatchInfo Error;
            public Element Element;
            public ElementResult Result;
            public SampleKey Key;
            public EntityFamily Family;
            public bool Incomplete;
        }

        /// <summary>The checks, values, state and sample of one row, in the order of the historical loop; the duplicate check and the
        /// staging are left to the caller, in the order of the rows.</summary>
        private static MappedRow Map(GPC.Model.Models.Model model, ResultImportBatch batch, ResultRecord row, AnalysisSource source, string analysisFingerprint, string transformation,
            Func<SourceIdentity, Element> find, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested(); var record = row?.Record;
            if (row == null || string.IsNullOrWhiteSpace(row.Case) || string.IsNullOrWhiteSpace(row.Record) || row.State == null)
                throw new ArgumentException("MissingResultStateOrRecord");
            // Results never create load definitions, since that would silently change the analysis fingerprint.
            ILoadCase loadCase = model.LoadCases.TryGetValue(row.Case, out var lc) ? (ILoadCase)lc :
                model.Combinations.TryGetValue(row.Case, out var combination) ? combination : throw new ArgumentException("MissingOrUnmappedCase");
            var family = row is BeamForceRecord ? EntityFamily.Beam : row is ShellForceRecord ? EntityFamily.Shell : EntityFamily.Node;
            var element = find(new SourceIdentity(source.Program, source.ModelRevision, family, row.ElementId));
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
            var state = row.State.Copy(); state.DatasetId = batch.DatasetId; state.InputFingerprint = analysisFingerprint; state.ModelRevision = source.ModelRevision;
            if (state.Semantics != batch.Semantics) throw new ArgumentException("InconsistentAnalysisSemantics");
            state.IsSynthetic = batch.IsSynthetic; state.SourceHash = batch.SourceHash; state.SourceRecord = record;
            state.Normalization = "N,mm,rad"; state.Transformation = transformation;
            state.Original = new OriginalResultData { Values = (double?[])row.Values.Clone(), Axes = CopyAxes(row.Axes, null),
                ComponentOrder = row is NodeForceRecord ? "Fx,Fy,Fz,Mx,My,Mz" : row is NodeDisplacementRecord ? "Dx,Dy,Dz,Rx,Ry,Rz"
                    : count == 6 ? "N,V1,V2,T,M1,M2" : "Fxx,Fyy,Fxy,Fxz,Fyz,Mxx,Myy,Mxy",
                Convention = batch.ResolvedConvention, ReaderVersion = batch.ReaderVersion, ForceToN = batch.Units.ForceToN,
                LengthToMm = batch.Units.LengthToMm, MomentToNmm = batch.Units.MomentToNmm, AngleToRad = batch.Units.AngleToRad,
                DenominatorLengthToMm = batch.ShellDenominatorLengthToMm };
            var axes = CopyAxes(row.Axes, batch.Units);
            var mapped = new MappedRow { Element = element, Family = family };
            ResultLocation sample;
            if (row is BeamForceRecord beam)
            {
                sample = new StationResultBeamForces(loadCase, new ResultBeamForces(values[0], values[1], values[2], values[3], values[4], values[5], axes), beam.Station)
                { Side = beam.Side, Body = beam.Body, StationDomain = beam.StationDomain, PhysicalDistance = beam.PhysicalDistance.HasValue ? batch.Units.Length(beam.PhysicalDistance.Value) : (double?)null, State = state };
                mapped.Key = new SampleKey(family, element.Id, row.Case, state) { Number1 = SampleKey.Scalar(beam.Station), Enum1 = (long)beam.Side };
                mapped.Result = new BeamResult(new[] { (IBeamResultLocation)sample });
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
                    var node = find(new SourceIdentity(source.Program, source.ModelRevision, EntityFamily.Node, shell.SourceNodeId)) as NodeElement;
                    if (node == null || !((AreaElement)element).Nodes.Any(n => ReferenceEquals(n, node))) throw new ArgumentException("InvalidShellPointNode");
                    nodeId = node.Id;
                }
                sample = new PointResultPlateForces(loadCase, new ResultPlateForces(axes, values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]), location, shell.PointKind.ToString())
                { PointKind = shell.PointKind, CoordinateKind = shell.CoordinateKind, SourceNodeId = nodeId, AveragingRegion = shell.AveragingRegion, State = state };
                mapped.Key = new SampleKey(family, element.Id, row.Case, state)
                { Number1 = SampleKey.Raw(location.X), Number2 = SampleKey.Raw(location.Y), Enum1 = (long)shell.PointKind, Enum2 = (long)shell.CoordinateKind, Integer1 = nodeId, Text1 = SampleKey.Text(shell.AveragingRegion) };
                mapped.Result = new PlateElementResult(new List<IPlateResultLocation> { (IPlateResultLocation)sample });
            }
            else
            {
                var node = (NodeElement)element;
                if (Axes.Length(axes.Origin - node.Position) > 1e-7) throw new ArgumentException("NodalResultReductionPointMustBeNode");
                if (row is NodeForceRecord nodal)
                {
                    var owner = nodal.OwnerSource == null ? null : find(nodal.OwnerSource);
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
                mapped.Key = new SampleKey(family, element.Id, row.Case, state)
                {
                    Flag = row is NodeDisplacementRecord, Enum3 = forceSample == null ? (long?)null : (long)forceSample.Kind, Enum4 = forceSample == null ? (long?)null : (long)forceSample.Body,
                    Enum5 = forceSample?.OwnerElementFamily == null ? (long?)null : (long)forceSample.OwnerElementFamily.Value, Integer1 = forceSample?.OwnerElementId,
                    Text1 = SampleKey.Text(forceSample?.ElementEnd), Text2 = SampleKey.Text(forceSample?.AggregationSet)
                };
                mapped.Result = new NodeResult(new List<INodeResultLocation> { (INodeResultLocation)sample });
            }
            mapped.Incomplete = state.Components.Any(c => c != ComponentAvailability.Available);
            return mapped;
        }

        /// <summary>
        /// The state and location of a sample that two samples of a dataset may not share: equal exactly when the canonical bytes of
        /// <see cref="ModelValues.Fingerprint"/> over the same values are equal (the digest the import compared before, one SHA-256 per
        /// sample): enumerations by their value, a station with -0 written as 0, the coordinates of a point and every other double by
        /// their bits, texts by their UTF-8 bytes (an unpaired surrogate becomes U+FFFD, as the writer of the canonical bytes encodes it).
        /// The family comes first and fixes which fields a key uses.
        /// </summary>
        private struct SampleKey : IEquatable<SampleKey>
        {
            private readonly EntityFamily family; private readonly int element; private readonly int? mode;
            private readonly string loadCase, phase, step, movingLoad, concomitant;
            public long Number1, Number2, Enum1, Enum2; public long? Enum3, Enum4, Enum5; public int? Integer1; public string Text1, Text2; public bool Flag;

            public SampleKey(EntityFamily family, int element, string loadCase, ResultState state) : this()
            {
                this.family = family; this.element = element; this.loadCase = Text(loadCase); phase = Text(state.Phase); step = Text(state.Step);
                mode = state.Mode; movingLoad = Text(state.MovingLoadPosition); concomitant = Text(state.ConcomitantStateId);
            }

            /// <summary>A double written on its own (<c>d == 0 ? 0.0 : d</c>).</summary>
            public static long Scalar(double value) => BitConverter.DoubleToInt64Bits(value == 0 ? 0.0 : value);
            /// <summary>A coordinate of a point, written as it is.</summary>
            public static long Raw(double value) => BitConverter.DoubleToInt64Bits(value);
            public static string Text(string value)
            {
                if (value == null) return null;
                for (int i = 0; i < value.Length; i++)
                    if (char.IsSurrogate(value[i])) return System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(value));
                return value;
            }

            public bool Equals(SampleKey other) => family == other.family && element == other.element && mode == other.mode &&
                string.Equals(loadCase, other.loadCase, StringComparison.Ordinal) && string.Equals(phase, other.phase, StringComparison.Ordinal) &&
                string.Equals(step, other.step, StringComparison.Ordinal) && string.Equals(movingLoad, other.movingLoad, StringComparison.Ordinal) &&
                string.Equals(concomitant, other.concomitant, StringComparison.Ordinal) && Number1 == other.Number1 && Number2 == other.Number2 &&
                Enum1 == other.Enum1 && Enum2 == other.Enum2 && Enum3 == other.Enum3 && Enum4 == other.Enum4 && Enum5 == other.Enum5 &&
                Integer1 == other.Integer1 && string.Equals(Text1, other.Text1, StringComparison.Ordinal) && string.Equals(Text2, other.Text2, StringComparison.Ordinal) && Flag == other.Flag;
            public override bool Equals(object obj) => obj is SampleKey other && Equals(other);
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = (int)family * 397 ^ element;
                    hash = hash * 31 + (loadCase == null ? 0 : StringComparer.Ordinal.GetHashCode(loadCase));
                    hash = hash * 31 + (concomitant == null ? 0 : StringComparer.Ordinal.GetHashCode(concomitant));
                    hash = hash * 31 + Number1.GetHashCode(); hash = hash * 31 + Number2.GetHashCode();
                    hash = hash * 31 + (int)Enum1; hash = hash * 31 + (int)Enum2; hash = hash * 31 + (Integer1 ?? -1);
                    return hash * 31 + (Flag ? 1 : 0);
                }
            }
        }

        private static CoordinateSystem CopyAxes(CoordinateSystem axes, ResultUnits units) => new CoordinateSystem(
            new Point3d(units == null ? axes.Origin.X : units.Length(axes.Origin.X), units == null ? axes.Origin.Y : units.Length(axes.Origin.Y), units == null ? axes.Origin.Z : units.Length(axes.Origin.Z)),
            new Vector3d(axes.V1.X, axes.V1.Y, axes.V1.Z), new Vector3d(axes.V2.X, axes.V2.Y, axes.V2.Z), new Vector3d(axes.V3.X, axes.V3.Y, axes.V3.Z));
    }
}
