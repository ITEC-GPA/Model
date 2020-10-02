using System;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Glass base abstract class that is the base for all the glasses inside GPC environment.
    /// </summary>
    public abstract class Glass : Element
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the object</param>
        protected Glass(Guid guid) : base(guid)
        {

        }
    }
}
