using System;
using System.Runtime.Serialization;

namespace GPC.Model.Combinations
{
    public class CombinationEn : Combination
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
    }
}