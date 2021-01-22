using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Combinations
{
    public class CombinationAsce : Combination
    {
        public enum CombinationType
        {
            LFRD,
            ASD
        }

        private CombinationType _combinationType;

        public CombinationType GetCombinationType => _combinationType;

        public CombinationAsce(string name, CombinationType combinationType, Guid guid)
            : base(name, guid)
        {
            this._combinationType = combinationType;
        }

        public CombinationAsce(string name, CombinationType combinationType)
            : this(name, combinationType, Guid.NewGuid())
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
    }
}