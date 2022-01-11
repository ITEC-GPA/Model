using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Combinations;

namespace GPC.Model.Standards
{
    public abstract class Standard
    {
        public abstract class CombinationsOptions
        {

            public override abstract bool Equals(object obj);

            public override abstract int GetHashCode();

        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{

		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public interface ICombinationsGenerator
        {
            /// <summary>
            /// Get all the combinations of the loadCaseBase <paramref name="loadCases"/> with the options of generation <paramref name="options"/>
            /// </summary>
            /// <param name="prefix">The common prefix for each combination in the collection</param>
            /// <param name="loadCases">The array of load case base to combine</param>
            /// <param name="options">The options of combinations parameter</param>
            /// <returns>The Combination collections</returns>
            CombinationsCollection CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string prefix = "cmb");
        }
    }
}
