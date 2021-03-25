using System;
using System.Runtime.Serialization;

namespace GPC.Model.FEM.Properties
{
    [Serializable]
    public abstract class ElementProperty : ModelObject
    {
        #region Public Constructors

        protected ElementProperty()
            : this(string.Empty, Guid.NewGuid())
        {
        }

        protected ElementProperty(string name)
            : base(Guid.NewGuid(), name)
        {
        }

        protected ElementProperty(Guid guid)
            : this(string.Empty, guid)
        {
        }

        protected ElementProperty(string name, Guid guid)
            : base(guid, name)
        {
        }

        protected ElementProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion 

        public override void GetObjectData(SerializationInfo info, StreamingContext context)

        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            ElementProperty objCasted = obj as ElementProperty;
            return !(objCasted is null) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ElementProperty obj1, ElementProperty obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ElementProperty obj1, ElementProperty obj2)
        {
            return !(obj1 == obj2);
        }
    }
}