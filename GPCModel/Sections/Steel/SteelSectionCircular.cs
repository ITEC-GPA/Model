using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Steel
{
	public class SteelSectionCircular : SectionCircular
	{
		public SteelSectionCircular(double diameter, SteelMaterial material, string name) 
			: base(diameter, material, name)
		{
		}

		public double CalculateWpl()
		{
			return Math.Pow(_diameter, 3.0) / 6.0;
		}

		public double CalculateJ()
		{
			return Math.Pow(_diameter, 4.0) / 64.0;
		}

		public double CalculateJp()
		{
			return Math.Pow(_diameter, 4.0) / 32.0;
		}

		public double CalculateJw()
		{
			return 0.0;
			//TODO: implementare
		}

		public double CalculateJt()
		{
			return 0.0;
			//TODO: implementare
		}
	}
}
