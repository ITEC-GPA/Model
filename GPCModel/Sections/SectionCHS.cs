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
        protected double _d; /// Diameter external
        protected double _t; /// Thickness
        protected double _dint;
        protected bool _isHotFinished;
        #endregion

        #region Properties
        public double D => _d;
        public double T => _t;
        public bool IsColdFormed { get => !_isHotFinished; set { _isHotFinished = !value; } }
        public bool IsHotFinished { get => _isHotFinished; set { _isHotFinished = value; } }
        #endregion

        #region Public Constructors
        public SectionCHS(double dext, double t, Material material, string name, bool isColdFormed = true) : base(material, name)
        {
            #region check_inputs
            if (t > dext/2.0)
            {
                dext = 0;
                t = 0;
                return;
            }
            if (t < 0 || dext < 0)
            {
                dext = 0;
                t = 0;
                return;
            }
            #endregion

            _d = dext;
            _t = t;
            _dint = _d - 2.0 * t;

            _area = (Math.Pow(_d, 2.0) * Math.PI) / 4.0 - (Math.Pow(_dint, 2.0) * Math.PI) / 4.0;

            _j22 = Math.PI * (Math.Pow(_d, 4.0) - Math.Pow(_dint, 4.0)) / (64.0);
            _j11 = _j22;

            _jt = Math.PI * (Math.Pow(_d, 4.0) - Math.Pow(_dint, 4.0)) / (32.0);
            _jw = 0;

            _wel22Top = Math.PI * (Math.Pow(_d, 4.0) - Math.Pow(_dint, 4.0)) / (32.0 * _d);
            _wel22Bottom = _wel22Top;
            _wel11Left = _wel22Top;
            _wel11Right = _wel22Top;

            _wpl11 = (Math.Pow(_d, 3.0) - Math.Pow(_dint, 3.0)) / (6.0);
            _wpl22 = _wpl11;

            _centroid = new Point2d(_d / 2.0, _d / 2.0);
            _shearCenter = _centroid;

            IsSymmetricAlongYLocalAxis = true;
            IsSymmetricAlongZLocalAxis = true;
            IsColdFormed = isColdFormed;
        }

        public SectionCHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _d = info.GetDouble("D");
            _t = info.GetDouble("T");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion

        #region Public Methods Specific
        public override double MinSigma(double NEd, double M2, double M1)
        {
            double sigmaN = NEd / _area;
            double M = Math.Sqrt(M1 * M1 + M2 * M2);
            double sigmaM = -M / Wel22Min;

            return sigmaN + sigmaM;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("D", _d);
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
                fill.Add(new Point2d(0.5 * _d * Math.Cos(teta), 0.5 * _d * Math.Sin(teta)));

                if (hole != null)
                {
                    hole.Add(new Point2d(0.5 * _dint * Math.Cos(teta), 0.5 * _dint * Math.Sin(teta)));
                }
            }

            Shape shape = new Shape(fill, hole != null ? new[] { hole } : null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }
        #endregion

        public override string ToString()
        {
            string s = "CHS section: \n";
            s = s + "D = " + _d + " mm \n";
            s = s + "t = " + _t + " mm \n";
            return s;
        }
    }
}