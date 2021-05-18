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
using GPC.Model.FEM.Materials;

namespace GPC.Model.Sections
{
    /*
    public class SectionCircular : Section
    {        
        #region Variables
        protected double _dext; /// Diameter external
        #endregion

        #region Properties
        public double Dext => _dext;
        #endregion

        #region Public Constructors
        public SectionCircular(double dext, double t, Material material, string name) : base(material.GetIsotropicFemMaterial(), name)
        {
            _dext = dext;

            _area = (Math.Pow(_dext, 2.0) * Math.PI) / 4.0;
            _wpl11 = wpl();
            _wpl22 = _wpl11;
        }

        public SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dext = info.GetDouble("Dext");
            _material = (IsotropicFemMaterial)info.GetValue("Material", typeof(IsotropicFemMaterial));
        }

        #endregion

        #region Public Methods Specific
        public double wpl()
        {
            return Math.Pow(_dext, 3.0) / 6.0;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Dext", _dext);
            info.AddValue("Material", _material);
        }

        public override ShapeMaterial[] GetShapes()
        {
            int divisions = 36;
            Polygon2d hole =null;
            Polygon2d fill = new Polygon2d();
    
            for (int i = 0; i < divisions; i++)
            {
                double teta = i * 2 * Math.PI / divisions;
                fill.Add(new Point2d(0.5 * _dext * Math.Cos(teta), 0.5 * _dext * Math.Sin(teta)));
            }

            Shape shape = new Shape(fill, hole != null ? new[] { hole } : null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }
        #endregion
        
    }
    */
}