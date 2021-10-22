using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.FEM.Materials;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Concrete
{
	[Serializable]
	public class ConcreteSectionRectangular : SectionRectangular, IConcreteSection
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

		public ConcreteSectionRectangular(double height, double width, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "") 
			: base(height, width, material, name)
		{
			_rebars = rebars;


		}

		public ConcreteSectionRectangular(SectionRectangular section, ReinforcedConcreteRebar[] rebars)
			: this(section.Height, section.Width, (ConcreteMaterial)section.Material, rebars, section.Name)
		{
			if (section.Material.GetType() != ConcreteMaterial.GetType())
				throw new ArgumentException("Material must be a ConcreteMaterial");
		}

		#endregion

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

		public Shape GetShape()
		{
			return new Shape(new Polygon3d(new Point3d[] { new Point3d(0, 0, 0), new Point3d(Width, 0, 0), new Point3d(Width, Height, 0), new Point3d(0, Height, 0) }));
		}

		public double GetHomogeneizedJ11()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ22(double n)
		{
			throw new NotImplementedException();
		}

		protected ReinforcedConcreteSection GetReinforcedConcreteSection()
		{
			return new ReinforcedConcreteSection(new ShapeEx(GetShape(), ConcreteMaterial), Rebars, Name);
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
