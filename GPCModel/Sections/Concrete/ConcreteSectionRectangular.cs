using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM.Materials;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Concrete
{
	[Serializable]
	public class ConcreteSectionRectangular : SectionRectangular, IConcreteSection
	{
		#region Variables

		protected Elements.Rebar[] _rebars;

		#endregion

		#region Properties

		public Elements.Rebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		#endregion

		#region Public Constructors

		public ConcreteSectionRectangular(double height, double width, ConcreteMaterial material, Elements.Rebar[] rebars, string name = "") 
			: base(height, width, material, name)
		{
			_rebars = rebars;
		}

		public ConcreteSectionRectangular(SectionRectangular section, Elements.Rebar[] rebars)
			: base(section)
		{
			_rebars = rebars;

			if (section.Material is ConcreteMaterial)
			{ }
			else
				throw new ArgumentException("Material must be a ConcreteMaterial");
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
