using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Bolt
{
    [Serializable]
    public class Hole : IEquatable<Hole>, ISerializable
    {
        #region Properties

        public double Diameter { get; set; }

        /// <summary>
        /// Rotation in radian, counterclockwise and zero in positive x-axis.
        /// </summary>
        public double Rotation { get; set; }

        /// <summary>
        /// For slotted holes, distance between centers. For circular hole this is equal to zero.
        /// </summary>
        public double SlotLength { get; set; }

        /// <summary>
        /// Adimensional position of bolt center, -1 to 1, 0 to use middle point.
        /// </summary>
        public double PosBolt { get; set; }

        public double MaxLength => Diameter + SlotLength;

        /// <summary>
        /// True for slotted hole.
        /// </summary>
        public bool IsSlotted => SlotLength > 0.01;

        #endregion

        #region Constructor

        public Hole(in double diameter)
        {
            Diameter = diameter;
            Rotation = 0;
            SlotLength = 0;
            PosBolt = 0;
        }

        public Hole(SerializationInfo info, StreamingContext context)
        {
            double version;
            version = info.GetInt32("Version");
            Diameter = info.GetDouble("Diameter");
            Rotation = info.GetDouble("Rotation");
            SlotLength = info.GetDouble("SlotLength");
            PosBolt = info.GetDouble("PosBolt");
        }

        #endregion

        #region Method

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int version = 1;
            info.AddValue("Version", version);
            info.AddValue("Diameter", Diameter);
            info.AddValue("Rotation", Rotation);
            info.AddValue("SlotLength", SlotLength);
            info.AddValue("PosBolt", PosBolt);
        }

        #endregion

        #region Comparer

        public override bool Equals(object obj)
        {
            return Equals(obj as Hole);
        }

        public bool Equals(Hole other) => Equals(other, 0.01);

        public bool Equals(Hole other, double tolerance = 0.01)
        {
            return !(other is null) &&
                   Math.Abs(Diameter - other.Diameter) < tolerance &&
                   Math.Abs(Rotation - other.Rotation) < tolerance &&
                   Math.Abs(SlotLength - other.SlotLength) < tolerance &&
                   Math.Abs(PosBolt - other.PosBolt) < tolerance;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -107;
                hashCode = hashCode * -67 + Diameter.GetHashCode();
                hashCode = hashCode * -67 + Rotation.GetHashCode();
                hashCode = hashCode * -67 + SlotLength.GetHashCode();
                hashCode = hashCode * -67 + PosBolt.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Hole left, Hole right)
        {
            return EqualityComparer<Hole>.Default.Equals(left, right);
        }

        public static bool operator !=(Hole left, Hole right)
        {
            return !(left == right);
        }

        #endregion
    }
}
