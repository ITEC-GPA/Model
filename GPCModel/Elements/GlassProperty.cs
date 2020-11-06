using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    [Serializable]
    public class GlassProperty : ElementProperty
    {
        public GlassProperty(GlassMaterial material, Guid guid) 
            : base(material, guid)
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
