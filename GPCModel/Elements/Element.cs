using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Element base abstract class that is the base for all the objects inside GPC environment.
    /// </summary>

    [Serializable]
    public abstract class Element : ModelObject
    {
        #region Public Constructors
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
    }
}