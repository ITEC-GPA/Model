using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// GlassPanel abstract class. This represent a single glass panel that can be a part of a insulating glass.
    /// </summary>
    [Serializable]
    public abstract class GlassPanel : Glass
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">The guid of the objeect</param>
        /// <param name="glassMaterial"></param>      
        protected GlassPanel(GlassMaterial glassMaterial, Guid guid)
            : base(glassMaterial, guid)
        {
        }

        protected GlassPanel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion PUBLIC METHODS
    }
}