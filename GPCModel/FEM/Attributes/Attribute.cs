using System;
using System.Runtime.Serialization;

namespace GPC.Model.FEM.Attributes
{
    [Serializable]
    public abstract class Attribute : ModelObject, ISerializable, ICloneable
    {
        protected Attribute(string name)
            : base(name)
        {

        }

        protected Attribute(Guid guid, string name) 
            : base(guid, name)
        {

        }

        protected Attribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public abstract object Clone();

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }



        #region Override Operator

        public static bool operator ==(Attribute obj1, Attribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Attribute obj1, Attribute obj2)
        {
            return !(obj1 == obj2);
        } 

        #endregion
    }
}