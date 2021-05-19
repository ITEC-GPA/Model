using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultLocationId : ModelObjectId, ISerializable
    {
        public ResultLocationId(int id) : this(id, "")
        {

        }

        public ResultLocationId(int id, string name) : base(id, name)
        {

        }

        public ResultLocationId(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            return obj is ResultLocationId point && base.Equals(point);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ResultLocationId obj1, ResultLocationId obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultLocationId obj1, ResultLocationId obj2)
        {
            return !(obj1 == obj2);
        }
    }
}