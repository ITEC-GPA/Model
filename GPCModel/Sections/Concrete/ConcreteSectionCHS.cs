using GPC.Geometry;
using GPC.Geometry.Meshes;
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

		protected ReinforcedConcreteRebar[] _rebars;
		protected Mesh _mesh;

		#endregion

		#region Properties

		public ReinforcedConcreteRebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		public Shape Shape => GetShape();

		public Mesh Mesh
		{
			get
			{
				if (_mesh == null)
					_mesh = GetReinforcedConcreteSection().Mesh;
				return _mesh;
			}
		}

		#endregion

		#region Public Constructors

		public ConcreteSectionCHS(double diameter, double thickness, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(diameter, thickness, material, name)
		{
			_rebars = rebars;
		}

		public ConcreteSectionCHS(SectionCHS sectionCHS, ReinforcedConcreteRebar[] rebars)
			: base(sectionCHS)
		{
			_rebars = rebars;

			if (sectionCHS.Material.GetType() != ConcreteMaterial.GetType())
				throw new ArgumentException("Material must be a ConcreteMaterial");
		}

		protected Shape GetShape(int edge = 32)
		{
			Polygon3d externalPolygon = ConvertCircleToPolygon(Diameter, edge);
			Polygon3d internalPolygon = ConvertCircleToPolygon(DiameterInternal, edge);

			return new Shape(externalPolygon, new Polygon3d[] { internalPolygon });
		}

		private Polygon3d ConvertCircleToPolygon(double radius, int edge)
		{
			if (edge < 2)
				throw new ArgumentException($"{edge} must be at least 3");

			Point3d[] vertices = new Point3d[edge];
			double teta = 2.0 * Math.PI / edge;

			for (int i = 0; i < edge; i++)
			{
				vertices[i] = new Point3d(radius * Math.Cos(teta * i), radius * Math.Sin(teta * i), 0.0);
			}

			return new Polygon3d(vertices.ToArray());
		}

		protected ReinforcedConcreteSection GetReinforcedConcreteSection()
		{
			return new ReinforcedConcreteSection(new ShapeEx(GetShape(), ConcreteMaterial), Rebars, Name);
		}

		public double GetHomogeneizedJ11()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ22(double n)
		{
			throw new NotImplementedException();
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

		#endregion

	}
}
