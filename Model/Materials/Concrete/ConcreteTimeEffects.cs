using System;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Linear creep (EN 1992-1-1:2004, Annex B) and secant modulus maturity (§3.1.3(3))
    /// of normal-weight concrete at 20 °C. Constant humidity, one sustained loading age;
    /// no shrinkage, nonlinear creep or temperature history. Units: MPa, mm, days, %.
    /// </summary>
    /// <remarks>
    /// This immutable calculation does not change material archives or the historical
    /// GetEpsilonCCInfiniteTime API (which returns a strain, not a creep coefficient).
    /// Phi is associated with the tangent reference modulus Ec = 1.05 Ecm28;
    /// Ecm(t) below is the secant modulus due to maturity, not an effective creep modulus.
    /// </remarks>
    public sealed class ConcreteTimeEffects
    {
        private readonly ConcreteMaterialEN1992 material;
        public double FckMpa { get; }
        public ConcreteMaterialEuropeanCommon.CementTypes Cement { get; }
        public double RelativeHumidityPercent { get; }
        public double NotionalSizeMm { get; }
        public double LoadingAgeDays { get; }
        /// <summary>Age modified for cement class in B.5 only; elapsed loading time remains unadjusted.</summary>
        public double AdjustedLoadingAgeDays { get; }
        public double CreepInfinity { get; }
        public double BetaHDays { get; }
        public double Ecm28Mpa { get; }
        public double CreepReferenceModulusMpa => 1.05 * Ecm28Mpa;

        public ConcreteTimeEffects(double fckMpa, ConcreteMaterialEuropeanCommon.CementTypes cement,
            double relativeHumidityPercent, double notionalSizeMm, double loadingAgeDays)
        {
            Range(fckMpa, 12, 90, nameof(fckMpa));
            Range(relativeHumidityPercent, 40, 100, nameof(relativeHumidityPercent));
            Positive(notionalSizeMm, nameof(notionalSizeMm));
            Range(loadingAgeDays, 0.5, double.MaxValue, nameof(loadingAgeDays));
            if (!Enum.IsDefined(typeof(ConcreteMaterialEuropeanCommon.CementTypes), cement))
                throw new ArgumentOutOfRangeException(nameof(cement));
            FckMpa = fckMpa; Cement = cement; RelativeHumidityPercent = relativeHumidityPercent;
            NotionalSizeMm = notionalSizeMm; LoadingAgeDays = loadingAgeDays;
            material = new ConcreteMaterialEN1992("Time effects", fckMpa,
                ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle, cementType: cement);
            Ecm28Mpa = material.Ecm;
            double fcm = Math.Abs(material.Fcm);
            double alpha = cement == ConcreteMaterialEuropeanCommon.CementTypes.ClassS ? -1 :
                cement == ConcreteMaterialEuropeanCommon.CementTypes.ClassR ? 1 : 0;
            AdjustedLoadingAgeDays = Math.Max(0.5, loadingAgeDays * Math.Pow(1 + 9 / (2 + Math.Pow(loadingAgeDays, 1.2)), alpha));
            CreepInfinity = material.NotionalCreepCoefficient(relativeHumidityPercent, notionalSizeMm, AdjustedLoadingAgeDays);
            double alpha3 = fcm <= 35 ? 1 : Math.Sqrt(35 / fcm);
            BetaHDays = Math.Min(1.5 * (1 + Math.Pow(0.012 * relativeHumidityPercent, 18)) * notionalSizeMm + 250 * alpha3, 1500 * alpha3);
        }

        /// <summary>
        /// Response at concrete age t. Creep is zero up to loading. Positive infinity gives
        /// the analytic limits, not a finite-time approximation. Zero and negative ages are invalid.
        /// </summary>
        public ConcreteTimePoint AtAge(double ageDays)
        {
            if (double.IsNaN(ageDays) || ageDays <= 0) throw new ArgumentOutOfRangeException(nameof(ageDays));
            double elapsed = Math.Max(0, ageDays - LoadingAgeDays);
            double phi = double.IsPositiveInfinity(ageDays) ? CreepInfinity :
                CreepInfinity * Math.Pow(elapsed / (BetaHDays + elapsed), 0.3);
            // Reuse Model's maturity API with its signed Fcm convention.
            double modulus = material.GetEcm(material.Fcm, ageDays);
            return new ConcreteTimePoint(ageDays, phi, modulus, modulus / Ecm28Mpa);
        }

        /// <summary>h0 = 2 Ac / u, with area in mm² and drying perimeter in mm.</summary>
        public static double NotionalSize(double concreteAreaMm2, double dryingPerimeterMm)
        {
            Positive(concreteAreaMm2, nameof(concreteAreaMm2));
            Positive(dryingPerimeterMm, nameof(dryingPerimeterMm));
            double size = concreteAreaMm2 / dryingPerimeterMm * 2;
            Positive(size, nameof(concreteAreaMm2));
            return size;
        }

        private static void Positive(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0) throw new ArgumentOutOfRangeException(name);
        }
        private static void Range(double value, double min, double max, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < min || value > max) throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>Immutable point: age in days, dimensionless creep, secant modulus in MPa and Ecm(t)/Ecm(28).</summary>
    public sealed class ConcreteTimePoint
    {
        public double AgeDays { get; }
        public double CreepCoefficient { get; }
        public double EcmMpa { get; }
        public double ModulusFactor { get; }
        internal ConcreteTimePoint(double ageDays, double creepCoefficient, double ecmMpa, double modulusFactor)
        { AgeDays = ageDays; CreepCoefficient = creepCoefficient; EcmMpa = ecmMpa; ModulusFactor = modulusFactor; }
    }
}
