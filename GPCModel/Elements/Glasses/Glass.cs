using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Glass base abstract class that is the base for all the glasses inside GPC environment.
    /// </summary>
    [Serializable]
    public abstract class Glass : ElementProperty
    {

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">The guid of the object</param>
        /// <param name="glassMaterial"></param>
        protected Glass(GlassMaterial glassMaterial, Guid guid)
            : base(glassMaterial, guid)
        {

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