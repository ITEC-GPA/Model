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
        protected double _dint; /// Diameter internal
        #endregion

        #region Properties
        public double Dext => _dext;
        public double Dint => _dint;
        #endregion

        #region Public Constructors

        public SectionCircular(double dext, double dint, Material material)
            : base()
        {
            _dext = dext;
            _dint = dint;
            _material = material;
        }

        public SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dext = info.GetDouble("Dext");
            _dint = info.GetDouble("Dint");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Dext", _dext);
            info.AddValue("Dint", _dint);
            info.AddValue("Material", _material);
        }

        public override void Calculate()
        {
            base.Calculate();
            _area = (Math.Pow(_dext, 2.0) * Math.PI) / 4.0 - (Math.Pow(_dint,2.0) *Math.PI)/4.0;
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