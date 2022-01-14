using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the NTC2018 for steel design
	/// </summary>
	/// <remarks>Reference: NTC2018. 17 January 2018</remarks>
	[Serializable]
	public class StandardNTC2018Steel : StandardEN1993p11, ISerializable
	{

		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardNTC2018Steel()
		{
		}
	}
}
