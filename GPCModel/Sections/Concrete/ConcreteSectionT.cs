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
	public class ConcreteSectionT : SectionT, IConcreteSection
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

		public ConcreteSectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(height, flangeLength, thicknessWeb, thicknessFlange, material, name)
		{
			_rebars = rebars;
		}

		public ConcreteSectionT(SectionT section, ReinforcedConcreteRebar[] rebars)
			: base(section)
		{
			_rebars = rebars;

			if (section.Material.GetType() != ConcreteMaterial.GetType())
				throw new ArgumentException("Material must be a ConcreteMaterial");
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

		public double GetHomogeneizedJ11()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ22(double n)
		{
			throw new NotImplementedException();
		}

		#endregion

		protected ReinforcedConcreteSection GetReinforcedConcreteSection()
		{
			return new ReinforcedConcreteSection(new ShapeEx(GetShape(), ConcreteMaterial), Rebars, Name);
		}

		public Shape GetShape()
		{
			return new Shape(new Polygon3d(new Point3d[] { new Point3d(0.0, Height, 0.0), new Point3d(LenghtFlange, Height, 0.0), 
				new Point3d(LenghtFlange, HeightWeb, 0.0), new Point3d(LenghtFlange / 2.0 + ThicknessWeb / 2.0 , HeightWeb, 0.0),
				new Point3d(LenghtFlange / 2.0 + ThicknessWeb / 2.0 , 0.0, 0.0), new Point3d(LenghtFlange / 2.0 - ThicknessWeb / 2.0 , 0.0, 0.0),
				new Point3d(LenghtFlange / 2.0 - ThicknessWeb / 2.0 , HeightWeb, 0), new Point3d(0.0 , HeightWeb, 0) }));
		}

		public virtual double CalculateN(ReinforcedConcreteRebar rebar)
		{
			return rebar.RebarMaterial.E / Material.E;
		}

		public virtual double CalculateN(int rebar)
		{
			return Rebars[rebar].RebarMaterial.E / Material.E;
		}
	}
}
