using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// InsulatingGlass abstract class. This represent an insulating glass that is an assembly of glassPanel separated by air.
    /// </summary>
    [Serializable]
    public abstract class InsulatingGlass : Glass
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the object</param>
        public InsulatingGlass(Guid guid) : base(guid)
        {

        }
        public InsulatingGlass(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
