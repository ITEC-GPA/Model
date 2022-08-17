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
    public abstract class Standard : ModelObject
    {
        protected string _remarks;

		public string Remarks
		{
			get => _remarks;
			set => _remarks = value;
		}

		public Standard(string name = "", string remarks = "")
            :base(name)
		{
            _remarks = remarks;
		}

		protected Standard(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_remarks = info.GetString("Remarks");
		}


		public void SetName(string name)
		{
			if(name != null)
				_name = name;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Remarks", _remarks);
		}

		public override bool Equals(object obj)
		{
            if (ReferenceEquals(this, obj))
                return true;

            return obj is Standard standard && 
				_remarks.Equals(standard.Remarks) && 
				base.Equals(obj);
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

		#region Nested Class

		public abstract class CombinationsOptions
        {
            public override abstract bool Equals(object obj);

            public override abstract int GetHashCode();
        }

		#endregion
	}
}
