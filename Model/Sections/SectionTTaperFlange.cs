using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A rolled T with a taper flange (ST of ASTM A6, cut from the S shapes; T of EN 10055): the inner face of the flange has a slope, the
    /// flange thickness <see cref="SectionT.ThicknessFlange"/> is measured at a given distance from the tip of the flange, the web meets the
    /// flange with the root fillets <see cref="SectionT.R"/> and the tips of the flange have the toe radius <see cref="R2"/>. Area, centroid,
    /// moments of inertia, elastic and plastic moduli are the ones of the exact outline; the thin walls have the flange thickness
    /// <see cref="SectionT.ThicknessFlange"/>. The flange is at the top, the origin is the bottom left corner
    /// </summary>
    [Serializable]
    public class SectionTTaperFlange : SectionT, ISerializable
    {
        private double _slope;
        private double _r2;
        private double _thicknessPoint;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The height h</param>
        /// <param name="flangeWidth">The width of the flange b</param>
        /// <param name="thicknessWeb">The thickness of the web tw</param>
        /// <param name="flangeThickness">The thickness of the flange tf at <paramref name="thicknessPoint"/> from its tips</param>
        /// <param name="slope">The slope of the inner face of the flange</param>
        /// <param name="rootRadius">The root fillet radius r1</param>
        /// <param name="toeRadius">The toe radius r2 of the tips of the flange</param>
        /// <param name="thicknessPoint">The distance from the tip of the flange where <paramref name="flangeThickness"/> is measured</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is negative or the flange has no thickness at its tips</exception>
        public SectionTTaperFlange(double height, double flangeWidth, double thicknessWeb, double flangeThickness, double slope, double rootRadius,
            double toeRadius, double thicknessPoint, string name)
            : base(height, flangeWidth, thicknessWeb, flangeThickness, name, rootRadius)
        {
            _slope = slope < 0 ? throw new ArgumentException("The slope of the flange cannot be negative") : slope;
            _r2 = toeRadius < 0 ? 0 : toeRadius;
            _thicknessPoint = thicknessPoint < 0 ? throw new ArgumentException("The point of the flange thickness cannot be negative") : thicknessPoint;
            if (TipThickness <= 0)
                throw new ArgumentException("The flange has no thickness at its tips");
            _edgeWorking = EdgeType.Fillet;
            ResetMesh();
            _shape = null;
            SetMechanicalProperties();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionTTaperFlange(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _slope = info.GetDouble("Slope");
            _r2 = info.GetDouble("R2");
            _thicknessPoint = info.GetDouble("ThicknessPoint");
        }

        /// <summary>The slope of the inner face of the flange</summary>
        public double Slope => _slope;

        /// <summary>The toe radius of the tips of the flange</summary>
        public double R2 => _r2;

        /// <summary>The distance from the tip of the flange where the flange thickness is measured</summary>
        public double ThicknessPoint => _thicknessPoint;

        /// <summary>The thickness of the flange at its tips</summary>
        public double TipThickness => ThicknessFlange - _slope * _thicknessPoint;

        /// <summary>The thickness of the flange at the faces of the web</summary>
        public double RootThickness => ThicknessFlange + _slope * ((LenghtFlange - ThicknessWeb) / 2.0 - _thicknessPoint);

        /// <summary>
        /// The working of the corners is part of the rolled geometry: always the fillets
        /// </summary>
        /// <param name="sectionType">Ignored</param>
        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
            _edgeWorking = EdgeType.Fillet;
        }

        /// <summary>
        /// The exact outline: taper flange, root fillets and toe radii
        /// </summary>
        /// <returns>The shape</returns>
        protected override Shape2d GetShape()
        {
            double b = LenghtFlange, h = Height, tw = ThicknessWeb, t0 = TipThickness, t1 = RootThickness;
            return SectionOutline.Create(new[]
            {
                new SectionOutline.Vertex(0.0, h),
                new SectionOutline.Vertex(b, h),
                new SectionOutline.Vertex(b, h - t0, _r2),
                new SectionOutline.Vertex((b + tw) / 2.0, h - t1, R),
                new SectionOutline.Vertex((b + tw) / 2.0, 0.0),
                new SectionOutline.Vertex((b - tw) / 2.0, 0.0),
                new SectionOutline.Vertex((b - tw) / 2.0, h - t1, R),
                new SectionOutline.Vertex(0.0, h - t0, _r2),
            });
        }

        /// <summary>The region of the plastic moduli: the exact outline</summary>
        /// <returns>The shape</returns>
        internal override Shape2d GetPlasticShape() => Shape;

        /// <summary>The area of the exact outline</summary>
        /// <returns>The area</returns>
        protected override double CalculateArea() => Shape.GetArea();

        /// <summary>The centroid of the exact outline</summary>
        /// <returns>The centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            SectionHelper.CalculateStaticMoments(Shape, out double sx, out double sy);
            return SectionHelper.CalculateCentroid(sx, sy, Shape.GetArea());
        }

        /// <summary>The moment of inertia about X of the exact outline</summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJxx()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out double jxx, out _, out _, out _);
            return jxx;
        }

        /// <summary>The moment of inertia about Y of the exact outline</summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJyy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out double jyy, out _, out _);
            return jyy;
        }

        /// <summary>The product of inertia: 0 (symmetric about Y)</summary>
        /// <returns>0</returns>
        protected override double CalculateJxy() => 0.0;

        /// <summary>The plastic modulus about X: the exact one of the outline, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplX() => double.NaN;

        /// <summary>The plastic modulus about Y: the exact one of the outline, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplY() => double.NaN;

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionTTaperFlangeVersion", 1);
            info.AddValue("Slope", _slope);
            info.AddValue("R2", _r2);
            info.AddValue("ThicknessPoint", _thicknessPoint);
        }

        /// <summary>
        /// The description of the section
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"T taper {Height}x{LenghtFlange}x{ThicknessWeb}x{ThicknessFlange} slope {_slope}";
    }
}
