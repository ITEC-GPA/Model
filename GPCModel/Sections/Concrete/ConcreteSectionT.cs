using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ConcreteSectionT : SectionT, IConcreteSection
	{
		#region Variables

		protected Elements.Rebar[] _rebars;

		#endregion

		#region Properties

		public Elements.Rebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		#endregion

		#region Public Constructors

		public ConcreteSectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, ConcreteMaterial material, Elements.Rebar[] rebars, string name = "")
			: base(height, flangeLength, thicknessWeb, thicknessFlange, material, name)
		{
			_rebars = rebars;
		}

		public ConcreteSectionT(SectionT section, Elements.Rebar[] rebars)
			: base(section)
		{
			_rebars = rebars;

			if (section.Material is ConcreteMaterial)
			{ }
			else
				throw new ArgumentException("Material must be a ConcreteMaterial");
		}

		#endregion
	}
}
