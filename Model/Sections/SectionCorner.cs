using System;

namespace GPC.Model.Sections
{
    /// <summary>
    /// The area of a corner of a thin wall section, added to the thin walls or removed from them: a fillet (a square minus the quarter circle
    /// inscribed in it) or a weld (a right isosceles triangle) placed in the corner between two perpendicular sides, with the exact centroid and
    /// own moments of inertia (see <see cref="ThinWallSection"/>)
    /// </summary>
    internal readonly struct SectionCorner
    {
        /// <summary>
        /// The properties of the corner area in the axes of its sides (corner in the origin, the area in the first quadrant)
        /// </summary>
        internal readonly struct Profile
        {
            /// <summary>
            /// Creates the properties
            /// </summary>
            /// <param name="area">The area</param>
            /// <param name="distance">The distance of the centroid from both the sides</param>
            /// <param name="ownJ">The moment of inertia about the axes through the centroid parallel to the sides</param>
            /// <param name="ownJxy">The product of inertia about the axes through the centroid parallel to the sides</param>
            internal Profile(double area, double distance, double ownJ, double ownJxy)
            {
                Area = area;
                Distance = distance;
                OwnJ = ownJ;
                OwnJxy = ownJxy;
            }

            /// <summary>
            /// The area
            /// </summary>
            internal double Area { get; }

            /// <summary>
            /// The distance of the centroid from both the sides
            /// </summary>
            internal double Distance { get; }

            /// <summary>
            /// The moment of inertia about the axes through the centroid parallel to the sides (the same for both)
            /// </summary>
            internal double OwnJ { get; }

            /// <summary>
            /// The product of inertia about the axes through the centroid parallel to the sides
            /// </summary>
            internal double OwnJxy { get; }
        }

        /// <summary>
        /// A fillet of radius r: area (1 - π / 4) r², static moment about a side (5 / 6 - π / 4) r³, moment of inertia about a side
        /// (1 - 5 π / 16) r⁴, product of inertia about the sides (19 / 24 - π / 4) r⁴. Before, the sections used the centroid at r / 3.5 (C) or
        /// r / 6 (H) from the sides instead of 0.2234 r, and the own moment (1 / 3 - π / 16) r⁴ (the one about the side far from the fillet)
        /// instead of 0.0075 r⁴
        /// </summary>
        /// <param name="r">The radius</param>
        /// <returns>The properties</returns>
        internal static Profile Fillet(double r)
        {
            double area = Math.Pow(r, 2) - Math.Pow(r, 2) * Math.PI / 4.0;
            if (area <= 0.0)
                return new Profile(0, 0, 0, 0);

            double distance = (5.0 / 6.0 - Math.PI / 4.0) * Math.Pow(r, 3) / area;
            return new Profile(area, distance,
                (1.0 - 5.0 * Math.PI / 16.0) * Math.Pow(r, 4) - area * distance * distance,
                (19.0 / 24.0 - Math.PI / 4.0) * Math.Pow(r, 4) - area * distance * distance);
        }

        /// <summary>
        /// A right isosceles triangle with legs a: area a² / 2, centroid a / 3 from the legs, own moment a⁴ / 36 and product -a⁴ / 72 (before,
        /// the sections used a⁴ / 24 and the centroid at a / 5 or a / 8.5 from the legs)
        /// </summary>
        /// <param name="a">The length of the legs</param>
        /// <returns>The properties</returns>
        internal static Profile Triangle(double a)
        {
            if (a <= 0.0)
                return new Profile(0, 0, 0, 0);

            return new Profile(Math.Pow(a, 2) / 2.0, a / 3.0, Math.Pow(a, 4) / 36.0, -Math.Pow(a, 4) / 72.0);
        }

        /// <summary>
        /// The inside corner of a working: a weld with throat r (legs 1.41 r) for <see cref="EdgeType.Chamfer"/>, a fillet of radius r for
        /// <see cref="EdgeType.Fillet"/>, nothing for <see cref="EdgeType.Sharp"/>
        /// </summary>
        /// <param name="edgeType">The working of the corner</param>
        /// <param name="r">The radius or the throat</param>
        /// <returns>The properties</returns>
        internal static Profile Inside(EdgeType edgeType, double r)
        {
            switch (edgeType)
            {
                case EdgeType.Chamfer:
                    return Triangle(1.41 * r);
                case EdgeType.Fillet:
                    return Fillet(r);
                default:
                    return new Profile(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Places a corner area in the section
        /// </summary>
        /// <param name="sign">1 for an added area, -1 for a removed one</param>
        /// <param name="profile">The properties of the area in the axes of its sides</param>
        /// <param name="x">X of the corner</param>
        /// <param name="y">Y of the corner</param>
        /// <param name="directionX">1 if the area is on the side of the positive X from the corner, -1 otherwise</param>
        /// <param name="directionY">1 if the area is on the side of the positive Y from the corner, -1 otherwise</param>
        internal SectionCorner(double sign, Profile profile, double x, double y, double directionX, double directionY)
        {
            Sign = sign;
            Area = profile.Area;
            X = x + directionX * profile.Distance;
            Y = y + directionY * profile.Distance;
            OwnJ = profile.OwnJ;
            OwnJxy = directionX * directionY * profile.OwnJxy;
        }

        /// <summary>
        /// 1 for an added area, -1 for a removed one
        /// </summary>
        internal double Sign { get; }

        /// <summary>
        /// The area
        /// </summary>
        internal double Area { get; }

        /// <summary>
        /// X of the centroid
        /// </summary>
        internal double X { get; }

        /// <summary>
        /// Y of the centroid
        /// </summary>
        internal double Y { get; }

        /// <summary>
        /// The moment of inertia about the X and Y axes through the centroid
        /// </summary>
        internal double OwnJ { get; }

        /// <summary>
        /// The product of inertia about the X and Y axes through the centroid
        /// </summary>
        internal double OwnJxy { get; }
    }
}
