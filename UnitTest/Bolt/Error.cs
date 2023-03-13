using System;

namespace GPC.Utilities.Maths
{
    public static class Error
    {
        public const double DoublePrecision = 1e-14;

        public static double CalcAbsoluteError(in double Measured, in double Real) => Measured - Real;

        public static double CalcRelativeError(in double Measured, in double Real)
        {
            if (Real == 0)
            {
                if (Measured == 0)
                    return 0;
                else
                    return CalcAbsoluteError(Measured, Real);
            }
            return CalcAbsoluteError(Measured, Real) / Real;
        }

        public static double TwoValuesRelativeError(in double A, in double B, in double zero = DoublePrecision)
        {
            if (Math.Abs(A) <= zero || Math.Abs(B) <= zero)
                return Math.Abs(CalcAbsoluteError(A, B));
            else
                return Math.Abs(CalcRelativeError(A, 0.5 * (A + B)));
        }

        public static bool AreEqualsDouble(in double A, in double B, in double tollerance = DoublePrecision) => TwoValuesRelativeError(A, B) < tollerance;
    }
}
