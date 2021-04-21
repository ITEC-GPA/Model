using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Combinations
{
    public sealed class CombinationEn : Combination, IEquatable<CombinationEn>, ICloneable
    {
        public enum CombinationType
        {
            UltimateEquilibrium,
            UltimateStructural,
            UltimateGeotechnical,
            UltimateFatigue,
            ServiceabilityCharacteristic,
            ServiceabilityFrequent,
            ServiceabilityQuasiPermanent
        }

        private CombinationType _combinationType;

        public CombinationType GetCombinationType => _combinationType;

        public CombinationEn(string name, CombinationType combinationType)
            : base(name)
        {
            this._combinationType = combinationType;
        }


        public CombinationEn(CombinationEn combinationEn)
            : base(combinationEn)
        {
            this._combinationType = combinationEn._combinationType;
        }


        public CombinationEn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _combinationType = (CombinationType)info.GetValue("CombinationType", typeof(CombinationType));
        }



        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CombinationType", _combinationType);
        }

        public override bool IsUltimate() => (_combinationType == CombinationType.UltimateEquilibrium ||
                                             _combinationType == CombinationType.UltimateFatigue ||
                                             _combinationType == CombinationType.UltimateGeotechnical ||
                                             _combinationType == CombinationType.UltimateStructural) ?
                                             true : false;

        public override string ToString()
        {
            return base.ToString();
        }



        public override object Clone()
        {
            return new CombinationEn(this);
        }

        /// <summary>
        /// Create a new empty <see cref="CombinationEn"/> object. I.e. with the same properties except the <see cref="Combination.LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        public override object CloneEmpty()
        {
            var cloned = new CombinationEn(this);
            cloned._coefficients.Clear();

            return cloned;
        }



        public override bool Equals(object obj)
        {
            return Equals(obj as CombinationEn);
        }

        public bool Equals(CombinationEn other)
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

        public static bool operator ==(CombinationEn obj1, CombinationEn obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(CombinationEn obj1, CombinationEn obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
