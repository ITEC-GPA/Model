using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    public sealed class ShellCheckInput
    {
        internal Models.Model Model { get; set; }
        internal string PreparedFingerprint { get; set; }
        public AreaElement Element { get; internal set; }
        public PointResultPlateForces Sample { get; internal set; }
        public ResultPlateForces LocalForces { get; internal set; }
        public ShellAssignments Assignments => Element.Assignments;
        public string Settings { get; internal set; }
        public string VerificationRevision { get; internal set; }
        public string SampleRevision { get; internal set; }
        public bool IsCurrent => VerificationRevision == Model.VerificationFingerprint(Settings)
            && AnalysisCompatibilityValidator.KnownAnalysisIsCompatible(Model)
            && SampleRevision == ModelArchive.Fingerprint(new object[] { Sample })
            && PreparedFingerprint == ModelArchive.Fingerprint(new object[] { LocalForces });
    }
    /// <summary>Physical inputs for a shell verifier. Ready here does not assert that a structural design method or engine is available.</summary>
    public sealed class ShellInputPreparation
    {
        public int ShellId { get; private set; }
        public PointResultPlateForces Sample { get; private set; }
        public ShellCheckInput Input { get; private set; }
        public DataStatus Status { get; private set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; private set; }

        public static ShellInputPreparation Prepare(Models.Model model, int shellId, PointResultPlateForces sample, string settings)
        {
            var shell = model.AreaElements[shellId]; var status = DataStatus.Insufficient;
            var errors = model.ValidateTopology().Concat(model.ValidateAssignments()).Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            var compatibility = AnalysisCompatibilityValidator.KnownAnalysisDiagnostic(model, out var compatibilityStatus);
            if (compatibility != null) { errors.Add(compatibility); status = compatibilityStatus; }
            var state = sample?.State;
            if (state != null && !ResultAlgebra.HasCurrentDerivation(model, shell, state)) { errors.Add(ModelDiagnostic.Error("StaleDerivedSources", shell)); status = DataStatus.Stale; }
            if (sample == null || !shell.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, sample))) errors.Add(ModelDiagnostic.Error("MissingShellSample", shell));
            if (!shell.Assignments.PhysicalThickness.HasValue) errors.Add(ModelDiagnostic.Error("MissingPhysicalThickness", shell));
            if (shell.PlateProperty == null) errors.Add(ModelDiagnostic.Error("MissingShellProperty", shell));
            if (shell.Assignments.Layers.Count == 0 || shell.Assignments.Layers.Any(l => l.Steel == null || string.IsNullOrWhiteSpace(l.PhysicalFace))) errors.Add(ModelDiagnostic.Error("MissingShellReinforcement", shell));
            if (state == null || state.Components == null || state.Components.Length != 8 || state.Components.Any(c => c != ComponentAvailability.Available)
                || sample.Case == null || string.IsNullOrWhiteSpace(state.ConcomitantStateId)) errors.Add(ModelDiagnostic.Error("IncompleteShellState", shell));
            if (state == null || string.IsNullOrEmpty(state.DatasetId) || !model.Datasets.TryGetValue(state.DatasetId, out var dataset)
                || dataset.ModelRevision != state.ModelRevision || dataset.InputFingerprint != state.InputFingerprint || dataset.NormalizedUnits != "N,mm,rad") errors.Add(ModelDiagnostic.Error("DatasetMismatchOrUnknownUnits", shell));
            if (state != null && state.InputFingerprint != model.AnalysisFingerprint()) { errors.Add(ModelDiagnostic.Error("StaleAnalysis", shell)); status = DataStatus.Stale; }
            if (state == null || (state.Semantics != AnalysisSemantics.LinearStatic && state.Semantics != AnalysisSemantics.NonlinearStatic && state.Semantics != AnalysisSemantics.ConcomitantEnvelopeState)) errors.Add(ModelDiagnostic.Error("NonConcomitantState", shell));
            if (state != null && (state.IsCumulative == false || (!string.IsNullOrEmpty(state.Phase) && state.IsCumulative != true))) errors.Add(ModelDiagnostic.Error("IncrementalOrUnknownPhaseState", shell));
            if (sample != null && (sample.PointKind == ShellResultPointKind.Unknown || sample.PointKind == ShellResultPointKind.Averaged || sample.CoordinateKind == ResultCoordinateKind.Unknown
                || sample.Location == null || double.IsNaN(sample.Location.X) || double.IsInfinity(sample.Location.X) || double.IsNaN(sample.Location.Y) || double.IsInfinity(sample.Location.Y))) errors.Add(ModelDiagnostic.Error("UnsupportedShellResultLocation", shell));
            var result = new ShellInputPreparation { ShellId = shellId, Sample = sample, Status = status, Diagnostics = errors };
            if (errors.Count > 0) return result;
            try
            {
                if (sample.Forces == null) throw new ArgumentException("MissingShellForces");
                var target = ResultTransformations.AtPoint(shell.Assignments.LayerAxes, sample.Forces.CoordinateSystem.Origin);
                var local = ResultTransformations.RotateShell(sample, target).Forces;
                result.Input = new ShellCheckInput { Model = model, Element = shell, Sample = sample, LocalForces = local, Settings = settings,
                    VerificationRevision = model.VerificationFingerprint(settings), SampleRevision = ModelArchive.Fingerprint(new object[] { sample }),
                    PreparedFingerprint = ModelArchive.Fingerprint(new object[] { local }) };
                result.Status = DataStatus.Ready;
            }
            catch (NotSupportedException ex) { errors.Add(ModelDiagnostic.Error("UnsupportedShellTransformation", shell, ex.Message)); result.Status = DataStatus.NotSupported; }
            catch (ArgumentException ex) { errors.Add(ModelDiagnostic.Error("InvalidShellInput", shell, ex.Message)); }
            return result;
        }
    }
}
