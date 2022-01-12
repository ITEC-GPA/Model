using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class ResultType : ModelObjectId, ISerializable
    {
        protected readonly CoordinateSystem _coordinateSystem;

        public CoordinateSystem CoordinateSystem => _coordinateSystem;


        protected ResultType(CoordinateSystem coordinateSystem, string name = "", int id = ModelObjectId.IDUNASSIGNED)
            : base(id, name)
        {
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
        }


        protected ResultType(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return base.Equals(obj); // Coordinate system non messo per scelta
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