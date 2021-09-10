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

		protected Polygon3d ConvertCircleToPolygon(double radius, int edge)
		{
			Point3d[] vertices = new Point3d[edge];
			double teta = 2.0 * Math.PI / edge;

			for (int i = 0; i < edge; i++)
			{
				vertices[i] = new Point3d(radius * Math.Cos(teta * i), radius * Math.Sin(teta * i), 0.0);
			}

			return new Polygon3d(vertices.ToArray());
		}

		#endregion

		#region Public Methods Override



		#endregion
	}
}


