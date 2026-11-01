using System.Collections.Generic;
using GPC.Model.Actions.Climate;
using GPC.Model.Materials;

namespace GPC.Model.Actions.TimeEffects
{
    public enum AxialRestraint { Free, FixedDistance, EndSpring }
    public sealed class ConcreteStressEvent
    {
        public string Name { get; set; } = "Incremento di tensione";
        public double AgeDays { get; set; } = 28;
        /// <summary>Positive tension; negative compression. MPa.</summary>
        public double IncrementMpa { get; set; } = -5;
    }
    public sealed class ConcreteObservation
    {
        public string Name { get; set; } = "Osservazione";
        public double AgeDays { get; set; } = 365;
    }
    public sealed class AxialDisplacementPoint
    {
        public string Name { get; set; } = "Spostamento relativo";
        public double AgeDays { get; set; } = 28;
        /// <summary>Relative displacement of the two supports; positive opening, mm. Linear interpolation in chronological age.</summary>
        public double DisplacementMm { get; set; }
    }
    /// <summary>Normal-weight concrete at constant 20 °C and RH. Axial uncracked bar only, SI/mm/MPa/days as named.</summary>
    public sealed class TimeEffectsInput
    {
        public ClimateStandard Standard { get; set; } = ClimateStandard.Ntc2018;
        public string NationalParameters { get; set; } = "";
        public double FckMpa { get; set; } = 30;
        public ConcreteMaterialEuropeanCommon.CementTypes Cement { get; set; } = ConcreteMaterialEuropeanCommon.CementTypes.ClassN;
        public double RelativeHumidityPercent { get; set; } = 70;
        public double AreaMm2 { get; set; } = 150000;
        public double DryingPerimeterMm { get; set; } = 1600;
        public double LengthM { get; set; } = 10;
        /// <summary>Reference stress-free configuration; shrinkage before this age is not restrained.</summary>
        public double ReferenceAgeDays { get; set; } = 7;
        public double DryingStartDays { get; set; } = 7;
        public bool IncludeShrinkage { get; set; } = true;
        public bool IncludeCreep { get; set; } = true;
        public AxialRestraint Restraint { get; set; } = AxialRestraint.Free;
        public double EndSpringKnMm { get; set; } = 50;
        /// <summary>Logarithmic grid intervals. Restrained solution is repeated at twice this resolution.</summary>
        public int TimeSteps { get; set; } = 150;
        public List<ConcreteStressEvent> StressEvents { get; set; } = new List<ConcreteStressEvent> { new ConcreteStressEvent() };
        public List<ConcreteObservation> Observations { get; set; } = new List<ConcreteObservation>
        {
            new ConcreteObservation { Name="28 giorni", AgeDays=28 }, new ConcreteObservation { Name="90 giorni", AgeDays=90 },
            new ConcreteObservation { Name="1 anno", AgeDays=365 },new ConcreteObservation { Name="10 anni", AgeDays=3650 }
        };
        public List<AxialDisplacementPoint> Displacements { get; set; } = new List<AxialDisplacementPoint>();
    }
    public sealed class ConcreteHistoryPoint
    {
        public double AgeDays { get; set; }
        public double EcmMpa { get; set; }
        public double FckMpa { get; set; }
        public double DryingShrinkage { get; set; }
        public double AutogenousShrinkage { get; set; }
        public double RelativeShrinkage { get; set; }
        public double FreeElasticStrain { get; set; }
        public double FreeCreepStrain { get; set; }
        public double FreeStrain { get; set; }
        public double ActualElasticStrain { get; set; }
        public double ActualCreepStrain { get; set; }
        public double ActualStrain { get; set; }
        public double StressMpa { get; set; }
        public double ExternalStressMpa { get; set; }
        public double AxialForceKn { get; set; }
        public double SupportForceKn { get; set; }
        public double FreeDisplacementMm { get; set; }
        public double ActualDisplacementMm { get; set; }
        public double PrescribedDisplacementMm { get; set; }
        public double MeanTensileStrengthMpa { get; set; }
        public bool WithinLinearRange { get; set; }
    }
    public sealed class TimeEffectsResult
    {
        public ClimateResult Summary { get; set; } = new ClimateResult();
        public List<ConcreteHistoryPoint> History { get; set; } = new List<ConcreteHistoryPoint>();
        public double NotionalSizeMm { get; set; }
        public double Kh { get; set; }
        public double DryingShrinkageInfinity { get; set; }
        public double AutogenousShrinkageInfinity { get; set; }
        public double MaxStressDiscretizationMpa { get; set; }
        public bool Converged { get; set; }
        public bool WithinLinearRange { get; set; }
    }
}
