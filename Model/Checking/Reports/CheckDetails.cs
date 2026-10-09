using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Results;

namespace GPC.Model.PostProcessing
{
    public enum CheckApplicability { Required, NotApplicable, Excluded }

    /// <summary>Optional solver evidence. Null convergence means it was not reported, never inferred convergence.</summary>
    [Serializable]
    public sealed class CheckConvergence
    {
        public bool Converged { get; private set; }
        public int? Iterations { get; private set; }
        public double? Residual { get; private set; }
        public double? Tolerance { get; private set; }
        public string Criterion { get; private set; }
        public CheckConvergence(bool converged, int? iterations, double? residual, double? tolerance, string criterion)
        {
            CheckValue.Text(criterion, nameof(criterion)); CheckValue.Finite(residual, nameof(residual)); CheckValue.Finite(tolerance, nameof(tolerance));
            if (iterations < 0 || residual < 0 || tolerance < 0) throw new ArgumentOutOfRangeException(nameof(iterations));
            Converged = converged; Iterations = iterations; Residual = residual; Tolerance = tolerance; Criterion = criterion;
        }
    }

    /// <summary>Stress/strain or other quantity at a point in the detail's stated local frame; mm for coordinates.</summary>
    [Serializable]
    public sealed class CheckPointValue
    {
        public string Key { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public double Value { get; private set; }
        public string Unit { get; private set; }
        public string Frame { get; private set; }
        public CheckPointValue(string key, double x, double y, double z, double value, string unit, string frame)
        {
            CheckValue.Text(key, nameof(key)); CheckValue.Text(unit, nameof(unit)); CheckValue.Text(frame, nameof(frame));
            foreach (var v in new[] { x, y, z, value }) CheckValue.Finite(v, nameof(value));
            Key = key; X = x; Y = y; Z = z; Value = value; Unit = unit; Frame = frame;
        }
    }

    /// <summary>Value snapshot of the chosen code, edition and actual coefficients; no live Standard reference.</summary>
    [Serializable]
    public sealed class CheckStandardContext
    {
        public string Code { get; private set; }
        public string Edition { get; private set; }
        public string NationalAnnex { get; private set; }
        public string Implementation { get; private set; }
        public string Parameters { get; private set; }
        public string Reference { get; private set; }
        public bool HasDeclaredEdition => !string.IsNullOrWhiteSpace(Edition);
        public string Identity => Core.ModelValues.Fingerprint(new object[] { Code, Edition, NationalAnnex, Implementation, Parameters });

        public CheckStandardContext(string code, string edition, string nationalAnnex, string implementation, string parameters, string reference = null)
        {
            CheckValue.Text(code, nameof(code)); CheckValue.Text(implementation, nameof(implementation));
            Code = code; Edition = edition; NationalAnnex = nationalAnnex; Implementation = implementation;
            Parameters = parameters; Reference = reference;
        }
    }

    /// <summary>Demand/capacity and explicit engine verdict. Signed values are retained; utilization, when provided, uses limit 1.</summary>
    [Serializable]
    public sealed class CheckMetric
    {
        public string Key { get; private set; }
        public double? Demand { get; private set; }
        public double? Capacity { get; private set; }
        public string Unit { get; private set; }
        public double? Utilization { get; private set; }
        public bool? Passed { get; private set; }
        public string Reference { get; private set; }
        public CheckMetric(string key, double? demand, double? capacity, string unit, double? utilization = null, bool? passed = null, string reference = null)
        {
            CheckValue.Text(key, nameof(key)); CheckValue.Text(unit, nameof(unit));
            CheckValue.Finite(demand, nameof(demand)); CheckValue.Finite(capacity, nameof(capacity)); CheckValue.Finite(utilization, nameof(utilization));
            if (utilization < 0) throw new ArgumentOutOfRangeException(nameof(utilization));
            if (passed.HasValue && utilization.HasValue && passed.Value != (utilization.Value <= 1))
                throw new ArgumentException("The explicit metric verdict disagrees with its normalized utilization.");
            Key = key; Demand = demand; Capacity = capacity; Unit = unit; Utilization = utilization; Passed = passed; Reference = reference;
        }
    }

    [Serializable]
    public sealed class CheckCalculationValue
    {
        public string Key { get; private set; }
        public double? Value { get; private set; }
        public string Unit { get; private set; }
        public string Expression { get; private set; }
        public CheckCalculationValue(string key, double? value, string unit, string expression)
        {
            CheckValue.Text(key, nameof(key)); CheckValue.Finite(value, nameof(value));
            Key = key; Value = value; Unit = unit; Expression = expression;
        }
    }

    /// <summary>Immutable six-component snapshot in N/Nmm. Axes are copied on input and output.</summary>
    [Serializable]
    public sealed class BeamForceSnapshot
    {
        private readonly CoordinateSystem _axes;
        public double N { get; private set; }
        public double V1 { get; private set; }
        public double V2 { get; private set; }
        public double T { get; private set; }
        public double M1 { get; private set; }
        public double M2 { get; private set; }
        public CoordinateSystem Axes => ActionTransformations.AtPoint(_axes, _axes.Origin);
        public BeamForceSnapshot(ResultBeamForces forces)
        {
            if (forces == null) throw new ArgumentNullException(nameof(forces));
            foreach (double value in new[] { forces.N, forces.V1, forces.V2, forces.T, forces.M1, forces.M2 }) CheckValue.Finite(value, nameof(forces));
            GPC.Model.PostProcessing.Axes.Validate(forces.CoordinateSystem);
            _axes = ActionTransformations.AtPoint(forces.CoordinateSystem, forces.CoordinateSystem.Origin);
            N = forces.N; V1 = forces.V1; V2 = forces.V2; T = forces.T; M1 = forces.M1; M2 = forces.M2;
        }
    }

    [Serializable]
    public sealed class CheckInputSnapshot
    {
        public string SectionFingerprint { get; private set; }
        public string SampleFingerprint { get; private set; }
        public string ForcesFingerprint { get; private set; }
        public BeamForceSnapshot BeamForces { get; private set; }
        public CheckInputSnapshot(string sectionFingerprint, string sampleFingerprint, string forcesFingerprint, BeamForceSnapshot beamForces)
        {
            CheckValue.Text(sectionFingerprint, nameof(sectionFingerprint)); CheckValue.Text(sampleFingerprint, nameof(sampleFingerprint));
            CheckValue.Text(forcesFingerprint, nameof(forcesFingerprint));
            SectionFingerprint = sectionFingerprint; SampleFingerprint = sampleFingerprint; ForcesFingerprint = forcesFingerprint;
            BeamForces = beamForces ?? throw new ArgumentNullException(nameof(beamForces));
        }
    }

    /// <summary>Data-only details. Method and utilization definition prevent comparisons between incompatible criteria.</summary>
    [Serializable]
    public abstract class CheckDetails
    {
        private readonly CheckMetric[] _metrics;
        private readonly CheckCalculationValue[] _trace;
        private readonly CheckPointValue[] _points;
        public string MethodId { get; private set; }
        public string UtilizationDefinition { get; private set; }
        public IReadOnlyList<CheckMetric> Metrics => Array.AsReadOnly(_metrics);
        public IReadOnlyList<CheckCalculationValue> Trace => Array.AsReadOnly(_trace);
        public IReadOnlyList<CheckPointValue> Points => Array.AsReadOnly(_points);
        public CheckConvergence Convergence { get; private set; }
        protected CheckDetails(string methodId, string utilizationDefinition, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace,
            IEnumerable<CheckPointValue> points = null, CheckConvergence convergence = null)
        {
            CheckValue.Text(methodId, nameof(methodId)); CheckValue.Text(utilizationDefinition, nameof(utilizationDefinition));
            _metrics = (metrics ?? throw new ArgumentNullException(nameof(metrics))).ToArray();
            _trace = (trace ?? Enumerable.Empty<CheckCalculationValue>()).ToArray();
            _points = (points ?? Enumerable.Empty<CheckPointValue>()).ToArray(); Convergence = convergence;
            if (_metrics.Any(m => m == null) || _trace.Any(v => v == null) || _points.Any(v => v == null) || _metrics.Select(m => m.Key).Distinct(StringComparer.Ordinal).Count() != _metrics.Length)
                throw new ArgumentException("Metric keys must be unique and detail entries cannot be null.");
            MethodId = methodId; UtilizationDefinition = utilizationDefinition;
        }
    }

    /// <summary>The concomitant resistance point in N/Nmm; its components are not independent capacities.</summary>
    [Serializable]
    public sealed class SectionResistanceDetails : CheckDetails
    {
        public double NRd { get; private set; }
        public double M1Rd { get; private set; }
        public double M2Rd { get; private set; }
        public BeamForceSnapshot Demand { get; private set; }
        public string FailureMode { get; private set; }
        public double StrainReferenceX { get; private set; }
        public double StrainReferenceY { get; private set; }
        public double StrainAtReference { get; private set; }
        public double ChiX { get; private set; }
        public double ChiY { get; private set; }
        public SectionResistanceDetails(string methodId, string criterion, BeamForceSnapshot demand,
            double nRd, double m1Rd, double m2Rd, double utilization, string failureMode,
            double referenceX, double referenceY, double strainAtReference, double chiX, double chiY)
            : base(methodId, criterion, new[] { new CheckMetric("N-M1-M2", null, null, "1", utilization, utilization <= 1) }, null)
        {
            foreach (var value in new[] { nRd, m1Rd, m2Rd, referenceX, referenceY, strainAtReference, chiX, chiY }) CheckValue.Finite(value, nameof(value));
            Demand = demand ?? throw new ArgumentNullException(nameof(demand)); NRd = nRd; M1Rd = m1Rd; M2Rd = m2Rd;
            FailureMode = failureMode; StrainReferenceX = referenceX; StrainReferenceY = referenceY;
            StrainAtReference = strainAtReference; ChiX = chiX; ChiY = chiY;
        }
    }

    /// <summary>Output contract only; it does not enable a shear implementation.</summary>
    [Serializable]
    public sealed class ShearCheckDetails : CheckDetails
    {
        public int Direction { get; private set; }
        public double CotTheta { get; private set; }
        public ShearCheckDetails(string methodId, int direction, double cotTheta, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null)
            : base(methodId, "shear-demand/capacity", metrics, trace)
        {
            if (direction != 1 && direction != 2) throw new ArgumentOutOfRangeException(nameof(direction));
            CheckValue.Finite(cotTheta, nameof(cotTheta)); Direction = direction; CotTheta = cotTheta;
        }
    }

    [Serializable]
    public sealed class TorsionCheckDetails : CheckDetails
    {
        public double RequiredLongitudinalArea { get; private set; }
        public TorsionCheckDetails(string methodId, double requiredLongitudinalArea, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null)
            : base(methodId, "torsion-and-shear-interaction", metrics, trace)
        {
            CheckValue.Finite(requiredLongitudinalArea, nameof(requiredLongitudinalArea));
            if (requiredLongitudinalArea < 0) throw new ArgumentOutOfRangeException(nameof(requiredLongitudinalArea));
            RequiredLongitudinalArea = requiredLongitudinalArea;
        }
    }

    [Serializable]
    public sealed class ServiceabilityCheckDetails : CheckDetails
    {
        public string CombinationCategory { get; private set; }
        public ServiceabilityCheckDetails(string methodId, string criterion, string combinationCategory,
            IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null,
            IEnumerable<CheckPointValue> points = null, CheckConvergence convergence = null) : base(methodId, criterion, metrics, trace, points, convergence)
        { CheckValue.Text(combinationCategory, nameof(combinationCategory)); CombinationCategory = combinationCategory; }
    }

    [Serializable]
    public sealed class DetailingCheckDetails : CheckDetails
    {
        public DetailingCheckDetails(string methodId, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null)
            : base(methodId, "explicit-detailing-verdict", metrics, trace) { }
    }

    internal static class CheckValue
    {
        internal static void Text(string value, string name) { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A nonempty value is required.", name); }
        internal static void Finite(double? value, string name) { if (value.HasValue && (double.IsNaN(value.Value) || double.IsInfinity(value.Value))) throw new ArgumentOutOfRangeException(name, "A finite value is required."); }
    }
}
