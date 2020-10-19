using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// This represent a triple glazing panel composed by three glass panels separated by air.
    /// </summary>
    [Serializable]
    public class TripleInsulatingGlass : InsulatingGlass
    {
        #region Variables
        private readonly GlassPanel _glassPanelOuter;
        private readonly double _airThicknessOuter;
        private readonly GlassPanel _glassPanelCentral;
        private readonly double _airThicknessInner;
        private readonly GlassPanel _glassPanelInner;
        #endregion

        #region Properties 
        public GlassPanel GlassPanelOuter => _glassPanelOuter;
        public double AirThicknessOuter => _airThicknessOuter;
        public GlassPanel GlassPanelCentral => _glassPanelCentral;
        public double AirThicknessInner => _airThicknessInner;
        public GlassPanel GlassPanelInner => _glassPanelInner;
        #endregion

        #region Public constructor
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
            if (airThicknessOuter < 0 || airThicknessInner < 0)
                throw new ArgumentOutOfRangeException("Air thickness can't be negative");
            
            this._glassPanelOuter = glassPanelOuter ?? throw new ArgumentException("Outer Glass panel can't be null");
            this._glassPanelCentral = glassPanelCentral ?? throw new ArgumentException("Central Glass panel can't be null");
            this._glassPanelInner = glassPanelInner ?? throw new ArgumentException("Inner Glass panel can't be null"); 

            this._airThicknessOuter = airThicknessOuter;
            this._airThicknessInner = airThicknessInner;
        }

        public TripleInsulatingGlass(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            _glassPanelOuter = (GlassPanel)info.GetValue("GlassPanelOuter", typeof(GlassPanel));
            _glassPanelCentral = (GlassPanel)info.GetValue("GlassPanelCentral", typeof(GlassPanel));
            _glassPanelInner = (GlassPanel)info.GetValue("GlassPanelInner", typeof(GlassPanel));
            _airThicknessOuter = info.GetDouble("AirThicknessOuter");
            _airThicknessInner = info.GetDouble("AirThicknessInner");
        }

        #endregion 

        #region PUBLIC METHODS
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassPanelOuter", _glassPanelOuter);
            info.AddValue("GlassPanelCentral", _glassPanelCentral);
            info.AddValue("GlassPanelInner", _glassPanelInner);
            info.AddValue("AirThicknessOuter", _airThicknessOuter);
            info.AddValue("AirThicknessInner", _airThicknessInner);
        }
        #endregion
    }
}
