using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Abstract class that represent the interlayer between two monolithic glasses to compose a laminated glass
    /// </summary>
    [Serializable]
    public abstract class Interlayer : Element
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the element</param>
        protected Interlayer(Guid guid) : base(guid)
        {

        }

        public Interlayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
