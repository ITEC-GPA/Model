using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A rectangular hollow section (RHS) with different thicknesses of the four walls; the origin is the bottom left corner
    /// </summary>
    [Serializable]
    public class SectionRHS : ThinWallSection, ISerializable, IEquatable<SectionRHS>
    {
        #region Varibles

        /// <summary>
        /// The height
        /// </summary>
        private double _h;
        /// <summary>
        /// The width
        /// </summary>
        private double _b;
        /// <summary>
        /// The thickness of the top wall
        /// </summary>
        private double _tfTop;
        /// <summary>
        /// The thickness of the bottom wall
        /// </summary>
        private double _tfBottom;
        /// <summary>
        /// The thickness of the left wall
        /// </summary>
        private double _twL;
        /// <summary>
        /// The thickness of the right wall
        /// </summary>
        private double _twR;

        /// <summary>
        /// The corner radius or the throat of the welds (not used in the calculations)
        /// </summary>
        private readonly double _r;

        #endregion

        #region Properties

        /// <summary>
        /// The width (the setter calculates the section again)
        /// </summary>
        public double Base
        {
            get => _b;
            set
            {
                if (_b != value)
                {
                    _b = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The internal width
        /// </summary>
        public double BaseInternal => _b - _twL - _twR;

        /// <summary>
        /// The height (the setter calculates the section again)
        /// </summary>
        public override double Height
        {
            get => _h;
            set
            {
                if (_h != value)
                {
                    _h = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The width
        /// </summary>
        public override double Width => _b;

        /// <summary>
        /// The internal height
        /// </summary>
        public double Heightinternal => _h - _tfBottom - _tfTop;

        /// <summary>
        /// The thickness of the top wall (the setter calculates the section again)
        /// </summary>
        public double ThicknessTop
        {
            get => _tfTop;
            set
            {
                if (_tfTop != value)
                {
                    _tfTop = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The thickness of the bottom wall (the setter calculates the section again)
        /// </summary>
        public double ThicknessBottom
        {
            get => _tfBottom;
            set
            {
                if (_tfBottom != value)
                {
                    _tfBottom = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The thickness of the left wall (the setter calculates the section again)
        /// </summary>
        public double ThicknessWebLeft
        {
            get => _twL;
            set
            {
                if (_twL != value)
                {
                    _twL = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The thickness of the right wall (the setter calculates the section again)
        /// </summary>
        public double ThicknessWebRight
        {
            get => _twR;
            set
            {
                if (_twR != value)
                {
                    _twR = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The corner radius or the throat of the welds (not used in the calculations)
        /// </summary>
        public double R => _r;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the section and calculates its properties (the dimensions are not checked)
        /// </summary>
        /// <param name="height">The height</param>
        /// <param name="width">The width</param>
        /// <param name="thicknessTopFlange">The thickness of the top wall</param>
        /// <param name="thicknessBottomFlange">The thickness of the bottom wall</param>
        /// <param name="thicknessWebLeft">The thickness of the left wall</param>
        /// <param name="thickenssWebRight">The thickness of the right wall</param>
        /// <param name="name">The name</param>
        /// <param name="radius">The corner radius or the throat of the welds (negative: 0)</param>
        public SectionRHS(double height, double width, double thicknessTopFlange, double thicknessBottomFlange,
            double thicknessWebLeft, double thickenssWebRight, string name, double radius = 0)
            : base(name)
        {
            _h = height;
            _b = width;
            _tfTop = thicknessTopFlange;
            _tfBottom = thicknessBottomFlange;
            _twL = thicknessWebLeft;
            _twR = thickenssWebRight;
            _r = radius < 0 ? 0 : radius;

            CalculateSection();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionRHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionRHSVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _h = info.GetDouble("Height");
            _b = info.GetDouble("Base");
            _tfTop = info.GetDouble("ThicknessTop");
            _tfBottom = info.GetDouble("ThicknessBottom");
            _twL = info.GetDouble("ThicknessWebLeft");
            _twR = info.GetDouble("ThicknessWebRight");
            _r = info.GetDouble("R");
        }

        #endregion

        #region Public method

        /// <summary>
        /// The distance of the centroid from the bottom side
        /// </summary>
        /// <returns>The distance</returns>
        public double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        /// <summary>
        /// The distance of the centroid from the top side
        /// </summary>
        /// <returns>The distance</returns>
        public double DistanceYCentroidFromTop()
        {
            return Height - CalculateCentroid().Y;
        }

        /// <summary>
        /// The distance of the centroid from the right side
        /// </summary>
        /// <returns>The distance of the centroid from the right side (x = <see cref="Base"/>). Before, the distance from the left side</returns>
        public double DistanceXCentroidFromRight()
        {
            return Base - CalculateCentroid().X;
        }

        /// <summary>
        /// The distance of the centroid from the left side
        /// </summary>
        /// <returns>The distance of the centroid from the left side (x = 0). Before, the distance from the right side</returns>
        public double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

        #endregion

        #region Public override method

        /// <summary>
        /// The shape of the section (without radii)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            return new Shape2d(
                new Polygon2d
                (
                    new Point2d[]
                    {
                        new Point2d(0.0, 0.0),
                        new Point2d(0.0, _h),
                        new Point2d(_b, _h),
                        new Point2d(_b, 0.0)
                    }
                ),
                new[]
                {
                    new Polygon2d
                    (
                        new Point2d[]
                        {
                            new Point2d(_twL, _tfBottom),
                            new Point2d(_twL, _h - _tfTop),
                            new Point2d(_b - _twR, _h - _tfTop),
                            new Point2d(_b - _twR, _tfBottom)
                        }
                    )
                });
        }

        /// <summary>
        /// Calculate the shear center: the centroid (not correct for a section without double symmetry)
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter()
        {
            // It returns the center of gravity anyway even though it is not correct for a non-symmetric section.
            // TODO: implement calculation for generic thin sections.
            // Currently we don't use this information.
            return _centroid;
            //if (_tfBottom == _tfTop && _twL == _twR)
            //    return _centroid;
            //else
            //    throw new Exception("Section RHS with different _thickness not yet implemented");
        }

        /// <summary>
        /// Calculate the warping constant: 0
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJw()
        {
            return 0;
        }

        /// <summary>
        /// Torsion constant of the closed section (Bredt or EN 10210-2): approximate; warping constant 0 of the thin-walled theory of the
        /// closed sections: approximate; shear centre in the centroid: exact with the double symmetry, otherwise not available (placeholder)
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Approximate, PropertyAvailability.Approximate,
                IsDoubleSymmetric ? PropertyAvailability.Exact : PropertyAvailability.NotAvailable);

        /// <summary>
        /// Calculate the torsion constant of the closed section (Bredt): 4 Am² / Σ(l / t), on the middle lines
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            double Amed = (_h - (_tfTop / 2.0) - (_tfBottom / 2.0)) * (_b - (_twL / 2.0) - (_twR / 2.0));
            double LmedTop = _b - _twL / 2.0 - _twR / 2.0;
            double LmedBottom = LmedTop;
            double LmedWeb1 = _h - _tfTop / 2.0 - _tfBottom / 2.0;
            double LmedWeb2 = LmedWeb1;
            return 4.0 * Amed * Amed / (LmedBottom / _tfBottom + LmedTop / _tfTop + LmedWeb1 / _twL + LmedWeb2 / _twR);
        }

        /// <summary>
        /// Calculate the plastic modulus respect to Y: closed form (two half C sections) for the section symmetric respect to Y, otherwise the one of the base class
        /// </summary>
        /// <returns>The plastic modulus respect to Y</returns>
        protected override double CalculateWplY()
        {
            if (_area / 2.0 >= _twL * Heightinternal + _tfTop * _twL + _tfBottom * _twL)
            {
                if (IsSymmetricAlongYLocalAxis)
                {
                    var halfSectionLeft = new SectionC(Height, ThicknessWebLeft, Base / 2, ThicknessTop, Base / 2, ThicknessBottom, string.Empty);
                    var halfSectionRigth = new SectionC(Height, ThicknessWebRight, Base / 2, ThicknessTop, Base / 2, ThicknessBottom, string.Empty);
                    return (_area / 2.0) * (halfSectionLeft.DistanceXCentroidFromRight() + halfSectionRigth.DistanceXCentroidFromRight());
                }
                else
                    return base.CalculateWplY();
            }
            else
                return base.CalculateWplY(); // plastic neutral axis in the web: the modulus of ThinWallSection (before, an exception in the constructor)
        }

        /// <summary>
        /// Calculate the plastic modulus respect to X: closed form (two half C sections) for the section symmetric respect to X, otherwise the one of the base class
        /// </summary>
        /// <returns>The plastic modulus respect to X</returns>
        protected override double CalculateWplX()
        {
            if (_area / 2.0 >= (_twR * Heightinternal)) //plateTop
            {
                if (IsSymmetricAlongXLocalAxis)
                {
                    var halfSectionTop = new SectionC(Base, ThicknessTop, Height / 2, _twR, Height / 2, _twL, string.Empty);
                    var halfSectionBottom = new SectionC(Base, ThicknessBottom, Height / 2, _twL, Height / 2, _twR, string.Empty);
                    return (_area / 2.0) * (halfSectionTop.DistanceXCentroidFromRight() + halfSectionBottom.DistanceXCentroidFromRight());
                }
                else
                    return base.CalculateWplX();
            }
            else
                return base.CalculateWplX(); // plastic neutral axis in the web: the modulus of ThinWallSection (before, an exception in the constructor)
        }

        // The moduli respect to X and Y (the principal ones are taken from them by Section when X and Y are principal, otherwise computed respect to
        // the rotated principal axes; before, the moduli respect to the axis 1 were computed respect to X also with J11 = Jyy or rotated axes).
        // Min: bottom and left fibres, Max: top and right fibres

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the left fibre
        /// </summary>
        /// <returns>The elastic modulus respect to Y of the left fibre</returns>
        protected override double CalculateWelYMin()
        {
            return Jyy / DistanceXCentroidFromLeft();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the right fibre
        /// </summary>
        /// <returns>The elastic modulus respect to Y of the right fibre</returns>
        protected override double CalculateWelYMax()
        {
            return Jyy / DistanceXCentroidFromRight();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the bottom fibre
        /// </summary>
        /// <returns>The elastic modulus respect to X of the bottom fibre</returns>
        protected override double CalculateWelXMin()
        {
            return Jxx / DistanceYCentroidFromBottom();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the top fibre
        /// </summary>
        /// <returns>The elastic modulus respect to X of the top fibre</returns>
        protected override double CalculateWelXMax()
        {
            return Jxx / (Height - DistanceYCentroidFromBottom());
        }

        /// <summary>
        /// The section is symmetric respect to X if the top and bottom walls have the same thickness
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            if (_tfBottom == _tfTop)
                return true;

            return false;
        }

        /// <summary>
        /// The section is symmetric respect to Y if the left and right walls have the same thickness
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            if (_twL == _twR)
                return true;

            return false;
        }

        #endregion

        /// <summary>
        /// The description of the section: "RHS h x twl x twr x b x tb x b x tt"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            return $"RHS {_h}x{_twL}x{_twR}x{_b}x{_tfBottom}x{_b}x{_tfTop}";
        }

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionRHSVersion", version);

            info.AddValue("Height", _h);
            info.AddValue("Base", _b);
            info.AddValue("ThicknessTop", _tfTop);
            info.AddValue("ThicknessBottom", _tfBottom);
            info.AddValue("ThicknessWebLeft", _twL);
            info.AddValue("ThicknessWebRight", _twR);
            info.AddValue("R", _r);
        }

        /// <summary>
        /// Builds the thin walls (webs between the flanges, flanges on the full width), discards the mesh and the shape and calculates the properties
        /// </summary>
        private void CalculateSection()
        {
            ThinWall webSx = new ThinWall(Heightinternal, ThicknessWebLeft, Math.PI / 2,
                new Point2d(ThicknessWebLeft / 2, Heightinternal / 2 + ThicknessBottom));
            ThinWall webDx = new ThinWall(Heightinternal, ThicknessWebRight, Math.PI / 2,
                new Point2d(Base - ThicknessWebRight / 2, Heightinternal / 2 + ThicknessBottom));
            ThinWall flangeTop = new ThinWall(Base, ThicknessTop, 0,
                new Point2d(Base / 2, Height - ThicknessTop / 2));
            ThinWall flangeBottom = new ThinWall(Base, ThicknessBottom, 0,
                new Point2d(Base / 2, ThicknessBottom / 2));

            SetThinWalls(new ThinWall[] { webSx, webDx, flangeBottom, flangeTop });

            ResetMesh();

            _shape = null; // before SetMechanicalProperties (it was after: the properties computed on the shape used the old one)

            SetMechanicalProperties();
        }

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another RHS (see <see cref="Equals(SectionRHS)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SectionRHS);
        }

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionRHS other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _h == other._h &&
                   _b == other._b &&
                   _tfTop == other._tfTop &&
                   _tfBottom == other._tfBottom &&
                   _twL == other._twL &&
                   _twR == other._twR &&
                   _r == other._r;
        }

        /// <summary>
        /// The hash code of the section and of the dimensions
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 372658446;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _h.GetHashCode();
                hashCode = hashCode * -1521134295 + _b.GetHashCode();
                hashCode = hashCode * -1521134295 + _tfTop.GetHashCode();
                hashCode = hashCode * -1521134295 + _tfBottom.GetHashCode();
                hashCode = hashCode * -1521134295 + _twL.GetHashCode();
                hashCode = hashCode * -1521134295 + _twR.GetHashCode();
                hashCode = hashCode * -1521134295 + _r.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionRHS)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionRHS left, SectionRHS right)
        {
            return EqualityComparer<SectionRHS>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionRHS)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionRHS left, SectionRHS right)
        {
            return !(left == right);
        }

        #endregion
    }
}
