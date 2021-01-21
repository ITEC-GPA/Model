using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    public abstract class Glass : ModelObject
    {

        protected Glass(Guid guid) 
            : base(guid)
        {

        }

        protected Glass(Guid guid, string name) 
            : base(guid, name)
        {

        }

        protected Glass(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
