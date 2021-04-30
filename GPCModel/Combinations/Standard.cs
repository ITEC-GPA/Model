using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Combinations
{
    public abstract class Standard
    {
        public abstract class CombinationsOptions
        {
        }

        /* TODO: da mettere in Combination
        protected CombinationsOptions _combinationsOptions;

        public CombinationsOptions Options
        {
            get => _combinationsOptions;
            protected set => _combinationsOptions = value;
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadCases"></param>
        /// <param name="options"></param>
        /// <returns>The Combination collections</returns>
        //public abstract CombinationsCollection CreateCombinations<T>(LoadCaseBase[] loadCases, T options) where T : CombinationsOptions;
        public abstract CombinationsCollection CreateCombinations(string name, LoadCaseBase[] loadCases, CombinationsOptions options);
    }
}
