using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionRectangular : ThinWallSection, ISerializable, IEquatable<SectionRectangular>
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
        public override double Width
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
        /// <param name="name">The name of the section</param>
        public SectionRectangular(double height, double width, double angle, string name = "")
            : base(name)
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
        /// <param name="name">The name of the section</param>
        /// <remarks>Angle of rotation is set to 0</remarks>
        public SectionRectangular(double height, double width, string name = "")
            : this(height, width, 0.0, name)
        {

        }

        public SectionRectangular(SectionRectangular section)
            : this(section.Height, section.Width, section.Name)
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

        // The axis 1 is the principal axis of the maximum moment of inertia: the local X axis of the rectangle (rotated by Angle) when the height is
        // not smaller than the width, otherwise the local Y axis (angle Angle - 90°). The moduli respect to X and Y are the ones respect to the local
        // axes of the rectangle. Before, the angle was always Angle while J11 was the bigger moment (with the width bigger than the height the axis 1
        // was the local X and J11 the moment respect to the local Y)

        /// <summary>
        /// True if the axis 1 is the local X axis of the rectangle (the height is not smaller than the width)
        /// </summary>
        private bool AxisOneIsLocalX => _height >= _width;

        /// <returns>The plastic modulus respect to the local X axis</returns>
        protected override double CalculateWplX()
        {
            return _width * Math.Pow(_height, 2.0) / 4.0;
        }

        /// <returns>The plastic modulus respect to the local Y axis</returns>
        protected override double CalculateWplY()
        {
            return _height * Math.Pow(_width, 2.0) / 4.0;
        }

        /// <returns>The plastic modulus respect to the axis 1</returns>
        protected override double CalculateWpl1()
        {
            return AxisOneIsLocalX ? CalculateWplX() : CalculateWplY();
        }

        /// <returns>The plastic modulus respect to the axis 2</returns>
        protected override double CalculateWpl2()
        {
            return AxisOneIsLocalX ? CalculateWplY() : CalculateWplX();
        }

        /// <returns>The elastic modulus respect to the local X axis (the same for the two fibres)</returns>
        protected override double CalculateWelXMax()
        {
            return _width * Math.Pow(_height, 2.0) / 6.0;
        }

        /// <returns>The elastic modulus respect to the local X axis (the same for the two fibres)</returns>
        protected override double CalculateWelXMin() => CalculateWelXMax();

        /// <returns>The elastic modulus respect to the local Y axis (the same for the two fibres)</returns>
        protected override double CalculateWelYMax()
        {
            return _height * Math.Pow(_width, 2.0) / 6.0;
        }

        /// <returns>The elastic modulus respect to the local Y axis (the same for the two fibres)</returns>
        protected override double CalculateWelYMin() => CalculateWelYMax();

        /// <returns>The elastic modulus respect to the axis 1</returns>
        protected override double CalculateWel1Max()
        {
            return AxisOneIsLocalX ? CalculateWelXMax() : CalculateWelYMax();
        }

        /// <returns>The elastic modulus respect to the axis 1</returns>
        protected override double CalculateWel1Min() => CalculateWel1Max();

        /// <returns>The elastic modulus respect to the axis 2</returns>
        protected override double CalculateWel2Max()
        {
            return AxisOneIsLocalX ? CalculateWelYMax() : CalculateWelXMax();
        }

        /// <returns>The elastic modulus respect to the axis 2</returns>
        protected override double CalculateWel2Min() => CalculateWel2Max();

        /// <returns>The angle of the axis 1: the rotation of the rectangle, minus 90° when the width is bigger than the height</returns>
        protected override double CalculateAngle()
        {
            return AxisOneIsLocalX ? _angle : _angle - Math.PI / 2.0;
        }

        // Symmetric respect to its local axes when they are the X and Y axes (the rotation of the rectangle is 0 or 90°; before, the comparison was
        // on the angle of the axis 1, that was the rotation of the rectangle)

        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return _angle == 0 || _angle == Math.PI / 2.0;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return _angle == 0 || _angle == Math.PI / 2.0;
        }

        private void CalculateSection()
        {
            ThinWall thin = new ThinWall(_width, _height, _angle, new Point2d(_width / 2.0, _height / 2.0));

            SetThinWalls(new ThinWall[] { thin });

            ResetMesh();

            _shape = null; // before SetMechanicalProperties (it was after: the properties computed on the shape used the old one)

            SetMechanicalProperties();
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

        #endregion

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            return Equals(obj as SectionRectangular);
        }

        public bool Equals(SectionRectangular other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _height == other._height &&
                   _width == other._width &&
                   _angle == other._angle;
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
