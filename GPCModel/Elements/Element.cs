using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Element base abstract class that is the base for all the objects inside GPC environment.
    /// </summary>

    [Serializable]
    public abstract class Element : ModelObject, IEquatable<Element>
    {
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

        public bool Equals(Element other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as Element);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _guid.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(_name);
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