using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
    public class StandardCopSuos2011 : Standard
    {
        #region PUBLIC ENUMS        

        /// <summary>
        /// The limit states. Reference: CopSuos2011
        /// </summary>
        public enum LimitStates
        {
            UltimateEquilibrium,
            UltimateStructural,            
            UltimateFatigue,            
            Serviceability,           
        }

        #endregion


        #region VARIABLES

        private double _gammaM1;
        private double _gammaM2;

        public double GammaM1 => _gammaM1;
        public double GammaM2 => _gammaM2;

        #endregion


        #region PUBLIC CONSTRUCTOR

        public StandardCopSuos2011()
        {
            _gammaM1 = 1.0;
            _gammaM2 = 1.2;
        }

        #endregion
        // TODO: sistemare StandardCopSuos2011

        public override CombinationsCollection CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string name = "cmb")
        {
            throw new NotImplementedException();
        }
    }
}
