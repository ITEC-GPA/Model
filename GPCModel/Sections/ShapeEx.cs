using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
	public class ShapeEx : Shape
	{
		protected Material _material;

		public Material Material => _material;


		public ShapeEx(Polygon3d fill, Material material, Polygon3d[] holes = null, ShapeEx[] childs = null, double tolerance = 0.0001) 
			: base(fill, holes, childs, tolerance)
		{
			_material = material;
		}

		public ShapeEx(Polygon3d fill, Material material, Polygon3d[] holes = null, ShapeEx[] childs = null)
			: this(fill, material, holes, childs, Geometry.GeometryBase.GetDefaultAngularTolerance())
		{
		}

		public ShapeEx(Shape shape, Material material, double tolerance = 0.0001) 
			: base(shape, tolerance)
		{
			_material = material;
		}

		public ShapeEx(Shape shape, Material material)
			: this(shape, material, Geometry.GeometryBase.GetDefaultAngularTolerance())
		{
		}
	}
}
