using System;
using System.Runtime.Serialization;

namespace GPC.Model.FreedomCases
{
    [Serializable]
    public class FreedomCase : ModelObject, ISerializable
    {
        public FreedomCase(string name)
            : base(Guid.NewGuid(), name)
        {
        }

        public FreedomCase(Guid guid, string name)
            : base(guid, name)
        {
        }

        public FreedomCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 17 + base.GetHashCode();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}