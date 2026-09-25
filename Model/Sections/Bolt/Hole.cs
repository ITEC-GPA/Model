using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
    /// A hole for a bolt, circular or slotted
    /// </summary>
    [Serializable]
    public class Hole : IEquatable<Hole>, ISerializable
    {
        #region Properties

        /// <summary>
        /// The diameter
        /// </summary>
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

        /// <summary>
        /// The overall length: diameter plus slot length
        /// </summary>
        public double MaxLength => Diameter + SlotLength;

        /// <summary>
        /// True for slotted hole.
        /// </summary>
        public bool IsSlotted => SlotLength > 0.01;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a circular hole
        /// </summary>
        /// <param name="diameter">The diameter</param>
        public Hole(in double diameter)
        {
            Diameter = diameter;
            Rotation = 0;
            SlotLength = 0;
            PosBolt = 0;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Serializes the hole
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Equality with another hole (see <see cref="Equals(Hole, double)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal hole</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Hole);
        }

        /// <summary>
        /// Equality with tolerance 0.01 (see <see cref="Equals(Hole, double)"/>)
        /// </summary>
        /// <param name="other">The hole to compare</param>
        /// <returns>True if the holes are equal</returns>
        public bool Equals(Hole other) => Equals(other, 0.01);

        /// <summary>
        /// Equality of diameter, rotation, slot length and bolt position within a tolerance
        /// </summary>
        /// <param name="other">The hole to compare</param>
        /// <param name="tolerance">The tolerance</param>
        /// <returns>True if the holes are equal</returns>
        public bool Equals(Hole other, double tolerance = 0.01)
        {
            return !(other is null) &&
                   Math.Abs(Diameter - other.Diameter) < tolerance &&
                   Math.Abs(Rotation - other.Rotation) < tolerance &&
                   Math.Abs(SlotLength - other.SlotLength) < tolerance &&
                   Math.Abs(PosBolt - other.PosBolt) < tolerance;
        }

        /// <summary>
        /// The hash code of the exact values (holes equal within the tolerance can have different hash codes)
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(Hole)"/>)
        /// </summary>
        /// <param name="left">The first hole</param>
        /// <param name="right">The second hole</param>
        /// <returns>True if the holes are equal</returns>
        public static bool operator ==(Hole left, Hole right)
        {
            return EqualityComparer<Hole>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(Hole)"/>)
        /// </summary>
        /// <param name="left">The first hole</param>
        /// <param name="right">The second hole</param>
        /// <returns>True if the holes are different</returns>
        public static bool operator !=(Hole left, Hole right)
        {
            return !(left == right);
        }

        #endregion
    }
}
