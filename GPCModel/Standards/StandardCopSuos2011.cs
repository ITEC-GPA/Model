using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        private readonly double _gammaM1;
        private readonly double _gammaM2;

        public double GammaM1 => _gammaM1;

        public double GammaM2 => _gammaM2;

        #endregion
                
        #region Public Constructor

        public StandardCopSuos2011()
        {
            _gammaM1 = 1.0;
            _gammaM2 = 1.2;
        }

        protected StandardCopSuos2011(SerializationInfo info, StreamingContext context)
        {
            _gammaM1 = info.GetDouble("GammaM1");
            _gammaM2 = info.GetDouble("GammaM2");
        }

        #endregion

        public CombinationsCollection CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string name = "cmb")
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
