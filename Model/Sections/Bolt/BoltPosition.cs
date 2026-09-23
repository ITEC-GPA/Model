using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Bolt
{
    [Serializable]
    public class BoltPosition : ModelObjectId, ISerializable
    {
        public Point2d Position { get; set; }
        public BoltSection BoltDef { get; set; }
        public Hole Hole { get; set; }

        public BoltPosition(Point2d _pos, BoltSection _bol, Hole hole, int id = IDUNASSIGNED, string name = "")
            : base(id, name)
        {
            Position = _pos;
            BoltDef = _bol;
            Hole = hole;
        }

        public override bool Equals(object obj)
        {
            return obj is BoltPosition other &&
                   EqualityComparer<Point2d>.Default.Equals(Position, other.Position) &&
                   EqualityComparer<Hole>.Default.Equals(Hole, other.Hole) &&
                   EqualityComparer<BoltSection>.Default.Equals(BoltDef, other.BoltDef);
        }

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            int version = 1;
            info.AddValue("Version", version);

            info.AddValue("Position", Position, typeof(Point2d));
            info.AddValue("BoltDef", BoltDef, typeof(BoltSection));
            info.AddValue("Hole", Hole, typeof(Hole));
        }

        protected BoltPosition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            double version = info.GetInt32("Version");

            Position = (Point2d)info.GetValue("Position", typeof(Point2d));
            BoltDef = (BoltSection)info.GetValue("BoltDef", typeof(BoltSection));
            Hole = (Hole)info.GetValue("Hole", typeof(Hole));
        }

        /// <summary>
        /// Calculate the centers in the case of slotted hole.
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
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
