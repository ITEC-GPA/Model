using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Element base abstract class that is the base for all the objects inside GPC environment.
    /// </summary>

    [Serializable]
    public abstract class Element : ModelObject
    {

        private int _id;

        public int Id { get => _id; set => _id = value; }

        #region Public Constructors

        protected Element() : base(Guid.NewGuid())
        {

        }

        protected Element(Guid guid)
            : base(guid)
        {

        }

        protected Element(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion Public Constructors

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            Element objCasted = obj as Element;
            return !(objCasted is null) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Element obj1, Element obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Element obj1, Element obj2)
        {
            return !(obj1 == obj2);
        }
    }
}