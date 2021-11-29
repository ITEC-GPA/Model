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
    public class ShapeEx : Shape2d, ISection
    {

        protected Material _material;

        public Material Material => _material;

        public virtual Shape2d Shape => this;


        public ShapeEx(Polygon2d fill, Material material, Polygon2d[] holes = null, ShapeEx[] childs = null, double tolerance = GeometryBase.Tolerance)
            : base(fill, holes, childs, tolerance)
        {
            _material = material;
        }

        public ShapeEx(Shape2d shape, Material material, double tolerance = GeometryBase.Tolerance)
            : base(shape, tolerance)
        {
            _material = material;
        }

        public ShapeEx(Shape2d shape, Material material)
            : this(shape, material, GeometryBase.Tolerance)
        {
        }

        public ShapeEx(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _material = (Material)info.GetValue("Material", typeof(Material));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Material", _material);
        }

        public Shape2d GetShape()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            return obj is ShapeEx ex &&
                   base.Equals(obj) &&
                   _material.Equals(ex._material);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _material.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ShapeEx left, ShapeEx right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShapeEx left, ShapeEx right)
        {
            return !(left == right);
        }
    }
}
