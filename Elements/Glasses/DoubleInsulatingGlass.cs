using System;

namespace GPC.Model.Elements
{
    /// <summary>
    /// This represent a double glazing panel composed by two glass panels separated by air.
    /// </summary>
    public class DoubleInsulatingGlass : InsulatingGlass
    {
        private readonly GlassPanel _glassPanelOuter;
        private readonly double _airThickness;
        private readonly GlassPanel _glassPanelInner;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="airThickness">air gap</param>
        public DoubleInsulatingGlass(GlassPanel glassPanelOuter, GlassPanel glassPanelInner, double airThickness) : this(glassPanelOuter, glassPanelInner, airThickness, Guid.Empty)
        {            

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Outer glass panel</param>
        /// <param name="airThickness">air gap</param>
        /// <param name="guid">The guid of the objec</param>
        public DoubleInsulatingGlass(GlassPanel glassPanelOuter, GlassPanel glassPanelInner, double airThickness, Guid guid) : base(guid)
        {
            this._glassPanelOuter = glassPanelOuter;
            this._glassPanelInner = glassPanelInner;
            this._airThickness = airThickness;
        }
    }
}
