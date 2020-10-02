using System;

namespace GPC.Model.Elements
{
    /// <summary>
    /// InsulatingGlass abstract class. This represent an insulating glass that is an assembly of glassPanel separated by air.
    /// </summary>
    public abstract class InsulatingGlass : Glass
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the object</param>
        public InsulatingGlass(Guid guid) : base(guid)
        {

        }
    }
}
