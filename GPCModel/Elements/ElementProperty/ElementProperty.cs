using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    [Serializable]
    public abstract class ElementProperty : ModelObject, IEquatable<ElementProperty>
    {

        #region Public Constructors
        protected ElementProperty(string name, Guid guid)
            : base(guid, name)
        {

        }

        protected ElementProperty(Guid guid)
            : this(string.Empty, guid)
        {

        }

        protected ElementProperty()
            : this(string.Empty, Guid.NewGuid())
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

        public bool Equals(ElementProperty other)
        {
            return !(other is null) && _name == other._name; 
        }
    }
}

