using System;

namespace GPC.Model.Elements
{
    /// <summary>
    /// This represent a triple glazing panel composed by three glass panels separated by air.
    /// </summary>
    public class TripleInsulatingGlass : InsulatingGlass
    {
        private readonly GlassPanel _glassPanelOuter;
        private readonly double _airThicknessOuter;
        private readonly GlassPanel _glassPanelCentral;
        private readonly double _airThicknessInner;
        private readonly GlassPanel _glassPanelInner;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="glassPanelCentral">Central glass panel</param>
        /// <param name="airThicknessOuter">Outer air thickness</param>
        /// <param name="airThicknessInner">Inner air thickness</param>
        public TripleInsulatingGlass(GlassPanel glassPanelOuter, GlassPanel glassPanelCentral, GlassPanel glassPanelInner, double airThicknessOuter, double airThicknessInner) 
                                    : this(glassPanelOuter, glassPanelCentral, glassPanelInner, airThicknessOuter, airThicknessInner, Guid.Empty)
        {

        }
        public TripleInsulatingGlass(GlassPanel glassPanelOuter, GlassPanel glassPanelCentral, GlassPanel glassPanelInner, double airThicknessOuter, double airThicknessInner, Guid guid) : base(guid)
        {
            this._glassPanelOuter = glassPanelOuter;
            this._glassPanelCentral = glassPanelCentral;
            this._glassPanelInner = glassPanelInner;

            this._airThicknessOuter = airThicknessOuter;
            this._airThicknessInner = airThicknessInner;
        }
    }
}
