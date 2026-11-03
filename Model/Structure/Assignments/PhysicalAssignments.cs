using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.FreedomCases;

namespace GPC.Model.Structure.Assignments
{
    public enum NodalTemperatureKind { Reference, Prescribed, Initial }
    public enum BeamInitialStateKind { AxialForce, AxialStrain }
    public enum DistributedSpringBehavior { Bilateral, CompressionOnly, TensionOnly }
    public enum ShellFace { PositiveNormal, NegativeNormal }
    public enum TaperAlignment { NegativeSide, Symmetric, PositiveSide }

    /// <summary>Absolute nodal temperature in degrees Celsius, with an explicit thermal role.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class NodalTemperatureAssignment
    {
        [field: DataMember(Order = 0)] public LoadCaseBase Case { get; set; }
        [field: DataMember(Order = 1)] public NodalTemperatureKind Kind { get; set; }
        [field: DataMember(Order = 2)] public double Celsius { get; set; }
    }

    /// <summary>Temperature derivatives along the beam section axes 1 and 2, in delta-C/mm.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamTemperatureGradient
    {
        [field: DataMember(Order = 0)] public LoadCaseBase Case { get; set; }
        [field: DataMember(Order = 1)] public double Axis1 { get; set; }
        [field: DataMember(Order = 2)] public double Axis2 { get; set; }
    }

    /// <summary>Positive tension; axial force in N or strain in mm/mm, as selected by Kind.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamInitialState
    {
        [field: DataMember(Order = 0)] public LoadCaseBase Case { get; set; }
        [field: DataMember(Order = 1)] public BeamInitialStateKind Kind { get; set; }
        [field: DataMember(Order = 2)] public double Value { get; set; }
    }

    /// <summary>Initial normal stresses sigma11, sigma22 and sigma33, in N/mm².
    /// Tension is positive. The third component is through-thickness normal stress, not in-plane shear.
    /// An adapter must reject a nonzero third component for formulations that cannot support it.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ShellInitialStress
    {
        [field: DataMember(Order = 0)] public LoadCaseBase Case { get; set; }
        [field: DataMember(Order = 1)] public Vector3d Components { get; set; }
    }

    /// <summary>Additional mass per physical area (t/mm²), dynamic participation ratio,
    /// and offset in global axes (mm). This is not a pressure or a force.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ShellNonStructuralMass
    {
        [field: DataMember(Order = 0)] public LoadCaseBase Case { get; set; }
        [field: DataMember(Order = 1)] public double MassPerArea { get; set; }
        [field: DataMember(Order = 2)] public double DynamicFactor { get; set; } = 1;
        [field: DataMember(Order = 3)] public Vector3d GlobalOffset { get; set; } = new Vector3d(0, 0, 0);
    }

    /// <summary>Distributed translational spring: stiffness N/mm² (force/length/displacement),
    /// gap in mm, and signed unit direction in the explicit axes. Null axes mean global.
    /// Direction points towards positive spring extension. Null FreedomCase means the default restraint set.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamDistributedSupport
    {
        [field: DataMember(Order = 0)] public FreedomCase FreedomCase { get; set; }
        [field: DataMember(Order = 1)] public double Stiffness { get; set; }
        [field: DataMember(Order = 2)] public double Gap { get; set; }
        [field: DataMember(Order = 3)] public DistributedSpringBehavior Behavior { get; set; }
        [field: DataMember(Order = 4)] public Vector3d Direction { get; set; }
        [field: DataMember(Order = 5)] public CoordinateSystem Axes { get; set; }
    }

    /// <summary>Normal foundation modulus N/mm³ (force/area/displacement), gap in mm.
    /// Face is explicit; null FreedomCase means the default restraint set.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ShellDistributedSupport
    {
        [field: DataMember(Order = 0)] public FreedomCase FreedomCase { get; set; }
        [field: DataMember(Order = 1)] public double Stiffness { get; set; }
        [field: DataMember(Order = 2)] public double Gap { get; set; }
        [field: DataMember(Order = 3)] public DistributedSpringBehavior Behavior { get; set; }
        [field: DataMember(Order = 4)] public ShellFace Face { get; set; }
    }

    /// <summary>Linear scaling of the physical section dimensions in section axes 1/2.
    /// The four scale factors are dimensionless and positive. Alignment identifies the fixed side.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamSectionTaper
    {
        [field: DataMember(Order = 0)] public double ScaleI1 { get; set; } = 1;
        [field: DataMember(Order = 1)] public double ScaleJ1 { get; set; } = 1;
        [field: DataMember(Order = 2)] public double ScaleI2 { get; set; } = 1;
        [field: DataMember(Order = 3)] public double ScaleJ2 { get; set; } = 1;
        [field: DataMember(Order = 4)] public TaperAlignment Alignment1 { get; set; }
        [field: DataMember(Order = 5)] public TaperAlignment Alignment2 { get; set; }
    }

    /// <summary>Physical additions to a node; null on older graphs.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class NodePhysicalData
    {
        [field: DataMember(Order = 0)] public List<NodalTemperatureAssignment> Temperatures { get; set; } = new List<NodalTemperatureAssignment>();
    }

    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamPhysicalData
    {
        /// <summary>Shared physical identity of a cable string; mapped to a solver-specific positive integer.</summary>
        [field: OptionalField, DataMember(Order = 5, EmitDefaultValue = false)] public Guid? StringGroup { get; set; }
        [field: DataMember(Order = 0)] public List<BeamTemperatureGradient> TemperatureGradients { get; set; } = new List<BeamTemperatureGradient>();
        [field: DataMember(Order = 1)] public List<BeamInitialState> InitialStates { get; set; } = new List<BeamInitialState>();
        [field: DataMember(Order = 2)] public List<BeamDistributedSupport> Supports { get; set; } = new List<BeamDistributedSupport>();
        [field: DataMember(Order = 3)] public BeamSectionTaper Taper { get; set; }
        /// <summary>Requested minimum number of result stations including ends; null leaves solver policy explicit.</summary>
        [field: DataMember(Order = 4)] public int? MinimumOutputStations { get; set; }
    }

    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ShellPhysicalData
    {
        [field: OptionalField, DataMember(Order = 4, EmitDefaultValue = false)] public LoadPatchDistribution LoadPatch { get; set; }
        [field: DataMember(Order = 0)] public List<ShellInitialStress> InitialStresses { get; set; } = new List<ShellInitialStress>();
        [field: DataMember(Order = 1)] public List<ShellNonStructuralMass> NonStructuralMasses { get; set; } = new List<ShellNonStructuralMass>();
        [field: DataMember(Order = 2)] public List<ShellDistributedSupport> Supports { get; set; } = new List<ShellDistributedSupport>();
    }

    /// <summary>Shared validation and case rebinding for physical assignment graphs.</summary>
    public static class PhysicalAssignments
    {
        /// <summary>Face pressures are element loads (<see cref="Loads.ShellFacePressure"/>, review B2); they need schema 9.</summary>
        public static bool UsesSurfacePressureContract(Models.Model model) => model.AreaElements.Values.Any(a=>a.Loads.Values.Any(l=>l is Loads.ShellFacePressure));
        public static bool HasData(Models.Model model) => model.AllElements.Any(e => Data(e) != null);
        public static bool UsesExtendedExportContract(Models.Model model) =>
            model.NodesElements.Values.Any(n => n.Assignments.Restrains.Any(r => r.FreedomCase != null)) ||
            model.BeamElements.Values.Any(b => b.BeamProperty is ElementProperties.ElasticBeamProperty || b.Assignments.Physical?.StringGroup != null) ||
            model.AreaElements.Values.Any(s => s.PlateProperty is ElementProperties.ElasticPlateProperty || s.PlateProperty is ElementProperties.MembranePlateProperty || s.Assignments.Physical?.LoadPatch != null);
        internal static object Data(Element element) => element is NodeElement n ? (object)n.Assignments.Physical
            : element is BeamElement b ? (object)b.Assignments.Physical : element is AreaElement s ? s.Assignments.Physical : null;

        internal static void RebindCases(Element element, Func<LoadCaseBase, LoadCaseBase> bind, Func<FreedomCase, FreedomCase> bindFreedom)
        {
            if (element is NodeElement node && node.Assignments.Physical != null)
                foreach (var v in node.Assignments.Physical.Temperatures) v.Case = bind(v.Case);
            if (element is BeamElement beam && beam.Assignments.Physical != null)
            {
                var p = beam.Assignments.Physical;
                foreach (var v in p.TemperatureGradients) v.Case = bind(v.Case);
                foreach (var v in p.InitialStates) v.Case = bind(v.Case);
                foreach (var v in p.Supports) if (v.FreedomCase != null) v.FreedomCase = bindFreedom(v.FreedomCase);
            }
            if (element is AreaElement area && area.Assignments.Physical != null)
            {
                var p = area.Assignments.Physical;
                foreach (var v in p.InitialStresses) v.Case = bind(v.Case);
                foreach (var v in p.NonStructuralMasses) v.Case = bind(v.Case);
                foreach (var v in p.Supports) if (v.FreedomCase != null) v.FreedomCase = bindFreedom(v.FreedomCase);
            }
        }

        public static void Validate(Element element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (element is NodeElement node && node.Assignments.Physical != null)
            {
                var values = Items(node.Assignments.Physical.Temperatures);
                Unique(values.Select(v => Case(v.Case)));
                foreach (var v in values) { Defined(v.Kind); Finite(v.Celsius); if (v.Celsius < -273.15) throw new ArgumentException("TemperatureBelowAbsoluteZero"); }
            }
            if (element is BeamElement beam && beam.Assignments.Physical != null)
            {
                var p = beam.Assignments.Physical;
                Unique(Items(p.TemperatureGradients).Select(v => Case(v.Case)));
                foreach (var v in p.TemperatureGradients) { Finite(v.Axis1); Finite(v.Axis2); }
                // A preload is one initial condition per case; force/strain alternatives cannot overwrite each other.
                Unique(Items(p.InitialStates).Select(v => Case(v.Case)));
                foreach (var v in p.InitialStates) { Defined(v.Kind); Finite(v.Value); }
                foreach (var v in Items(p.Supports))
                {
                    // Review B9: a support has a stiffness, and the gap is a clearance in mm.
                    Positive(v.Stiffness); Nonnegative(v.Gap); Defined(v.Behavior);
                    if (v.Behavior == DistributedSpringBehavior.Bilateral && v.Gap != 0) throw new ArgumentException("GapRequiresUnilateralSupport"); Vector(v.Direction);
                    if (Math.Abs(Axes.Length(v.Direction) - 1) > 1e-8) throw new ArgumentException("SupportDirectionMustBeUnit");
                    if (v.Axes != null) Axes.Validate(v.Axes);
                }
                if (p.StringGroup == Guid.Empty) throw new ArgumentException("EmptyBeamStringGroup");
                if (p.MinimumOutputStations.HasValue && p.MinimumOutputStations < 2) throw new ArgumentException("InsufficientOutputStations");
                if (p.Taper != null)
                {
                    var t = p.Taper; Positive(t.ScaleI1); Positive(t.ScaleJ1); Positive(t.ScaleI2); Positive(t.ScaleJ2);
                    Defined(t.Alignment1); Defined(t.Alignment2);
                }
            }
            if (element is AreaElement area && area.Assignments.Physical != null)
            {
                var p = area.Assignments.Physical;
                p.LoadPatch?.Validate(area.Nodes.Count);
                Unique(Items(p.InitialStresses).Select(v => Case(v.Case)));
                foreach (var v in p.InitialStresses) Vector(v.Components);
                Unique(Items(p.NonStructuralMasses).Select(v => Case(v.Case)));
                foreach (var v in p.NonStructuralMasses) { Nonnegative(v.MassPerArea); Nonnegative(v.DynamicFactor); Vector(v.GlobalOffset); }
                foreach (var v in Items(p.Supports)) { Positive(v.Stiffness); Nonnegative(v.Gap); Defined(v.Behavior);
                    if (v.Behavior == DistributedSpringBehavior.Bilateral && v.Gap != 0) throw new ArgumentException("GapRequiresUnilateralSupport"); Defined(v.Face); }
            }
        }

        private static IEnumerable<T> Items<T>(IEnumerable<T> items) where T : class
        { if (items == null || items.Any(v => v == null)) throw new ArgumentException("MissingPhysicalAssignment"); return items; }
        private static string Case(LoadCaseBase value)
        { if (value == null || string.IsNullOrWhiteSpace(value.Name)) throw new ArgumentException("MissingPhysicalAssignmentCase"); return value.Name; }
        private static void Unique(IEnumerable<string> keys)
        { var used = new HashSet<string>(StringComparer.Ordinal); if (keys.Any(k => !used.Add(k))) throw new ArgumentException("ConflictingPhysicalAssignments"); }
        private static void Defined<T>(T value) { if (!Enum.IsDefined(typeof(T), value)) throw new ArgumentException("UnknownPhysicalAssignmentKind"); }
        private static void Vector(Vector3d v) { if (v == null) throw new ArgumentException("MissingPhysicalAssignmentVector"); Finite(v.X); Finite(v.Y); Finite(v.Z); }
        private static void Finite(double x) { if (double.IsNaN(x) || double.IsInfinity(x)) throw new ArgumentException("NonFinitePhysicalAssignment"); }
        private static void Nonnegative(double x) { Finite(x); if (x < 0) throw new ArgumentException("NegativePhysicalAssignment"); }
        private static void Positive(double x) { Finite(x); if (x <= 0) throw new ArgumentException("NonPositivePhysicalAssignment"); }
    }
}
