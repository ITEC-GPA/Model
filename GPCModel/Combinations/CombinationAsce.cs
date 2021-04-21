using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Combinations
{
    public sealed class CombinationAsce : Combination, IEquatable<CombinationAsce>
    {
        public enum CombinationType
        {
            LFRD,
            ASD
        }

        private CombinationType _combinationType;

        public CombinationType GetCombinationType => _combinationType;

        public CombinationAsce(string name, CombinationType combinationType)
            : base(name)
        {
            this._combinationType = combinationType;
        }


        public CombinationAsce(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _combinationType = (CombinationType)info.GetValue("CombinationType", typeof(CombinationType));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CombinationType", _combinationType);
        }

        public override bool IsUltimate() => _combinationType == CombinationType.LFRD ? true : false;

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CombinationAsce);
        }

        public bool Equals(CombinationAsce other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;
            
            return other != null && base.Equals(other) && _combinationType == other._combinationType;
        }

        public override int GetHashCode()
        {
            var hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _combinationType.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CombinationAsce obj1, CombinationAsce obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(CombinationAsce obj1, CombinationAsce obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
