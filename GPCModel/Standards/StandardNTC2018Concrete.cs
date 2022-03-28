using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the NTC2018 for concrete design
	/// "Norme tecniche per	le costruzioni"
	/// </summary>
	/// <remarks>Reference: NTC2018. 17 January 2018</remarks>
	[Serializable]
	public class StandardNTC2018Concrete : StandardEN1992p11, ISerializable
	{

		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardNTC2018Concrete(string name = "NTC2018", string remarks = "Norme tecniche per le costruzioni. 17 January 2018")
			: base(name, remarks)
		{
			_alphaCC = 0.85;
		}

		public StandardNTC2018Concrete(string name = "NTC2018")
			: this(name, "Norme tecniche per le costruzioni. 17 January 2018")
		{
		}

		public StandardNTC2018Concrete()
			: this("NTC2018", "Norme tecniche per le costruzioni. 17 January 2018")
		{
		}

		protected StandardNTC2018Concrete(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		public override bool Equals(object obj)
		{
			return obj is StandardNTC2018Concrete standard && base.Equals(standard);
		}

		public override int GetHashCode()
		{
			return 624022166 + base.GetHashCode();
		}
	}
}
