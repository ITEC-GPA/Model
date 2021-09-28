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

		protected ReinforcedConcreteRebar[] _rebars;

		#endregion

		#region Properties

		public ReinforcedConcreteRebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		#endregion

		#region Public Constructors

		public ConcreteSectionCircular(double diameter, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(diameter, material, name)
		{
			_rebars = rebars;
		}

		#endregion

	}
}


