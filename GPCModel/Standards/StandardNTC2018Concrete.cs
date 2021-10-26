using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the NTC2018 for concrete design
	/// </summary>
	public class StandardNTC2018Concrete : StandardEN1992p11
	{

		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardNTC2018Concrete()
		{
			_alphaCC = 0.85;
		}
	}
}
