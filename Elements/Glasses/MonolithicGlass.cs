
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Monolithic glass. This represent the simpler glass panel. It is composed by a single layer of glass
    /// </summary>
    [Serializable]
    public class MonolithicGlass : GlassPanel
    {
        #region CONSTRUCTORS
        public MonolithicGlass() 
            : this(Guid.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the glass</param>
        public MonolithicGlass(Guid guid) 
            : base(guid)
        {
        }

        public MonolithicGlass(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region PUBLIC METHODS
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
        #endregion
    }
}
