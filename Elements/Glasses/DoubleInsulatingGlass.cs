using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// This represent a double glazing panel composed by two glass panels separated by air.
    /// </summary>
    [Serializable]
    public class DoubleInsulatingGlass : InsulatingGlass
    {
        #region Variables
        private readonly GlassPanel _glassPanelOuter;
        private readonly double _airThickness;
        private readonly GlassPanel _glassPanelInner;
        #endregion

        #region Properties
        public GlassPanel GlassPanelOuter => _glassPanelOuter;
        public double AirThickness => _airThickness;
        public GlassPanel GlassPanelInner => _glassPanelInner;
        #endregion

        #region Public constructor
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
            if (airThickness < 0)
                throw new ArgumentOutOfRangeException("Air thickness can't be negative");

            this._glassPanelOuter = glassPanelOuter ?? throw new ArgumentException("Outer Glass panel can't be null");
            this._glassPanelInner = glassPanelInner ?? throw new ArgumentException("Inner Glass panel can't be null");

            this._airThickness = airThickness;
        }

        public DoubleInsulatingGlass(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            _glassPanelOuter = (GlassPanel)info.GetValue("GlassPanelOuter", typeof(GlassPanel));
            _glassPanelInner = (GlassPanel)info.GetValue("GlassPanelInner", typeof(GlassPanel));
            _airThickness = info.GetDouble("AirThickness");
        }

        #endregion 

        #region PUBLIC METHODS
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassPanelOuter", _glassPanelOuter);
            info.AddValue("GlassPanelInner", _glassPanelInner);
            info.AddValue("AirThickness", _airThickness);
        }

        #endregion 
    }
}
