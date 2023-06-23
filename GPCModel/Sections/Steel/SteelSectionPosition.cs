using GPC.Geometry;
using System;

namespace GPC.Model.Sections.Steel
{
    /// <summary>
    /// Generic steel section and its displacement including:
    /// 1) Rotation around a point RotationCenter;
    /// 2) Traslation.
    /// Operations done in this order.
    /// </summary>
    public class SteelSectionPosition
    {
        #region Properties

        public SteelSection Section { get; set; }

        /// <summary>
        /// Center of rotation.
        /// Position in the local system (intial without displacement) of the point around which to perform the rotation.
        /// Currently the points of sections with predefined shapes such as H, L, C etc are all in
        /// the first quadrant (coordinates with positive X and Y). If you want to rotate around the
        /// center of gravity just assign the center of gravity to this property.
        /// </summary>
        public Point2d RotationCenter { get; set; }

        /// <summary>
        /// Angle of rotation in radians. Positive counterclockwise, angle with zero value for positive X direction.
        /// </summary>
        public double Rotation { get; set; }

        /// <summary>
        /// Traslation.
        /// </summary>
        public Vector2d Traslation { get; set; }

        /// <summary>
        /// Defines whether the section is entirely outside or inside the concrete area.
        /// </summary>
        public bool IsInsideConcrete { get; set; }

        /// <summary>
        /// The ID is that of the section.
        /// </summary>
        internal int Id
        {
            get => Section.Id;
            set => Section.Id = value;
        }

        #endregion

        #region Constructor

        public SteelSectionPosition(SteelSection steelSection, Point2d rotationCenter, double rotation, Vector2d traslation)
        {
            Section = steelSection ?? throw new ArgumentNullException(nameof(steelSection));
            RotationCenter = rotationCenter ?? Point2d.Origin;
            Rotation = rotation;
            Traslation = traslation ?? new Vector2d(0.0, 0.0);
            IsInsideConcrete = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Given a point in the local system of the ThinWallSection return the point in the global system.
        /// </summary>
        /// <param name="point2D"></param>
        /// <returns></returns>
        Point2d PositionToGlobal(in Point2d point2D)
        {
            var globPoint2d = (Point2d)point2D.Clone();
            globPoint2d.Rotate(RotationCenter, Rotation);
            globPoint2d.Move(Traslation);
            return globPoint2d;
        }

        /// <summary>
        /// Given a point in the global system return the point in the local system of the ThinWallSection.
        /// </summary>
        /// <param name="point2D"></param>
        /// <returns></returns>
        Point2d PositionToLocal(in Point2d point2D)
        {
            var localPoint2d = (Point2d)point2D.Clone();
            localPoint2d.Move(new Vector3d(-Traslation.X, -Traslation.Y, 0.0));
            localPoint2d.Rotate(RotationCenter, -Rotation);
            return localPoint2d;
        }

        internal double CalculateArea() => Section.Area;

        internal Point2d CalculateCentroid() => PositionToGlobal(Section.Centroid);

        /// <summary>
        /// Calculate the moment of inertia Jxx of the section considering rotation and displacement of the steel section.
        /// Rotate-translate the inertia in the global XY reference system.
        /// </summary>
        /// <returns>Jxx</returns>
        internal double CalculateJxx(in Point2d inertiaPole)
        {
            double JxxG = SectionHelper.CalculateJAlpha(Section.Jxx, Section.Jyy, Section.Jxy, -Rotation);
            var centroid = CalculateCentroid();
            return JxxG + Section.Area * (centroid.Y - inertiaPole.Y) * (centroid.Y - inertiaPole.Y);
        }

        /// <summary>
        /// Calculate the moment of inertia Jyy of the section considering rotation and displacement of the steel section.
        /// Rotate-translate the inertia in the global XY reference system.
        /// </summary>
        /// <returns>Jyy</returns>
        internal double CalculateJyy(in Point2d inertiaPole)
        {
            double JyyG = SectionHelper.CalculateJAlpha(Section.Jxx, Section.Jyy, Section.Jxy, -Rotation + 0.5 * Math.PI);
            var centroid = CalculateCentroid();
            return JyyG + Section.Area * (centroid.X - inertiaPole.X) * (centroid.X - inertiaPole.X);
        }

        /// <summary>
        /// Calculate the product of inertia Jxy of the section considering rotation and displacement of the steel section.
        /// Rotate-translate the inertia in the global XY reference system.
        /// </summary>
        /// <returns>Jxy</returns>
        internal double CalculateJxy(in Point2d inertiaPole)
        {
            double JxyG = SectionHelper.CalculateJxyAlpha(Section.Jxx, Section.Jyy, Section.Jxy, -Rotation);
            var centroid = CalculateCentroid();
            return JxyG + Section.Area * (centroid.X - inertiaPole.X) * (centroid.Y - inertiaPole.Y);
        }

        #endregion
    }
}
