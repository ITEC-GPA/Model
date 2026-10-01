using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A section of parts welded along their sides, so that they work as one region: an H with cover plates, two channels welded toe to toe
    /// (a closed box), a cruciform of plates, any combination of sections of Model. The parts must not overlap and must touch along their sides.
    /// Area, centroid, moments of inertia and moduli are the ones of <see cref="SectionBuiltUp"/>; torsion constant, warping constant and shear
    /// centre are solved with the finite elements on the parts joined (the closed cells formed by the parts are solved as such). Parts that do
    /// not touch: the torsion constant is the sum of their ones, warping constant and shear centre are not available
    /// </summary>
    [Serializable]
    public class SectionWelded : SectionBuiltUp, ISerializable
    {
        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="parts">The parts (at least one)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If there are no parts</exception>
        public SectionWelded(IEnumerable<Part> parts, string name)
            : base(parts, name)
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionWelded(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// An H with cover plates on its flanges, centred, at the bottom and/or at the top; the origin is the bottom left corner of the bounding box
        /// </summary>
        /// <param name="beam">The H (with the working of its corners set)</param>
        /// <param name="plateWidth">The width of the plates</param>
        /// <param name="plateThickness">The thickness of the plates</param>
        /// <param name="top">True for a plate on the top flange</param>
        /// <param name="bottom">True for a plate on the bottom flange</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If a dimension of the plates is not positive or there are no plates</exception>
        public static SectionWelded CoverPlated(SectionH beam, double plateWidth, double plateThickness, bool top = true, bool bottom = true, string name = "")
        {
            if (beam is null)
                throw new ArgumentNullException(nameof(beam));
            if (!(plateWidth > 0) || !(plateThickness > 0) || !(top || bottom))
                throw new ArgumentException("The plates need positive dimensions, at least one plate");

            double width = Math.Max(beam.Width, plateWidth), y = bottom ? plateThickness : 0.0;
            var plate = new SectionRectangular(plateThickness, plateWidth, "plate");
            var parts = new List<Part> { new Part(beam, (width - beam.Width) / 2, y) };
            if (bottom)
                parts.Add(new Part(plate, (width - plateWidth) / 2, 0));
            if (top)
                parts.Add(new Part(plate, (width - plateWidth) / 2, y + beam.Height));
            return new SectionWelded(parts, name);
        }

        /// <summary>
        /// Two channels welded toe to toe: a closed box, symmetric about the vertical axis X = 0
        /// </summary>
        /// <param name="channel">The channel (web on the left, flanges towards the positive X), with the working of its corners set</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionWelded ChannelBox(SectionC channel, string name = "")
        {
            if (channel is null)
                throw new ArgumentNullException(nameof(channel));
            return new SectionWelded(new[] { new Part(channel, -channel.Width, 0), new Part(channel, channel.Width, 0, mirrorX: true) }, name);
        }

        /// <summary>
        /// A cruciform of plates: a horizontal plate and two vertical half plates welded to it, centred at the origin
        /// </summary>
        /// <param name="width">The width of the horizontal plate</param>
        /// <param name="height">The height of the cruciform</param>
        /// <param name="thickness">The thickness of the plates</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If a dimension is not positive or the plates are too thick</exception>
        public static SectionWelded CruciformPlates(double width, double height, double thickness, string name = "")
        {
            if (!(width > thickness) || !(height > thickness) || !(thickness > 0))
                throw new ArgumentException("The plates must be longer than thick");
            var horizontal = new SectionRectangular(thickness, width, "horizontal");
            var half = new SectionRectangular((height - thickness) / 2, thickness, "vertical");
            return new SectionWelded(new[]
            {
                new Part(horizontal, -width / 2, -thickness / 2), new Part(half, -thickness / 2, thickness / 2),
                new Part(half, -thickness / 2, -height / 2),
            }, name);
        }

        /// <summary>
        /// The torsion of the parts joined, with the finite elements (see the class)
        /// </summary>
        /// <param name="meshSize">The size of the elements (not positive: the default)</param>
        /// <returns>The torsion properties</returns>
        public override SectionTorsionProperties CalculateTorsionProperties(double meshSize = 0) =>
            SectionTorsion.Calculate(GetOutlines().Select(o => new TorsionRegion(o)).ToArray(), meshSize);

        /// <summary>
        /// The torsion constant, the warping constant and the shear centre are computed with the finite elements at the first access
        /// </summary>
        private protected override bool SolvesTorsionNumerically => true;

        /// <summary>The torsion constant: computed with the finite elements at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateJt() => double.NaN;

        /// <summary>The warping constant: computed with the finite elements at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateJw() => double.NaN;

        /// <summary>The shear centre: computed with the finite elements at the first access</summary>
        /// <returns>null</returns>
        protected override Point2d CalculateShearCenter() => null;

        /// <summary>
        /// Geometric properties exact (the parts), torsion constant, warping constant and shear centre numerical
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Numerical, PropertyAvailability.Numerical, PropertyAvailability.Numerical);

        /// <summary>
        /// The description of the section
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"Welded section of {Parts.Count} parts";
    }
}
