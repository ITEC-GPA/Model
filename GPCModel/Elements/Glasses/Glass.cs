using System;
using System.Collections.Generic;
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            Glass objCasted = obj as Glass;
            return !(objCasted is null) && base.Equals(objCasted);
        }


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Glass obj1, Glass obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Glass obj1, Glass obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
