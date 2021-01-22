using System;
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
            return !(other is null) && base.Equals(other);
        }
    }
}