using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ConcreteSectionCHS : SectionCHS, IConcreteSection
	{
		#region Variables

		protected Elements.Rebar[] _rebars;

		#endregion

		#region Properties

		public Elements.Rebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		#endregion

		#region Public Constructors

		public ConcreteSectionCHS(double diameter, double thickness, ConcreteMaterial material, Elements.Rebar[] rebars, string name = "")
			: base(diameter, thickness, material, name)
		{
			_rebars = rebars;
		}

		public ConcreteSectionCHS(SectionCHS sectionCHS, Elements.Rebar[] rebars)
			: base(sectionCHS)
		{
			_rebars = rebars;

			if (sectionCHS.Material.GetType() != ConcreteMaterial.GetType())
				throw new ArgumentException("Material must be a ConcreteMaterial");
		}

		#endregion

	}
}
