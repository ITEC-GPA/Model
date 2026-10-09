using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restraints;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;

namespace GPC.Model.Structure.Assignments
{
    /// <summary>DOF order DX,DY,DZ,RX,RY,RZ; forces Fx,Fy,Fz,Mx,My,Mz. Canonical N,mm,rad.
    /// Entries have units row action / column displacement: N/mm, N/rad, Nmm/mm, Nmm/rad.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class SpringMatrix
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private double[] _values;
        [field: System.Runtime.Serialization.DataMember(Name = "<CoordinateSystem>k__BackingField", IsRequired = true)]
        public CoordinateSystem CoordinateSystem { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Kind>k__BackingField", IsRequired = true)]
        public SpringKind Kind { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }

        public SpringMatrix(double[] rowMajor, CoordinateSystem axes, SpringKind kind = SpringKind.SymmetricElastic)
        {
            _values = rowMajor == null ? null : (double[])rowMajor.Clone();
            CoordinateSystem = axes;
            Kind = kind;
            Validate();
        }

        public double this[int row, int column] => _values[row * 6 + column];
        public double[] ToArray() => (double[])_values.Clone();
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) => Validate();
        private void Validate()
        {
            if (_values == null || _values.Length != 36)
                throw new ArgumentException("A spring requires all 36 coefficients.");
            foreach (double v in _values)
                NumericGuard.Finite(v, "stiffness");
            Axes.Validate(CoordinateSystem);
        }

        public IReadOnlyList<ModelDiagnostic> Diagnose()
        {
            var issues = new List<ModelDiagnostic>();
            if (Kind != SpringKind.SymmetricElastic && Kind != SpringKind.GeneralLinear)
                issues.Add(ModelDiagnostic.Error("UnsupportedSpringLaw"));
            if (Kind == SpringKind.SymmetricElastic)
            {
                bool symmetric = true;
                for (int r = 0; r < 6; r++)
                    for (int c = r + 1; c < 6; c++)
                        if (Math.Abs(this[r, c] - this[c, r]) > 1e-10 * Math.Max(1, Math.Max(Math.Abs(this[r, c]), Math.Abs(this[c, r]))))
                            symmetric = false;
                if (!symmetric)
                    issues.Add(ModelDiagnostic.Error("NonSymmetricSpring"));
                else
                {
                    var m = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.Create(6, 6, (r, c) => this[r, c]);
                    if (m.Evd(MathNet.Numerics.LinearAlgebra.Symmetricity.Symmetric).EigenValues.Any(v => v.Real < -1e-10))
                        issues.Add(ModelDiagnostic.Error("NonPositiveSpring"));
                }
            }

            return issues;
        }

        public double[] Apply(double[] displacement)
        {
            if (displacement == null || displacement.Length != 6)
                throw new ArgumentException("Six displacements are required.");
            if (Diagnose().Count != 0)
                throw new NotSupportedException("Spring is preserved but not eligible for linear evaluation.");
            var result = new double[6];
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 6; c++)
                    result[r] += this[r, c] * NumericGuard.Finite(displacement[c], "displacement");
            return result;
        }

        public SpringMatrix ToCoordinateSystem(CoordinateSystem target)
        {
            Axes.Validate(target);
            if (Axes.Length(target.Origin - CoordinateSystem.Origin) > 1e-8)
                throw new NotSupportedException("Spring rotation requires the same point.");
            var oldAxes = new[]
            {
                CoordinateSystem.V1,
                CoordinateSystem.V2,
                CoordinateSystem.V3
            };
            var newAxes = new[]
            {
                target.V1,
                target.V2,
                target.V3
            };
            var q = new double[6, 6];
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 6; c++)
                    if (r / 3 == c / 3)
                        q[r, c] = Axes.Dot(newAxes[r % 3], oldAxes[c % 3]);
            var result = new double[36];
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 6; c++)
                    for (int i = 0; i < 6; i++)
                        for (int j = 0; j < 6; j++)
                            result[r * 6 + c] += q[r, i] * this[i, j] * q[c, j];
            return new SpringMatrix(result, target, Kind)
            {
                SourceRecord = SourceRecord
            };
        }
    }
}
