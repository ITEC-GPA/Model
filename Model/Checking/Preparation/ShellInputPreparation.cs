using GPC.Model.Models;
using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.ElementProperties;
using GPC.Model.Core;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Analysis;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Results.State;
using GPC.Model.Structure.Assignments;

namespace GPC.Model.Checking.Preparation
{
    public enum ShellInputAxes { Element, Reinforcement, Explicit }
    public sealed class ShellCheckInput
    {
        internal Models.Model Model { get; set; }
        internal string PreparedFingerprint { get; set; }
        internal string DatasetFingerprint { get; set; }
        public AreaElement Element { get; internal set; }
        public PointResultPlateForces Sample { get; internal set; }
        public ResultPlateForces LocalForces { get; internal set; }
        /// <summary>Detached assignments. Element is retained only for source identity/currentness and compatibility.</summary>
        public ShellAssignments Assignments { get; internal set; }
        public PlateProperty Property { get; internal set; }
        public IReadOnlyList<ShellRebarLayer> Reinforcement => Property is Sections.Concrete.ReinforcedConcretePlateSection rc
            ? rc.RebarLayers.AsReadOnly() : (IReadOnlyList<ShellRebarLayer>)Array.Empty<ShellRebarLayer>();
        public GPC.Geometry.CoordinateSystem SectionAxes => Assignments.LayerAxes;
        internal string AssignmentsFingerprint { get; set; }
        internal string PropertyFingerprint { get; set; }
        public ShellThickness Thickness { get; internal set; }
        public string Settings { get; internal set; }
        public string VerificationRevision { get; internal set; }
        public string SampleRevision { get; internal set; }
        public bool IsCurrent => !string.IsNullOrEmpty(Sample?.State?.DatasetId) && Model.AreaElements.TryGetValue(Element.Id, out var current) && ReferenceEquals(current, Element)
            && Element.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, Sample))
            && Model.Datasets.TryGetValue(Sample.State.DatasetId, out var dataset) && DatasetFingerprint == ModelValues.Fingerprint(new object[] { dataset })
            && ResultAlgebra.HasCurrentDerivation(Model, Element, Sample.State)
            && VerificationRevision == Model.VerificationFingerprint(Settings)
            && AnalysisCompatibilityValidator.KnownAnalysisIsCompatible(Model)
            && SampleRevision == ModelValues.Fingerprint(new object[] { Sample })
            && AssignmentsFingerprint == ModelValues.Fingerprint(new object[] { Assignments })
            && PropertyFingerprint == ModelValues.Fingerprint(new object[] { Property })
            && PreparedFingerprint == ModelValues.Fingerprint(new object[] { LocalForces });
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
            => PrepareCore(model, shellId, sample, PlateSections.SectionAxes(model.AreaElements[shellId]), settings, true);
        /// <summary>Material-neutral shell actions in an explicitly supplied coordinate system. Reinforcement requirements belong to the selected verifier.</summary>
        public static ShellInputPreparation PrepareActions(Models.Model model, int shellId, PointResultPlateForces sample, GPC.Geometry.CoordinateSystem axes, string settings)
            => PrepareCore(model, shellId, sample, axes, settings, false);
        private static ShellInputPreparation PrepareCore(Models.Model model, int shellId, PointResultPlateForces sample, GPC.Geometry.CoordinateSystem axes, string settings, bool reinforcementRequired)
        {
            var shell = model.AreaElements[shellId]; var status = DataStatus.Insufficient;
            var errors = global::GPC.Model.Checking.Preparation.ValidationReadScope.Errors(model).ToList();
            var compatibility = AnalysisCompatibilityValidator.KnownAnalysisDiagnostic(model, out var compatibilityStatus);
            if (compatibility != null) { errors.Add(compatibility); status = compatibilityStatus; }
            var state = sample?.State;
            if (state != null && !ResultAlgebra.HasCurrentDerivation(model, shell, state)) { errors.Add(ModelDiagnostic.Error("StaleDerivedSources", shell)); status = DataStatus.Stale; }
            if (sample == null || !shell.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, sample))) errors.Add(ModelDiagnostic.Error("MissingShellSample", shell));
            try { if (!PlateSections.PhysicalThickness(shell).HasValue) errors.Add(ModelDiagnostic.Error("MissingPhysicalThickness",shell)); }
            catch (ArgumentException ex) { errors.Add(ModelDiagnostic.Error(ex.Message,shell)); }
            if (shell.PlateProperty is null) errors.Add(ModelDiagnostic.Error("MissingShellProperty", shell));

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
                var target = ActionTransformations.AtPoint(axes, sample.Forces.CoordinateSystem.Origin);
                var local = ActionTransformations.RotateShell(sample, target).Forces;
                var property = PlateSections.CopyForChecking(shell);
                var assignments = ModelValues.CopyValue(shell.Assignments);
                assignments.LayerAxes=ActionTransformations.AtPoint(PlateSections.SectionAxes(shell),PlateSections.SectionAxes(shell).Origin);
                assignments.PhysicalThickness=property.PhysicalThickness; assignments.Layers.Clear();
                if(property is Sections.Concrete.ReinforcedConcretePlateSection rc) assignments.Layers.AddRange(rc.RebarLayers);
                if(reinforcementRequired && assignments.Layers.Count==0) throw new ArgumentException("MissingShellReinforcement");
                result.Input = new ShellCheckInput { Model = model, Element = shell, Sample = sample, LocalForces = local, Settings = settings, Thickness = ShellThickness.From(shell),
                    Assignments = assignments, Property = property,
                    AssignmentsFingerprint = ModelValues.Fingerprint(new object[] { assignments }), PropertyFingerprint = ModelValues.Fingerprint(new object[] { property }),
                    VerificationRevision = model.VerificationFingerprint(settings), SampleRevision = ModelValues.Fingerprint(new object[] { sample }),
                    DatasetFingerprint = ModelValues.Fingerprint(new object[] { model.Datasets[state.DatasetId] }),
                    PreparedFingerprint = ModelValues.Fingerprint(new object[] { local }) };
                result.Status = DataStatus.Ready;
            }
            catch (NotSupportedException ex) { errors.Add(ModelDiagnostic.Error("UnsupportedShellTransformation", shell, ex.Message)); result.Status = DataStatus.NotSupported; }
            catch (ArgumentException ex) { errors.Add(ModelDiagnostic.Error("InvalidShellInput", shell, ex.Message)); }
            return result;
        }
    }
}
