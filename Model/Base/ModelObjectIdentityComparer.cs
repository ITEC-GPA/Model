using System;
using System.Collections.Generic;

namespace GPC.Model
{
    /// <summary>
    /// Stable entity identity for sets and dictionaries, independent of mutable names and properties.
    /// The legacy Equals/GetHashCode contracts remain available for content comparisons.
    /// Ownership of a live model graph still requires reference equality, not merely equal GUIDs.
    /// </summary>
    [Serializable]
    public sealed class ModelObjectIdentityComparer : IEqualityComparer<ModelObject>
    {
        public static ModelObjectIdentityComparer Instance { get; } = new ModelObjectIdentityComparer();
        private ModelObjectIdentityComparer() { }
        public bool Equals(ModelObject x, ModelObject y) => ReferenceEquals(x, y)
            || !(x is null) && !(y is null) && x.Guid == y.Guid;
        public int GetHashCode(ModelObject obj) => obj is null ? 0 : obj.Guid.GetHashCode();
    }
}
