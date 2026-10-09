using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.Checking.Contracts;
using GPC.Model.Core.Coordinates;

namespace GPC.Model.Checking.Reports
{
    public enum CheckApplicability
    {
        Required,
        NotApplicable,
        Excluded
    }

    /// <summary>Optional solver evidence. Null convergence means it was not reported, never inferred convergence.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckConvergence
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Converged>k__BackingField", IsRequired = true)]
        public bool Converged { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Iterations>k__BackingField", IsRequired = true)]
        public int? Iterations { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Residual>k__BackingField", IsRequired = true)]
        public double? Residual { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Tolerance>k__BackingField", IsRequired = true)]
        public double? Tolerance { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Criterion>k__BackingField", IsRequired = true)]
        public string Criterion { get; private set; }

        public CheckConvergence(bool converged, int? iterations, double? residual, double? tolerance, string criterion)
        {
            CheckValue.Text(criterion, nameof(criterion));
            CheckValue.Finite(residual, nameof(residual));
            CheckValue.Finite(tolerance, nameof(tolerance));
            if (iterations < 0 || residual < 0 || tolerance < 0)
                throw new ArgumentOutOfRangeException(nameof(iterations));
            Converged = converged;
            Iterations = iterations;
            Residual = residual;
            Tolerance = tolerance;
            Criterion = criterion;
        }
    }

    /// <summary>Stress/strain or other quantity at a point in the detail's stated local frame; mm for coordinates.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckPointValue
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Key>k__BackingField", IsRequired = true)]
        public string Key { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<X>k__BackingField", IsRequired = true)]
        public double X { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Y>k__BackingField", IsRequired = true)]
        public double Y { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Z>k__BackingField", IsRequired = true)]
        public double Z { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Value>k__BackingField", IsRequired = true)]
        public double Value { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Unit>k__BackingField", IsRequired = true)]
        public string Unit { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Frame>k__BackingField", IsRequired = true)]
        public string Frame { get; private set; }

        public CheckPointValue(string key, double x, double y, double z, double value, string unit, string frame)
        {
            CheckValue.Text(key, nameof(key));
            CheckValue.Text(unit, nameof(unit));
            CheckValue.Text(frame, nameof(frame));
            foreach (var v in new[]
            {
                x,
                y,
                z,
                value
            }

            )
                CheckValue.Finite(v, nameof(value));
            Key = key;
            X = x;
            Y = y;
            Z = z;
            Value = value;
            Unit = unit;
            Frame = frame;
        }
    }

    /// <summary>Value snapshot of the chosen code, edition and actual coefficients; no live Standard reference.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckStandardContext
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Code>k__BackingField", IsRequired = true)]
        public string Code { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Edition>k__BackingField", IsRequired = true)]
        public string Edition { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<NationalAnnex>k__BackingField", IsRequired = true)]
        public string NationalAnnex { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Implementation>k__BackingField", IsRequired = true)]
        public string Implementation { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Parameters>k__BackingField", IsRequired = true)]
        public string Parameters { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Reference>k__BackingField", IsRequired = true)]
        public string Reference { get; private set; }
        public bool HasDeclaredEdition => !string.IsNullOrWhiteSpace(Edition);
        public string Identity => Core.ModelValues.Fingerprint(new object[] { Code, Edition, NationalAnnex, Implementation, Parameters });

        public CheckStandardContext(string code, string edition, string nationalAnnex, string implementation, string parameters, string reference = null)
        {
            CheckValue.Text(code, nameof(code));
            CheckValue.Text(implementation, nameof(implementation));
            Code = code;
            Edition = edition;
            NationalAnnex = nationalAnnex;
            Implementation = implementation;
            Parameters = parameters;
            Reference = reference;
        }
    }

    /// <summary>Demand/capacity and explicit engine verdict. Signed values are retained; utilization, when provided, uses limit 1.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckMetric
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Key>k__BackingField", IsRequired = true)]
        public string Key { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Demand>k__BackingField", IsRequired = true)]
        public double? Demand { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Capacity>k__BackingField", IsRequired = true)]
        public double? Capacity { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Unit>k__BackingField", IsRequired = true)]
        public string Unit { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Utilization>k__BackingField", IsRequired = true)]
        public double? Utilization { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Passed>k__BackingField", IsRequired = true)]
        public bool? Passed { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Reference>k__BackingField", IsRequired = true)]
        public string Reference { get; private set; }

        public CheckMetric(string key, double? demand, double? capacity, string unit, double? utilization = null, bool? passed = null, string reference = null)
        {
            CheckValue.Text(key, nameof(key));
            CheckValue.Text(unit, nameof(unit));
            CheckValue.Finite(demand, nameof(demand));
            CheckValue.Finite(capacity, nameof(capacity));
            CheckValue.Finite(utilization, nameof(utilization));
            if (utilization < 0)
                throw new ArgumentOutOfRangeException(nameof(utilization));
            if (passed.HasValue && utilization.HasValue && passed.Value != (utilization.Value <= 1))
                throw new ArgumentException("The explicit metric verdict disagrees with its normalized utilization.");
            Key = key;
            Demand = demand;
            Capacity = capacity;
            Unit = unit;
            Utilization = utilization;
            Passed = passed;
            Reference = reference;
        }
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckCalculationValue
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Key>k__BackingField", IsRequired = true)]
        public string Key { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Value>k__BackingField", IsRequired = true)]
        public double? Value { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Unit>k__BackingField", IsRequired = true)]
        public string Unit { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Expression>k__BackingField", IsRequired = true)]
        public string Expression { get; private set; }

        public CheckCalculationValue(string key, double? value, string unit, string expression)
        {
            CheckValue.Text(key, nameof(key));
            CheckValue.Finite(value, nameof(value));
            Key = key;
            Value = value;
            Unit = unit;
            Expression = expression;
        }
    }

    /// <summary>Immutable six-component snapshot in N/Nmm. Axes are copied on input and output.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamForceSnapshot
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly CoordinateSystem _axes;
        [field: System.Runtime.Serialization.DataMember(Name = "<N>k__BackingField", IsRequired = true)]
        public double N { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<V1>k__BackingField", IsRequired = true)]
        public double V1 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<V2>k__BackingField", IsRequired = true)]
        public double V2 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<T>k__BackingField", IsRequired = true)]
        public double T { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<M1>k__BackingField", IsRequired = true)]
        public double M1 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<M2>k__BackingField", IsRequired = true)]
        public double M2 { get; private set; }
        public CoordinateSystem Axes => ActionTransformations.AtPoint(_axes, _axes.Origin);

        public BeamForceSnapshot(ResultBeamForces forces)
        {
            if (forces == null)
                throw new ArgumentNullException(nameof(forces));
            foreach (double value in new[]
            {
                forces.N,
                forces.V1,
                forces.V2,
                forces.T,
                forces.M1,
                forces.M2
            }

            )
                CheckValue.Finite(value, nameof(forces));
            global::GPC.Model.Core.Coordinates.Axes.Validate(forces.CoordinateSystem);
            _axes = ActionTransformations.AtPoint(forces.CoordinateSystem, forces.CoordinateSystem.Origin);
            N = forces.N;
            V1 = forces.V1;
            V2 = forces.V2;
            T = forces.T;
            M1 = forces.M1;
            M2 = forces.M2;
        }
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckInputSnapshot
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<SectionFingerprint>k__BackingField", IsRequired = true)]
        public string SectionFingerprint { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SampleFingerprint>k__BackingField", IsRequired = true)]
        public string SampleFingerprint { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ForcesFingerprint>k__BackingField", IsRequired = true)]
        public string ForcesFingerprint { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<BeamForces>k__BackingField", IsRequired = true)]
        public BeamForceSnapshot BeamForces { get; private set; }

        public CheckInputSnapshot(string sectionFingerprint, string sampleFingerprint, string forcesFingerprint, BeamForceSnapshot beamForces)
        {
            CheckValue.Text(sectionFingerprint, nameof(sectionFingerprint));
            CheckValue.Text(sampleFingerprint, nameof(sampleFingerprint));
            CheckValue.Text(forcesFingerprint, nameof(forcesFingerprint));
            SectionFingerprint = sectionFingerprint;
            SampleFingerprint = sampleFingerprint;
            ForcesFingerprint = forcesFingerprint;
            BeamForces = beamForces ?? throw new ArgumentNullException(nameof(beamForces));
        }
    }

    /// <summary>Data-only details. Method and utilization definition prevent comparisons between incompatible criteria.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public abstract class CheckDetails
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly CheckMetric[] _metrics;
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly CheckCalculationValue[] _trace;
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly CheckPointValue[] _points;
        [field: System.Runtime.Serialization.DataMember(Name = "<MethodId>k__BackingField", IsRequired = true)]
        public string MethodId { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<UtilizationDefinition>k__BackingField", IsRequired = true)]
        public string UtilizationDefinition { get; private set; }
        public IReadOnlyList<CheckMetric> Metrics => Array.AsReadOnly(_metrics);
        public IReadOnlyList<CheckCalculationValue> Trace => Array.AsReadOnly(_trace);
        public IReadOnlyList<CheckPointValue> Points => Array.AsReadOnly(_points);

        [field: System.Runtime.Serialization.DataMember(Name = "<Convergence>k__BackingField", IsRequired = true)]
        public CheckConvergence Convergence { get; private set; }

        protected CheckDetails(string methodId, string utilizationDefinition, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace, IEnumerable<CheckPointValue> points = null, CheckConvergence convergence = null)
        {
            CheckValue.Text(methodId, nameof(methodId));
            CheckValue.Text(utilizationDefinition, nameof(utilizationDefinition));
            _metrics = (metrics ?? throw new ArgumentNullException(nameof(metrics))).ToArray();
            _trace = (trace ?? Enumerable.Empty<CheckCalculationValue>()).ToArray();
            _points = (points ?? Enumerable.Empty<CheckPointValue>()).ToArray();
            Convergence = convergence;
            if (_metrics.Any(m => m == null) || _trace.Any(v => v == null) || _points.Any(v => v == null) || _metrics.Select(m => m.Key).Distinct(StringComparer.Ordinal).Count() != _metrics.Length)
                throw new ArgumentException("Metric keys must be unique and detail entries cannot be null.");
            MethodId = methodId;
            UtilizationDefinition = utilizationDefinition;
        }
    }

    /// <summary>The concomitant resistance point in N/Nmm; its components are not independent capacities.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class SectionResistanceDetails : CheckDetails
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<NRd>k__BackingField", IsRequired = true)]
        public double NRd { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<M1Rd>k__BackingField", IsRequired = true)]
        public double M1Rd { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<M2Rd>k__BackingField", IsRequired = true)]
        public double M2Rd { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Demand>k__BackingField", IsRequired = true)]
        public BeamForceSnapshot Demand { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<FailureMode>k__BackingField", IsRequired = true)]
        public string FailureMode { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<StrainReferenceX>k__BackingField", IsRequired = true)]
        public double StrainReferenceX { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<StrainReferenceY>k__BackingField", IsRequired = true)]
        public double StrainReferenceY { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<StrainAtReference>k__BackingField", IsRequired = true)]
        public double StrainAtReference { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ChiX>k__BackingField", IsRequired = true)]
        public double ChiX { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ChiY>k__BackingField", IsRequired = true)]
        public double ChiY { get; private set; }

        public SectionResistanceDetails(string methodId, string criterion, BeamForceSnapshot demand, double nRd, double m1Rd, double m2Rd, double utilization, string failureMode, double referenceX, double referenceY, double strainAtReference, double chiX, double chiY) : base(methodId, criterion, new[] { new CheckMetric("N-M1-M2", null, null, "1", utilization, utilization <= 1) }, null)
        {
            foreach (var value in new[]
            {
                nRd,
                m1Rd,
                m2Rd,
                referenceX,
                referenceY,
                strainAtReference,
                chiX,
                chiY
            }

            )
                CheckValue.Finite(value, nameof(value));
            Demand = demand ?? throw new ArgumentNullException(nameof(demand));
            NRd = nRd;
            M1Rd = m1Rd;
            M2Rd = m2Rd;
            FailureMode = failureMode;
            StrainReferenceX = referenceX;
            StrainReferenceY = referenceY;
            StrainAtReference = strainAtReference;
            ChiX = chiX;
            ChiY = chiY;
        }
    }

    /// <summary>Output contract only; it does not enable a shear implementation.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ShearCheckDetails : CheckDetails
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Direction>k__BackingField", IsRequired = true)]
        public int Direction { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<CotTheta>k__BackingField", IsRequired = true)]
        public double CotTheta { get; private set; }

        public ShearCheckDetails(string methodId, int direction, double cotTheta, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null) : base(methodId, "shear-demand/capacity", metrics, trace)
        {
            if (direction != 1 && direction != 2)
                throw new ArgumentOutOfRangeException(nameof(direction));
            CheckValue.Finite(cotTheta, nameof(cotTheta));
            Direction = direction;
            CotTheta = cotTheta;
        }
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class TorsionCheckDetails : CheckDetails
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<RequiredLongitudinalArea>k__BackingField", IsRequired = true)]
        public double RequiredLongitudinalArea { get; private set; }

        public TorsionCheckDetails(string methodId, double requiredLongitudinalArea, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null) : base(methodId, "torsion-and-shear-interaction", metrics, trace)
        {
            CheckValue.Finite(requiredLongitudinalArea, nameof(requiredLongitudinalArea));
            if (requiredLongitudinalArea < 0)
                throw new ArgumentOutOfRangeException(nameof(requiredLongitudinalArea));
            RequiredLongitudinalArea = requiredLongitudinalArea;
        }
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ServiceabilityCheckDetails : CheckDetails
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<CombinationCategory>k__BackingField", IsRequired = true)]
        public string CombinationCategory { get; private set; }

        public ServiceabilityCheckDetails(string methodId, string criterion, string combinationCategory, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null, IEnumerable<CheckPointValue> points = null, CheckConvergence convergence = null) : base(methodId, criterion, metrics, trace, points, convergence)
        {
            CheckValue.Text(combinationCategory, nameof(combinationCategory));
            CombinationCategory = combinationCategory;
        }
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class DetailingCheckDetails : CheckDetails
    {
        public DetailingCheckDetails(string methodId, IEnumerable<CheckMetric> metrics, IEnumerable<CheckCalculationValue> trace = null) : base(methodId, "explicit-detailing-verdict", metrics, trace)
        {
        }
    }

    internal static class CheckValue
    {
        internal static void Text(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A nonempty value is required.", name);
        }

        internal static void Finite(double? value, string name)
        {
            if (value.HasValue && (double.IsNaN(value.Value) || double.IsInfinity(value.Value)))
                throw new ArgumentOutOfRangeException(name, "A finite value is required.");
        }
    }
}
