using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Glass base abstract class that is the base for all the glasses inside GPC environment.
    /// </summary>
    [Serializable]
    public abstract class Glass : Element
    {
        protected GlassMaterial _glassMaterial;

        public GlassMaterial GlassMaterial => _glassMaterial;

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">The guid of the object</param>
        protected Glass(GlassMaterial glassMaterial, Guid guid)
            : base(guid)
        {
            this._glassMaterial = glassMaterial;
        }

        protected Glass(SerializationInfo info, StreamingContext context)
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