using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Combinations
{
    public sealed class CombinationAsce : Combination, IEquatable<CombinationAsce>, ICloneable
    {
        #region VARIABLES

        private StandardASCE16 _standardASCE16;

        private StandardASCE16.LimitState _combinationType;

        public StandardASCE16.LimitState GetCombinationType => _combinationType;

        #endregion


        #region PUBLIC CONSTRUCTOR

        public CombinationAsce(string name, StandardASCE16.LimitState combinationType)
            : base(name)
        {
            this._combinationType = combinationType;
        }

        public CombinationAsce(CombinationAsce combination)
            : base(combination)
        {
            this._combinationType = combination._combinationType;
        }


        public CombinationAsce(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _combinationType = (StandardASCE16.LimitState)info.GetValue("CombinationType", typeof(StandardASCE16.LimitState));
        }

        #endregion


        #region PUBLIC OVERRIDE METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CombinationType", _combinationType);
        }

        public override bool IsUltimate() => _combinationType == StandardASCE16.LimitState.LFRD ? true : false;

        public override string ToString()
        {
            return base.ToString();
        }


        public override object Clone()
        {
            return new CombinationAsce(this);
        }

        /// <summary>
        /// Create a new empty <see cref="CombinationAsce"/> object. I.e. with the same properties except the <see cref="Combination.LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        public override object CloneEmpty()
        {
            var cloned = new CombinationAsce(this);
            cloned._coefficients.Clear();

            return cloned;
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

        #endregion

    }
}
