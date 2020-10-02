using System;

namespace GPC.Model.Elements
{
    /// <summary>
    /// GlassPanel abstract class. This represent a single glass panel that can be a part of a insulating glass.
    /// </summary>
    public abstract class GlassPanel : Glass
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the objeect</param>
        protected GlassPanel(Guid guid) : base(guid)
        {

        }
    }
}
