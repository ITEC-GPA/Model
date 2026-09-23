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
        BottomLeft = 1, // default
        BottomCenter = 2,
        BottomRight = 3,
        MiddleLeft = 4,
        MiddleCenter = 5,
        MiddleRight = 6,
        TopLeft = 7,
        TopCenter = 8,
        TopRight = 9,
        Centroid = 10, // it's like using MiddleCenter and MiddleCenterType == Centroid
        ShearCenter = 11 // it's like using MiddleCenter and MiddleCenterType == ShearCenter
    }

    /// <summary>
    /// Choice affecting the position of midpoints.
    /// </summary>
    public enum MiddleCenterType
    {
        Midpoint = 0, // default
        Centroid = 1,
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

        private double _rotation;
        private InsertionPointType _cardinalPoint;
        private MiddleCenterType _middleCenter;
        private Point2d _insertionPoint;

        #endregion

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
        /// Given a point in the local system of the ThinWallSection return the point in the global system.
        /// </summary>
        /// <param name="point2D"></param>
        /// <returns></returns>
        public Point2d PositionToGlobal(in Point2d point2D)
        {
            var globPoint2d = (Point2d)point2D.Clone();
            globPoint2d.Move(-_insertionPoint.X, -_insertionPoint.Y);
            globPoint2d.Rotate(RotationCenter, Rotation);
            globPoint2d.Move(Traslation);
            return globPoint2d;
        }

        /// <summary>
        /// Given a point in the global system return the point in the local system of the ThinWallSection.
        /// </summary>
        /// <param name="point2D"></param>
        /// <returns></returns>
        public Point2d PositionToLocal(in Point2d point2D)
        {
            var localPoint2d = (Point2d)point2D.Clone();
            localPoint2d.Move(-Traslation.X, -Traslation.Y);
            localPoint2d.Rotate(RotationCenter, -Rotation);
            localPoint2d.Move(_insertionPoint.X, _insertionPoint.Y);
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
