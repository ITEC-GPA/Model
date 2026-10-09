using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A rolled I section with taper flanges (IPN and J of EN 10365, S of ASTM A6): the inner faces of the flanges have a slope, the flange
    /// thickness <see cref="SectionH.ThicknessTopFlange"/> is measured at a given distance from the tip of the flange, the web meets the
    /// flanges with the root fillets <see cref="SectionH.R"/> and the tips of the flanges have the toe radius <see cref="R2"/>. Area, centroid,
    /// moments of inertia, elastic and plastic moduli are the ones of the exact outline; the thin walls (used by the checks of the
    /// classification) have the flange thickness <see cref="SectionH.ThicknessTopFlange"/>. The origin is the bottom left corner
    /// </summary>
    [Serializable]
    public class SectionHTaperFlange : SectionH, ISerializable
    {
        private double _slope;
        private double _r2;
        private double _thicknessPoint;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="height">The height h</param>
        /// <param name="thicknessWeb">The thickness of the web tw</param>
        /// <param name="flangeWidth">The width of the flanges b</param>
        /// <param name="flangeThickness">The thickness of the flanges tf at <paramref name="thicknessPoint"/> from their tips</param>
        /// <param name="slope">The slope of the inner faces of the flanges (e.g. 0.14 for the IPN)</param>
        /// <param name="rootRadius">The root fillet radius r1</param>
        /// <param name="toeRadius">The toe radius r2 of the tips of the flanges</param>
        /// <param name="thicknessPoint">The distance from the tip of the flange where <paramref name="flangeThickness"/> is measured</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is negative or the flanges have no thickness at their tips</exception>
        public SectionHTaperFlange(double height, double thicknessWeb, double flangeWidth, double flangeThickness, double slope, double rootRadius,
            double toeRadius, double thicknessPoint, string name)
            : base(height, thicknessWeb, flangeWidth, flangeThickness, flangeWidth, flangeThickness, name, rootRadius)
        {
            _slope = slope < 0 ? throw new ArgumentException("The slope of the flanges cannot be negative") : slope;
            _r2 = toeRadius < 0 ? 0 : toeRadius;
            _thicknessPoint = thicknessPoint < 0 ? throw new ArgumentException("The point of the flange thickness cannot be negative") : thicknessPoint;
            if (TipThickness <= 0)
                throw new ArgumentException("The flanges have no thickness at their tips");
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
        protected SectionHTaperFlange(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _slope = info.GetDouble("Slope");
            _r2 = info.GetDouble("R2");
            _thicknessPoint = info.GetDouble("ThicknessPoint");
        }

        /// <summary>
        /// The slope of the inner faces of the flanges
        /// </summary>
        public double Slope => _slope;

        /// <summary>
        /// The toe radius of the tips of the flanges
        /// </summary>
        public double R2 => _r2;

        /// <summary>
        /// The distance from the tip of the flange where the flange thickness is measured
        /// </summary>
        public double ThicknessPoint => _thicknessPoint;

        /// <summary>
        /// The thickness of the flanges at their tips
        /// </summary>
        public double TipThickness => ThicknessTopFlange - _slope * _thicknessPoint;

        /// <summary>
        /// The thickness of the flanges at the faces of the web
        /// </summary>
        public double RootThickness => ThicknessTopFlange + _slope * ((LenghtTopFlange - ThicknessWeb) / 2.0 - _thicknessPoint);

        /// <summary>
        /// The working of the corners is part of the rolled geometry: always the fillets
        /// </summary>
        /// <param name="sectionType">Ignored</param>
        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
            _edgeWorking = EdgeType.Fillet;
        }

        /// <summary>
        /// The exact outline: taper flanges, root fillets and toe radii
        /// </summary>
        /// <returns>The shape</returns>
        protected override Shape2d GetShape()
        {
            return SectionOutline.Create(GetCurveVertices());
        }

        /// <summary>The worked boundary as tangent lines and circular arcs.</summary>
        public override System.Collections.Generic.IReadOnlyList<SectionCurveOutline> GetCurveOutlines()
        {
            return new[] { new SectionCurveOutline(SectionOutline.Curve(GetCurveVertices())) };
        }

        private SectionOutline.Vertex[] GetCurveVertices()
        {
            double b = LenghtTopFlange, h = Height, tw = ThicknessWeb;
            double t0 = TipThickness, t1 = RootThickness;
            double left = (b - tw) / 2.0, right = (b + tw) / 2.0;
            return new[]
            {
                new SectionOutline.Vertex(0.0, 0.0),
                new SectionOutline.Vertex(b, 0.0),
                new SectionOutline.Vertex(b, t0, _r2),
                new SectionOutline.Vertex(right, t1, R),
                new SectionOutline.Vertex(right, h - t1, R),
                new SectionOutline.Vertex(b, h - t0, _r2),
                new SectionOutline.Vertex(b, h),
                new SectionOutline.Vertex(0.0, h),
                new SectionOutline.Vertex(0.0, h - t0, _r2),
                new SectionOutline.Vertex(left, h - t1, R),
                new SectionOutline.Vertex(left, t1, R),
                new SectionOutline.Vertex(0.0, t0, _r2),
            };
        }

        /// <summary>
        /// The region of the plastic moduli: the exact outline
        /// </summary>
        /// <returns>The shape</returns>
        internal override Shape2d GetPlasticShape() => Shape;

        /// <summary>
        /// The area of the exact outline
        /// </summary>
        /// <returns>The area</returns>
        protected override double CalculateArea() => Shape.GetArea();

        /// <summary>
        /// The centroid of the exact outline
        /// </summary>
        /// <returns>The centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            SectionHelper.CalculateStaticMoments(Shape, out double sx, out double sy);
            return SectionHelper.CalculateCentroid(sx, sy, Shape.GetArea());
        }

        /// <summary>
        /// The moment of inertia about X of the exact outline
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJxx()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out double jxx, out _, out _, out _);
            return jxx;
        }

        /// <summary>
        /// The moment of inertia about Y of the exact outline
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJyy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out double jyy, out _, out _);
            return jyy;
        }

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
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionHTaperFlangeVersion", 1);
            info.AddValue("Slope", _slope);
            info.AddValue("R2", _r2);
            info.AddValue("ThicknessPoint", _thicknessPoint);
        }

        /// <summary>
        /// The description of the section
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            return $"I taper {Height}x{ThicknessWeb}x{LenghtTopFlange}x{ThicknessTopFlange} slope {_slope}";
        }
    }
}
