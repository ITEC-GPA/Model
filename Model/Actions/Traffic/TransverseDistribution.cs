using System.Collections.Generic;

namespace GPC.Model.Actions.Traffic
{
    public enum TransverseDistributionMethod { RigidCourbon, DeformableFourier }
    /// <summary>Straight, simply supported, constant-section girders. EI and GJ include the assigned effective section, in kN m².</summary>
    public sealed class DistributionGirder
    {
        public string Name { get; set; } = "Trave";
        public double PositionM { get; set; }
        public double FlexuralRigidityKnM2 { get; set; } = 12000000;
        public double TorsionalRigidityKnM2 { get; set; } = 200000;
    }
    public sealed class DistributionLane
    {
        public string Name { get; set; } = "Corsia";
        public double LeftM { get; set; } = -3;
        public double WidthM { get; set; } = 3;
        public double StartM { get; set; }
        public double EndM { get; set; } = 25;
        public double LoadKnM2 { get; set; } = 9;
    }
    public sealed class DistributionTandem
    {
        public string Name { get; set; } = "Tandem";
        public double CentreTransverseM { get; set; } = -1.5;
        public double CentreLongitudinalM { get; set; } = 12.5;
        public double AxleLoadKn { get; set; } = 300;
        public double AxleSpacingM { get; set; } = 1.2;
        public double WheelSpacingM { get; set; } = 2;
        public double FootprintLengthM { get; set; } = .4;
        public double FootprintWidthM { get; set; } = .4;
    }
    /// <summary>Mechanical distribution, separate from normative traffic generation. Positive vertical loads are downward.
    /// The deformable model uses longitudinal sine modes, girder bending/torsion and an Euler-Bernoulli transverse strip.
    /// Intermediate discrete diaphragms, skew, curvature, continuity and local plate stress are outside this model.</summary>
    public sealed class TransverseDistributionInput
    {
        public TransverseDistributionMethod Method { get; set; } = TransverseDistributionMethod.DeformableFourier;
        public bool ApplicabilityConfirmed { get; set; }
        public double SpanM { get; set; } = 25;
        public double SectionM { get; set; } = 12.5;
        public double DeckLeftM { get; set; } = -5;
        public double DeckRightM { get; set; } = 5;
        /// <summary>EI of slab per metre longitudinal width (E t³/12 for independent strips), in kN m. No longitudinal slab bending is added separately.</summary>
        public double TransverseRigidityKnM { get; set; } = 39062.5;
        public double RelativeTolerance { get; set; } = .001;
        public int MaximumModes { get; set; } = 512;
        public double InfluenceLongitudinalM { get; set; } = 12.5;
        public int InfluenceStations { get; set; } = 61;
        public List<DistributionGirder> Girders { get; set; } = new List<DistributionGirder>
        {
            new DistributionGirder {Name="T1",PositionM=-4},new DistributionGirder {Name="T2",PositionM=0},new DistributionGirder {Name="T3",PositionM=4}
        };
        public List<DistributionLane> Lanes { get; set; } = new List<DistributionLane> { new DistributionLane {Name="L1"} };
        public List<DistributionTandem> Tandems { get; set; } = new List<DistributionTandem> { new DistributionTandem {Name="TS1"} };
    }
    public sealed class DistributionGirderResult
    {
        public string Name { get; set; } = "";
        public double PositionM { get; set; }
        public double MomentKnM { get; set; }
        public double Percentage { get; set; }
        public double TorsionKnM { get; set; }
    }
    public sealed class DistributionLoadResult
    {
        public string Name { get; set; } = "";
        public double TotalLoadKn { get; set; }
        public double StaticMomentKnM { get; set; }
        public double CalculatedMomentKnM { get; set; }
        public double EquilibriumRelativeError { get; set; }
        public double ConvergenceRelativeError { get; set; }
        public List<DistributionGirderResult> Girders { get; set; } = new List<DistributionGirderResult>();
    }
    public sealed class DistributionInfluencePoint
    {
        public double PositionM { get; set; }
        /// <summary>Moment in each girder per vertical unit force at the declared longitudinal influence position, in m.</summary>
        public List<double> MomentOrdinatesM { get; set; } = new List<double>();
    }
    public sealed class TransverseDistributionResult
    {
        public string Method { get; set; } = "";
        public int Modes { get; set; }
        public bool Converged { get; set; }
        public double MaximumResidual { get; set; }
        public double InfluenceConvergenceRelativeError { get; set; }
        public double InfluenceEquilibriumRelativeError { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
        public List<DistributionLoadResult> Cases { get; set; } = new List<DistributionLoadResult>();
        public List<DistributionInfluencePoint> Influence { get; set; } = new List<DistributionInfluencePoint>();
    }
}
