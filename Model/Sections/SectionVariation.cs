using GPC.Geometry;
using System;
using System.Linq;

namespace GPC.Model.Sections
{
    /// <summary>
    /// The law of a variable section along the member, from the start (ratio 0) to the end (ratio 1)
    /// </summary>
    public enum VariationLaw
    {
        /// <summary>Linear: the edges of the member are straight</summary>
        Linear,
        /// <summary>Parabolic with the vertex at the start (the edges tangent to the start section direction): ratio²</summary>
        ParabolicFlatAtStart,
        /// <summary>Parabolic with the vertex at the end: 1 - (1 - ratio)²</summary>
        ParabolicFlatAtEnd,
    }

    /// <summary>
    /// A variable section (tapered or haunched member: H, boxes, hollow sections, girders): the section at a position along the member is the
    /// outline whose vertices move from the ones of the start section to the ones of the end section with the law (straight edges of the member
    /// for the linear law, the real geometry of the plates cut straight; for the parametric sections with vertical webs the same as the section
    /// of the interpolated dimensions). The two sections must have outlines with the same structure (fill, holes, childs) and the same number
    /// of vertices: the same class with the same corners (e.g. two <see cref="SectionH"/> with or without fillets, two
    /// <see cref="SectionBoxGirder"/> with the same cells). The section at a position is a generic <see cref="Section"/> of the outline: exact
    /// geometric properties, torsion from the finite elements. The integration along the member (stiffness, checks) is not here
    /// </summary>
    [Serializable]
    public sealed class SectionVariation
    {
        private readonly Shape2d _start, _end;

        /// <summary>
        /// Creates the variation
        /// </summary>
        /// <param name="start">The section at the start</param>
        /// <param name="end">The section at the end</param>
        /// <param name="law">The law</param>
        /// <exception cref="ArgumentException">If a section has no single outline or the outlines are not compatible</exception>
        public SectionVariation(Section start, Section end, VariationLaw law = VariationLaw.Linear)
        {
            Start = start ?? throw new ArgumentNullException(nameof(start));
            End = end ?? throw new ArgumentNullException(nameof(end));
            Law = law;
            _start = start.GetPlasticShape() ?? throw new ArgumentException("The start section has no single outline", nameof(start));
            _end = end.GetPlasticShape() ?? throw new ArgumentException("The end section has no single outline", nameof(end));
            if (!Compatible(_start, _end))
                throw new ArgumentException("The outlines of the two sections have different structures or numbers of vertices");
        }

        /// <summary>The section at the start</summary>
        public Section Start { get; }

        /// <summary>The section at the end</summary>
        public Section End { get; }

        /// <summary>The law</summary>
        public VariationLaw Law { get; }

        /// <summary>
        /// The fraction of the change at a position
        /// </summary>
        /// <param name="ratio">The position along the member, 0 at the start and 1 at the end</param>
        /// <returns>The fraction: 0 at the start, 1 at the end</returns>
        public double Factor(double ratio)
        {
            switch (Law)
            {
                case VariationLaw.ParabolicFlatAtStart:
                    return ratio * ratio;
                case VariationLaw.ParabolicFlatAtEnd:
                    return 1 - (1 - ratio) * (1 - ratio);
                default:
                    return ratio;
            }
        }

        /// <summary>
        /// The section at a position
        /// </summary>
        /// <param name="ratio">The position along the member, from 0 (start) to 1 (end)</param>
        /// <param name="name">The name (null: the name of the start section and the ratio)</param>
        /// <returns>A generic section of the outline at the position, with its properties calculated</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the ratio is outside [0, 1]</exception>
        public Section SectionAt(double ratio, string name = null)
        {
            if (!(ratio >= 0 && ratio <= 1))
                throw new ArgumentOutOfRangeException(nameof(ratio), "The position must be between 0 and 1");
            double f = Factor(ratio);
            var section = new Section(Interpolate(_start, _end, f), name ?? $"{Start.Name} @ {ratio:0.###}");
            section.SetMechanicalProperties();
            return section;
        }

        private static bool Compatible(Shape a, Shape b)
        {
            if (a.Fill.Count != b.Fill.Count || (a.Holes?.Length ?? 0) != (b.Holes?.Length ?? 0) || (a.Childs?.Length ?? 0) != (b.Childs?.Length ?? 0))
                return false;
            for (int i = 0; i < (a.Holes?.Length ?? 0); i++)
            {
                if (a.Holes[i].Count != b.Holes[i].Count)
                    return false;
            }
            for (int i = 0; i < (a.Childs?.Length ?? 0); i++)
            {
                if (!Compatible(a.Childs[i], b.Childs[i]))
                    return false;
            }
            return true;
        }

        private static Shape2d Interpolate(Shape a, Shape b, double f)
        {
            Polygon2d Ring(Polygon3d p, Polygon3d q) => new Polygon2d(Enumerable.Range(0, p.Count)
                .Select(i => new Point2d(p[i].X + (q[i].X - p[i].X) * f, p[i].Y + (q[i].Y - p[i].Y) * f)).ToArray());

            Polygon2d[] holes = a.Holes?.Select((h, i) => Ring(h, b.Holes[i])).ToArray();
            Shape2d[] childs = a.Childs?.Select((c, i) => Interpolate(c, b.Childs[i], f)).ToArray();
            return new Shape2d(Ring(a.Fill, b.Fill), holes != null && holes.Length > 0 ? holes : null, childs != null && childs.Length > 0 ? childs : null);
        }
    }
}
