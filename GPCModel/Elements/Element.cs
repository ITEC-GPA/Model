using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// This class rapresnet a base class for all the phisical element inside the gpc model library
    /// </summary>

    [Serializable]
    public abstract class Element : ModelObjectId, ISerializable
    {

        protected Element() 
            : base(Guid.NewGuid())
        {

        }

        protected Element(int id)
            : this(id, "", Guid.NewGuid())
        {

        }

        protected Element(Guid guid)
            : base(guid)
        {

        }

        protected Element(int id, Guid guid)
            : this(id, "", guid)
        {

        }

        protected Element(int id, string name, Guid guid)
            : base(id, name, guid)
        {

        }

        protected Element(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _id = info.GetInt32("Id");
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <inheritdoc cref="ModelObjectId.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Element obj1, Element obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(Element obj1, Element obj2)
        {
            return !(obj1 == obj2);
        }
    }
}