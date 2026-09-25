using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
    /// A bolt of a grid: position, bolt and hole
    /// </summary>
    [Serializable]
    public class BoltPosition : ModelObjectId, ISerializable
    {
        /// <summary>
        /// The position of the bolt (the insertion point of the hole)
        /// </summary>
        public Point2d Position { get; set; }
        /// <summary>
        /// The bolt
        /// </summary>
        public BoltSection BoltDef { get; set; }
        /// <summary>
        /// The hole
        /// </summary>
        public Hole Hole { get; set; }

        /// <summary>
        /// Creates the bolt position
        /// </summary>
        /// <param name="_pos">The position</param>
        /// <param name="_bol">The bolt</param>
        /// <param name="hole">The hole</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        public BoltPosition(Point2d _pos, BoltSection _bol, Hole hole, int id = IDUNASSIGNED, string name = "")
            : base(id, name)
        {
            Position = _pos;
            BoltDef = _bol;
            Hole = hole;
        }

        /// <summary>
        /// Equality of position, hole and bolt (the id and the name are not compared)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal bolt position</returns>
        public override bool Equals(object obj)
        {
            return obj is BoltPosition other &&
                   EqualityComparer<Point2d>.Default.Equals(Position, other.Position) &&
                   EqualityComparer<Hole>.Default.Equals(Hole, other.Hole) &&
                   EqualityComparer<BoltSection>.Default.Equals(BoltDef, other.BoltDef);
        }

        /// <summary>
        /// The hash code of position, hole and bolt
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + EqualityComparer<Point2d>.Default.GetHashCode(Position);
                hashCode = hashCode * -17 + EqualityComparer<Hole>.Default.GetHashCode(Hole);
                hashCode = hashCode * -17 + EqualityComparer<BoltSection>.Default.GetHashCode(BoltDef);
                return hashCode;
            }
        }

        /// <summary>
        /// Serializes the bolt position
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            int version = 1;
            info.AddValue("Version", version);

            info.AddValue("Position", Position, typeof(Point2d));
            info.AddValue("BoltDef", BoltDef, typeof(BoltSection));
            info.AddValue("Hole", Hole, typeof(Hole));
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected BoltPosition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            double version = info.GetInt32("Version");

            Position = (Point2d)info.GetValue("Position", typeof(Point2d));
            BoltDef = (BoltSection)info.GetValue("BoltDef", typeof(BoltSection));
            Hole = (Hole)info.GetValue("Hole", typeof(Hole));
        }

        /// <summary>
        /// Calculate the centers in the case of slotted hole: at SlotLength (1 + PosBolt) before and SlotLength (1 - PosBolt) after the
        /// position along the slot (their distance is 2 SlotLength, while <see cref="Hole.SlotLength"/> is the distance between the centers)
        /// </summary>
        /// <returns>The two centers</returns>
        public Point2d[] CalculateSlottedCenters()
        {
            double d1 = Hole.SlotLength * (1.0 + Hole.PosBolt); // Distance of the first center from the insertion point.
            double d2 = Hole.SlotLength * (1.0 - Hole.PosBolt); // Distance of the second center from the insertion point.
            var retPoints = new Point2d[2];
            var cosRot = Math.Cos(Hole.Rotation);
            var sinRot = Math.Sin(Hole.Rotation);
            retPoints[0] = new Point2d(Position.X - d1 * cosRot, Position.Y - d1 * sinRot); // First center.
            retPoints[1] = new Point2d(Position.X + d2 * cosRot, Position.Y + d2 * sinRot); // Second center.
            return retPoints;
        }

        /// <summary>
        /// Calculate the centers, two in the case of slotted hole, one for normal holes.
        /// </summary>
        /// <returns>The centers</returns>
        public Point2d[] CalculateCenters()
        {
            Point2d[] centers;
            if (Hole.IsSlotted)
                centers = CalculateSlottedCenters();
            else
                centers = new Point2d[] { Position };

            return centers;
        }
    }
}
