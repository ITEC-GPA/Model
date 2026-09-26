using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A welded I/H section with two bottom flange plates: the first plate is welded to the web, the second one below it (e.g. a bridge girder
    /// with a cover plate). Symmetric respect to the vertical axis; the origin is the bottom left corner of the bounding box and the plates are
    /// centred on the widest one. The properties are the exact ones of the four plates plus the corners between web and flanges (fillets or
    /// welds, see <see cref="ThinWallSection"/>)
    /// </summary>
    [Serializable]
    public class SectionHDoubleBottomFlange : ThinWallSection, ISerializable, IEquatable<SectionHDoubleBottomFlange>
    {
        #region Variables

        private double _h, _tw, _btop, _ttop, _b1, _t1, _b2, _t2;
        private readonly double _r;

        #endregion

        #region Properties

        /// <summary>
        /// The overall height, from the bottom of the second plate to the top of the top flange (the setter calculates the section again)
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
        /// The width of the widest plate
        /// </summary>
        public override double Width => Math.Max(_btop, Math.Max(_b1, _b2));

        /// <summary>
        /// The thickness of the web
        /// </summary>
        public double ThicknessWeb => _tw;

        /// <summary>
        /// The width of the top flange
        /// </summary>
        public double LengthTopFlange => _btop;

        /// <summary>
        /// The thickness of the top flange
        /// </summary>
        public double ThicknessTopFlange => _ttop;

        /// <summary>
        /// The width of the first bottom plate (welded to the web)
        /// </summary>
        public double LengthBottomFlange => _b1;

        /// <summary>
        /// The thickness of the first bottom plate
        /// </summary>
        public double ThicknessBottomFlange => _t1;

        /// <summary>
        /// The width of the second bottom plate (below the first one)
        /// </summary>
        public double LengthSecondBottomFlange => _b2;

        /// <summary>
        /// The thickness of the second bottom plate
        /// </summary>
        public double ThicknessSecondBottomFlange => _t2;

        /// <summary>
        /// The height of the web between the top flange and the first bottom plate
        /// </summary>
        public double HeightWeb => _h - _ttop - _t1 - _t2;

        /// <summary>
        /// The fillet radius or the throat of the welds between the web and the flanges
        /// </summary>
        public double R => _r;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The overall height (top flange, web and both bottom plates)</param>
        /// <param name="thicknessWeb">The thickness of the web</param>
        /// <param name="topFlangeLength">The width of the top flange</param>
        /// <param name="topFlangeThickness">The thickness of the top flange</param>
        /// <param name="bottomFlangeLength">The width of the first bottom plate, welded to the web</param>
        /// <param name="bottomFlangeThickness">The thickness of the first bottom plate</param>
        /// <param name="secondBottomFlangeLength">The width of the second bottom plate, below the first one</param>
        /// <param name="secondBottomFlangeThickness">The thickness of the second bottom plate</param>
        /// <param name="name">The name</param>
        /// <param name="radius">The fillet radius or the throat of the welds between web and flanges (negative: 0)</param>
        /// <exception cref="ArgumentException">If a dimension is not positive or the web height is not positive</exception>
        public SectionHDoubleBottomFlange(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness,
            double bottomFlangeLength, double bottomFlangeThickness, double secondBottomFlangeLength, double secondBottomFlangeThickness,
            string name = "", double radius = 0)
            : base(name)
        {
            _h = Positive(height, nameof(height));
            _tw = Positive(thicknessWeb, nameof(thicknessWeb));
            _btop = Positive(topFlangeLength, nameof(topFlangeLength));
            _ttop = Positive(topFlangeThickness, nameof(topFlangeThickness));
            _b1 = Positive(bottomFlangeLength, nameof(bottomFlangeLength));
            _t1 = Positive(bottomFlangeThickness, nameof(bottomFlangeThickness));
            _b2 = Positive(secondBottomFlangeLength, nameof(secondBottomFlangeLength));
            _t2 = Positive(secondBottomFlangeThickness, nameof(secondBottomFlangeThickness));
            _r = radius < 0 ? 0 : radius;
            if (HeightWeb <= 0)
                throw new ArgumentException("The height must be greater than the sum of the flange thicknesses");
            if (_tw > Math.Min(_btop, _b1))
                throw new ArgumentException("The web cannot be wider than the flanges it is welded to");

            CalculateSection();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionHDoubleBottomFlange(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _h = info.GetDouble("Height");
            _tw = info.GetDouble("ThicknessWeb");
            _btop = info.GetDouble("LengthTopFlange");
            _ttop = info.GetDouble("ThicknessTopFlange");
            _b1 = info.GetDouble("LengthBottomFlange");
            _t1 = info.GetDouble("ThicknessBottomFlange");
            _b2 = info.GetDouble("LengthSecondBottomFlange");
            _t2 = info.GetDouble("ThicknessSecondBottomFlange");
            _r = info.GetDouble("R");
        }

        private static double Positive(double value, string name) =>
            value > 0 && !double.IsInfinity(value) ? value : throw new ArgumentException(name + " must be positive");

        #endregion

        #region Calculation

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("SectionHDoubleBottomFlangeVersion", 1);
            info.AddValue("Height", _h);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("LengthTopFlange", _btop);
            info.AddValue("ThicknessTopFlange", _ttop);
            info.AddValue("LengthBottomFlange", _b1);
            info.AddValue("ThicknessBottomFlange", _t1);
            info.AddValue("LengthSecondBottomFlange", _b2);
            info.AddValue("ThicknessSecondBottomFlange", _t2);
            info.AddValue("R", _r);
        }

        /// <summary>
        /// The shape of the section (without fillets or welds)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            double xc = Width / 2, y1 = _t2, y2 = _t2 + _t1, y3 = _h - _ttop;
            var points = new List<Point2d>();
            void Add(double x, double y)
            {
                if (points.Count == 0 || Math.Abs(points[points.Count - 1].X - x) > 1e-12 || Math.Abs(points[points.Count - 1].Y - y) > 1e-12)
                    points.Add(new Point2d(x, y));
            }

            // clockwise from the bottom left corner of the second plate, as the other H sections
            Add(xc - _b2 / 2, 0); Add(xc - _b2 / 2, y1); Add(xc - _b1 / 2, y1); Add(xc - _b1 / 2, y2); Add(xc - _tw / 2, y2);
            Add(xc - _tw / 2, y3); Add(xc - _btop / 2, y3); Add(xc - _btop / 2, _h); Add(xc + _btop / 2, _h); Add(xc + _btop / 2, y3);
            Add(xc + _tw / 2, y3); Add(xc + _tw / 2, y2); Add(xc + _b1 / 2, y2); Add(xc + _b1 / 2, y1); Add(xc + _b2 / 2, y1); Add(xc + _b2 / 2, 0);
            return new Shape2d(new Polygon2d(points.ToArray()));
        }

        /// <summary>
        /// The product of inertia: 0 (symmetric section)
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJxy() => 0;

        /// <summary>
        /// The four corners between the web and the top flange and the first bottom plate, welds (chamfer) or fillets, with their exact
        /// centroids and own moments (see <see cref="SectionH"/>); nothing for the sharp corners
        /// </summary>
        /// <returns>The corners</returns>
        private protected override SectionCorner[] GetCorners()
        {
            if (_edgeWorking == EdgeType.Sharp)
                return new SectionCorner[0];

            SectionCorner.Profile profile = SectionCorner.Inside(_edgeWorking, R);
            double xc = Width / 2, left = xc - _tw / 2, right = xc + _tw / 2, bottom = _t2 + _t1, top = _h - _ttop;
            return new SectionCorner[]
            {
                new SectionCorner(1, profile, left, bottom, -1, 1),
                new SectionCorner(1, profile, right, bottom, 1, 1),
                new SectionCorner(1, profile, left, top, -1, -1),
                new SectionCorner(1, profile, right, top, 1, -1),
            };
        }

        /// <summary>
        /// The section is symmetric respect to X only in the degenerate case of a top flange equal to the two bottom plates
        /// </summary>
        /// <returns>False</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis() => false;

        /// <summary>
        /// The section is always symmetric respect to Y
        /// </summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => true;

        /// <summary>
        /// The bottom flange as a whole (both plates): moment of inertia about the vertical axis and height of its centroid
        /// </summary>
        private (double Inertia, double Y) BottomFlange()
        {
            double a1 = _b1 * _t1, a2 = _b2 * _t2;
            return ((_t1 * Math.Pow(_b1, 3) + _t2 * Math.Pow(_b2, 3)) / 12, (a1 * (_t2 + _t1 / 2) + a2 * _t2 / 2) / (a1 + a2));
        }

        /// <summary>
        /// Calculate the shear center: on the vertical axis, between the centroids of the flanges in the ratio of their moments of inertia
        /// about the vertical axis (the two bottom plates as one flange)
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter()
        {
            double top = _ttop * Math.Pow(_btop, 3) / 12;
            var (bottom, yBottom) = BottomFlange();
            double yTop = _h - _ttop / 2;
            return new Point2d(Width / 2, yBottom + (yTop - yBottom) * top / (top + bottom));
        }

        /// <summary>
        /// Calculate the warping constant (CNR DT 208/2011, as <see cref="SectionH"/>): d² If,bottom If,top / Iz, with d the distance of the
        /// centroids of the flanges (the two bottom plates as one flange)
        /// </summary>
        /// <returns>The warping constant</returns>
        protected override double CalculateJw()
        {
            double top = _ttop * Math.Pow(_btop, 3) / 12;
            var (bottom, yBottom) = BottomFlange();
            double d = _h - _ttop / 2 - yBottom, iz = top + bottom + HeightWeb * Math.Pow(_tw, 3) / 12;
            return d * d * bottom * top / iz;
        }

        /// <summary>
        /// Builds the thin walls (web between the top flange and the first plate, plates on their full width), discards the mesh and the shape
        /// and calculates the properties
        /// </summary>
        private void CalculateSection()
        {
            if (HeightWeb <= 0)
                throw new ArgumentException("The height must be greater than the sum of the flange thicknesses");

            double xc = Width / 2;
            SetThinWalls(new ThinWall[] {
                new ThinWall(HeightWeb, _tw, Math.PI / 2, new Point2d(xc, _t2 + _t1 + HeightWeb / 2)),
                new ThinWall(_btop, _ttop, 0, new Point2d(xc, _h - _ttop / 2)),
                new ThinWall(_b1, _t1, 0, new Point2d(xc, _t2 + _t1 / 2)),
                new ThinWall(_b2, _t2, 0, new Point2d(xc, _t2 / 2)) });

            ResetMesh();
            _shape = null;
            SetMechanicalProperties();
        }

        /// <summary>
        /// The description of the section: "H2 h x tw x btop x ttop x b1 x t1 x b2 x t2"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"H2 {_h}x{_tw}x{_btop}x{_ttop}x{_b1}x{_t1}x{_b2}x{_t2}";

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another section of this type (see <see cref="Equals(SectionHDoubleBottomFlange)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj) => Equals(obj as SectionHDoubleBottomFlange);

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionHDoubleBottomFlange other) =>
            !(other is null) && base.Equals(other) && _h == other._h && _tw == other._tw && _btop == other._btop && _ttop == other._ttop &&
            _b1 == other._b1 && _t1 == other._t1 && _b2 == other._b2 && _t2 == other._t2 && _r == other._r;

        /// <summary>
        /// The hash code of the section and of the dimensions
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 1348754217;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                foreach (double value in new[] { _h, _tw, _btop, _ttop, _b1, _t1, _b2, _t2, _r })
                    hashCode = hashCode * -1521134295 + value.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionHDoubleBottomFlange)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionHDoubleBottomFlange left, SectionHDoubleBottomFlange right) =>
            EqualityComparer<SectionHDoubleBottomFlange>.Default.Equals(left, right);

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionHDoubleBottomFlange)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionHDoubleBottomFlange left, SectionHDoubleBottomFlange right) => !(left == right);

        #endregion
    }
}
