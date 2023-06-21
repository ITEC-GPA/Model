using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace GPC.Model.Sections.Steel
{
    /// <summary>
    /// Generic steel section and its displacement including:
    /// 1) rotation around a point;
    /// 2) translation.
    /// Operations done in this order.
    /// </summary>
    public class SteelSectionPosition
    {
        #region Properties

        public ThinWallSection Section { get; set; }

        /// <summary>
        /// Center of rotation.
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

        #endregion

        #region Constructor

        public SteelSectionPosition(ThinWallSection thinWallSect, Point2d rotationCenter, double rotation, Vector2d traslation)
        {
            Section = thinWallSect ?? throw new ArgumentNullException(nameof(thinWallSect));
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

        #endregion
    }
}
