using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionRectangular : Section
    {
        #region Variables
        protected double _d; /// Height
        protected double _b; /// Width
        #endregion

        #region Properties
        public double D => _d;
        public double B => _b;
        #endregion

        #region Public Constructors

        public SectionRectangular(double b, double d, Material material)
            : base()
        {
            _b = b;
            _d = d;
            _material = material;
        }

        public SectionRectangular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _b = info.GetDouble("B");
            _d = info.GetDouble("D");
            _material = (Material)info.GetValue("Material", typeof(Material));
            info.AddValue("Material", _material);
        }

        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("B", _d);
            info.AddValue("D", _d);
        }

        public override void Calculate()
        {
            base.Calculate();
            _area = _b * _d;
        }

        public override ShapeMaterial[] GetShapes()
        {

            Polygon2d poly = new Polygon2d();
            poly.Add(-0.5 * _b, -0.5 * _d);
            poly.Add(+0.5 * _b, -0.5 * _d);
            poly.Add(+0.5 * _b, +0.5 * _d);
            poly.Add(-0.5 * _b, +0.5 * _d);
            Shape2d shape = new Shape2d(poly, null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }
        #endregion
    }
}