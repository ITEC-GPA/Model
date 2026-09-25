using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Glass
{
    /// <summary>
    /// The air chamber between two glass panels of an insulating glass (the class is not [Serializable]: a glass with an air chamber cannot be serialized by the formatter)
    /// </summary>
    public sealed class AirChamber : ModelObjectId, IEquatable<AirChamber>, IGlassLayer
    {
        #region Variables

        /// <summary>
        /// The thickness
        /// </summary>
        private double _thickness;

        #endregion

        #region Properties

        /// <summary>
        /// The thickness (the setter does not check the value)
        /// </summary>
        public double Thickness { get => _thickness; set => _thickness = value; }

        #endregion

        #region Constructor


        /// <summary>
        /// Creates the air chamber
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="thickness">The thickness</param>
        /// <exception cref="ArgumentException">If <paramref name="thickness"/> is not bigger than the tolerance</exception>
        public AirChamber(string name, double thickness)
            : base(name)
        {
            _thickness = thickness > GPC.Geometry.GeometryBase.Tolerance ? thickness : throw new ArgumentException("Air _thickness can't be negative or zero");
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private AirChamber(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _thickness = info.GetDouble("Thickness");
        }

        #endregion

        #region Equals - hashcode - operators - serialization

        /// <summary>
        /// Equality of the thickness and of the name
        /// </summary>
        /// <param name="other">The air chamber to compare</param>
        /// <returns>True if the air chambers are equal</returns>
        public bool Equals(AirChamber other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._thickness.Equals(_thickness) && base.Equals(other);
        }

        /// <summary>
        /// The hash code of the name and of the thickness
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _thickness.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Serializes the air chamber
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Thickness", _thickness);
        }

        /// <summary>
        /// Equality with another air chamber (see <see cref="Equals(AirChamber)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal air chamber</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AirChamber);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(AirChamber)"/>)
        /// </summary>
        /// <param name="obj1">The first air chamber</param>
        /// <param name="obj2">The second air chamber</param>
        /// <returns>True if the air chambers are equal</returns>
        public static bool operator ==(AirChamber obj1, AirChamber obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(AirChamber)"/>)
        /// </summary>
        /// <param name="obj1">The first air chamber</param>
        /// <param name="obj2">The second air chamber</param>
        /// <returns>True if the air chambers are different</returns>
        public static bool operator !=(AirChamber obj1, AirChamber obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}