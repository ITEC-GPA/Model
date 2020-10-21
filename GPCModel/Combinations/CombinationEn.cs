using System;
using System.Runtime.Serialization;

namespace GPC.Model.Combinations
{
    public class CombinationEn : ModelObject, ICombination
    {
        private CombinationType _combinationType;
        private string _name;

        public CombinationType GetCombinationType => _combinationType;
        public string Name => _name;

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

        protected CombinationEn(CombinationType combinationType, Guid guid) 
            : base(guid)
        {
            this._combinationType = combinationType;
        }

        protected CombinationEn(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _combinationType = (CombinationType)info.GetValue("CombinationType", typeof(CombinationType));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CombinationType", _combinationType);
        }

        public bool isUltimate =>   (_combinationType == CombinationType.UltimateEquilibrium || 
                                    _combinationType == CombinationType.UltimateFatigue || 
                                    _combinationType == CombinationType.UltimateGeotechnical || 
                                    _combinationType == CombinationType.UltimateStructural) ? 
                                    true : false;
    }
}
