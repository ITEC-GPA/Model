using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ConcreteSectionCircular : SectionCircular, IConcreteSection
	{
		#region Variables

		protected Elements.Rebar[] _rebars;

		#endregion

		#region Properties

		public Elements.Rebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		public double Height => Diameter;

		#endregion

		#region Public Constructors

		public ConcreteSectionCircular(double diameter, ConcreteMaterial material, Elements.Rebar[] rebars, string name = "")
			: base(diameter, material, name)
		{
			_rebars = rebars;
		}

		#endregion


		#region Public Methods Specific

		#endregion

		#region Private Methods Specific

		#endregion

		#region Public Methods Override



		#endregion
	}
}


