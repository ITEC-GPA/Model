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
    }
}