using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.FEM.Properties;
using GPC.Model.Elements;

namespace GPC.Model.Sections
{
    public class Section : ElementProperty
    {
        public struct ShapeMaterial
        {
            public Shape Shape { get; set; }
            public Material Material { get; set; }
        }

        #region Variables
        protected Material _material;
        protected double _area;
        protected double _j11;
        protected double _j22;
        protected double _wpl11;
        protected double _wpl22;
        protected double _wel11Left; //Wel calcolato per punto più a snistra
        protected double _wel22Top; //Wel calcolato per punto superiore (+ alto)
        protected double _wel11Right; //Wel calcolato per punto più a destra
        protected double _wel22Bottom; //Wel calcolato per punto inferiore (+ basso)
        protected double _jt;
        protected double _jw;
        protected Point2d _shearCenter;
        protected Point2d _centroid;
        protected double _angleX1;
        #endregion

        #region Properties

        public Material Material
        {
            get => _material;
            set => _material = value;
        }

        public double Area
        {
            get => _area;
            set => _area = value;
        }
        public double Jt
        {
            get => _jt;
            set => _jt = value;
        }
        public double Jw
        {
            get => _jw;
            set => _jw = value;
        }

        public double J11
        {
            get => _j11;
            set => _j11 = value;
        }

        public double J22
        {
            get => _j22;
            set => _j22 = value;
        }

        public double Wpl11
        {
            get => _wpl11;
            set => _wpl11 = value;
        }

        public double Wpl22
        {
            get => _wpl22;
            set => _wpl22 = value;
        }

        public double Wel11Min
        {
            get => Math.Min(_wel11Left, _wel11Right);
            
        }

        public double Wel22Min
        {
            get => Math.Min(_wel22Top, _wel22Bottom);
            
        }

        public Point2d Centroid
        {
            get => _centroid;
            set => _centroid = value;
        }

        public Point2d ShearCenter
        {
            get => _shearCenter;
            set => _shearCenter = value;
        }

        public double AngleX1
        {
            get => _angleX1;
            set => _angleX1 = value;
        }

        public double InertiaRadius1 => Math.Sqrt(J11 / Area);
        public double InertiaRadius2 => Math.Sqrt(J22 / Area);

        public bool IsSymmetricAlongYLocalAxis { get; set; }
        public bool IsSymmetricAlongZLocalAxis { get; set; }
        public bool IsDoubleSymmetric {
            get
            {
                if (IsSymmetricAlongZLocalAxis && IsSymmetricAlongYLocalAxis) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        #endregion

        #region Public Constructors

        public Section(Material material, string name) : base(name)
        {
            _material = material;
        }

        public Section(Material[] materials, string name) : base(name)
        {

        }

        public Section(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _area = info.GetDouble("Area");
            _jt = info.GetDouble("Jt");
            _jw = info.GetDouble("Jw");
            _j11 = info.GetDouble("J11");
            _j22 = info.GetDouble("J22");
            _centroid = (Point2d)info.GetValue("Centroid", typeof(Point2d));
            _shearCenter = (Point2d)info.GetValue("ShearCenter", typeof(Point2d));
            _angleX1 = info.GetDouble("AngleX1");
        }
        #endregion

        #region Public Methods Specific
        public double GetWel11(double d)
        {
            /*ShapeMaterial[] shapes = GetShapes();
            for (int i = 0; i < shapes.Count(); i++)
            {
                var s = shapes[i].Shape.Fill;
            }
            return 0;*/
            return J11 / d;
        }

        public double GetWel22(double d)
        {
            return J22/d;
        }

        #endregion

        #region Virtual Methods
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Area", _area);
            info.AddValue("Jt", _jt);
            info.AddValue("Jw", _jw);
            info.AddValue("J11", _j11);
            info.AddValue("J22", _j22);
            info.AddValue("Centroid", _centroid, typeof(Point2d));
            info.AddValue("ShearCenter", _shearCenter, typeof(Point2d));
            info.AddValue("AngleX1", _angleX1);
        }

        public virtual ShapeMaterial[] GetShapes()
        {
            return null;
        }

        public virtual double MinSigma(double N, double M2, double M1)
        {
            double sigmaN = N / _area;
            double sigmaMy;
            if (M2 > 0) { 
                sigmaMy = - M2 / _wel22Top;
            } else
            {
                sigmaMy = M2 / _wel22Bottom;
            }

            double sigmaMz;
            if (M1 > 0)
            {
                sigmaMz = -M1 / _wel11Left;
            }
            else
            {
                sigmaMz = M1 / _wel11Right;
            }

            return sigmaN + sigmaMy + sigmaMz;
        }
        #endregion
    }
}
