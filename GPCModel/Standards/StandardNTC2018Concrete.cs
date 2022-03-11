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

		protected StandardNTC2018Concrete(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		public override bool Equals(object obj)
		{
			return obj is StandardNTC2018Concrete code &&
				   _gammaC == code._gammaC &&
				   _gammaCAccidental == code._gammaCAccidental &&
				   _gammaCE == code._gammaCE &&
				   _gammaS == code._gammaS &&
				   _gammaSAccidental == code._gammaSAccidental &&
				   _gammaSPrestress == code._gammaSPrestress &&
				   _gammaSPrestressAccidental == code._gammaSPrestressAccidental &&
				   _alphaCC == code._alphaCC &&
				   _alphaCT == code._alphaCT &&
				   _steelCoefficientStrainTension == code._steelCoefficientStrainTension &&
				   _gammaF == code._gammaF;
		}

		public override int GetHashCode()
		{
			return 624022166 + base.GetHashCode();
		}
	}
}
