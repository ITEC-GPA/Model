using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml;

namespace GPC.Model.Core
{
    /// <summary>The text of doubles in archives and copies on .NET Framework, whose XmlConvert does not round every decimal to the
    /// nearest double (about 1.5 in 10,000 computed values written by .NET come back 1 ulp off) and writes "R" strings that do not
    /// always read back the same double. .NET reads and writes them exactly: there ContractXmlReader and ContractXmlWriter keep
    /// XmlConvert. Integer arithmetic only, so the result does not depend on x87 or SSE code.</summary>
    internal static class DoubleText
    {
        // Deciding between two doubles takes at most 768 significant digits; the digits beyond count only as "more than zero".
        private const int MaxDigits = 800;
        private static readonly char[] Blanks = { ' ', '\t', '\n', '\r', '\v', '\f' };
        // The powers of ten of the texts written by .NET (up to 17 digits, exponents to +-324), computed once.
        private static readonly BigInteger[] Powers = PowersOfTen(400);
        private static readonly BigInteger Digits16 = Power(16), Digits17 = Power(17);
        private static BigInteger[] PowersOfTen(int count)
        {
            var powers = new BigInteger[count]; powers[0] = BigInteger.One;
            for (int k = 1; k < count; k++) powers[k] = powers[k - 1] * 10;
            return powers;
        }
        private static BigInteger Power(int k) => k < Powers.Length ? Powers[k] : BigInteger.Pow(10, k);

        /// <summary>The double nearest to the decimal text, ties to even, as .NET reads it. XmlConvert reads the text first: what it
        /// accepts or refuses and its special values (INF, NaN, signed zero) stay as they were.</summary>
        internal static double Parse(string text)
        {
            double read = XmlConvert.ToDouble(text);
            if (double.IsNaN(read) || double.IsInfinity(read)) return read;
            string s = text.Trim(Blanks);
            int i = 0, n = s.Length;
            bool negative = n > 0 && s[0] == '-';
            if (n > 0 && (s[0] == '-' || s[0] == '+')) i++;
            var digits = new StringBuilder(24);
            int exponent = 0; // the value is digits * 10^exponent
            bool any = false, sticky = false;
            for (; i < n && IsDigit(s[i]); i++)
            {
                any = true;
                if (digits.Length == 0 && s[i] == '0') continue;
                if (digits.Length < MaxDigits) digits.Append(s[i]); else { exponent++; sticky |= s[i] != '0'; }
            }
            if (i < n && s[i] == '.')
                for (i++; i < n && IsDigit(s[i]); i++)
                {
                    any = true;
                    if (digits.Length == 0 && s[i] == '0') exponent--;
                    else if (digits.Length < MaxDigits) { digits.Append(s[i]); exponent--; }
                    else sticky |= s[i] != '0';
                }
            if (i < n && (s[i] == 'e' || s[i] == 'E'))
            {
                bool minus = ++i < n && s[i] == '-';
                if (i < n && (s[i] == '-' || s[i] == '+')) i++;
                if (i == n || !IsDigit(s[i])) return read;
                int power = 0;
                for (; i < n && IsDigit(s[i]); i++) if (power < 100000) power = power * 10 + s[i] - '0';
                exponent += minus ? -power : power;
            }
            // Another form than XmlConvert's, or zero: the value it read, with its sign.
            if (!any || i != n || digits.Length == 0) return read;
            if (sticky) { digits.Append('1'); exponent--; }
            // Below half the smallest subnormal (2.5e-324) every value rounds to zero; above the largest double XmlConvert refused it.
            int magnitude = digits.Length + exponent; // the value is below 10^magnitude
            if (magnitude < -330) return negative ? -0.0 : 0.0;
            if (magnitude > 310) return read;
            double value;
            if (exponent >= 0 && magnitude <= 15)
            {
                long integer = long.Parse(digits.ToString(), CultureInfo.InvariantCulture);
                for (int k = 0; k < exponent; k++) integer *= 10;
                value = integer; // below 2^53: exact
            }
            else value = Nearest(digits.Length <= 19 ? new BigInteger(ulong.Parse(digits.ToString(), CultureInfo.InvariantCulture))
                : BigInteger.Parse(digits.ToString(), CultureInfo.InvariantCulture), exponent);
            return negative ? -value : value;
        }

        /// <summary>A text that <see cref="Parse"/> and .NET read back as the same double: that of XmlConvert when it does, otherwise
        /// the 17 significant digits nearest to the value, computed exactly.</summary>
        internal static string Format(double value)
        {
            string text = XmlConvert.ToString(value);
            if (value == 0 || double.IsNaN(value) || double.IsInfinity(value)
                || BitConverter.DoubleToInt64Bits(Parse(text)) == BitConverter.DoubleToInt64Bits(value)) return text;
            long bits = BitConverter.DoubleToInt64Bits(value);
            int biased = (int)(bits >> 52) & 0x7FF;
            long fraction = bits & 0xFFFFFFFFFFFFFL;
            var significand = new BigInteger(biased == 0 ? fraction : fraction | 1L << 52);
            int power2 = (biased == 0 ? 1 : biased) - 1075; // |value| = significand * 2^power2
            int power10 = (int)Math.Floor(Math.Log10(Math.Abs(value))) - 16; // an estimate, corrected below
            BigInteger digits;
            while (true)
            {
                digits = Rounded(significand, power2, power10);
                if (digits >= Digits17) power10++;
                else if (digits < Digits16) power10--;
                else break;
            }
            string d = digits.ToString(CultureInfo.InvariantCulture);
            int e = power10 + 16;
            return (bits < 0 ? "-" : "") + d[0] + "." + d.Substring(1) + "E" + (e < 0 ? "-" : "+") + Math.Abs(e).ToString("00", CultureInfo.InvariantCulture);
        }

        private static bool IsDigit(char c) => c >= '0' && c <= '9';

        // The double nearest to digits * 10^exponent (digits > 0), ties to even, built from its bits.
        private static double Nearest(BigInteger digits, int exponent)
        {
            BigInteger numerator = digits, denominator = BigInteger.One;
            if (exponent >= 0) numerator *= Power(exponent); else denominator = Power(-exponent);
            // A quotient of 54 or 55 bits: the 53 of the significand, the rounding bit and maybe one more.
            int shift = 54 - (BitLength(numerator) - BitLength(denominator));
            if (shift >= 0) numerator <<= shift; else denominator <<= -shift;
            BigInteger quotient = BigInteger.DivRem(numerator, denominator, out var remainder);
            bool sticky = !remainder.IsZero;
            if (BitLength(quotient) > 54) { sticky |= !quotient.IsEven; quotient >>= 1; shift--; }
            // 2^53 <= quotient < 2^54: the value is quotient * 2^-shift plus the sticky rest, of binary exponent 53 - shift.
            int binary = 53 - shift;
            // The rounding bit, and one more bit for each binary order below the normal range (subnormals keep fewer bits).
            int dropped = 1 + Math.Max(0, -1022 - binary);
            if (dropped > 55) return 0;
            BigInteger rest = quotient & ((BigInteger.One << dropped) - 1), half = BigInteger.One << (dropped - 1);
            quotient >>= dropped;
            int c = rest.CompareTo(half);
            if (c > 0 || c == 0 && (sticky || !quotient.IsEven)) quotient += 1;
            long significand = (long)quotient;
            // Subnormal: the significand is the bits, and a carry to 2^52 gives the bits of the smallest normal.
            if (dropped > 1) return BitConverter.Int64BitsToDouble(significand);
            if (significand == 1L << 53) { significand >>= 1; binary++; }
            if (binary > 1023) return double.PositiveInfinity;
            return BitConverter.Int64BitsToDouble(((long)(binary + 1023) << 52) | (significand & 0xFFFFFFFFFFFFFL));
        }

        // significand * 2^power2 / 10^power10, rounded to the nearest integer, ties to even.
        private static BigInteger Rounded(BigInteger significand, int power2, int power10)
        {
            BigInteger numerator = significand, denominator = BigInteger.One;
            if (power2 >= 0) numerator <<= power2; else denominator <<= -power2;
            if (power10 >= 0) denominator *= Power(power10); else numerator *= Power(-power10);
            BigInteger quotient = BigInteger.DivRem(numerator, denominator, out var remainder);
            int c = (remainder << 1).CompareTo(denominator);
            return c > 0 || c == 0 && !quotient.IsEven ? quotient + 1 : quotient;
        }

        private static int BitLength(BigInteger value)
        {
            byte[] bytes = value.ToByteArray();
            int top = bytes.Length - 1;
            while (top > 0 && bytes[top] == 0) top--;
            int bits = top * 8;
            for (int b = bytes[top]; b != 0; b >>= 1) bits++;
            return bits;
        }
    }
}
