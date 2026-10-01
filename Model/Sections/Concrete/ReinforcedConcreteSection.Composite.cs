using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;
using GPC.Model.Sections.Steel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections.Concrete
{
    // The typologies of the steel-concrete composite sections: static methods that build them as a ReinforcedConcreteSection (concrete shape,
    // steel sections and their positions), the only container of the composite sections used by the checks. The homogenized properties use the
    // exact overlap of the steel with the concrete. The quantities that depend on the code are not applied: the effective width of the slab is
    // the width given, the confinement, the classification and the reductions are the ones of the checks

    public partial class ReinforcedConcreteSection
    {
        /// <summary>
        /// A concrete-filled tube: the steel tube (any hollow section with one hole: <see cref="SectionCHS"/>, <see cref="SectionRHS"/>,
        /// <see cref="SectionRHSRoundedCorners"/>...) and the concrete core, its hole. The coordinates are the ones of the tube
        /// </summary>
        /// <param name="tube">The tube</param>
        /// <param name="steel">The steel of the tube</param>
        /// <param name="concrete">The concrete</param>
        /// <param name="formedType">Hot finished or cold formed tube</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If the tube has not exactly one hole</exception>
        public static ReinforcedConcreteSection CreateFilledTube(Section tube, SteelMaterial steel, ConcreteMaterial concrete,
            Section.FormedTypes formedType = Section.FormedTypes.HotFinished, string name = "")
        {
            var steelSection = new SteelSection(tube ?? throw new ArgumentNullException(nameof(tube)), steel, Section.SectionTypes.Rolled, formedType);
            Polygon2d hole = SingleHole(tube, nameof(tube));

            var core = new Section(new Shape2d(hole), name);
            core.SetMechanicalProperties();
            return new ReinforcedConcreteSection(core, concrete, null,
                new List<SteelSectionPosition> { new SteelSectionPosition(steelSection, null, 0.0, null) });
        }

        /// <summary>
        /// A concrete-filled double skin tube: the outer tube, the inner tube centred in it (the middles of the bounding boxes coincide) and the
        /// concrete between them. The coordinates are the ones of the outer tube
        /// </summary>
        /// <param name="outerTube">The outer tube (one hole)</param>
        /// <param name="innerTube">The inner tube (its outline must be inside the hole of the outer tube)</param>
        /// <param name="steel">The steel of the tubes</param>
        /// <param name="concrete">The concrete</param>
        /// <param name="formedType">Hot finished or cold formed tubes</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If the outer tube has not exactly one hole or the inner tube is not inside it</exception>
        public static ReinforcedConcreteSection CreateDoubleSkinTube(Section outerTube, Section innerTube, SteelMaterial steel, ConcreteMaterial concrete,
            Section.FormedTypes formedType = Section.FormedTypes.HotFinished, string name = "")
        {
            var outer = new SteelSection(outerTube ?? throw new ArgumentNullException(nameof(outerTube)), steel, Section.SectionTypes.Rolled, formedType);
            var inner = new SteelSection(innerTube ?? throw new ArgumentNullException(nameof(innerTube)), steel, Section.SectionTypes.Rolled, formedType);
            Polygon2d hole = SingleHole(outerTube, nameof(outerTube));

            var innerPosition = new SteelSectionPosition(inner, null, 0.0,
                new Vector2d(outerTube.Width / 2 - innerTube.Width / 2, outerTube.Height / 2 - innerTube.Height / 2));
            Shape2d innerOutline = innerPosition.GetGlobalOutlines().Single();
            var holeShape = new Shape2d(hole);
            for (int i = 0; i < innerOutline.Fill.Count; i++)
            {
                if (!holeShape.IsPointInside(new Point2d(innerOutline.Fill[i].X, innerOutline.Fill[i].Y)))
                    throw new ArgumentException("The inner tube must be inside the hole of the outer tube", nameof(innerTube));
            }

            var ring = new Section(new Shape2d(hole, new[] { Plane(innerOutline.Fill) }), name);
            ring.SetMechanicalProperties();
            return new ReinforcedConcreteSection(ring, concrete, null,
                new List<SteelSectionPosition> { new SteelSectionPosition(outer, null, 0.0, null), innerPosition });
        }

        /// <summary>
        /// A steel section encased in a rectangle of concrete (SRC): the middle of the bounding box of the steel section in the centre of the
        /// rectangle, rotated by <paramref name="rotation"/>; optionally four bars at the corners. The origin is the bottom left corner of the
        /// rectangle
        /// </summary>
        /// <param name="width">The width of the concrete</param>
        /// <param name="height">The height of the concrete</param>
        /// <param name="steelProfile">The steel section</param>
        /// <param name="steel">The steel of the section</param>
        /// <param name="concrete">The concrete</param>
        /// <param name="rotation">The rotation of the steel section (radians, counterclockwise; e.g. π/2 for an H about its weak axis)</param>
        /// <param name="cornerRebar">The bars at the corners (null: none)</param>
        /// <param name="rebarCover">The distance of the centres of the corner bars from the sides</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If the steel section is not inside the concrete or a bar is not inside it</exception>
        public static ReinforcedConcreteSection CreateEncased(double width, double height, Section steelProfile, SteelMaterial steel, ConcreteMaterial concrete,
            double rotation = 0.0, IRebarSection cornerRebar = null, double rebarCover = 0.0, string name = "")
        {
            var position = Centred(steelProfile, steel, rotation, width / 2, height / 2);
            var section = new ReinforcedConcreteSection(new SectionRectangular(height, width, name), concrete, null, new List<SteelSectionPosition> { position });
            if (!position.IsInsideConcrete)
                throw new ArgumentException("The steel section must be inside the concrete", nameof(steelProfile));

            if (cornerRebar != null)
            {
                if (!(rebarCover > 0) || 2 * rebarCover >= Math.Min(width, height))
                    throw new ArgumentException("The cover of the corner bars must be positive and less than half the sides", nameof(rebarCover));
                foreach (var (x, y) in new[] { (rebarCover, rebarCover), (width - rebarCover, rebarCover), (width - rebarCover, height - rebarCover), (rebarCover, height - rebarCover) })
                    section.AddRebar(new ReinforcedConcreteRebar(cornerRebar, new Point2d(x, y)));
            }
            return section;
        }

        /// <summary>
        /// A steel section encased in a circle of concrete (SRC): the middle of the bounding box of the steel section in the centre, rotated by
        /// <paramref name="rotation"/>; optionally bars on a circle (see <see cref="AddRadialRebars"/>). The centre is (D / 2, D / 2)
        /// </summary>
        /// <param name="diameter">The diameter of the concrete</param>
        /// <param name="steelProfile">The steel section</param>
        /// <param name="steel">The steel of the section</param>
        /// <param name="concrete">The concrete</param>
        /// <param name="rotation">The rotation of the steel section (radians, counterclockwise)</param>
        /// <param name="rebar">The bars (null: none)</param>
        /// <param name="rebarCount">The number of bars</param>
        /// <param name="rebarCover">The distance of the centres of the bars from the outside</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If the steel section is not inside the concrete</exception>
        public static ReinforcedConcreteSection CreateEncasedCircular(double diameter, Section steelProfile, SteelMaterial steel, ConcreteMaterial concrete,
            double rotation = 0.0, IRebarSection rebar = null, int rebarCount = 0, double rebarCover = 0.0, string name = "")
        {
            var position = Centred(steelProfile, steel, rotation, diameter / 2, diameter / 2);
            var section = new ReinforcedConcreteSection(new SectionCircular(diameter, name), concrete, null, new List<SteelSectionPosition> { position });
            if (!position.IsInsideConcrete)
                throw new ArgumentException("The steel section must be inside the concrete", nameof(steelProfile));

            if (rebar != null && rebarCount > 0)
                section.AddRadialRebars(diameter, rebarCover, rebarCount, rebar);
            return section;
        }

        /// <summary>
        /// A partially encased H section: the concrete between the flanges, flush with their tips (the rectangle of the bounding box of the H; the
        /// homogenized properties subtract the steel inside it). The coordinates are the ones of the H
        /// </summary>
        /// <param name="steelProfile">The H section (flanges of the same width)</param>
        /// <param name="steel">The steel of the section</param>
        /// <param name="concrete">The concrete</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If the flanges have different widths</exception>
        public static ReinforcedConcreteSection CreatePartiallyEncased(SectionH steelProfile, SteelMaterial steel, ConcreteMaterial concrete, string name = "")
        {
            if (steelProfile is null)
                throw new ArgumentNullException(nameof(steelProfile));
            if (Math.Abs(steelProfile.LenghtTopFlange - steelProfile.LenghtBottomFlange) > 1e-9 * steelProfile.Width)
                throw new ArgumentException("The flanges of a partially encased section must have the same width", nameof(steelProfile));

            var position = new SteelSectionPosition(new SteelSection(steelProfile, steel), null, 0.0, null);
            return new ReinforcedConcreteSection(new SectionRectangular(steelProfile.Height, steelProfile.Width, name), concrete, null,
                new List<SteelSectionPosition> { position });
        }

        /// <summary>
        /// A slab on steel girders (composite beams, bridge decks): the slab from X = 0 to <paramref name="slabWidth"/> and from Y = 0 to
        /// <paramref name="slabThickness"/>, the girders under it with the middle of their bounding box at the given X and their top at
        /// -<paramref name="haunchHeight"/>; optionally a haunch of concrete on each top flange (the width of the flange at the bottom, wider by
        /// <paramref name="haunchSlope"/> on each side per unit of height). An open steel box (<see cref="SectionSteelBox"/>) is closed by the slab:
        /// see <see cref="CreateSlabOnSteelBox"/>. The slab width is the effective one: the code is not applied. The bars can be added with
        /// <see cref="AddRebarRow"/>
        /// </summary>
        /// <param name="slabWidth">The (effective) width of the slab</param>
        /// <param name="slabThickness">The thickness of the slab</param>
        /// <param name="concrete">The concrete</param>
        /// <param name="girders">The girders and the X of the middle of their bounding boxes</param>
        /// <param name="steel">The steel of the girders</param>
        /// <param name="haunchHeight">The height of the haunches (0: none)</param>
        /// <param name="haunchSlope">The widening of the haunches on each side per unit of height (0: vertical sides)</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If a dimension is not positive, there are no girders or a haunch is outside the slab</exception>
        public static ReinforcedConcreteSection CreateSlabOnGirders(double slabWidth, double slabThickness, ConcreteMaterial concrete,
            IEnumerable<(Section Girder, double X)> girders, SteelMaterial steel, double haunchHeight = 0.0, double haunchSlope = 0.0, string name = "")
        {
            if (!(slabWidth > 0) || !(slabThickness > 0))
                throw new ArgumentException("The width and the thickness of the slab must be positive");
            if (haunchHeight < 0 || haunchSlope < 0)
                throw new ArgumentException("The height and the slope of the haunches cannot be negative");
            var list = girders?.ToList() ?? throw new ArgumentNullException(nameof(girders));
            if (list.Count == 0 || list.Any(g => g.Girder is null))
                throw new ArgumentException("At least one girder is needed", nameof(girders));

            var positions = new List<SteelSectionPosition>();
            var notches = new List<(double TopLeft, double BottomLeft, double BottomRight, double TopRight)>();
            foreach (var (girder, x) in list)
            {
                double left = x - girder.Width / 2;
                positions.Add(new SteelSectionPosition(new SteelSection(girder, steel), null, 0.0, new Vector2d(left, -haunchHeight - girder.Height)));
                if (haunchHeight > 0)
                {
                    foreach (var (centre, width) in TopFlanges(girder))
                    {
                        double c = left + centre, bottom = width / 2, top = width / 2 + haunchHeight * haunchSlope;
                        if (c - top < -1e-9 * slabWidth || c + top > slabWidth * (1 + 1e-9))
                            throw new ArgumentException("The haunches must be under the slab", nameof(haunchHeight));
                        notches.Add((Math.Max(0, c - top), c - bottom, c + bottom, Math.Min(slabWidth, c + top)));
                    }
                }
            }

            ISectionShape slab;
            if (notches.Count == 0)
            {
                slab = new SectionRectangular(slabThickness, slabWidth, name);
            }
            else
            {
                // the outline: the bottom of the slab from left to right with the haunches, then the top
                notches.Sort((a, b) => a.TopLeft.CompareTo(b.TopLeft));
                var points = new List<Point2d> { new Point2d(0, 0) };
                void Add(double px, double py)
                {
                    Point2d last = points[points.Count - 1];
                    if (Math.Abs(last.X - px) > 1e-9 * slabWidth || Math.Abs(last.Y - py) > 1e-9 * slabWidth)
                        points.Add(new Point2d(px, py));
                }
                for (int i = 0; i < notches.Count; i++)
                {
                    if (i > 0 && notches[i].TopLeft < notches[i - 1].TopRight - 1e-9 * slabWidth)
                        throw new ArgumentException("The haunches must not overlap", nameof(haunchHeight));
                    Add(notches[i].TopLeft, 0);
                    Add(notches[i].BottomLeft, -haunchHeight);
                    Add(notches[i].BottomRight, -haunchHeight);
                    Add(notches[i].TopRight, 0);
                }
                Add(slabWidth, 0);
                Add(slabWidth, slabThickness);
                Add(0, slabThickness);

                var shape = new Section(new Shape2d(new Polygon2d(points.ToArray())), name);
                shape.SetMechanicalProperties();
                slab = shape;
            }

            return new ReinforcedConcreteSection(slab, concrete, null, positions);
        }

        /// <summary>
        /// An open steel box closed by the slab ("cassoncino"): <see cref="CreateSlabOnGirders"/> with the box in the middle of the slab. The
        /// torsion of the closed cell is given by <see cref="CalculateHomogenizedTorsionProperties"/>
        /// </summary>
        /// <param name="slabWidth">The (effective) width of the slab</param>
        /// <param name="slabThickness">The thickness of the slab</param>
        /// <param name="concrete">The concrete</param>
        /// <param name="box">The open steel box</param>
        /// <param name="steel">The steel of the box</param>
        /// <param name="haunchHeight">The height of the haunches on the top flanges (0: none)</param>
        /// <param name="haunchSlope">The widening of the haunches on each side per unit of height</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static ReinforcedConcreteSection CreateSlabOnSteelBox(double slabWidth, double slabThickness, ConcreteMaterial concrete, SectionSteelBox box,
            SteelMaterial steel, double haunchHeight = 0.0, double haunchSlope = 0.0, string name = "") =>
            CreateSlabOnGirders(slabWidth, slabThickness, concrete, new[] { ((Section)box, slabWidth / 2) }, steel, haunchHeight, haunchSlope, name);

        /// <summary>
        /// Adds a row of bars at the height <paramref name="y"/>: as many as fit between <paramref name="x0"/> and <paramref name="x1"/> (their
        /// centres) at the distance <paramref name="pitch"/>, centred
        /// </summary>
        /// <param name="rebar">The bar</param>
        /// <param name="y">The height of the centres</param>
        /// <param name="pitch">The distance between the centres</param>
        /// <param name="x0">The leftmost position of a centre</param>
        /// <param name="x1">The rightmost position of a centre</param>
        /// <returns>The number of bars added</returns>
        /// <exception cref="ArgumentException">If the pitch is not positive or <paramref name="x1"/> is less than <paramref name="x0"/></exception>
        public int AddRebarRow(IRebarSection rebar, double y, double pitch, double x0, double x1)
        {
            if (rebar is null)
                throw new ArgumentNullException(nameof(rebar));
            if (!(pitch > 0) || x1 < x0)
                throw new ArgumentException("The pitch must be positive and the row not empty");

            int intervals = (int)Math.Floor((x1 - x0) / pitch + 1e-9);
            double start = 0.5 * (x0 + x1 - intervals * pitch);
            for (int i = 0; i <= intervals; i++)
                AddRebar(new ReinforcedConcreteRebar(rebar, new Point2d(start + i * pitch, y)));
            return intervals + 1;
        }

        /// <summary>
        /// The position of a steel section with the middle of its bounding box at (x, y), rotated about it
        /// </summary>
        private static SteelSectionPosition Centred(Section profile, SteelMaterial steel, double rotation, double x, double y) =>
            new SteelSectionPosition(new SteelSection(profile ?? throw new ArgumentNullException(nameof(profile)), steel), Point2d.Origin, rotation,
                new Vector2d(x, y), InsertionPointType.MiddleCenter, MiddleCenterType.Midpoint);

        /// <summary>
        /// The hole of a tube (its exact outline must have one)
        /// </summary>
        private static Polygon2d SingleHole(Section tube, string parameter)
        {
            Shape2d outline = tube.GetPlasticShape();
            if (outline?.Holes is null || outline.Holes.Length != 1)
                throw new ArgumentException("The tube must have exactly one hole", parameter);
            return Plane(outline.Holes[0]);
        }

        private static Polygon2d Plane(Polygon3d polygon) =>
            new Polygon2d(Enumerable.Range(0, polygon.Count).Select(i => new Point2d(polygon[i].X, polygon[i].Y)).ToArray());

        /// <summary>
        /// The top flanges of a girder: the X of their middle from the left of the bounding box and their width (the whole width for the
        /// sections without known flanges)
        /// </summary>
        private static IEnumerable<(double Centre, double Width)> TopFlanges(Section girder)
        {
            switch (girder)
            {
                case SectionSteelBox box:
                    return new[] { (box.Width / 2 - box.WebSpacingTop / 2, box.LengthTopFlange), (box.Width / 2 + box.WebSpacingTop / 2, box.LengthTopFlange) };
                case SectionHInclinedWeb inclined:
                    return new[] { (inclined.TopAxisX, inclined.LengthTopFlange) };
                case SectionHDoubleBottomFlange doubleBottom:
                    return new[] { (doubleBottom.Width / 2, doubleBottom.LengthTopFlange) };
                case SectionH h:
                    return new[] { (h.Width / 2, h.LenghtTopFlange) };
                default:
                    return new[] { (girder.Width / 2, girder.Width) };
            }
        }
    }
}
