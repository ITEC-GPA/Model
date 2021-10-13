using GPC.Geometry;
using GPC.Geometry.Meshes;
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

		public Shape Shape => GetShape();

		public Mesh Mesh => throw new NotImplementedException();

		#endregion

		#region Public Constructors

		public ConcreteSectionCircular(double diameter, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(diameter, material, name)
		{
			_rebars = rebars;
		}

		public double GetHomogenizedArea(double n)
		{
			throw new NotImplementedException();
		}

		public double GetHomogenizedArea()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ11(double n)
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ22()
		{
			throw new NotImplementedException();
		}

		private Shape GetShape()
		{
			Polygon3d poly = ConvertCircleToPolygon();
			return new Shape(poly);
		}

		public double GetHomogeneizedJ11()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ22(double n)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}


