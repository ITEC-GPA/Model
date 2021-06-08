using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Combinations
{
    public abstract class Standard
    {
        public abstract class CombinationsOptions
        {

            public override abstract bool Equals(object obj);

            public override abstract int GetHashCode();

        }

        /// <summary>
        /// Get all the combinations of the loadCaseBase <paramref name="loadCases"/> with the options of generation <paramref name="options"/>
        /// </summary>
        /// <param name="name">The name of the collection of combinations</param>
        /// <param name="loadCases">The array of load case base to combine</param>
        /// <param name="options">The options of combinations parameter</param>
        /// <returns>The Combination collections</returns>
        public abstract CombinationsCollection CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string name = "cmb");


    }
}
