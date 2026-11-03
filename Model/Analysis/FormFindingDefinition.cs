using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;

namespace GPC.Model.Analysis
{
    public enum FormFindingMethod { LinearForceDensity, NonlinearForceDensity, SteppedLinearForceDensity }
    public enum FormFindingGoalKind { ForceDensity, AxialForce, MembraneForcePerWidth, Length, CableForce, CurvatureRatio }
    public enum FabricDirection { Unspecified, Warp, Weft }

    /// <summary>One objective on an explicitly connected beam. Target units: N/mm for
    /// force density and membrane force/width, N for axial force, mm for length.
    /// Force and force-density targets are signed (tension positive); zero is permitted unless the selected algorithm divides by that target.
    /// Weight is a dimensionless equation weight, not a physical mass or force.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class FormFindingGoal
    {
        [field: DataMember(Order = 0)] public Guid BeamGuid { get; set; }
        [field: DataMember(Order = 1)] public FormFindingGoalKind Kind { get; set; }
        [field: DataMember(Order = 2)] public double Target { get; set; }
        [field: DataMember(Order = 3)] public double Weight { get; set; } = 1;
        [field: DataMember(Order = 4)] public bool ProjectToPlane { get; set; }
        [field: DataMember(Order = 5)] public FabricDirection Direction { get; set; }
        /// <summary>Chord in mm for CurvatureRatio (Target = chord/sag, dimensionless). Required only for that kind.</summary>
        [field: OptionalField]
        [field: DataMember(Order = 6, EmitDefaultValue = false)] public double? ChordLengthMm { get; set; }
    }

    /// <summary>Penalties on the initial global coordinates. Zero disables a coordinate
    /// objective. These values are neither nodal weights nor prescribed displacements.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class FormFindingCoordinateGoal
    {
        [field: DataMember(Order = 0)] public Guid NodeGuid { get; set; }
        [field: DataMember(Order = 1)] public Vector3d Weights { get; set; } = new Vector3d(0, 0, 0);
    }

    /// <summary>Physical surface mass in tonne/mm². Independent of the fictitious
    /// thickness used internally by a form-finding solver.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class FormFindingSurfaceMass
    {
        [field: DataMember(Order = 0)] public Guid AreaGuid { get; set; }
        [field: DataMember(Order = 1)] public double MassPerArea { get; set; }
    }

    /// <summary>Explicit form-finding vocabulary; no solver object or opaque property bag.
    /// Loads and kinematic restraints belong to the model nodes. Pressure follows the
    /// ordered surface normal; positive gravity acts along global -Z. All inputs use N, mm, s, tonne.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class FormFindingDefinition
    {
        [field: DataMember(Order = 0)] public FormFindingMethod Method { get; set; }
        [field: DataMember(Order = 1)] public CoordinateSystem ProjectionPlane { get; set; }
        [field: DataMember(Order = 2)] public double Pressure { get; set; }
        [field: DataMember(Order = 3)] public double Gravity { get; set; }
        [field: DataMember(Order = 4)] public int MaxIterations { get; set; } = 20;
        [field: DataMember(Order = 5)] public double RelativeTolerance { get; set; } = 1e-4;
        [field: DataMember(Order = 6)] public int SolverMaxIterations { get; set; }
        [field: DataMember(Order = 7)] public double SolverToleranceA { get; set; } = 1e-6;
        [field: DataMember(Order = 8)] public double SolverToleranceB { get; set; } = 1e-6;
        [field: DataMember(Order = 9)] public List<FormFindingGoal> Goals { get; set; } = new List<FormFindingGoal>();
        [field: DataMember(Order = 10)] public List<FormFindingCoordinateGoal> CoordinateGoals { get; set; } = new List<FormFindingCoordinateGoal>();
        [field: DataMember(Order = 11)] public List<FormFindingSurfaceMass> SurfaceMasses { get; set; } = new List<FormFindingSurfaceMass>();

        public bool UsesExtendedContract => Method == FormFindingMethod.SteppedLinearForceDensity
            || Goals != null && Goals.Any(g => g != null && (g.Kind == FormFindingGoalKind.CurvatureRatio || g.ChordLengthMm.HasValue || g.Target <= 0));

        public void Validate(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (!Enum.IsDefined(typeof(FormFindingMethod), Method) || MaxIterations <= 0 || SolverMaxIterations < 0)
                throw new ArgumentException("InvalidFormFindingOptions");
            Nonnegative(Gravity); Finite(Pressure); Positive(RelativeTolerance); Nonnegative(SolverToleranceA); Nonnegative(SolverToleranceB);
            if (Goals == null || CoordinateGoals == null || SurfaceMasses == null) throw new ArgumentException("MissingFormFindingCollection");
            if (ProjectionPlane != null) Core.Coordinates.Axes.Validate(ProjectionPlane);
            var beams = new HashSet<Guid>(model.BeamElements.Values.Select(b => b.Guid));
            var nodes = new HashSet<Guid>(model.NodesElements.Values.Select(n => n.Guid));
            var areas = new HashSet<Guid>(model.AreaElements.Values.Select(a => a.Guid));
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var goal in Goals)
            {
                if (goal == null || !beams.Contains(goal.BeamGuid) || !Enum.IsDefined(typeof(FormFindingGoalKind), goal.Kind)
                    || !Enum.IsDefined(typeof(FabricDirection), goal.Direction) || !keys.Add(goal.BeamGuid + ":" + goal.Kind))
                    throw new ArgumentException("InvalidOrDuplicateFormFindingGoal");
                Finite(goal.Target); Nonnegative(goal.Weight);
                if (goal.Kind == FormFindingGoalKind.Length) Positive(goal.Target);
                if ((goal.Kind == FormFindingGoalKind.CurvatureRatio || goal.Kind == FormFindingGoalKind.AxialForce && Method == FormFindingMethod.SteppedLinearForceDensity) && goal.Target == 0)
                    throw new ArgumentException("ZeroTargetDivisor");
                if (goal.Kind == FormFindingGoalKind.CurvatureRatio)
                {
                    if (!goal.ChordLengthMm.HasValue) throw new ArgumentException("MissingCurvatureChord");
                    Positive(goal.ChordLengthMm.Value);
                }
                else if (goal.ChordLengthMm.HasValue) throw new ArgumentException("ChordRequiresCurvatureGoal");
                if ((goal.Kind == FormFindingGoalKind.Length || goal.Kind == FormFindingGoalKind.AxialForce)
                    && Method == FormFindingMethod.LinearForceDensity)
                    throw new ArgumentException("ObjectiveRequiresIterativeMethod");
                if (goal.Kind != FormFindingGoalKind.MembraneForcePerWidth && goal.Direction != FabricDirection.Unspecified)
                    throw new ArgumentException("FabricDirectionRequiresMembraneGoal");
                if (goal.ProjectToPlane && ProjectionPlane == null) throw new ArgumentException("MissingProjectionPlane");
            }
            foreach (var group in Goals.Where(g => g.Kind != FormFindingGoalKind.Length && g.Kind != FormFindingGoalKind.AxialForce).GroupBy(g => g.BeamGuid))
                if (group.Count() > 1) throw new ArgumentException("ConflictingForceDensitySources");
            keys.Clear();
            foreach (var goal in CoordinateGoals)
            {
                if (goal == null || !nodes.Contains(goal.NodeGuid) || !keys.Add(goal.NodeGuid.ToString()) || goal.Weights == null)
                    throw new ArgumentException("InvalidOrDuplicateCoordinateGoal");
                Nonnegative(goal.Weights.X); Nonnegative(goal.Weights.Y); Nonnegative(goal.Weights.Z);
                if (Method != FormFindingMethod.NonlinearForceDensity && (goal.Weights.X != 0 || goal.Weights.Y != 0 || goal.Weights.Z != 0))
                    throw new ArgumentException("CoordinateGoalsRequireNonlinearMethod");
            }
            keys.Clear();
            foreach (var mass in SurfaceMasses)
            {
                if (mass == null || !areas.Contains(mass.AreaGuid) || !keys.Add(mass.AreaGuid.ToString())) throw new ArgumentException("InvalidOrDuplicateSurfaceMass");
                Nonnegative(mass.MassPerArea);
            }
        }
        private static void Finite(double x) { if (double.IsNaN(x) || double.IsInfinity(x)) throw new ArgumentException("NonFiniteFormFindingValue"); }
        private static void Nonnegative(double x) { Finite(x); if (x < 0) throw new ArgumentException("NegativeFormFindingValue"); }
        private static void Positive(double x) { Finite(x); if (x <= 0) throw new ArgumentException("NonPositiveFormFindingValue"); }
    }
}
