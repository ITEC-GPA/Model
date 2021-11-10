using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
			: this(fill, material, holes, childs, GeometryBase.GetDefaultAngularTolerance())
		{
		}

		public ShapeEx(Shape shape, Material material, double tolerance = 0.0001) 
			: base(shape, tolerance)
		{
			_material = material;
		}

		public ShapeEx(Shape shape, Material material)
			: this(shape, material, GeometryBase.GetDefaultAngularTolerance())
		{
		}

		public ShapeEx(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			_material = (Material)info.GetValue("Material", typeof(Material));
		}



		#region Field Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Material", _material);
		}

		#endregion
	}
}
