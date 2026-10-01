using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A square or rectangular hollow section of constant thickness with rounded corners (SHS, RHS of EN 10210-2 and EN 10219-2, HSS of
    /// AISC): the outside corners have the radius <see cref="OutsideRadius"/>, the inside ones <see cref="InsideRadius"/>. Area, centroid and
    /// moments of inertia include the corners exactly, the plastic moduli are the exact ones of the outline, the torsion constant and the
    /// torsion modulus are the ones of EN 10210-2 (Annex B). The origin is the bottom left corner of the bounding box
    /// </summary>
    [Serializable]
    public class SectionRHSRoundedCorners : SectionRHS, ISerializable
    {
        private double _ro;
        private double _ri;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The height h</param>
        /// <param name="width">The width b</param>
        /// <param name="thickness">The thickness t of the walls</param>
        /// <param name="outsideRadius">The radius of the outside corners</param>
        /// <param name="insideRadius">The radius of the inside corners</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive or a radius does not fit in the section</exception>
        public SectionRHSRoundedCorners(double height, double width, double thickness, double outsideRadius, double insideRadius, string name)
            : base(height, width, thickness, thickness, thickness, thickness, name, outsideRadius)
        {
            if (height <= 0 || width <= 0 || thickness <= 0 || 2 * thickness >= Math.Min(height, width))
                throw new ArgumentException("The dimensions of the hollow section are not valid");
            _ro = outsideRadius < 0 ? throw new ArgumentException("The radius cannot be negative") : outsideRadius;
            _ri = insideRadius < 0 ? throw new ArgumentException("The radius cannot be negative") : insideRadius;
            if (2 * _ro > Math.Min(height, width) || 2 * _ri > Math.Min(height, width) - 2 * thickness)
                throw new ArgumentException("The corner radii do not fit in the section");
            ResetMesh();
            _shape = null;
            SetMechanicalProperties();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionRHSRoundedCorners(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _ro = info.GetDouble("OutsideRadius");
            _ri = info.GetDouble("InsideRadius");
        }

        /// <summary>
        /// The radius of the outside corners
        /// </summary>
        public double OutsideRadius => _ro;

        /// <summary>
        /// The radius of the inside corners
        /// </summary>
        public double InsideRadius => _ri;

        /// <summary>
        /// The thickness of the walls
        /// </summary>
        public double Thickness => ThicknessTop;

        /// <summary>
        /// The torsion modulus Ct of EN 10210-2: It / (t + K / t)
        /// </summary>
        public double TorsionModulus
        {
            get
            {
                TorsionTerms(out double k, out _);
                return Jt / (Thickness + k / Thickness);
            }
        }

        /// <summary>
        /// The rounded corners are part of the geometry: the working of the corners is not used
        /// </summary>
        /// <param name="sectionType">Ignored</param>
        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
        }

        /// <summary>
        /// The corners: the outside ones removed (fillets of radius <see cref="OutsideRadius"/>), the inside ones added (fillets of radius
        /// <see cref="InsideRadius"/> in the corners of the hole)
        /// </summary>
        /// <returns>The corners</returns>
        private protected override SectionCorner[] GetCorners()
        {
            double h = Height, b = Base, t = Thickness;
            SectionCorner.Profile outside = SectionCorner.Fillet(_ro), inside = SectionCorner.Fillet(_ri);
            return new[]
            {
                new SectionCorner(-1, outside, 0, 0, 1, 1),
                new SectionCorner(-1, outside, b, 0, -1, 1),
                new SectionCorner(-1, outside, 0, h, 1, -1),
                new SectionCorner(-1, outside, b, h, -1, -1),
                new SectionCorner(1, inside, t, t, 1, 1),
                new SectionCorner(1, inside, b - t, t, -1, 1),
                new SectionCorner(1, inside, t, h - t, 1, -1),
                new SectionCorner(1, inside, b - t, h - t, -1, -1),
            };
        }

        /// <summary>
        /// The exact outline: the rounded outside boundary and the rounded hole
        /// </summary>
        /// <returns>The shape</returns>
        protected override Shape2d GetShape()
        {
            double h = Height, b = Base, t = Thickness;
            Polygon2d outside = SectionOutline.Polygon(new[]
            {
                new SectionOutline.Vertex(0, 0, _ro), new SectionOutline.Vertex(b, 0, _ro),
                new SectionOutline.Vertex(b, h, _ro), new SectionOutline.Vertex(0, h, _ro),
            });
            Polygon2d hole = SectionOutline.Polygon(new[]
            {
                new SectionOutline.Vertex(t, t, _ri), new SectionOutline.Vertex(b - t, t, _ri),
                new SectionOutline.Vertex(b - t, h - t, _ri), new SectionOutline.Vertex(t, h - t, _ri),
            });
            return new Shape2d(outside, new[] { hole });
        }

        /// <summary>
        /// The region of the plastic moduli: the exact outline
        /// </summary>
        /// <returns>The shape</returns>
        internal override Shape2d GetPlasticShape() => Shape;

        /// <summary>
        /// The plastic modulus respect to X: the exact one of the outline, computed at the first access
        /// </summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplX() => double.NaN;

        /// <summary>
        /// The plastic modulus respect to Y: the exact one of the outline, computed at the first access
        /// </summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplY() => double.NaN;

        /// <summary>
        /// The torsion constant of EN 10210-2 (Annex B): It = t³ p / 3 + 2 K Ah, with p = 2 [(b - t) + (h - t)] - 2 Rc (4 - π),
        /// Ah = (b - t) (h - t) - Rc² (4 - π), K = 2 Ah t / p and Rc = (ro + ri) / 2
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            TorsionTerms(out double k, out double ah);
            double t = Thickness;
            return Math.Pow(t, 3) * Perimeter() / 3.0 + 2.0 * k * ah;
        }

        private double Perimeter()
        {
            double t = Thickness, rc = (_ro + _ri) / 2.0;
            return 2.0 * ((Base - t) + (Height - t)) - 2.0 * rc * (4.0 - Math.PI);
        }

        private void TorsionTerms(out double k, out double ah)
        {
            double t = Thickness, rc = (_ro + _ri) / 2.0;
            ah = (Base - t) * (Height - t) - rc * rc * (4.0 - Math.PI);
            k = 2.0 * ah * t / Perimeter();
        }

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionRHSRoundedCornersVersion", 1);
            info.AddValue("OutsideRadius", _ro);
            info.AddValue("InsideRadius", _ri);
        }

        /// <summary>
        /// The description of the section
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"RHS {Height}x{Base}x{Thickness} ro {_ro} ri {_ri}";
    }
}
