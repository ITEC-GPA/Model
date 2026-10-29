using System.Collections.Generic;

namespace GPC.Model.Actions.Climate
{
    /// <summary>Explicit supported normative families. EN denotes first-generation recommended values, not a national annex.</summary>
    public enum ClimateStandard { Ntc2018, EurocodeFirstGeneration }
    public enum SnowZone { AlpineI, MediterraneanI, II, III }
    public enum SnowExposure { Normal, Windswept, Sheltered }
    public enum SnowRoof { Flat, Monopitch, Duopitch, MultiSpan, Cylindrical, LowerRoof }
    public enum WindRoof { Flat, Monopitch, Duopitch }
    public enum InternalPressure { DistributedOpenings, DominantFace, Assigned }
    public enum BuildingThermalMethod { SimplifiedNtc, AssignedSurfaceProfile, HomogeneousLayer }
    public enum ThermalMaterial { ReinforcedConcrete, Steel }
    public enum BridgeDeck { SteelBox, SteelTrussOrPlate, Composite, ConcreteBox, ConcreteBeam, ConcreteSlab }
    public enum BridgeSurfacing { Unsurfaced, Waterproofed, Asphalt, Ballast750 }

    /// <summary>SI inputs: m, m/s, kg/m³, kN/m², °C; angles in degrees. No partial or combination factors.</summary>
    public sealed class SnowInput
    {
        public ClimateStandard Standard { get; set; } = ClimateStandard.Ntc2018;
        public string NationalParameters { get; set; } = "";
        public SnowZone Zone { get; set; } = SnowZone.II;
        public double AltitudeM { get; set; } = 200;
        public bool AssignedGroundLoad { get; set; }
        public double GroundLoadKnM2 { get; set; } = 1;
        public SnowExposure Exposure { get; set; } = SnowExposure.Normal;
        public double ThermalCoefficient { get; set; } = 1;
        public string ThermalJustification { get; set; } = "";
        public SnowRoof Roof { get; set; } = SnowRoof.Duopitch;
        public double PitchLeftDeg { get; set; } = 20;
        public double PitchRightDeg { get; set; } = 20;
        public bool SnowGuards { get; set; }
        public double WidthM { get; set; } = 20;
        public double RiseM { get; set; } = 3;
        public double UpperWidthM { get; set; } = 10;
        public double StepHeightM { get; set; } = 3;
        public double SlidingCoefficient { get; set; }
        public double SnowDensityKnM3 { get; set; } = 2;
    }

    public sealed class WindSiteInput
    {
        public ClimateStandard Standard { get; set; } = ClimateStandard.Ntc2018;
        public string NationalParameters { get; set; } = "";
        public int NtcZone { get; set; } = 3;
        public double AltitudeM { get; set; } = 200;
        public bool AssignedBaseSpeed { get; set; }
        public double BaseSpeedMs { get; set; } = 27;
        public double ReturnYears { get; set; } = 50;
        /// <summary>NTC: 1..5 (I..V). EN: 0..4 (0..IV).</summary>
        public int ExposureCategory { get; set; } = 2;
        public double TopographyFactor { get; set; } = 1;
        public double DirectionFactor { get; set; } = 1;
        public double SeasonFactor { get; set; } = 1;
        public double ProbabilityK { get; set; } = .2;
        public double ProbabilityExponent { get; set; } = .5;
        public double TurbulenceFactor { get; set; } = 1;
        public double AirDensityKgM3 { get; set; } = 1.25;
    }

    public sealed class BuildingWindInput
    {
        public WindSiteInput Site { get; set; } = new WindSiteInput();
        /// <summary>Width normal to wind and depth parallel to wind, for the selected direction.</summary>
        public double WidthM { get; set; } = 20;
        public double DepthM { get; set; } = 30;
        public double HeightM { get; set; } = 10;
        public WindRoof Roof { get; set; } = WindRoof.Duopitch;
        public double PitchDeg { get; set; } = 20;
        /// <summary>0: wind normal to ridge from low eave; 90: parallel; 180: reverse, monopitch only.</summary>
        public int WindDirectionDeg { get; set; }
        public double ParapetM { get; set; }
        public double LoadedAreaM2 { get; set; } = 10;
        public InternalPressure InternalPressure { get; set; } = InternalPressure.DistributedOpenings;
        public double DominantOpeningRatio { get; set; } = 2;
        public double DominantExternalCoefficient { get; set; } = .8;
        public double InternalCoefficientMin { get; set; } = -.3;
        public double InternalCoefficientMax { get; set; } = .2;
        public double InternalReferenceHeightM { get; set; } = 10;
        public double StructuralFactor { get; set; } = 1;
        public bool StructuralFactorAssessed { get; set; }
        public double FrictionCoefficient { get; set; } = .01;
    }

    public sealed class BridgeWindInput
    {
        public WindSiteInput Site { get; set; } = new WindSiteInput();
        public double WidthM { get; set; } = 12;
        public double TotalHeightM { get; set; } = 2;
        public double DeckCentreHeightM { get; set; } = 15;
        public double BridgeLengthM { get; set; } = 100;
        public double StructuralFactor { get; set; } = 1;
        public bool ApplicabilityConfirmed { get; set; }
        public bool AssignedDragCoefficient { get; set; }
        public double DragCoefficient { get; set; } = 1;
        public bool AdjacentDeck { get; set; }
        public double SecondWidthM { get; set; } = 12;
        public double GapM { get; set; } = 1;
        public bool CoupledDecks { get; set; }
    }

    public sealed class ThermalSiteInput
    {
        public ClimateStandard Standard { get; set; } = ClimateStandard.Ntc2018;
        public string NationalParameters { get; set; } = "";
        public int NtcZone { get; set; } = 2;
        public double AltitudeM { get; set; } = 200;
        public bool AssignedTemperatures { get; set; }
        public double MinimumAirC { get; set; } = -10;
        public double MaximumAirC { get; set; } = 40;
    }

    public sealed class BuildingThermalInput
    {
        public ThermalSiteInput Site { get; set; } = new ThermalSiteInput();
        public BuildingThermalMethod Method { get; set; } = BuildingThermalMethod.SimplifiedNtc;
        public ThermalMaterial Material { get; set; } = ThermalMaterial.ReinforcedConcrete;
        public bool Exposed { get; set; } = true;
        public bool SimplifiedApplicabilityConfirmed { get; set; }
        public double InitialTemperatureC { get; set; } = 15;
        public double SummerOuterSurfaceC { get; set; } = 50;
        public double SummerInnerSurfaceC { get; set; } = 25;
        public double WinterOuterSurfaceC { get; set; } = -10;
        public double WinterInnerSurfaceC { get; set; } = 20;
        public string ProfileSource { get; set; } = "";
        public double SummerInnerAirC { get; set; } = 20;
        public double WinterInnerAirC { get; set; } = 25;
        public double SolarIncrementC { get; set; } = 30;
        public double ConductivityWmK { get; set; } = 1.5;
        public double InnerResistanceM2KW { get; set; } = .13;
        public double OuterResistanceM2KW { get; set; } = .04;
        public double ExpansionPerKelvin { get; set; } = .00001;
        public bool AssignedExpansionCoefficient { get; set; }
        public double LengthM { get; set; } = 30;
        public double ThicknessM { get; set; } = .3;
    }

    public sealed class BridgeThermalInput
    {
        public ThermalSiteInput Site { get; set; } = new ThermalSiteInput();
        public BridgeDeck Deck { get; set; } = BridgeDeck.ConcreteBox;
        public BridgeSurfacing Surfacing { get; set; } = BridgeSurfacing.Asphalt;
        public double SurfacingMm { get; set; } = 50;
        public double InitialTemperatureC { get; set; } = 15;
        public double ExpansionPerKelvin { get; set; } = .00001;
        public bool AssignedExpansionCoefficient { get; set; }
        public double LengthM { get; set; } = 100;
        public double DepthM { get; set; } = 2;
        public double WidthM { get; set; } = 12;
        public double OmegaN { get; set; } = .35;
        public double OmegaM { get; set; } = .75;
        public bool BearingSettingTemperatureKnown { get; set; }
        public double HorizontalDifferenceC { get; set; } = 5;
        public bool AssignedBridgeTemperatures { get; set; }
        public double MinimumBridgeC { get; set; } = -5;
        public double MaximumBridgeC { get; set; } = 42;
        public bool AssignedGradients { get; set; }
        public double HeatingDifferenceC { get; set; } = 10;
        public double CoolingDifferenceC { get; set; } = 5;
    }

    /// <summary>One auditable quantity. Formula identifies the mathematical operation; Reference identifies its source.</summary>
    public sealed class ClimateValue
    {
        public string Symbol { get; set; } = "";
        public string Description { get; set; } = "";
        public double Value { get; set; }
        public string Unit { get; set; } = "";
        public string Formula { get; set; } = "";
        public string Reference { get; set; } = "";
    }
    public sealed class ClimateCase
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ClimateValue> Values { get; set; } = new List<ClimateValue>();
    }
    public sealed class ClimateResult
    {
        public string Standard { get; set; } = "";
        public string Scope { get; set; } = "";
        public List<string> Notes { get; set; } = new List<string>();
        public List<ClimateValue> Parameters { get; set; } = new List<ClimateValue>();
        public List<ClimateCase> Cases { get; set; } = new List<ClimateCase>();
    }
}
