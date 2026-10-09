using System;

namespace GPC.Model.PostProcessing
{
    /// <summary>Explicit factors to N/mm/rad, allowing mixed units for different exported columns.</summary>
    public sealed class ResultUnits
    {
        public double ForceToN { get; }
        public double LengthToMm { get; }
        public double MomentToNmm { get; }
        public double AngleToRad { get; }
        public ResultUnits(double forceToN, double lengthToMm, double momentToNmm, double angleToRad = 1)
        {
            foreach (var v in new[] { forceToN, lengthToMm, momentToNmm, angleToRad })
                if (NumericGuard.Finite(v, "unitFactor") <= 0) throw new ArgumentOutOfRangeException("unitFactor");
            ForceToN = forceToN; LengthToMm = lengthToMm; MomentToNmm = momentToNmm; AngleToRad = angleToRad;
        }
        private static double Scale(double value, double factor) => NumericGuard.Finite(NumericGuard.Finite(value, "value") * factor, "convertedValue");
        public double Force(double value) => Scale(value, ForceToN);
        public double Length(double value) => Scale(value, LengthToMm);
        public double Angle(double value) => Scale(value, AngleToRad);
        public double Area(double value) => Scale(value, LengthToMm * LengthToMm);
        public double GeometricInertia(double value) => Scale(value, Math.Pow(LengthToMm, 4));
        public double Stress(double value, double denominatorLengthToMm) => Scale(value, ForceToN / Math.Pow(Positive(denominatorLengthToMm), 2));
        public double BeamMoment(double value) => Scale(value, MomentToNmm);
        public double ShellMoment(double value, double denominatorLengthToMm) => BeamMoment(value) / Positive(denominatorLengthToMm);
        public double ForcePerLength(double value, double denominatorLengthToMm) => Scale(value, ForceToN / Positive(denominatorLengthToMm));
        private static double Positive(double value) => NumericGuard.Finite(value, "lengthFactor") > 0 ? value : throw new ArgumentOutOfRangeException("lengthFactor");
        public double[] Spring(double[] matrix)
        {
            if (matrix == null || matrix.Length != 36) throw new ArgumentException("36 coefficients required.");
            var result = new double[36];
            for (int r = 0; r < 6; r++) for (int c = 0; c < 6; c++) result[r * 6 + c] = Scale(matrix[r * 6 + c],
                (r < 3 ? ForceToN : MomentToNmm) / (c < 3 ? LengthToMm : AngleToRad));
            return result;
        }
    }
}
