using System.Collections.Generic;
using GPC.Model.Actions.Climate;

namespace GPC.Model.Actions.General
{
    public enum OccupancyCategory { ResidentialA, StairsAB, OfficeB1, OfficeB2, GatheringC1, GatheringC2, GatheringC3, GatheringC4, GatheringC5, ShopD1, RetailD2, StorageE1, VehiclesF, VehiclesG, MaintenanceH, Assigned }
    public enum ImposedReduction { None, LoadedArea, Storeys }
    public sealed class LoadLayer
    {
        public string Name { get; set; } = "Strato";
        public double ThicknessM { get; set; } = .05;
        public double UnitWeightKnM3 { get; set; } = 20;
        public bool Structural { get; set; }
    }
    public sealed class BuildingLoadsInput
    {
        public ClimateStandard Standard { get; set; }
        public string NationalParameters { get; set; } = "";
        public OccupancyCategory Occupancy { get; set; } = OccupancyCategory.ResidentialA;
        public bool AssignedImposedLoads { get; set; }
        public double DistributedKnM2 { get; set; } = 2;
        /// <summary>Per contact footprint, not the sum of a two-wheel axle.</summary>
        public double ConcentratedKn { get; set; } = 2;
        public double HorizontalKnM { get; set; } = 1;
        public double ContactSideM { get; set; } = .05;
        public double PartitionLineKnM { get; set; }
        public bool PartitionDistributionConfirmed { get; set; }
        public ImposedReduction Reduction { get; set; }
        public double LoadedAreaM2 { get; set; } = 20;
        public int Storeys { get; set; } = 3;
        public double TributaryWidthM { get; set; } = 5;
        public List<LoadLayer> Layers { get; set; } = new List<LoadLayer>
        {
            new LoadLayer{Name="Soletta",ThicknessM=.25,UnitWeightKnM3=25,Structural=true},
            new LoadLayer{Name="Massetto",ThicknessM=.06,UnitWeightKnM3=20},
            new LoadLayer{Name="Pavimento",ThicknessM=.02,UnitWeightKnM3=22}
        };
    }
    public enum ImpactTarget { SupportingMember, Superstructure, Forklift }
    public enum ImpactRoad { Motorway, Rural, Urban, CarParking, LorryParking }
    public sealed class VehicleImpactInput
    {
        public ClimateStandard Standard { get; set; }
        public string NationalParameters { get; set; } = "";
        public ImpactTarget Target { get; set; }
        public ImpactRoad Road { get; set; }
        public double MemberWidthM { get; set; } = 1;
        public double ClearanceM { get; set; } = 5.5;
        public double EnTruckImpactHeightM { get; set; } = 1.5;
        public double EnFullImpactClearanceM { get; set; } = 5;
        public double EnNoImpactClearanceM { get; set; } = 6;
        public double ForkliftWeightKn { get; set; } = 50;
        public bool AssignedImpactForce { get; set; }
        public double ImpactForceKn { get; set; } = 1000;
        public double TransverseForceKn { get; set; } = 500;
        public string ForceSource { get; set; } = "";
    }
    public sealed class RoadTrafficInput
    {
        public ClimateStandard Standard { get; set; }
        public string NationalParameters { get; set; } = "";
        public double CarriagewayWidthM { get; set; } = 10;
        public double LoadedLengthM { get; set; } = 25;
        public double RadiusM { get; set; } = 1000;
        public bool StraightBridge { get; set; } = true;
        public double AlphaQ1 { get; set; } = 1;
        public double AlphaQ2 { get; set; } = 1;
        public double AlphaQ3 { get; set; } = 1;
        public double Alphaq1 { get; set; } = 1;
        public double AlphaqOther { get; set; } = 1;
        public double AlphaqRemaining { get; set; } = 1;
        public double BetaQ { get; set; } = 1;
        public double PavementThicknessM { get; set; } = .08;
        public double SlabThicknessM { get; set; } = .25;
        public double JointDistanceM { get; set; } = 6;
        public bool AddJointAmplification { get; set; }
        public double NtcJointFactor { get; set; } = 1;
        public string JointSource { get; set; } = "";
        public double FootwayWidthM { get; set; } = 1.5;
    }
    public enum CombinationFamily { Ultimate, Characteristic, Frequent, QuasiPermanent, Accidental }
    public enum CombinationSet { Structural, Equilibrium, Geotechnical }
    public enum EnUltimateExpression { Equation610, Equations610a610b }
    public enum ActionNature { PermanentG1, PermanentG2, Prestress, Variable, Accidental }
    public enum ActionPsiCategory { Residential, Office, Gathering, Retail, Storage, VehiclesF, VehiclesG, RoofH, Wind, SnowLow, SnowHigh, Temperature, Assigned }
    public sealed class CombinationAction
    {
        public string Name { get; set; } = "Q";
        public ActionNature Nature { get; set; } = ActionNature.Variable;
        /// <summary>Signed effect for ONE common response component, e.g. kN m. Not a mixture of forces and moments.</summary>
        public double Effect { get; set; } = 1;
        /// <summary>Same nonempty source shares one favourable/unfavourable choice (G) or presence and leading status (Q).</summary>
        public string Correlation { get; set; } = "";
        /// <summary>Nonempty name identifies mutually exclusive Q alternatives. Correlated parts may coexist.</summary>
        public string ExclusiveGroup { get; set; } = "";
        public ActionPsiCategory PsiCategory { get; set; } = ActionPsiCategory.Residential;
        public double Psi0 { get; set; } = .7;
        public double Psi1 { get; set; } = .5;
        public double Psi2 { get; set; } = .3;
        public bool DefinedPermanentG2 { get; set; }
        public bool OmitFavourableG2 { get; set; }
    }
    public sealed class ActionCombinationsInput
    {
        public ClimateStandard Standard { get; set; }
        public string NationalParameters { get; set; } = "";
        public CombinationFamily Family { get; set; }
        public CombinationSet Set { get; set; }
        public EnUltimateExpression EnExpression { get; set; }
        public double EnXi { get; set; } = .85;
        public bool EnAccidentalLeadingPsi1 { get; set; } = true;
        public string EffectUnit { get; set; } = "kN·m";
        public bool RecommendedEnFactorsConfirmed { get; set; }
        public List<CombinationAction> Actions { get; set; } = new List<CombinationAction>
        {
            new CombinationAction{Name="G1",Nature=ActionNature.PermanentG1,Effect=100},
            new CombinationAction{Name="G2",Nature=ActionNature.PermanentG2,Effect=30},
            new CombinationAction{Name="Uso",Effect=50},
            new CombinationAction{Name="Vento",Effect=-20,PsiCategory=ActionPsiCategory.Wind}
        };
    }
}
