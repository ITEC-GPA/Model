using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GPC.Model
{
    internal sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
        public bool Equals(T x, T y) => ReferenceEquals(x, y);
        public int GetHashCode(T value) => RuntimeHelpers.GetHashCode(value);
    }
}
