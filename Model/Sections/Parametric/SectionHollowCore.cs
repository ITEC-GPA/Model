using GPC.Geometry;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A hollow core slab ("solaio alveolare"): a rectangle with a row of equal voids, circular (equal width and height) or oval with
    /// semicircular ends, equally spaced and centred in the width, at a given height. The keys of the joints are not modelled. Symmetric about
    /// the vertical axis; torsion constant, warping constant and shear centre from the finite elements
    /// </summary>
    [Serializable]
    public class SectionHollowCore : SectionParametric, ISerializable
    {
        private readonly double _width, _height, _voidWidth, _voidHeight, _voidSpacing, _voidCentre;
        private readonly int _voids;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="width">The width of the slab</param>
        /// <param name="height">The height of the slab</param>
        /// <param name="voids">The number of voids</param>
        /// <param name="voidWidth">The width of a void</param>
        /// <param name="voidHeight">The height of a void (equal to the width: circular)</param>
        /// <param name="voidSpacing">The distance between the centres of two voids</param>
        /// <param name="voidCentreHeight">The height of the centres of the voids (0: half the height)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is not positive or the voids do not fit</exception>
        public SectionHollowCore(double width, double height, int voids, double voidWidth, double voidHeight, double voidSpacing,
            double voidCentreHeight = 0.0, string name = "")
            : base(name)
        {
            _width = Positive(width, nameof(width));
            _height = Positive(height, nameof(height));
            _voids = voids >= 1 ? voids : throw new ArgumentException("At least one void", nameof(voids));
            _voidWidth = Positive(voidWidth, nameof(voidWidth));
            _voidHeight = Positive(voidHeight, nameof(voidHeight));
            _voidSpacing = _voids == 1 ? 0.0 : Positive(voidSpacing, nameof(voidSpacing));
            _voidCentre = voidCentreHeight > 0 ? voidCentreHeight : _height / 2;
            if (_voids > 1 && _voidSpacing <= _voidWidth)
                throw new ArgumentException("The voids overlap", nameof(voidSpacing));
            if ((_voids - 1) * _voidSpacing + _voidWidth >= _width || _voidCentre - _voidHeight / 2 <= 0 || _voidCentre + _voidHeight / 2 >= _height)
                throw new ArgumentException("The voids do not fit in the slab");
            Build();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionHollowCore(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _width = info.GetDouble("HollowCoreWidth");
            _height = info.GetDouble("HollowCoreHeight");
            _voids = info.GetInt32("HollowCoreVoids");
            _voidWidth = info.GetDouble("HollowCoreVoidWidth");
            _voidHeight = info.GetDouble("HollowCoreVoidHeight");
            _voidSpacing = info.GetDouble("HollowCoreVoidSpacing");
            _voidCentre = info.GetDouble("HollowCoreVoidCentre");
        }

        /// <summary>The number of voids</summary>
        public int Voids => _voids;

        /// <summary>The rectangular boundary and exact circular or stadium-shaped voids.</summary>
        public override System.Collections.Generic.IReadOnlyList<SectionCurveOutline> GetCurveOutlines() => new[]
        {
            new SectionCurveOutline(Shape.Fill.ToCurve3d(), Enumerable.Range(0, _voids).Select(k =>
                SectionOutline.StadiumCurve(_width / 2 + (k - (_voids - 1) / 2.0) * _voidSpacing,
                    _voidCentre, _voidWidth, _voidHeight)))
        };

        /// <summary>
        /// The rectangle with the voids
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d CreateOutline()
        {
            var rectangle = new Polygon2d(new[] { new Point2d(0, 0), new Point2d(_width, 0), new Point2d(_width, _height), new Point2d(0, _height) });
            Polygon2d[] voids = Enumerable.Range(0, _voids).Select(k =>
            {
                double cx = _width / 2 + (k - (_voids - 1) / 2.0) * _voidSpacing;
                return Math.Abs(_voidWidth - _voidHeight) <= 1e-12 * _voidWidth
                    ? Ellipse(cx, _voidCentre, _voidWidth / 2, _voidHeight / 2)
                    : Stadium(cx, _voidCentre, _voidWidth, _voidHeight);
            }).ToArray();
            return new Shape2d(rectangle, voids);
        }

        /// <summary>Symmetric about the vertical axis</summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => true;

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionHollowCoreVersion", 1);
            info.AddValue("HollowCoreWidth", _width);
            info.AddValue("HollowCoreHeight", _height);
            info.AddValue("HollowCoreVoids", _voids);
            info.AddValue("HollowCoreVoidWidth", _voidWidth);
            info.AddValue("HollowCoreVoidHeight", _voidHeight);
            info.AddValue("HollowCoreVoidSpacing", _voidSpacing);
            info.AddValue("HollowCoreVoidCentre", _voidCentre);
        }

        /// <summary>
        /// The description: "HC width x height / voids"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"HC {_width}x{_height}/{_voids}";
    }
}
