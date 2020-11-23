using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    [Serializable]
    public abstract class ElementProperty : ModelObject
    {

        #region Public Constructors
        protected ElementProperty(Guid guid)
            : base(guid)
        {

        }

        protected ElementProperty()
            : this(Guid.Empty)
        {

        }

        protected ElementProperty(SerializationInfo info, StreamingContext context)
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

