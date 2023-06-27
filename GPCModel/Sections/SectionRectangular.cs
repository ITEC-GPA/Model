using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionRectangular : ThinWallSection, ISerializable
    {
        #region Variables

        private double _angle;
        protected double _height;
        protected double _width;

        #endregion

        #region Properties

        /// <summary>
        /// The height of the section
        /// </summary>
        public override double Height 
        {
			get => _height; 
            set
			{
				if (_height != value)
				{
					_height = value;
                    CalculateSection();
                }
			}
		}

        /// <summary>
        /// The width of the section
        /// </summary>
        public double Width
        {
			get => _width; 
            set
			{
				if (_width != value)
				{
					_width = value;
                    CalculateSection();
                }
			}
		}

        public double Angle 
        {
			get => _angle; 
            set
			{
				if (_angle != value)
				{
					_angle = value;
                    CalculateSection();
                }
			}
		}

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

        public SectionRectangular(double height, double width, double angle, Material material, string name = "")
            : base(material, name)
        {
            _height = height <= 0 ? throw new ArgumentException($"Height cannot be lower than zero") : height;
            _width = width <= 0 ? throw new ArgumentException($"Width cannot be lower than zero") : width;
            _angle = angle;

            CalculateSection();
        }

        /// <summary>
        /// Default rectangular section constructor
        /// </summary>
        /// <param name="height">The height of the section</param>
        /// <param name="width">The width of the section</param>
        /// <param name="material">The material of the section</param>
        /// <param name="name">The name of the section</param>
        /// <remarks>Angle of rotation is set to 0</remarks>
        public SectionRectangular(double height, double width, Material material, string name = "")
            : this(height, width, 0.0, material, name)
        {

        }

        public SectionRectangular(SectionRectangular section)
            : this(section.Height, section.Width, section.Material, section.Name)
        {

        }

        protected SectionRectangular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionRectangularVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _height = info.GetDouble("Height");
            _width = info.GetDouble("Width");
            _angle = info.GetDouble("Angle");
        }

		#endregion

		#region Public methods

		protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(new Point2d[] { new Point2d(0, 0), new Point2d(Width, 0), new Point2d(Width, Height), new Point2d(0, Height) }));
        }

        protected override double CalculateArea()
        {
            return Width * Height;
        }

        protected override Point2d CalculateCentroid()
        {
            return new Point2d((_width / 2.0), (_height / 2.0));
        }

        protected override Point2d CalculateShearCenter()
        {
            return CalculateCentroid();
        }

        protected override double CalculateJt()
        {
            return Math.Max(_height, _width) * Math.Pow(Math.Min(_height, _width), 3) * GetAlpha();
        }

        protected override double CalculateJw()
        {
            return 0;
            //TODO: implementare
        }

        protected virtual double GetAlpha()
        {
            double latoMaggiore = Math.Max(_height, _width);
            double latoMinore = Math.Min(_height, _width);

            return 1.0 / 3.0 - 0.21 * latoMinore / latoMaggiore * (1.0 - 1.0 / 12.0 * Math.Pow(latoMinore / latoMaggiore, 4.0));
        }

        protected override double CalculateWpl1()
        {
            return _width * Math.Pow(_height, 2.0) / 4.0;
        }

        protected override double CalculateWpl2()
        {
            return _height * Math.Pow(_width, 2.0) / 4.0;
        }

        protected override double CalculateWel1Max()
        {
            return _width * Math.Pow(_height, 2.0) / 6.0;
        }

        protected override double CalculateWel1Min()
        {
            return _width * Math.Pow(_height, 2.0) / 6.0;
        }

        protected override double CalculateWel2Max()
        {
            return _height * Math.Pow(_width, 2.0) / 6.0;
        }

        protected override double CalculateWel2Min()
        {
            return _height * Math.Pow(_width, 2.0) / 6.0;
        }

        protected override double CalculateWelXMax() => CalculateWel1Max();

        protected override double CalculateWelXMin() => CalculateWel1Min();

        protected override double CalculateWelYMax() => CalculateWel2Max();

        protected override double CalculateWelYMin() => CalculateWel2Min();

        protected override double CalculateAngle()
        {
            return _angle;
        }

        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {

            if (_angleX1 == 0 || _angleX1 == Math.PI / 2.0)
            {
                return true;
            }
            return false;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            if (_angleX1 == 0 || _angleX1 == Math.PI / 2.0)
            {
                return true;
            }
            return false;
        }

        private void CalculateSection()
        {
            ThinWall thin = new ThinWall(_width, _height, _angle, new Point2d(_width / 2.0, _height / 2.0));

            SetThinWalls(new ThinWall[] { thin });

            SetMechanicalProperties();
            _mesh = GetMesh();
        }

        #endregion

        #region Operators 

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionRectangularVersion", version);

            info.AddValue("Height", _height);
            info.AddValue("Width", _width);
            info.AddValue("Angle", _angle);
        }

        public override string ToString()
        {
            return $"Rectangular {_height}x{_width}";
        }

        public override bool Equals(object obj)
        {
            return obj is SectionRectangular rectangular &&
                   base.Equals(obj) &&
                   _height == rectangular._height &&
                   _width == rectangular._width;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _height.GetHashCode();
                hashCode = hashCode * -23 + _width.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SectionRectangular left, SectionRectangular right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SectionRectangular left, SectionRectangular right)
        {
            return !(left == right);
        }

		#endregion
	}
}
