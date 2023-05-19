using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static GPC.Model.Sections.Section;

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
    }
}
