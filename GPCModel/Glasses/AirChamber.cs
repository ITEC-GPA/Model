using System;
using System.Runtime.Serialization;

namespace GPC.Model.Glasses
{
    public sealed class AirChamber : ModelObject, IGlassPackage, IEquatable<AirChamber>
    {
        private double _thickness;

        public double Thickness => _thickness;

        public AirChamber(string name, double thickness) : base(name)
        {
            _thickness = thickness > 0.001 ? thickness : throw new ArgumentException("Air thickness can't be negative or zero");
        }

        public AirChamber(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public bool Equals(AirChamber other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._thickness.Equals(_thickness) && base.Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _thickness.GetHashCode();
            return hashCode;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
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


    }
}