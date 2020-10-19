using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Glass base abstract class that is the base for all the glasses inside GPC environment.
    /// </summary>
    [Serializable]
    public abstract class Glass : Element
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the object</param>
        protected Glass(Guid guid) : base(guid)
        {

        }
        public Glass(SerializationInfo info, StreamingContext context) 
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
