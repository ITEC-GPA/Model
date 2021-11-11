using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionCHS : Section
    {
        #region Variables

        protected readonly double _externalDiameter; // Diameter external
        protected readonly double _thickness; // Thickness

        #endregion


        #region Properties

        /// <summary>
        /// The external diameter of CHS
        /// </summary>
        public double Diameter => _externalDiameter;

        /// <summary>
        /// The Thickness of the section
        /// </summary>
        public double Thickness => _thickness;

        /// <summary>
        /// The internal diameter of CHS
        /// </summary>
        public double DiameterInternal => _externalDiameter - (2 * _thickness);

        #endregion


        #region Public Constructors

        public SectionCHS(double externalDiameter, double thickness, Material material, string name)
            : base(material, name)
        {
            #region Check inputs

            if (thickness > externalDiameter / 2.0)
                throw new ArgumentException($"Diameter cannot be lower than 2 * thickness ");

            _externalDiameter = externalDiameter < 0 ? throw new ArgumentException($"Diameter cannot be lower than zero") : externalDiameter;
            _thickness = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;

            #endregion

            _isSymmetricAlongYLocalAxis = true;
            _isSymmetricAlongXLocalAxis = true;

            SetMechanicalProperties();
        }

        public SectionCHS(SectionCHS section)
            : this(section.Diameter, (section.Diameter - section.DiameterInternal) / 2.0, section.Material, section.Name)
        {

        }

        public SectionCHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _externalDiameter = info.GetDouble("D");
            _thickness = info.GetDouble("T");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion


        #region Public method

        protected virtual void SetMechanicalProperties()
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

        protected virtual double CalculateArea()
        {
            return (Math.Pow(Diameter, 2.0) * Math.PI) / 4.0 - (Math.Pow(DiameterInternal, 2.0) * Math.PI) / 4.0;
        }

        protected virtual double CalculateJ()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (64.0);
        }

        protected virtual double CalculateJt()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0);
        }

        protected virtual double CalculateJw()
        {
            return 0;
        }

        protected virtual Point2d CalculateCentroid()
        {
            return new Point2d(Diameter / 2.0, Diameter / 2.0);
        }

        protected virtual double CalculateWel()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0 * _externalDiameter);
        }

        protected virtual double CalculateWpl()
        {
            return (Math.Pow(Diameter, 3.0) - Math.Pow(DiameterInternal, 3.0)) / (6.0);
        }

        #endregion


        #region Public override methods 

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("D", _externalDiameter);
            info.AddValue("T", _thickness);
            info.AddValue("Material", _material);
        }

        public override ShapeMaterial[] GetShapes()
        {
            int divisions = 36;
            Polygon2d hole = null;
            Polygon2d fill = new Polygon2d();

            if (Math.Abs(DiameterInternal) > 1)
            {
                hole = new Polygon2d();
            }
            for (int i = 0; i < divisions; i++)
            {
                double teta = i * 2 * Math.PI / divisions;
                fill.Add(new Point2d(0.5 * Diameter * Math.Cos(teta), 0.5 * _externalDiameter * Math.Sin(teta)));

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
            s = s + "D = " + _externalDiameter + " mm \n";
            s = s + "t = " + _thickness + " mm \n";
            return s;
        }

        #endregion
    }
}