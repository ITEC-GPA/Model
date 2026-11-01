using System.Collections.Generic;
using GPC.Model.Actions.Climate;

namespace GPC.Model.Actions.Cranes
{
    public enum CraneDistribution { FourWheelBridge, SupplierWheelComponents }
    public enum HoistingClass { HC1, HC2, HC3, HC4 }
    public enum CraneReleaseDevice { Grab, Magnet }
    public enum CraneActionComponent { BridgeAcceleration, Skewing, CrabAcceleration, InServiceWind, BridgeBuffer, CrabBuffer, Tilting }
    public sealed class CranePosition
    {
        public string Name { get; set; } = "Carrello a sinistra";
        public double CraneXM { get; set; }
        public double CrabYM { get; set; } = 1;
        /// <summary>Sign of the automatically calculated bridge acceleration. Supplier actions retain their signs.</summary>
        public int TravelDirection { get; set; } = 1;
    }
    public sealed class CraneWheelInput
    {
        public string Name { get; set; } = "Ruota";
        public int Position { get; set; } = 1;
        public int Rail { get; set; } = 1;
        public double XM { get; set; }
        public double CraneWeightKn { get; set; } = 20;
        public double HoistLoadKn { get; set; } = 50;
    }
    /// <summary>Signed components at runway level. Vertical positive down, x along runway, y towards rail 2.
    /// Acceleration and bridge buffer are before phi5/phi7. Other components are characteristic (accidental: design) values.</summary>
    public sealed class CraneAuxiliaryAction
    {
        public string Name { get; set; } = "Azione del costruttore";
        /// <summary>One based position; zero applies to every position.</summary>
        public int Position { get; set; }
        public CraneActionComponent Component { get; set; } = CraneActionComponent.Skewing;
        public double XM { get; set; }
        public double YM { get; set; }
        public double VerticalDownKn { get; set; }
        public double LongitudinalKn { get; set; }
        public double TransverseKn { get; set; }
        public string Source { get; set; } = "";
    }
    public sealed class CraneActionsInput
    {
        public ClimateStandard Standard { get; set; } = ClimateStandard.EurocodeFirstGeneration;
        public string NationalParameters { get; set; } = "";
        public string SupplierSource { get; set; } = "";
        public bool StaticComponentsConfirmed { get; set; }
        public string ApplicabilityAssessment { get; set; } = "";
        public CraneDistribution Distribution { get; set; }
        public double LengthM { get; set; } = 20;
        public double WheelbaseM { get; set; } = 4;
        public double BridgeWeightKn { get; set; } = 100;
        public double CrabWeightKn { get; set; } = 20;
        public double HoistLoadKn { get; set; } = 200;
        public HoistingClass HoistingClass { get; set; } = HoistingClass.HC2;
        public double HoistingSpeedMs { get; set; } = .2;
        public bool SupplierPhi2 { get; set; }
        public double AssignedPhi2 { get; set; } = 1.168;
        public bool TrackTolerancesConfirmed { get; set; }
        public double Phi4 { get; set; } = 1;
        public double Phi5 { get; set; } = 1.5;
        public bool AutomaticBridgeAcceleration { get; set; } = true;
        public double DriveForceKn { get; set; } = 10;
        public double RailHeadWidthMm { get; set; } = 60;
        public double WheelEccentricityFactor { get; set; } = .25;
        public double ResidualHoistFraction { get; set; } = .1;
        public bool IncludeRelease { get; set; }
        public CraneReleaseDevice ReleaseDevice { get; set; }
        public double ReleasedMassFraction { get; set; } = .9;
        public bool IncludeTests { get; set; } = true;
        public double DynamicTestRatio { get; set; } = 1.1;
        public double StaticTestRatio { get; set; } = 1.25;
        public bool IncludeAccidental { get; set; }
        public double Phi7 { get; set; } = 1.25;
        public List<CranePosition> CranePositions { get; set; } = new List<CranePosition>
        { new CranePosition(),new CranePosition{Name="Carrello a destra",CrabYM=19,TravelDirection=-1} };
        public List<CraneWheelInput> SupplierWheels { get; set; } = new List<CraneWheelInput>();
        public List<CraneAuxiliaryAction> AuxiliaryActions { get; set; } = new List<CraneAuxiliaryAction>();
    }
    public sealed class CranePointAction
    {
        public string Name { get; set; } = "";
        public int Rail { get; set; }
        public double XM { get; set; }
        public double YM { get; set; }
        public double VerticalDownKn { get; set; }
        public double LongitudinalKn { get; set; }
        public double TransverseKn { get; set; }
        /// <summary>Magnitude of both alternative local torsions from +/- e. Never add both simultaneously.</summary>
        public double EccentricTorsionKnM { get; set; }
    }
    public sealed class CraneGroupResult
    {
        public string Name { get; set; } = "";
        public int Position { get; set; }
        public int Group { get; set; }
        public bool Accidental { get; set; }
        public bool Test { get; set; }
        public double CraneFactor { get; set; }
        public double HoistFactor { get; set; }
        public double HorizontalFactor { get; set; }
        public List<CranePointAction> Points { get; set; } = new List<CranePointAction>();
    }
    public sealed class CraneActionsResult
    {
        public ClimateResult Summary { get; set; } = new ClimateResult();
        public List<CraneGroupResult> Groups { get; set; } = new List<CraneGroupResult>();
    }
}
