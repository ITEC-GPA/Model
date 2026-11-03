using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;

namespace GPC.Model.Actions.Moving
{
    public enum MovingLoadAxes { Path, Global }
    public enum MovingPointMobility { Grouped, Floating }
    public enum MovingDistributionMobility { Grouped, Leading, Trailing, FullLength, Floating }
    public enum MovingVehiclePlacement { SingleLane, AdjacentLanePair }
    public enum MovingVehicleDirection { Forward, Reverse }
    public enum MovingLaneFactorRule { UniformByActiveCount, BySeverityRank }
    public enum MovingPathShape { Straight, Circular, Parabolic }
    public enum MovingPathSurface { Flat, Curved }
    public enum MovingPathTarget { AllStructuralElements, Beams, Shells, Solids, Selection }

    /// <summary>Mechanical moving loads in N, mm and s. Library references, expression dialects,
    /// display colours and native entity-set numbers belong to the software adapter.</summary>
    [Serializable, DataContract]
    public sealed class MovingLoadDefinition
    {
        [field:DataMember(Order=0)] public List<MovingLoadPath> Paths { get; set; } = new List<MovingLoadPath>();
        public bool UsesExtendedContract => Paths.Any(p=>p.Options!=null||p.Template.Centrifugal!=null||p.Template.Vehicles.Any(v=>v.HeatSources!=null||v.PointForces.Any(f=>f.ApplyCentrifugal.HasValue)||v.DistributedForces.Any(f=>f.ApplyCentrifugal.HasValue)));
        public void Validate(Models.Model model)
        {
            if(model==null) throw new ArgumentNullException(nameof(model));
            MovingValidation.Items(Paths,"MissingMovingPaths");
            if(Paths.Select(p=>p.Id).Distinct().Count()!=Paths.Count) throw new ArgumentException("DuplicateMovingPathIdentity");
            var cases=new HashSet<Guid>(model.LoadCases.Values.Select(c=>c.Guid));
            var structural=new HashSet<Guid>(model.BeamElements.Values.Select(b=>b.Guid)
                .Concat(model.AreaElements.Values.Select(a=>a.Guid)).Concat(model.VolumeElements.Values.Select(v=>v.Guid)));
            foreach(var path in Paths)
            {
                path.Validate();
                path.Options?.Validate();
                if(!cases.Contains(path.CaseGuid)) throw new ArgumentException("UnknownMovingLoadCase");
                if(path.TargetElementGuids.Any(g=>!structural.Contains(g))) throw new ArgumentException("UnknownMovingTargetElement");
            }
        }
    }

    [Serializable, DataContract]
    public sealed class MovingLoadPath
    {
        [field:DataMember(Order=0)] public Guid Id { get; set; }
        [field:DataMember(Order=1)] public Guid CaseGuid { get; set; }
        [field:DataMember(Order=2)] public MovingLoadTemplate Template { get; set; }
        [field:DataMember(Order=3)] public MovingPathShape Shape { get; set; }
        [field:DataMember(Order=4)] public MovingPathSurface Surface { get; set; }
        [field:DataMember(Order=5)] public Point3d Start { get; set; }
        [field:DataMember(Order=6)] public Point3d End { get; set; }
        /// <summary>Third point defines the oriented plane and, for curved paths, the curvature.</summary>
        [field:DataMember(Order=7)] public Point3d Reference { get; set; }
        [field:DataMember(Order=8)] public MovingPathTarget Target { get; set; }
        [field:DataMember(Order=9)] public Guid[] TargetElementGuids { get; set; } = Array.Empty<Guid>();
        /// <summary>Longitudinal sampling intervals. This numerical control does not depend on the FEM mesh.</summary>
        [field:DataMember(Order=10)] public int Divisions { get; set; } = 20;
        /// <summary>Load-application controls that change the applied loads; null keeps the solver defaults.</summary>
        [field:OptionalField,DataMember(Order=11,EmitDefaultValue=false)] public MovingPathOptions Options { get; set; }
        public void Validate()
        {
            if(Id==Guid.Empty||CaseGuid==Guid.Empty) throw new ArgumentException("MissingMovingPathIdentity");
            MovingValidation.Enum(Shape); MovingValidation.Enum(Surface); MovingValidation.Enum(Target);
            if(Template==null) throw new ArgumentException("MissingMovingTemplate"); Template.Validate();
            MovingValidation.Point(Start); MovingValidation.Point(End); MovingValidation.Point(Reference);
            var a=new Vector3d(End.X-Start.X,End.Y-Start.Y,End.Z-Start.Z);
            var b=new Vector3d(Reference.X-Start.X,Reference.Y-Start.Y,Reference.Z-Start.Z);
            double lengthA=a.Length, lengthB=b.Length;
            MovingValidation.Positive(lengthA); MovingValidation.Positive(lengthB);
            // Normalize before the cross product: the test is dimensionless and avoids length^4 overflow.
            var normal=(a/lengthA).CrossProduct(b/lengthB);
            if(!(normal.Length>1e-10)) throw new ArgumentException("DegenerateMovingPathPlane");
            if(Shape==MovingPathShape.Straight && Surface==MovingPathSurface.Curved) throw new ArgumentException("CurvedSurfaceRequiresCurvedPath");
            if(Divisions<1) throw new ArgumentException("InvalidMovingPathDivisions");
            if(TargetElementGuids==null || TargetElementGuids.Any(g=>g==Guid.Empty) || TargetElementGuids.Distinct().Count()!=TargetElementGuids.Length)
                throw new ArgumentException("InvalidMovingTargetSelection");
            if((Target==MovingPathTarget.Selection)!=(TargetElementGuids.Length>0)) throw new ArgumentException("MovingTargetSelectionMismatch");
        }
    }

    /// <summary>Numerical controls of the load application along a path. They change the applied loads, so they are physical input
    /// of the analysis (copy, archive and fingerprint). Display colour and native entity-set numbers stay with the software adapter.</summary>
    [Serializable, DataContract]
    public sealed class MovingPathOptions
    {
        /// <summary>Trailing loads enter and leave the path gradually instead of being cut at its ends.</summary>
        [field:DataMember(Order=0)] public bool TransitionTrailingLoad { get; set; }
        /// <summary>Beams within the lateral tolerance of the path are loaded.</summary>
        [field:DataMember(Order=1)] public bool BeamLateralTolerance { get; set; }
        /// <summary>Dimensionless tolerance of the search for loaded elements, relative to their size.</summary>
        [field:DataMember(Order=2)] public double RelativeTolerance { get; set; } = .01;
        /// <summary>Static cases at the centres of the path divisions (no end positions, no continuous envelope).</summary>
        [field:DataMember(Order=3)] public bool ConvertToStaticCases { get; set; }
        public void Validate() { MovingValidation.Nonnegative(RelativeTolerance); }
    }

    [Serializable, DataContract]
    public sealed class MovingLoadTemplate
    {
        [field:DataMember(Order=0)] public string Name { get; set; }
        [field:DataMember(Order=1)] public MovingVehicle[] Vehicles { get; set; } = Array.Empty<MovingVehicle>();
        /// <summary>Factors indexed by active-lane count or severity rank, as selected by LaneFactorRule; never by physical lane position.</summary>
        [field:DataMember(Order=2)] public double[] LaneFactors { get; set; } = new[]{1.0};
        [field:DataMember(Order=3)] public MovingLaneFactorRule LaneFactorRule { get; set; }
        [field:DataMember(Order=4)] public double MinimumLaneWidth { get; set; }
        [field:DataMember(Order=5,EmitDefaultValue=false)] public MovingCentrifugalLaw Centrifugal { get; set; }
        /// <summary>Factors for active lanes ordered from greatest to least influence.
        /// This returns multipliers only; it does not determine critical vehicle positions or solve an influence envelope.</summary>
        public double[] ActiveLaneFactors(int count)
        {
            Validate();
            if(count<1||count>LaneFactors.Length) throw new ArgumentOutOfRangeException(nameof(count));
            return LaneFactorRule==MovingLaneFactorRule.UniformByActiveCount
                ? Enumerable.Repeat(LaneFactors[count-1],count).ToArray() : LaneFactors.Take(count).ToArray();
        }
        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("MissingMovingTemplateName");
            MovingValidation.Items(Vehicles,"MissingMovingVehicles");
            if(LaneFactors==null||LaneFactors.Length==0) throw new ArgumentException("MissingMovingLaneFactors");
            foreach(var factor in LaneFactors) MovingValidation.Nonnegative(factor);
            MovingValidation.Nonnegative(MinimumLaneWidth); MovingValidation.Enum(LaneFactorRule);
            Centrifugal?.Validate();
            foreach(var vehicle in Vehicles)
            {
                vehicle.Validate();
                if(Centrifugal==null&&(vehicle.PointForces.Any(f=>f.ApplyCentrifugal==true)||vehicle.DistributedForces.Any(f=>f.ApplyCentrifugal==true)))throw new ArgumentException("MissingMovingCentrifugalLaw");
                if(vehicle.AllowedLanes.Any(l=>l>LaneFactors.Length)) throw new ArgumentException("UnknownMovingVehicleLane");
                if(vehicle.Placement==MovingVehiclePlacement.AdjacentLanePair && LaneFactors.Length<2) throw new ArgumentException("MovingVehicleNeedsTwoLanes");
            }
        }
    }

    [Serializable, DataContract]
    public sealed class MovingVehicle
    {
        [field:DataMember(Order=0)] public string Name { get; set; }
        [field:DataMember(Order=1)] public MovingVehiclePlacement Placement { get; set; }
        [field:DataMember(Order=2)] public MovingVehicleDirection Direction { get; set; }
        /// <summary>Speed in mm/s; sign is represented by Direction.</summary>
        [field:DataMember(Order=3)] public double Speed { get; set; }
        /// <summary>Seconds. Negative values represent a head start before simulation time zero.</summary>
        [field:DataMember(Order=4)] public double StartTime { get; set; }
        /// <summary>Optional mutually exclusive vehicle group used in influence envelopes.</summary>
        [field:DataMember(Order=5)] public string ExclusiveSet { get; set; }
        /// <summary>One-based lane positions; empty means every lane.</summary>
        [field:DataMember(Order=6)] public int[] AllowedLanes { get; set; } = Array.Empty<int>();
        [field:DataMember(Order=7)] public MovingPointForce[] PointForces { get; set; } = Array.Empty<MovingPointForce>();
        [field:DataMember(Order=8)] public MovingDistributedForce[] DistributedForces { get; set; } = Array.Empty<MovingDistributedForce>();
        [field:DataMember(Order=9,EmitDefaultValue=false)] public MovingHeatSource[] HeatSources { get; set; }
        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("MissingMovingVehicleName");
            MovingValidation.Enum(Placement); MovingValidation.Enum(Direction); MovingValidation.Nonnegative(Speed); MovingValidation.Finite(StartTime);
            if(AllowedLanes==null||AllowedLanes.Any(l=>l<1)||AllowedLanes.Distinct().Count()!=AllowedLanes.Length) throw new ArgumentException("InvalidMovingVehicleLanes");
            if(PointForces==null||DistributedForces==null||PointForces.Length+DistributedForces.Length+(HeatSources?.Length??0)==0) throw new ArgumentException("MissingMovingVehicleForces");
            foreach(var force in PointForces) { if(force==null) throw new ArgumentException("NullMovingForce"); force.Validate(); }
            foreach(var force in DistributedForces) { if(force==null) throw new ArgumentException("NullMovingForce"); force.Validate(); }
            if(HeatSources!=null)foreach(var heat in HeatSources){if(heat==null)throw new ArgumentException("NullMovingHeatSource");heat.Validate();}
        }
    }

    [Serializable, DataContract]
    public sealed class MovingPointForce
    {
        [field:DataMember(Order=0)] public MovingPointMobility Mobility { get; set; }
        [field:DataMember(Order=1)] public MovingLoadAxes Axes { get; set; }
        [field:DataMember(Order=2)] public bool ApplyAdjacencyFactor { get; set; }
        [field:DataMember(Order=3)] public double X { get; set; }
        [field:DataMember(Order=4)] public double Y { get; set; }
        /// <summary>Signed components in N, in the specified axes.</summary>
        [field:DataMember(Order=5)] public Vector3d Force { get; set; } = new Vector3d(0,0,0);
        [field:DataMember(Order=6,EmitDefaultValue=false)] public bool? ApplyCentrifugal { get; set; }
        public void Validate() { MovingValidation.Enum(Mobility); MovingValidation.Enum(Axes); MovingValidation.Finite(X); MovingValidation.Finite(Y); MovingValidation.Vector(Force); }
    }

    [Serializable, DataContract]
    public sealed class MovingDistributedForce
    {
        [field:DataMember(Order=0)] public MovingDistributionMobility Mobility { get; set; }
        [field:DataMember(Order=1)] public MovingLoadAxes Axes { get; set; }
        [field:DataMember(Order=2)] public bool ApplyAdjacencyFactor { get; set; }
        [field:DataMember(Order=3)] public double X1 { get; set; }
        [field:DataMember(Order=4)] public double X2 { get; set; }
        [field:DataMember(Order=5)] public double Y1 { get; set; }
        [field:DataMember(Order=6)] public double Y2 { get; set; }
        /// <summary>N/mm measured along path x, independent of footprint width in y. Zero defines an unloaded moving region.</summary>
        [field:DataMember(Order=7)] public Vector3d ForcePerLength { get; set; } = new Vector3d(0,0,0);
        [field:DataMember(Order=8,EmitDefaultValue=false)] public bool? ApplyCentrifugal { get; set; }
        public void Validate()
        {
            MovingValidation.Enum(Mobility); MovingValidation.Enum(Axes); MovingValidation.Vector(ForcePerLength);
            MovingValidation.Finite(X1); MovingValidation.Finite(X2); MovingValidation.Finite(Y1); MovingValidation.Finite(Y2);
            if(((Mobility==MovingDistributionMobility.Grouped||Mobility==MovingDistributionMobility.Floating)&&X2<=X1)||Y2<Y1) throw new ArgumentException("InvalidMovingForceFootprint");
        }
        /// <summary>Resultant on the specified footprint. Width spreads the same force across the receiving surface.</summary>
        public Vector3d Resultant()
        {
            Validate();
            if(Mobility!=MovingDistributionMobility.Grouped&&Mobility!=MovingDistributionMobility.Floating)throw new InvalidOperationException("MovingResultantRequiresLoadedPathLength");
            return Resultant(X2-X1);
        }
        /// <summary>Resultant over an explicitly evaluated loaded length, e.g. the leading/trailing portion of a path.</summary>
        public Vector3d Resultant(double loadedLength)
        {Validate();MovingValidation.Nonnegative(loadedLength);var result=ForcePerLength*loadedLength;MovingValidation.Vector(result);return result;}
    }

    /// <summary>Grouped heat source; X/Y and footprint sizes in mm, total signed power in N mm/s.
    /// A nonzero footprint distributes the same total power over its area. Zero sizes define a point source.</summary>
    [Serializable,DataContract]
    public sealed class MovingHeatSource
    {
        [field:DataMember(Order=0)] public double X { get; set; }
        [field:DataMember(Order=1)] public double Y { get; set; }
        [field:DataMember(Order=2)] public double SizeX { get; set; }
        [field:DataMember(Order=3)] public double SizeY { get; set; }
        [field:DataMember(Order=4)] public double Power { get; set; }
        public void Validate()
        {
            MovingValidation.Finite(X);MovingValidation.Finite(Y);MovingValidation.Finite(Power);
            MovingValidation.Nonnegative(SizeX);MovingValidation.Nonnegative(SizeY);
            if((SizeX==0)!=(SizeY==0))throw new ArgumentException("MovingHeatRequiresPointOrAreaFootprint");
        }
    }

    /// <summary>Signed radial force = OffsetExpression + FactorExpression * vertical force.
    /// The explicit expression language and units are part of the physical input and its fingerprint.
    /// An adapter must support the declared dialect; expressions are never evaluated as application code.</summary>
    [Serializable,DataContract]
    public sealed class MovingCentrifugalLaw
    {
        [field:DataMember(Order=0)] public string ExpressionLanguage { get; set; }
        [field:DataMember(Order=1)] public string OffsetExpression { get; set; }
        [field:DataMember(Order=2)] public string FactorExpression { get; set; }
        [field:DataMember(Order=3)] public string LengthUnit { get; set; } = "mm";
        [field:DataMember(Order=4)] public string ForceUnit { get; set; } = "N";
        [field:DataMember(Order=5)] public double? MinimumFactor { get; set; }
        [field:DataMember(Order=6)] public double? MaximumFactor { get; set; }
        [field:DataMember(Order=7)] public bool LimitFactor { get; set; }
        public void Validate()
        {
            // Native libraries may leave one term empty. Preserve it verbatim; the declared dialect defines its meaning.
            if(new[]{ExpressionLanguage,LengthUnit,ForceUnit}.Any(string.IsNullOrWhiteSpace)||OffsetExpression==null||FactorExpression==null||
                (string.IsNullOrWhiteSpace(OffsetExpression)&&string.IsNullOrWhiteSpace(FactorExpression)))throw new ArgumentException("IncompleteMovingCentrifugalLaw");
            if(MinimumFactor.HasValue!=MaximumFactor.HasValue)throw new ArgumentException("IncompleteMovingCentrifugalLimits");
            if(LimitFactor&&!MinimumFactor.HasValue)throw new ArgumentException("MissingMovingCentrifugalLimits");
            if(MinimumFactor.HasValue){MovingValidation.Finite(MinimumFactor.Value);MovingValidation.Finite(MaximumFactor.Value);if(MinimumFactor>MaximumFactor)throw new ArgumentException("ReversedMovingCentrifugalLimits");}
        }
    }

    internal static class MovingValidation
    {
        internal static void Finite(double value) { if(double.IsNaN(value)||double.IsInfinity(value)) throw new ArgumentException("NonFiniteMovingLoadValue"); }
        internal static void Positive(double value) { Finite(value); if(value<=0) throw new ArgumentException("NonPositiveMovingLoadValue"); }
        internal static void Nonnegative(double value) { Finite(value); if(value<0) throw new ArgumentException("NegativeMovingLoadValue"); }
        internal static void Enum<T>(T value) { if(!System.Enum.IsDefined(typeof(T),value)) throw new ArgumentException("UnknownMovingLoadOption"); }
        internal static void Vector(Vector3d value) { if(value==null) throw new ArgumentException("MissingMovingLoadVector"); Finite(value.X); Finite(value.Y); Finite(value.Z); }
        internal static void Point(Point3d value) { if(value==null) throw new ArgumentException("MissingMovingPathPoint"); Finite(value.X); Finite(value.Y); Finite(value.Z); }
        internal static void Items<T>(ICollection<T> items,string error) where T:class
        { if(items==null||items.Count==0||items.Any(i=>i==null)) throw new ArgumentException(error); }
    }
}
