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
	public class ConcreteSectionCircular : SectionCircular
	{
		#region Variables

		protected Rebars _rebars;

		#endregion

		#region Properties

		public Rebars Rebars => _rebars;

		#endregion

		#region Public Constructors

		public ConcreteSectionCircular(double diameter, ConcreteMaterial material, Rebars rebars, string name = "")
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


