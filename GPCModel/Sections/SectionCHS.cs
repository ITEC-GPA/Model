using GPC.Geometry;
using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionCHS : Section
    {
        #region Variables

        protected readonly double _d; // Diameter external
        protected readonly double _t; // Thickness

        #endregion


        #region Properties

        /// <summary>
        /// The external diameter of CHS
        /// </summary>
        public double Diameter => _d;

        /// <summary>
        /// The Thickness of the section
        /// </summary>
        public double Thickness => _t;

        /// <summary>
        /// The internal diameter of CHS
        /// </summary>
        public double DiameterInternal => _d - (2 * _t);

        #endregion


        #region Public Constructors

        public SectionCHS(double dext, double t, Material material, string name) 
            : base(material, name)
        {
            #region Check inputs

            if (t > dext / 2.0)
                throw new ArgumentException($"Diameter cannot be lower than 2 * thickness ");       

            _d = dext < 0 ? throw new ArgumentException($"Diameter cannot be lower than zero") : dext;
            _t = t < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : t;

            #endregion

            _isSymmetricAlongYLocalAxis = true;
            _isSymmetricAlongXLocalAxis = true;

            SetMechanicalProperties();
        }

        public SectionCHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _d = info.GetDouble("D");
            _t = info.GetDouble("T");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion


        #region Public method

        protected void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _j11 = CalculateJ();
            _j22 = CalculateJ();
            _jxx = CalculateJ();
            _jyy = CalculateJ();
            _jt = CalculateJt();
            _jw = CalculateJw();
            _centroid = CalculateCentroid();
            _shearCenter = CalculateCentroid();
            _wel1 = CalculateWel();
            _wel2 = CalculateWel();
            _wpl1 = CalculateWpl();
            _wpl2 = CalculateWpl();
        }

        protected double CalculateArea()
        {
            return (Math.Pow(_d, 2.0) * Math.PI) / 4.0 - (Math.Pow(DiameterInternal, 2.0) * Math.PI) / 4.0;
        }

        protected double CalculateJ()
        {
            return Math.PI * (Math.Pow(_d, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (64.0);
        }

        protected double CalculateJt()
        {
            return Math.PI * (Math.Pow(_d, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0);
        }

        protected double CalculateJw()
        {
            return 0;
        }

        protected Point2d CalculateCentroid()
        {
            return new Point2d(_d / 2.0, _d / 2.0);
        }

        protected double CalculateWel()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0 * _d);
        }

        protected double CalculateWpl()
        {
            return (Math.Pow(Diameter, 3.0) - Math.Pow(DiameterInternal, 3.0)) / (6.0);
        }
        
        #endregion


        #region Public override methods 

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

            if (Math.Abs(DiameterInternal) > 1)
            {
                hole = new Polygon2d();
            }       
            for (int i = 0; i < divisions; i++)
            {
                double teta = i * 2 * Math.PI / divisions;
                fill.Add(new Point2d(0.5 * _d * Math.Cos(teta), 0.5 * _d * Math.Sin(teta)));

                if (hole != null)
                {
                    hole.Add(new Point2d(0.5 * DiameterInternal * Math.Cos(teta), 0.5 * DiameterInternal * Math.Sin(teta)));
                }
            }

            Shape shape = new Shape(fill, hole != null ? new[] { hole } : null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }

        public override string ToString()
        {
            string s = "CHS section: \n";
            s = s + "D = " + _d + " mm \n";
            s = s + "t = " + _t + " mm \n";
            return s;
        }

        #endregion
    }
}