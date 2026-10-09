using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A T section: the flange on the top, the web centred below it; the origin is the bottom left corner of the bounding box
    /// </summary>
    [Serializable]
    public class SectionT : ThinWallSection, ISerializable, IEquatable<SectionT>
    {
        #region Variables

        /// <summary>
        /// The height
        /// </summary>
        protected double _h;
        /// <summary>
        /// The thickness of the web
        /// </summary>
        protected double _tw;
        /// <summary>
        /// The thickness of the flange
        /// </summary>
        protected double _tf;
        /// <summary>
        /// The width of the flange
        /// </summary>
        protected double _b;
        /// <summary>
        /// The fillet radius or the throat of the welds, in the calculations when the working of the corners is set (before, never used)
        /// </summary>
        private readonly double _r;

        #endregion

        #region Properties

        /// <summary>
        /// The height (the setter calculates the section again)
        /// </summary>
        public override double Height
        {
            get => _h;
            set
            {
                if (value != _h)
                {
                    _h = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The width of the flange
        /// </summary>
        public override double Width => _b;

        /// <summary>
        /// The height of the web below the flange
        /// </summary>
        public double HeightWeb => _h - _tf;

        /// <summary>
        /// The thickness of the web (the setter calculates the section again)
        /// </summary>
        public double ThicknessWeb
        {
            get => _tw;
            set
            {
                if (value != _tw)
                {
                    _tw = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The thickness of the flange (the setter calculates the section again)
        /// </summary>
        public double ThicknessFlange
        {
            get => _tf;
            set
            {
                if (value != _tf)
                {
                    _tf = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The width of the flange (the setter calculates the section again)
        /// </summary>
        public double LenghtFlange
        {
            get => _b;
            set
            {
                if (value != _b)
                {
                    _b = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The fillet radius or the throat of the welds (not used in the calculations)
        /// </summary>
        public double R => _r;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The height</param>
        /// <param name="flangeLength">The width of the flange</param>
        /// <param name="thicknessWeb">The thickness of the web</param>
        /// <param name="thicknessFlange">The thickness of the flange</param>
        /// <param name="name">The name</param>
        /// <param name="radius">The fillet radius or the throat of the welds</param>
        /// <exception cref="ArgumentException">If a dimension is negative</exception>
        public SectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, string name,
            double radius = 0) : base(name)
        {
            #region Check inputs

            _h = height < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : height;                   // spessore anima;
            _b = flangeLength < 0 ? throw new ArgumentException($"Flange lenght cannot be lower than zero") : flangeLength;                   // spessore anima;
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : thicknessWeb;                // spessore anima;
            _tf = thicknessFlange < 0 ? throw new ArgumentException($"Flange thickness cannot be lower than zero") : thicknessFlange;             // spessore flangia;
            _r = radius;        // raggio di curvatura o altezza di gola

            #endregion

            CalculateSection();
        }

        /// <summary>
        /// Creates a copy of a section (the radius is not copied)
        /// </summary>
        /// <param name="sectionT">The section to copy</param>
        public SectionT(SectionT sectionT)
            : this(sectionT.Height, sectionT.LenghtFlange, sectionT.ThicknessWeb, sectionT.ThicknessFlange, sectionT.Name)
        {

        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionT(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionTVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _h = info.GetDouble("Height");
            _tw = info.GetDouble("ThicknessWeb");
            _tf = info.GetDouble("ThicknessFlange");
            _b = info.GetDouble("LenghtFlange");
            _r = info.GetDouble("R");
        }

        #endregion

        #region Public method

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionTVersion", version);

            info.AddValue("Height", _h);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("ThicknessFlange", _tf);
            info.AddValue("LenghtFlange", _b);
            info.AddValue("R", _r);
        }

        /// <summary>
        /// True if the section has fillets or welds between the web and the flange (see <see cref="GetCorners"/>)
        /// </summary>
        private bool HasWorkedCorners => _edgeWorking != EdgeType.Sharp && _r > 0.0;

        /// <summary>
        /// The two inside corners between the web and the flange, fillets (radius R) or welds (throat R), included by
        /// <see cref="ThinWallSection"/> in the area, in the centroid and in the moments of inertia; nothing for the sharp corners (before,
        /// the radius was not used)
        /// </summary>
        /// <returns>The corners</returns>
        private protected override SectionCorner[] GetCorners()
        {
            if (!HasWorkedCorners)
                return new SectionCorner[0];

            SectionCorner.Profile profile = SectionCorner.Inside(_edgeWorking, _r);
            return new[]
            {
                new SectionCorner(1, profile, LenghtFlange / 2.0 - ThicknessWeb / 2.0, HeightWeb, -1, -1),
                new SectionCorner(1, profile, LenghtFlange / 2.0 + ThicknessWeb / 2.0, HeightWeb, 1, -1),
            };
        }

        /// <summary>
        /// The region of the exact plastic moduli: the shape with the corners of <see cref="GetCorners"/>
        /// </summary>
        /// <returns>The outline</returns>
        internal override Shape2d GetPlasticShape()
        {
            if (!HasWorkedCorners)
                return Shape;

            return SectionOutline.Create(GetCurveVertices());
        }

        /// <summary>The worked boundary as tangent lines and circular arcs.</summary>
        public override System.Collections.Generic.IReadOnlyList<SectionCurveOutline> GetCurveOutlines()
        {
            if (!HasWorkedCorners)
                return base.GetCurveOutlines();

            return new[] { new SectionCurveOutline(SectionOutline.Curve(GetCurveVertices())) };
        }

        private SectionOutline.Vertex[] GetCurveVertices()
        {
            double b = LenghtFlange, tw = ThicknessWeb, hw = HeightWeb;
            return new[]
            {
                new SectionOutline.Vertex(0.0, Height),
                new SectionOutline.Vertex(b, Height),
                new SectionOutline.Vertex(b, hw),
                SectionOutline.Inside(b / 2.0 + tw / 2.0, hw, _edgeWorking, _r),
                new SectionOutline.Vertex(b / 2.0 + tw / 2.0, 0.0),
                new SectionOutline.Vertex(b / 2.0 - tw / 2.0, 0.0),
                SectionOutline.Inside(b / 2.0 - tw / 2.0, hw, _edgeWorking, _r),
                new SectionOutline.Vertex(0.0, hw),
            };
        }

        /// <summary>
        /// Calculate the plastic modulus respect to X: closed form with the plastic neutral axis in the web or in the flange; with fillets or
        /// welds <see cref="double.NaN"/>, the exact modulus of the outline (see <see cref="GetPlasticShape"/>) is computed at the first access
        /// </summary>
        /// <returns>The plastic modulus respect to X</returns>
        protected override double CalculateWplX()
        {
            if (HasWorkedCorners)
                return double.NaN;

            if (_area / 2.0 >= _b * _tf)
            {
                double yPlastic = _area / 2.0 / _tw;
                var halfSectionTop = new SectionT(Height - yPlastic, _b, _tw, _tf, string.Empty);
                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + yPlastic / 2.0);
            }
            else
            {
                double hTopPlastic = (_area / 2.0) / _b;
                //can't use SectionT because infinite loop
                double Aweb = _tw * (Height - _tf);
                double Aflange = _b * (_tf - hTopPlastic);
                // distances from the plastic neutral axis of the part below it (before, hTopPlastic instead of _tf - hTopPlastic: e.g. +10% for
                // a T cut from an IPE 300)
                double S = Aweb * ((Height - _tf) / 2.0 + (_tf - hTopPlastic)) + Aflange * (_tf - hTopPlastic) / 2.0;
                return (_area / 2.0) * (hTopPlastic / 2.0 + S / (Aweb + Aflange));
            }
        }

        /// <summary>
        /// Calculate the plastic modulus respect to Y: tf b² / 4 + (h - tf) tw² / 4
        /// </summary>
        /// <returns>The plastic modulus respect to Y (the symmetry axis)</returns>
        protected override double CalculateWplY()
        {
            if (HasWorkedCorners)
                return double.NaN;

            return 1.0 / 4.0 * _tf * Math.Pow(_b, 2.0) + 1.0 / 4.0 * (Height - _tf) * Math.Pow(_tw, 2.0);
        }

        // The moduli respect to X and Y with Jxx and Jyy (before, J11 and J22, assuming that the axis 1 was X: wrong for a wide T with Jyy bigger
        // than Jxx); the principal ones are taken from them by Section. Min: bottom and left fibres, Max: top and right fibres (before, respect to Y
        // the Min was on the right)

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
        /// <returns>The elastic modulus respect to X of the bottom fibre (the end of the web)</returns>
        protected override double CalculateWelXMin()
        {
            return Jxx / DistanceYCentroidFromBottom();
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the top fibre
        /// </summary>
        /// <returns>The elastic modulus respect to X of the top fibre (the flange)</returns>
        protected override double CalculateWelXMax()
        {
            return Jxx / (Height - DistanceYCentroidFromBottom());
        }

        /// <summary>
        /// The distance of the centroid from the bottom side (the end of the web)
        /// </summary>
        /// <returns>The distance</returns>
        internal virtual double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        /// <summary>
        /// The distance of the centroid from the top side
        /// </summary>
        /// <returns>The distance</returns>
        internal virtual double DistanceYCentroidFromTop()
        {
            return Height - CalculateCentroid().Y;
        }

        /// <summary>
        /// The distance of the centroid from the right end of the flange
        /// </summary>
        /// <returns>The distance</returns>
        internal virtual double DistanceXCentroidFromRight()
        {
            return LenghtFlange - CalculateCentroid().X;
        }

        /// <summary>
        /// The distance of the centroid from the left end of the flange
        /// </summary>
        /// <returns>The distance</returns>
        internal virtual double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

        #endregion

        #region Public override method

        /// <summary>
        /// The shape of the section (without radius)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0.0, Height),
                new Point2d(LenghtFlange, Height),
                new Point2d(LenghtFlange, HeightWeb),
                new Point2d(LenghtFlange / 2.0 + ThicknessWeb / 2.0, HeightWeb),
                new Point2d(LenghtFlange / 2.0 + ThicknessWeb / 2.0, 0.0),
                new Point2d(LenghtFlange / 2.0 - ThicknessWeb / 2.0, 0.0),
                new Point2d(LenghtFlange / 2.0 - ThicknessWeb / 2.0, HeightWeb),
                new Point2d(0.0, HeightWeb) }));
        }

        /// <summary>
        /// Calculate the shear center: the intersection of the middle lines (the middle of the flange)
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter()
        {
            return new Point2d(_b / 2.0, Height - _tf / 2.0);
        }

        /// <summary>
        /// Calculate the warping constant (Bleich 1952, Picard and Beaulieu 1991)
        /// </summary>
        /// <returns>The warping constant</returns>
        protected override double CalculateJw()
        {
            return Math.Pow(_b, 3.0) * Math.Pow(_tf, 3.0) / 144.0 + Math.Pow(Height - _tf / 2.0, 3.0) * Math.Pow(_tw, 3.0) / 36.0; //Bleich 1952, Picard and Beaulieu 1991
        }

        /// <summary>
        /// Calculate the torsion constant: (b tf³ + (h - tf / 2) tw³) / 3
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            if (_edgeWorking == EdgeType.Fillet && _r > 0.0)
            {
                // half of the formula of the rolled I sections with fillets (SectionH): one flange, the web from the flange, one pair of
                // fillets (α1, D1 of the web-flange junction). Before, the fillets were not in the torsion constant: -18% for a WT20X74.5
                double alpha1 = -0.042 + 0.2204 * _tw / _tf + 0.1355 * _r / _tf - 0.0865 * _r * _tw / Math.Pow(_tf, 2) -
                    0.0725 * Math.Pow(_tw, 2) / Math.Pow(_tf, 2);
                double d1 = (Math.Pow(_tf + _r, 2.0) + (_r + 0.25 * _tw) * _tw) / (2.0 * _r + _tf);
                return _b * Math.Pow(_tf, 3) / 3.0 + (Height - _tf) * Math.Pow(_tw, 3) / 3.0 + alpha1 * Math.Pow(d1, 4) - 0.210 * Math.Pow(_tf, 4);
            }
            return (_b * Math.Pow(_tf, 3.0) + (Height - _tf / 2.0) * Math.Pow(_tw, 3.0)) / 3.0;
        }

        /// <summary>
        /// The section is never symmetric respect to X
        /// </summary>
        /// <returns>False</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return false;
        }

        /// <summary>
        /// The section is always symmetric respect to Y
        /// </summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return true;
        }

        /// <summary>
        /// A multi-line description of the section (dimensions in mm)
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + Height + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }

        /// <summary>
        /// Builds the thin walls (web below the flange, flange on the full width), discards the mesh and the shape and calculates the properties
        /// </summary>
        private void CalculateSection()
        {
            ThinWall web = new ThinWall(HeightWeb, ThicknessWeb, Math.PI / 2,
                new Point2d(LenghtFlange / 2, HeightWeb / 2));
            ThinWall flange = new ThinWall(LenghtFlange, ThicknessFlange, 0,
                new Point2d(LenghtFlange / 2, HeightWeb + ThicknessFlange / 2));

            SetThinWalls(new ThinWall[] { web, flange });

            ResetMesh();

            _shape = null; // before SetMechanicalProperties (it was after: the properties computed on the shape used the old one)

            SetMechanicalProperties();
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another T section (see <see cref="Equals(SectionT)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SectionT);
        }

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionT other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _h == other._h &&
                   _tw == other._tw &&
                   _tf == other._tf &&
                   _b == other._b &&
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
                int hashCode = 2054582011;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _h.GetHashCode();
                hashCode = hashCode * -1521134295 + _tw.GetHashCode();
                hashCode = hashCode * -1521134295 + _tf.GetHashCode();
                hashCode = hashCode * -1521134295 + _b.GetHashCode();
                hashCode = hashCode * -1521134295 + _r.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionT)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionT left, SectionT right)
        {
            return EqualityComparer<SectionT>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionT)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionT left, SectionT right)
        {
            return !(left == right);
        }

        #endregion
    }
}
