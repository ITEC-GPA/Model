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
    public class SectionCHS : Section
    {
        #region Variables

        protected double _d; // Diameter external
        protected double _t; // Thickness

        #endregion


        #region Properties

        /// <summary>
        /// The external diameter of CHS
        /// </summary>
        public double D => _d;

        /// <summary>
        /// The Thickness of the section
        /// </summary>
        public double T => _t;

        /// <summary>
        /// The internal diameter of CHS
        /// </summary>
        public double Dint => _d - (2 * _t);

        /// <summary>
        /// Is true if the section is symmetric along Y-axis
        /// </summary>
        public bool IsSymmetricAlongYLocalAxis = true;

        /// <summary>
        /// Is true if the section is symmetric along Z-axis
        /// </summary>
        public bool IsSymmetricAlongZLocalAxis = true;

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


        private void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _jxx = CalculateJ();
            _jyy = CalculateJ();
            _jt = CalculateJt();
            _jw = CalculateJw();
            _centroid = CalculateCentroid();
            _shearCenter = CalculateCentroid();
        }


        public double CalculateArea()
        {
            return (Math.Pow(_d, 2.0) * Math.PI) / 4.0 - (Math.Pow(Dint, 2.0) * Math.PI) / 4.0;
        }


        public double CalculateJ()
        {
            return Math.PI * (Math.Pow(_d, 4.0) - Math.Pow(Dint, 4.0)) / (64.0);
        }

        public double CalculateJt()
        {
            return Math.PI * (Math.Pow(_d, 4.0) - Math.Pow(Dint, 4.0)) / (32.0);
        }

        public double CalculateJw()
        {
            return 0;
        }

        public Point2d CalculateCentroid()
        {
            return new Point2d(_d / 2.0, _d / 2.0);
        }


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

            if (Math.Abs(Dint) > 1)
            {
                hole = new Polygon2d();
            }       
            for (int i = 0; i < divisions; i++)
            {
                double teta = i * 2 * Math.PI / divisions;
                fill.Add(new Point2d(0.5 * _d * Math.Cos(teta), 0.5 * _d * Math.Sin(teta)));

                if (hole != null)
                {
                    hole.Add(new Point2d(0.5 * Dint * Math.Cos(teta), 0.5 * Dint * Math.Sin(teta)));
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