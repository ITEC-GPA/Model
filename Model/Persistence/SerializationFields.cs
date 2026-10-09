using System;
using System.Runtime.Serialization;

namespace GPC.Model
{
    internal static class SerializationFields
    {
        // Missing legacy fields only. Malformed present fields must not be silently replaced.
        public static bool Has(SerializationInfo info, string name)
        {
            foreach (SerializationEntry entry in info) if (entry.Name == name) return true;
            return false;
        }

        public static T Read<T>(SerializationInfo info, string name, T legacyDefault = default(T))
        {
            return Has(info, name) ? (T)info.GetValue(name, typeof(T)) : legacyDefault;
        }
    }

    internal static class NumericGuard
    {
        public static double Finite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(name, "A finite value is required.");
            return value;
        }
        public static double Station(double value)
        {
            Finite(value, nameof(value));
            if (value < 0 || value > 1) throw new ArgumentOutOfRangeException(nameof(value), "Station must be in [0,1].");
            return value;
        }
    }
}
