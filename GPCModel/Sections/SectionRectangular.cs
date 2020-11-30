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
        protected double _h; /// Height
        protected double _b; /// Width
        #endregion

        #region Properties
        public double H => _h;
        public double B => _b;
        #endregion

        #region Public Constructors

        public SectionRectangular(double b, double h, Material material)
            : base(material)
        {
            _b = b;
            _h = h;

            _area = _b * _h;

            _j22 = 1.0 / 12.0 * _b * Math.Pow(_h, 3.0);
            _j11 = 1.0 / 12.0 * _h * Math.Pow(_b, 3.0);
            _angleX1 = 0.0;

            _shearCenter = new Point2d(_b / 2.0, _h / 2.0);
            _centroid = _shearCenter;
            
            _wpl22 = _area / 2.0 * _h / 2.0;
            _wpl11 = _area / 2.0 * _b / 2.0;

            _wel22Top = _j22 / (_h / 2.0);
            _wel22Bottom = _j22 / (_h / 2.0);
            _wel11Left = _j11 / (_b / 2.0);
            _wel11Right = _j11 / (_b / 2.0);
        }

        public SectionRectangular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _b = info.GetDouble("B");
            _h = info.GetDouble("H");
            _material = (Material)info.GetValue("Material", typeof(Material));
            info.AddValue("Material", _material);
        }

        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("B", _b);
            info.AddValue("H", _h);
        }

        public override ShapeMaterial[] GetShapes()
        {
            Polygon2d poly = new Polygon2d();
            poly.Add(-0.5 * _b, -0.5 * _h);
            poly.Add(+0.5 * _b, -0.5 * _h);
            poly.Add(+0.5 * _b, +0.5 * _h);
            poly.Add(-0.5 * _b, +0.5 * _h);
            Shape shape = new Shape(poly, null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }
        #endregion
    }
}