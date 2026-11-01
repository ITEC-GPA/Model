using System.Collections.Generic;
using GPC.Model.Actions.Climate;
using GPC.Model.Actions.TimeEffects;
using GPC.Model.Materials;

namespace GPC.Model.Actions.Prestressing
{
    public enum PrestressingMethod { PreTensioned, PostTensionedBonded, PostTensionedUnbonded }
    public enum TendonProfile { Straight, Parabolic }
    public enum JackingEnd { Left, Right, BothSymmetric }
    public enum PrestressRelaxationClass { OrdinaryWireStrand, LowRelaxationWireStrand, HotRolledBar }
    /// <summary>One tendon or one equivalent family with identical profile, constant Ap and constant concrete section.
    /// Planar profile z positive upward from the concrete centroid. No heat curing or overstressing.</summary>
    public sealed class PrestressActionsInput
    {
        public ClimateStandard Standard { get; set; } = ClimateStandard.Ntc2018;
        public string NationalParameters { get; set; } = "";
        public PrestressingMethod Method { get; set; } = PrestressingMethod.PostTensionedBonded;
        public TendonProfile Profile { get; set; } = TendonProfile.Parabolic;
        public JackingEnd Jacking { get; set; } = JackingEnd.Left;
        public double LengthM { get; set; } = 30;
        public double StartEccentricityM { get; set; }
        public double MiddleEccentricityM { get; set; } = -.4;
        public double EndEccentricityM { get; set; }
        public double TendonAreaMm2 { get; set; } = 1800;
        public double TendonModulusMpa { get; set; } = 195000;
        public double FpkMpa { get; set; } = 1860;
        public double Fp01kMpa { get; set; } = 1640;
        public double JackingStressMpa { get; set; } = 1350;
        public double FrictionCoefficient { get; set; } = .19;
        /// <summary>Unintentional angular deviation per metre, inside the exponent μ(θ+k s), not an independent force-decay coefficient.</summary>
        public double WobbleRadM { get; set; } = .006;
        /// <summary>Per active end. BothSymmetric requires equal slip at both ends.</summary>
        public double AnchorageSlipMm { get; set; } = 6;
        public string SystemSource { get; set; } = "";
        public double JackingAgeDays { get; set; } = 28;
        public double TransferAgeDays { get; set; } = 28;
        public int SuccessiveTendons { get; set; } = 1;
        /// <summary>Positive compressive stress change in concrete adjacent to the tendon, from the structural model.</summary>
        public double ConcreteStressChangeMpa { get; set; } = 8;
        /// <summary>Positive compressive stress at tendon level under the sustained quasi-permanent state, after immediate losses.</summary>
        public double ConcreteQpStressMpa { get; set; } = 6;
        /// <summary>Additional tendon stress from the quasi-permanent actions, used only for the relaxation reference state.
        /// Zero explicitly assumes σp,QP = σpm0. This load-induced stress is not exported as prestress prestrain.</summary>
        public double TendonQpStressIncrementMpa { get; set; }
        public string ConcreteStressSource { get; set; } = "";
        public double ConcreteAreaMm2 { get; set; } = 500000;
        public double ConcreteInertiaMm4 { get; set; } = 41666666667;
        public double DryingPerimeterMm { get; set; } = 3000;
        public double FckMpa { get; set; } = 40;
        public ConcreteMaterialEuropeanCommon.CementTypes Cement { get; set; } = ConcreteMaterialEuropeanCommon.CementTypes.ClassN;
        public double RelativeHumidityPercent { get; set; } = 70;
        public double DryingStartDays { get; set; } = 7;
        public PrestressRelaxationClass Relaxation { get; set; } = PrestressRelaxationClass.LowRelaxationWireStrand;
        public bool CertifiedRelaxation { get; set; }
        public double CertifiedRho1000Percent { get; set; } = 2.5;
        public bool IncludeDeferredLosses { get; set; } = true;
        public double EnJackingFpkFactor { get; set; } = .8;
        public double EnJackingFp01Factor { get; set; } = .9;
        public double EnInitialFpkFactor { get; set; } = .75;
        public double EnInitialFp01Factor { get; set; } = .85;
        public int ProfileIntervals { get; set; } = 200;
        public List<ConcreteObservation> Observations { get; set; } = new List<ConcreteObservation>
        {
            new ConcreteObservation {Name="Trasferimento",AgeDays=28},new ConcreteObservation {Name="1 anno",AgeDays=365},
            new ConcreteObservation {Name="10 anni",AgeDays=3650},new ConcreteObservation {Name="100 anni",AgeDays=36500}
        };
    }
    public sealed class PrestressProfilePoint
    {
        public double XM { get; set; }
        public double EccentricityM { get; set; }
        public double ArcLengthM { get; set; }
        public double TangentRadians { get; set; }
        public double AngularDeviationRad { get; set; }
        public double JackingStressMpa { get; set; }
        public double AfterFrictionMpa { get; set; }
        public double AfterAnchoringMpa { get; set; }
        public double ShortRelaxationLossMpa { get; set; }
        public double ElasticLossMpa { get; set; }
        public double InitialStressMpa { get; set; }
        public double ShrinkageContributionMpa { get; set; }
        public double CreepContributionMpa { get; set; }
        public double RelaxationContributionMpa { get; set; }
        public double DeferredLossMpa { get; set; }
        public double EffectiveStressMpa { get; set; }
        public double EffectiveForceKn { get; set; }
        /// <summary>Stress-free prestrain magnitude associated with the effective state, σeff/Ep. Distinct from the original jack stress.</summary>
        public double EffectivePrestrain { get; set; }
        public double ConcreteAxialForceKn { get; set; }
        public double ConcretePrimaryMomentKnM { get; set; }
        public bool WithinInitialStressLimit { get; set; }
    }
    public sealed class PrestressStageResult
    {
        public string Name { get; set; } = "";
        public double AgeDays { get; set; }
        public double CreepCoefficient { get; set; }
        public double ShrinkageSinceTransfer { get; set; }
        public List<PrestressProfilePoint> Points { get; set; } = new List<PrestressProfilePoint>();
    }
    public sealed class PrestressActionsResult
    {
        public ClimateResult Summary { get; set; } = new ClimateResult();
        public List<PrestressStageResult> Stages { get; set; } = new List<PrestressStageResult>();
        public double JackingLimitMpa { get; set; }
        public double InitialLimitMpa { get; set; }
        public double Rho1000Percent { get; set; }
        public double JackingElongationMm { get; set; }
        public double AnchorageRecoveryMm { get; set; }
        public double MaxDiscretizationMpa { get; set; }
        public bool Converged { get; set; }
        public bool WithinStressLimits { get; set; }
    }
}
