using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    [Serializable]
    public abstract class ElementProperty : ModelObject
    {
        #region Variables

        protected Material _material;
        #endregion

        #region Properties
        public Material Material => _material;
        #endregion

        #region Public Constructors
        protected ElementProperty(Material material, Guid guid)
            : base(guid)
        {
            this._material = material;
        }

        protected ElementProperty(Material material)
            : this(material, Guid.Empty)
        {

        }

        protected ElementProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion Public Constructors

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Material", _material);
        }
    }
}

