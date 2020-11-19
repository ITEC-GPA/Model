using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    [Serializable]
    public abstract class GlassProperty : ElementProperty
    {
        public GlassProperty(Guid guid) 
            : base(guid)
        {

        }

        public GlassProperty(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
