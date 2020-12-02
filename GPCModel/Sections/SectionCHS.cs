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
    public class SectionCHS : Section
    {
        #region Variables
        protected double _dext; /// Diameter external
        protected double _t; /// Thickness
        protected double _dint;
        #endregion

        #region Properties
        public double Dext => _dext;
        public double T => _t;
        public bool IsColdFormed;
        #endregion

        #region Public Constructors
        public SectionCHS(double dext, double t, Material material) : base(material)
        {
            _dext = dext;
            _t = t;
            _dint = _dext - 2.0 * t;

            _area = (Math.Pow(_dext, 2.0) * Math.PI) / 4.0 - (Math.Pow(_dint, 2.0) * Math.PI) / 4.0;

            _j22 = Math.PI * (Math.Pow(_dext, 4.0) - Math.Pow(_dint, 4.0)) / (64.0);
            _j11 = _j22;

            _jt = Math.PI * (Math.Pow(_dext, 4.0) - Math.Pow(_dint, 4.0)) / (32.0);
            _jw = 0;

            _wel22Top = Math.PI * (Math.Pow(_dext, 4.0) - Math.Pow(_dint, 4.0)) / (32.0 * _dext);
            _wel22Bottom = _wel22Top;
            _wel11Left = _wel22Top;
            _wel11Right = _wel22Top;

            _wpl11 = (Math.Pow(_dext, 3.0) - Math.Pow(_dint, 3.0)) / (6.0);
            _wpl22 = _wpl11;

            _centroid = new Point2d(Dext / 2.0, Dext / 2.0);
            _shearCenter = _centroid;

            IsSymmetricAlongYLocalAxis = true;
            IsSymmetricAlongZLocalAxis = true;
        }

        public SectionCHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dext = info.GetDouble("Dext");
            _t = info.GetDouble("T");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion

        #region Public Methods Specific
        public override double MinSigma(double NEd, double M1Ed, double M2Ed)
        {
            double sigmaN = NEd / _area;
            double M = Math.Sqrt(M1Ed * M1Ed + M2Ed * M2Ed);
            double sigmaM = -M / Wel22Min;

            return sigmaN + sigmaM;
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

            Shape shape = new Shape(fill, hole != null ? new[] { hole } : null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }
        #endregion
    }
}