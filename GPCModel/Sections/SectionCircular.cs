using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionCircular : Section
    {
        #region Variables
        protected double _dext; /// Diameter external
        protected double _t; /// Thickness
        protected double _dint;
        #endregion

        #region Properties
        public double Dext => _dext;
        public double T => _t;
        #endregion

        #region Public Constructors
        public SectionCircular(double dext, double t, Material material) : base(material)
        {
            _dext = dext;
            _t = t;
            _dint = _dext - 2.0 * t;

            _area = (Math.Pow(_dext, 2.0) * Math.PI) / 4.0 - (Math.Pow(_dint, 2.0) * Math.PI) / 4.0;
            _wpl11 = wpl();
            _wpl22 = _wpl11;
        }

        public SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dext = info.GetDouble("Dext");
            _t = info.GetDouble("T");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion

        #region Public Methods Specific
        public double wpl()
        {
            return (Math.Pow(_dext, 3.0) - Math.Pow(_dint, 3.0)) / (6.0);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Dext", _dext);
            info.AddValue("T", _t);
            info.AddValue("Material", _material);
        }

        public override ShapeMaterial[] GetShapes()
        {
            int divisions = 36;
            Polygon2d hole =null;
            Polygon2d fill = new Polygon2d();

            if (Math.Abs(_dint) > 1)
            {
                hole = new Polygon2d();
            }       
            for (int i = 0; i < divisions; i++)
            {
                double teta = i * 2 * Math.PI / divisions;
                fill.Add(new Point2d(0.5 * _dext * Math.Cos(teta), 0.5 * _dext * Math.Sin(teta)));

                if (hole != null)
                {
                    hole.Add(new Point2d(0.5 * _dint * Math.Cos(teta), 0.5 * _dint * Math.Sin(teta)));
                }
            }

            Shape2d shape = new Shape2d(fill, hole != null ? new[] { hole } : null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }
        #endregion
    }
}