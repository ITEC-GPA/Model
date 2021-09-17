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

		public SteelSectionCircular(SectionCircular sectionCircular)
			:base(sectionCircular)
		{
			if (sectionCircular.Material is SteelMaterial)
			{ }
			else
				throw new ArgumentException("Material must be SteelMaterial");
		}

	}
}
