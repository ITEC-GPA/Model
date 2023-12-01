using GPC.Model.Collections;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    [Serializable]
    public class StandardCopSuos2011 : Standard, Standard.ICombinationsGenerator, ISerializable
    {
        #region Public Enum

        /// <summary>
        /// The limit states. Reference: CopSuos2011
        /// </summary>
        public enum LimitStates
        {
            UltimateEquilibrium,
            UltimateStrength,
            UltimateFatigue,
            UltimateIntegrityAndRobustness,
            UltimateFracture,
            Serviceability,
        }

        #endregion

        #region Variables

        private double _gammaM1;
        private double _gammaM2;

        #endregion

        #region Properties

        public double GammaM1 { get => _gammaM1; set => _gammaM1 = value; }

        public double GammaM2 { get => _gammaM2; set => _gammaM2 = value; }

        #endregion
        public override StandardGroupType StandardGroup => StandardGroupType.HongKong;

        #region Public Constructor

        public StandardCopSuos2011(string name = "Cop2011", string remarks = "")
            : base(name, remarks)
        {
            _gammaM1 = 1.0;
            _gammaM2 = 1.2;
        }

        public StandardCopSuos2011(string name = "Cop2011")
            : this(name, "")
        {
        }

        public StandardCopSuos2011()
            : this("Cop2011", "")
        {
        }

        protected StandardCopSuos2011(SerializationInfo info, StreamingContext context)
        {
            _gammaM1 = info.GetDouble("GammaM1");
            _gammaM2 = info.GetDouble("GammaM2");
        }

        #endregion

        public UniqueNameCollection<Combination> CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string name = "cmb")
        {
            throw new NotImplementedException();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("GammaM1", _gammaM1);
            info.AddValue("GammaM2", _gammaM2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is StandardCopSuos2011 suos &&
                   _gammaM1 == suos._gammaM1 &&
                   _gammaM2 == suos._gammaM2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + _gammaM1.GetHashCode();
                hashCode = hashCode * -23 + _gammaM2.GetHashCode();
                return hashCode;
            }
        }
    }
}
