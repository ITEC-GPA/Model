using GPC.Geometry;
using GPC.Utilities.Extensions;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Steel
{
    /// <summary>
    /// Indexes used by SAP2000.
    /// </summary>
    public enum InsertionPointType
    {
        /// <summary>
        /// Bottom left corner of the bounding box (default)
        /// </summary>
        BottomLeft = 1,
        /// <summary>
        /// Bottom side, at the X of the middle point
        /// </summary>
        BottomCenter = 2,
        /// <summary>
        /// Bottom right corner of the bounding box
        /// </summary>
        BottomRight = 3,
        /// <summary>
        /// Left side, at the Y of the middle point
        /// </summary>
        MiddleLeft = 4,
        /// <summary>
        /// The middle point (see <see cref="MiddleCenterType"/>)
        /// </summary>
        MiddleCenter = 5,
        /// <summary>
        /// Right side, at the Y of the middle point
        /// </summary>
        MiddleRight = 6,
        /// <summary>
        /// Top left corner of the bounding box
        /// </summary>
        TopLeft = 7,
        /// <summary>
        /// Top side, at the X of the middle point
        /// </summary>
        TopCenter = 8,
        /// <summary>
        /// Top right corner of the bounding box
        /// </summary>
        TopRight = 9,
        /// <summary>
        /// The centroid: it's like using MiddleCenter and MiddleCenterType == Centroid
        /// </summary>
        Centroid = 10,
        /// <summary>
        /// The shear center: it's like using MiddleCenter and MiddleCenterType == ShearCenter
        /// </summary>
        ShearCenter = 11
    }

    /// <summary>
    /// Choice affecting the position of midpoints.
    /// </summary>
    public enum MiddleCenterType
    {
        /// <summary>
        /// The middle of the bounding box (default)
        /// </summary>
        Midpoint = 0,
        /// <summary>
        /// The centroid
        /// </summary>
        Centroid = 1,
        /// <summary>
        /// The shear center
        /// </summary>
        ShearCenter = 2
    }

    /// <summary>
    /// Generic steel section and its displacement including:
    /// 1) Section translation from InsertionPoint to origin (0, 0).
    /// 2) Rotation around a point RotationCenter;
    /// 3) Traslation.
    /// Operations done in this order.
    /// </summary>
    [Serializable]
    public class SteelSectionPosition : ISerializable
    {
        #region Fields

        /// <summary>
        /// The rotation (radians)
        /// </summary>
        private double _rotation;
        /// <summary>
        /// The insertion point type
        /// </summary>
        private InsertionPointType _cardinalPoint;
        /// <summary>
        /// The type of the middle points
        /// </summary>
        private MiddleCenterType _middleCenter;
        /// <summary>
        /// The insertion point (local coordinates of the section)
        /// </summary>
        private Point2d _insertionPoint;

        #endregion

        #region Properties

        /// <summary>
        /// The steel section
        /// </summary>
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
        public double Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        /// <summary>
        /// Utility. Angle of rotation in sexagesimal degrees.
        /// </summary>
        public double RotationDegrees
        {
            get => _rotation.ToDegrees();
            set => _rotation = value.ToRadians();
        }

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

        /// <summary>
        /// Profile insertion point, at present all sections are drawn entirely in the first quadrant,
        /// with positive X and Y, thus with insertion point in the lower left.
        /// The profile is drawn with the origin (0, 0) in the lower left corner, so default is BottomLeft.
        /// </summary>
        public InsertionPointType CardinalPoint
        {
            get => _cardinalPoint;
            set
            {
                if (_cardinalPoint != value)
                {
                    _cardinalPoint = value;
                    _insertionPoint = GetInsertionPoint();
                }
            }
        }

        /// <summary>
        /// Choice affecting the position of midpoints.
        /// </summary>
        public MiddleCenterType MiddleCenter
        {
            get => _middleCenter;
            set
            {
                if (_middleCenter != value)
                {
                    _middleCenter = value;
                    _insertionPoint = GetInsertionPoint();
                }
            }
        }

        /// <summary>
        /// Position of insertion point, depends on CardinalPoint and MiddleCenter.
        /// </summary>
        public Point2d InsertionPoint => _insertionPoint;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the position of a steel section
        /// </summary>
        /// <param name="steelSection">The steel section</param>
        /// <param name="rotationCenter">The center of rotation (null: the origin)</param>
        /// <param name="rotation">The rotation (radians, counterclockwise)</param>
        /// <param name="traslation">The translation (null: none)</param>
        /// <param name="cardinalPoint">The insertion point type</param>
        /// <param name="middleCenter">The type of the middle points</param>
        /// <exception cref="ArgumentNullException">If <paramref name="steelSection"/> is null</exception>
        public SteelSectionPosition(SteelSection steelSection, Point2d rotationCenter, double rotation, Vector2d traslation, InsertionPointType cardinalPoint = InsertionPointType.BottomLeft, MiddleCenterType middleCenter = MiddleCenterType.Midpoint)
        {
            Section = steelSection ?? throw new ArgumentNullException(nameof(steelSection));
            RotationCenter = rotationCenter ?? Point2d.Origin;
            Rotation = rotation;
            Traslation = traslation ?? new Vector2d(0.0, 0.0);
            CardinalPoint = cardinalPoint;
            MiddleCenter = middleCenter;
            IsInsideConcrete = true;
        }

        /// <summary>
        /// Deserialization constructor (version 1: insertion point bottom left)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SteelSectionPosition(SerializationInfo info, StreamingContext context)
        {
            int version = info.GetInt32("SteelSectionPositionVersion");

            IsInsideConcrete = info.GetBoolean("IsInsideConcrete");
            Rotation = info.GetDouble("Rotation");
            RotationCenter = (Point2d)info.GetValue("RotationCenter", typeof(Point2d));
            Section = (SteelSection)info.GetValue("Section", typeof(SteelSection));
            Traslation = (Vector2d)info.GetValue("Traslation", typeof(Vector2d));
            if (version >= 2)
            {
                CardinalPoint = (InsertionPointType)info.GetValue("CardinalPoint", typeof(InsertionPointType));
                MiddleCenter = (MiddleCenterType)info.GetValue("MiddleCenter", typeof(MiddleCenterType));
            }
            else
            {
                CardinalPoint = InsertionPointType.BottomLeft;
                MiddleCenter = MiddleCenterType.Midpoint;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The middle point of the section: the middle of the bounding box, the centroid or the shear center (see <see cref="MiddleCenter"/>)
        /// </summary>
        /// <returns>The point</returns>
        private Point2d GetMiddleCenterPoint()
        {
            switch (MiddleCenter)
            {
                case MiddleCenterType.Centroid:
                    return Section.Centroid;
                case MiddleCenterType.ShearCenter:
                    return Section.ShearCenter;
                case MiddleCenterType.Midpoint:
                default:
                    return new Point2d(0.5 * Section.Width, 0.5 * Section.Height);
            }
        }

        /// <summary>
        /// The insertion point from <see cref="CardinalPoint"/> and <see cref="MiddleCenter"/>
        /// </summary>
        /// <returns>The point</returns>
        private Point2d GetInsertionPoint()
        {
            switch (CardinalPoint)
            {
                case InsertionPointType.BottomLeft:
                    return Point2d.Origin;

                case InsertionPointType.BottomCenter:
                    {
                        var midPoint = GetMiddleCenterPoint();
                        return new Point2d(midPoint.X, 0.0);
                    }
                case InsertionPointType.BottomRight:
                    return new Point2d(Section.Width, 0.0);
                case InsertionPointType.MiddleLeft:
                    {
                        var midPoint = GetMiddleCenterPoint();
                        return new Point2d(0.0, midPoint.Y);
                    }
                case InsertionPointType.MiddleCenter:
                    return GetMiddleCenterPoint();
                case InsertionPointType.MiddleRight:
                    {
                        var midPoint = GetMiddleCenterPoint();
                        return new Point2d(Section.Width, midPoint.Y);
                    }
                case InsertionPointType.TopLeft:
                    return new Point2d(0.0, Section.Height);
                case InsertionPointType.TopCenter:
                    {
                        var midPoint = GetMiddleCenterPoint();
                        return new Point2d(midPoint.X, Section.Height);
                    }
                case InsertionPointType.TopRight:
                    return new Point2d(Section.Width, Section.Height);
                case InsertionPointType.Centroid:
                    return Section.Centroid;
                case InsertionPointType.ShearCenter:
                    return Section.ShearCenter;
                default:
                    return Point2d.Origin;
            }
        }

        /// <summary>
        /// Given a point in the local system of the ThinWallSection return the point in the global system: moved from the insertion point to the
        /// origin, rotated around <see cref="RotationCenter"/> and translated.
        /// </summary>
        /// <param name="point2D">The local point</param>
        /// <returns>A new global point</returns>
        public Point2d PositionToGlobal(in Point2d point2D)
        {
            var globPoint2d = (Point2d)point2D.Clone();
            globPoint2d.Move(-_insertionPoint.X, -_insertionPoint.Y);
            globPoint2d.Rotate(RotationCenter, Rotation);
            globPoint2d.Move(Traslation);
            return globPoint2d;
        }

        /// <summary>
        /// Given a point in the global system return the point in the local system of the ThinWallSection (inverse of <see cref="PositionToGlobal"/>).
        /// </summary>
        /// <param name="point2D">The global point</param>
        /// <returns>A new local point</returns>
        public Point2d PositionToLocal(in Point2d point2D)
        {
            var localPoint2d = (Point2d)point2D.Clone();
            localPoint2d.Move(-Traslation.X, -Traslation.Y);
            localPoint2d.Rotate(RotationCenter, -Rotation);
            localPoint2d.Move(_insertionPoint.X, _insertionPoint.Y);
            return localPoint2d;
        }

        /// <summary>
        /// The area of the section
        /// </summary>
        /// <returns>The area</returns>
        internal double CalculateArea() => Section.Area;

        /// <summary>
        /// The centroid in global coordinates
        /// </summary>
        /// <returns>The centroid</returns>
        internal Point2d CalculateCentroid() => PositionToGlobal(Section.Centroid);

        /// <summary>
        /// Calculate the moment of inertia Jxx of the section considering rotation and displacement of the steel section.
        /// Rotate-translate the inertia in the global XY reference system.
        /// </summary>
        /// <param name="inertiaPole">The point of the axis</param>
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
        /// <param name="inertiaPole">The point of the axis</param>
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
        /// <param name="inertiaPole">The point of the axes</param>
        /// <returns>Jxy</returns>
        internal double CalculateJxy(in Point2d inertiaPole)
        {
            double JxyG = SectionHelper.CalculateJxyAlpha(Section.Jxx, Section.Jyy, Section.Jxy, -Rotation);
            var centroid = CalculateCentroid();
            return JxyG + Section.Area * (centroid.X - inertiaPole.X) * (centroid.Y - inertiaPole.Y);
        }

        /// <summary>
        /// Serializes the position (version 2)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int version = 2;
            info.AddValue("SteelSectionPositionVersion", version);

            info.AddValue("IsInsideConcrete", IsInsideConcrete);
            info.AddValue("Rotation", Rotation);
            info.AddValue("RotationCenter", RotationCenter, typeof(Point2d));
            info.AddValue("Section", Section, typeof(SteelSection));
            info.AddValue("Traslation", Traslation, typeof(Vector2d));
            info.AddValue("CardinalPoint", CardinalPoint, typeof(InsertionPointType));
            info.AddValue("MiddleCenter", MiddleCenter, typeof(MiddleCenterType));
        }

        #endregion
    }
}
