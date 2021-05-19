using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class ResultType : ModelObject, ISerializable
    {
        protected readonly CoordinateSystem _coordinateSystem;

        protected ResultType(CoordinateSystem coordinateSystem) : base()
        {
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
        }

        protected ResultType(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode();
            }
        }

        public static bool operator ==(ResultType obj1, ResultType obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultType obj1, ResultType obj2)
        {
            return !(obj1 == obj2);
        }
    }
}