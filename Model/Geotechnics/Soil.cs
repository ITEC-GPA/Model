using System;

namespace GPC.Model.Geotechnics
{
    /// <summary>
    /// Characteristic parameters of a soil. Units of Model: unit weights N/mm³, strengths and moduli MPa (N/mm²), angles rad.
    /// Optional parameters are null when not given: a method that needs them must report missing data, never assume them.
    /// </summary>
    [Serializable]
    public sealed class Soil : IEquatable<Soil>
    {
        private readonly string _name, _source;
        private readonly double _unitWeight, _saturatedUnitWeight, _frictionAngle, _cohesion;
        private readonly double? _undrainedShearStrength, _youngModulus, _constrainedModulus, _poissonRatio;

        public string Name => _name;
        /// <summary>γ above the water table, N/mm³.</summary>
        public double UnitWeight => _unitWeight;
        /// <summary>γsat below the water table, N/mm³.</summary>
        public double SaturatedUnitWeight => _saturatedUnitWeight;
        /// <summary>Effective friction angle φ'k, rad.</summary>
        public double FrictionAngle => _frictionAngle;
        /// <summary>Effective cohesion c'k, MPa.</summary>
        public double EffectiveCohesion => _cohesion;
        /// <summary>Undrained shear strength cu,k, MPa; null when not given (drained analyses only).</summary>
        public double? UndrainedShearStrength => _undrainedShearStrength;
        /// <summary>Young's modulus E, MPa.</summary>
        public double? YoungModulus => _youngModulus;
        /// <summary>Constrained (oedometric) modulus Eoed, MPa.</summary>
        public double? ConstrainedModulus => _constrainedModulus;
        public double? PoissonRatio => _poissonRatio;
        /// <summary>Provenance of the parameters (geotechnical report, tests, correlations).</summary>
        public string Source => _source;

        public Soil(string name, double unitWeight, double saturatedUnitWeight, double frictionAngle, double effectiveCohesion, string source,
            double? undrainedShearStrength = null, double? youngModulus = null, double? constrainedModulus = null, double? poissonRatio = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the soil parameters is required.", nameof(source));
            if (!Positive(unitWeight)) throw new ArgumentOutOfRangeException(nameof(unitWeight));
            if (!Positive(saturatedUnitWeight) || saturatedUnitWeight < unitWeight) throw new ArgumentOutOfRangeException(nameof(saturatedUnitWeight), "γsat ≥ γ > 0 required.");
            if (!Finite(frictionAngle) || frictionAngle < 0 || frictionAngle >= Math.PI / 2) throw new ArgumentOutOfRangeException(nameof(frictionAngle), "0 ≤ φ' < π/2 rad.");
            if (!Finite(effectiveCohesion) || effectiveCohesion < 0) throw new ArgumentOutOfRangeException(nameof(effectiveCohesion));
            if (undrainedShearStrength.HasValue && !Positive(undrainedShearStrength.Value)) throw new ArgumentOutOfRangeException(nameof(undrainedShearStrength));
            if (youngModulus.HasValue && !Positive(youngModulus.Value)) throw new ArgumentOutOfRangeException(nameof(youngModulus));
            if (constrainedModulus.HasValue && !Positive(constrainedModulus.Value)) throw new ArgumentOutOfRangeException(nameof(constrainedModulus));
            if (poissonRatio.HasValue && (!Finite(poissonRatio.Value) || poissonRatio < 0 || poissonRatio >= .5)) throw new ArgumentOutOfRangeException(nameof(poissonRatio));
            _name = name; _source = source; _unitWeight = unitWeight; _saturatedUnitWeight = saturatedUnitWeight; _frictionAngle = frictionAngle;
            _cohesion = effectiveCohesion; _undrainedShearStrength = undrainedShearStrength; _youngModulus = youngModulus;
            _constrainedModulus = constrainedModulus; _poissonRatio = poissonRatio;
        }

        public bool Equals(Soil o) => o != null && _name == o._name && _source == o._source && _unitWeight == o._unitWeight && _saturatedUnitWeight == o._saturatedUnitWeight
            && _frictionAngle == o._frictionAngle && _cohesion == o._cohesion && _undrainedShearStrength == o._undrainedShearStrength
            && _youngModulus == o._youngModulus && _constrainedModulus == o._constrainedModulus && _poissonRatio == o._poissonRatio;
        public override bool Equals(object obj) => Equals(obj as Soil);
        public override int GetHashCode() => unchecked((_name?.GetHashCode() ?? 0) * 31 + _unitWeight.GetHashCode() * 17 + _frictionAngle.GetHashCode());
        public override string ToString() => _name;

        internal static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        internal static bool Positive(double v) => Finite(v) && v > 0;
    }

    /// <summary>Conversions from the usual geotechnical units to the units of Model (N, mm, MPa, rad). Use them only at the interfaces.</summary>
    public static class SoilUnits
    {
        /// <summary>1 kN/m³ = 1e-6 N/mm³.</summary>
        public const double KiloNewtonPerCubicMetre = 1e-6;
        /// <summary>1 kPa = 1e-3 MPa.</summary>
        public const double KiloPascal = 1e-3;
        /// <summary>1 m = 1000 mm.</summary>
        public const double Metre = 1000;
        /// <summary>1° = π/180 rad.</summary>
        public const double Degree = Math.PI / 180;
        /// <summary>Unit weight of water 9.81 kN/m³, N/mm³ (value used by the legacy ANTHEA calculations).</summary>
        public const double WaterUnitWeight = 9.81 * KiloNewtonPerCubicMetre;
        /// <summary>
        /// Acceleration of the gravity of the geotechnical unit weights, 9.81 m/s² in mm/s² (as <see cref="WaterUnitWeight"/>): the unit weight of a
        /// Model material is <see cref="Materials.Material.GetUnitWeight"/>(Gravity), N/mm³ (steel 7.85e-9 t/mm³ · 9810 = 77.0 kN/m³).
        /// </summary>
        public const double Gravity = 9810;
    }
}
