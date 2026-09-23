using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Glass
{
    public sealed class AirChamber : ModelObjectId, IEquatable<AirChamber>, IGlassLayer
    {
        #region Variables

        private double _thickness;

        #endregion

        #region Properties

        public double Thickness { get => _thickness; set => _thickness = value; }

        #endregion

        #region Constructor


        public AirChamber(string name, double thickness)
            : base(name)
        {
            _thickness = thickness > GPC.Geometry.GeometryBase.Tolerance ? thickness : throw new ArgumentException("Air _thickness can't be negative or zero");
        }

        private AirChamber(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _thickness = info.GetDouble("Thickness");
        }

        #endregion

        #region Equals - hashcode - operators - serialization

        public bool Equals(AirChamber other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._thickness.Equals(_thickness) && base.Equals(other);
        }

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Thickness", _thickness);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AirChamber);
        }

        public static bool operator ==(AirChamber obj1, AirChamber obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(AirChamber obj1, AirChamber obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}