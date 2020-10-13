using System;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Abstract class that represent the interlayer between two monolithic glasses to compose a laminated glass
    /// </summary>
    public abstract class Interlayer : Element
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">The guid of the element</param>
        protected Interlayer(Guid guid) : base(guid)
        {

        }
    }
}
