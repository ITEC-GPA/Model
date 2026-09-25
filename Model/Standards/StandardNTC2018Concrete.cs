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

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardNTC2018Concrete(string name = "NTC2018")
			: this(name, "Norme tecniche per le costruzioni. 17 January 2018")
		{
		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardNTC2018Concrete()
			: this("NTC2018", "Norme tecniche per le costruzioni. 17 January 2018")
		{
		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardNTC2018Concrete(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		/// <summary>
		/// Equality with an object of the same type
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is equal</returns>
		public override bool Equals(object obj)
		{
			return obj is StandardNTC2018Concrete standard && base.Equals(standard);
		}

		/// <summary>
		/// The hash code of the coefficients and of the base
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return 624022166 + base.GetHashCode();
		}
	}
}
