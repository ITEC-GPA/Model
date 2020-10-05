
using System;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Monolithic glass. This represent the simpler glass panel. It is composed by a single layer of glass
    /// </summary>
    public class MonolithicGlass : GlassPanel
    {
        public MonolithicGlass() : this(Guid.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the glass</param>
        public MonolithicGlass(Guid guid) : base(guid)
        {
        }
    }
}
