using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
	public class SectionRectangular : Section, ISection
	{
        #region Variables

        protected readonly double _height;
        protected readonly double _width;

        #endregion


        #region Properties

        /// <summary>
        /// The height of the section
        /// </summary>
        public double Height => _height;

        /// <summary>
        /// The width of the section
        /// </summary>
        public double Width => _width;

        #endregion


        #region Public Constructors

        /// <summary>
        /// Default rectangular section constructor
        /// </summary>
        /// <param name="height">The height of the section</param>
        /// <param name="width">The width of the section</param>
        /// <param name="angle">Angle of rotation of the section</param>
        /// <param name="material">The material of the section</param>
        /// <param name="name">The name of the section</param>
        /// <param name="id">The unique id</param>
        public SectionRectangular(double height, double width, double angle, Material material, string name = "", int id = IDUNASSIGNED)
			: base(material, name)
		{
			_height = height;
			_width = width;
            _angleX1 = angle;
			_id = id;
            if (_angleX1 == 0 || _angleX1 == Math.PI / 2.0)
            {
                _isSymmetricAlongYLocalAxis = true;
                _isSymmetricAlongYLocalAxis = true;
            }

            SetMechanicalProperties();
        }

        /// <summary>
        /// Default rectangular section constructor
        /// </summary>
        /// <param name="height">The height of the section</param>
        /// <param name="width">The width of the section</param>
        /// <param name="material">The material of the section</param>
        /// <param name="name">The name of the section</param>
        /// <param name="id">The unique id</param>
        /// <remarks>Angle of rotation is setted to 0</remarks>
        public SectionRectangular(double height, double width, Material material, string name = "", int id = IDUNASSIGNED)
            : this(height, width, 0.0, material, name, id)
        {

        }

        public SectionRectangular(SectionRectangular section, int id = IDUNASSIGNED)
			: this(section.Height, section.Width, section.Material, section.Name, id)
		{
		}

		public SectionRectangular(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
			_height = info.GetDouble("Height");
			_width = info.GetDouble("Width");
		}

        #endregion


        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(new Point2d[] { new Point2d(0, 0), new Point2d(Width, 0), new Point2d(Width, Height), new Point2d(0, Height) }));
        }



        /// <summary>
        /// Internal method to set the mechanical properties to the section
        /// </summary>
        protected virtual void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _centroid = CalculateCentroid();
            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _jxy = CalculateJxy();
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();
            _jw = CalculateJw();
            _jt = CalculateJt();
            _shearCenter = CalculateShearCenter();
            _wel1 = CalculateWel1();
            _wel2 = CalculateWel2();
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();
        }

        protected virtual double CalculateArea()
		{
			return Width * Height;
		}

        protected virtual Point2d CalculateCentroid()
        {
            return new Point2d((_width / 2.0), (_height / 2.0));
        }

        protected virtual Point2d CalculateShearCenter()
		{
            return CalculateCentroid();
		}

        /// <summary>
        /// Calculate the first moment of inertia of the wall respect the X-axis passing throw the centroid
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJxx()
        {
            if (_angleX1 == 0)
                return _width * Math.Pow(_height, 3) / 12.0;

            else
            {
                double J1 = _width * Math.Pow(_height, 3) / 12.0;
                double J2 = _height * Math.Pow(_width, 3) / 12.0;

                return (J1 + J2) / 2.0 + (J1 - J2) / 2.0 * Math.Cos(2.0 * _angleX1);
            }
        }

        /// <summary>
        /// Calculate the first moment of inertia of the wall respect the X-axis passing throw the <paramref name="point"/>
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJxx(Point2d point)
        {
            return CalculateJxx() + CalculateArea() * Math.Pow((point.Y), 2);
        }

        protected virtual double CalculateJxx(double distance)
        {
            return CalculateJxx() + CalculateArea() * Math.Pow(distance, 2);
        }

        /// <summary>
        /// Calculate the first moment of inertia of the wall respect the Y-axis passing throw the centroid
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJyy()
        {
            if(_angleX1 == 0)
                return _height * Math.Pow(_width, 3) / 12.0;

            else
            {
                double J1 = _width * Math.Pow(_height, 3) / 12.0;
                double J2 = _height * Math.Pow(_width, 3) / 12.0;

                return (J1 + J2) / 2.0 - (J1 - J2) / 2.0 * Math.Cos(2.0 * _angleX1);
            }
        }

        /// <summary>
        /// Calculate the first moment of inertia of the wall respect the Y-axis passing throw the <paramref name="point"/>
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJyy(Point2d point)
        {
            return CalculateJyy() + CalculateArea() * Math.Pow((point.X), 2);
        }

        protected virtual double CalculateJyy(double distance)
        {
            return CalculateJyy() + CalculateArea() * Math.Pow(distance, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJxy()
        {
            double J1 = _width * Math.Pow(_height, 3) / 12.0;
            double J2 = _height * Math.Pow(_width, 3) / 12.0;

            return (J1 - J2) / 2.0 * Math.Sin(2.0 * _angleX1);
        }

        protected virtual double CalculateJxy(Point2d point)
        {
            double J1 = _width * Math.Pow(_height, 3) / 12.0;
            double J2 = _height * Math.Pow(_width, 3) / 12.0;

            return (J1 - J2) / 2.0 * Math.Sin(2.0 * _angleX1) + point.X * point.Y * Area;
        }

        protected virtual double CalculateJxy(double distanceX, double distanceY)
        {
            double J1 = _width * Math.Pow(_height, 3) / 12.0;
            double J2 = _height * Math.Pow(_width, 3) / 12.0;

            return (J1 - J2) / 2.0 * Math.Sin(2.0 * _angleX1) + distanceX * distanceY * Area;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the X-axis (the Y-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJ11()
        {
            return CalculateJxx();
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the Y-axis (the Z-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJ22()
        {
            return CalculateJyy();
        }

        protected virtual double CalculateJt()
        {
            return Math.Max(_height, _width) * Math.Pow(Math.Min(_height, _width), 3) * GetAlpha();
        }

        protected virtual double CalculateJw()
        {
            return 0;
            //TODO: implementare
        }

        /// <summary>
        /// Calculate the polar moment of inertia 
        /// </summary>
        /// <returns></returns>
        protected virtual double CalculateJpolar()
        {
            return CalculateJxx() + CalculateJyy();
        }

        protected virtual double GetAlpha()
        {
            double latoMaggiore = Math.Max(_height, _width);
            double latoMinore = Math.Min(_height, _width);

            return 1.0 / 3.0 - 0.21 * latoMinore / latoMaggiore * (1.0 - 1.0 / 12.0 * Math.Pow(latoMinore / latoMaggiore, 4.0));
        }

        protected virtual double CalculateWpl1()
		{
            return _width * Math.Pow(_height, 2.0) / 4.0;
        }

        protected virtual double CalculateWpl2()
		{
            return _height * Math.Pow(_width, 2.0) / 4.0;
        }

        protected virtual double CalculateWel1()
		{
            return _width * Math.Pow(_height, 2.0) / 6.0;
		}

        protected virtual double CalculateWel2()
		{
            return _height * Math.Pow(_width, 2.0) / 6.0;
        }
    }
}
