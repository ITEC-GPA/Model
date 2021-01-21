using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// This represent a triple glazing panel composed by three glass panels separated by air.
    /// </summary>
    [Serializable]
    public class TripleInsulatingGlass : Glass, IInsulatingGlass
    {
        #region Variables

        protected readonly IGlassPanel _glassPanelOuter;
        protected readonly double _airThicknessOuter;
        protected readonly IGlassPanel _glassPanelCentral;
        protected readonly double _airThicknessInner;
        protected readonly IGlassPanel _glassPanelInner;

        #endregion Variables

        #region Properties

        public IGlassPanel GlassPanelOuter => _glassPanelOuter;
        public double AirThicknessOuter => _airThicknessOuter;
        public IGlassPanel GlassPanelCentral => _glassPanelCentral;
        public double AirThicknessInner => _airThicknessInner;
        public IGlassPanel GlassPanelInner => _glassPanelInner;

        #endregion Properties

        #region Public constructor

        /// <summary>
        ///
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="glassPanelCentral">Central glass panel</param>
        /// <param name="airThicknessOuter">Outer air thickness</param>
        /// <param name="airThicknessInner">Inner air thickness</param>
        public TripleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelCentral, IGlassPanel glassPanelInner, double airThicknessOuter, double airThicknessInner)
            : this(name, glassPanelOuter, glassPanelCentral, glassPanelInner, airThicknessOuter, airThicknessInner, Guid.Empty)
        {

        }

        public TripleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelCentral, IGlassPanel glassPanelInner, double airThicknessOuter, double airThicknessInner, Guid guid)
            : base(guid, name)
        {
            if (airThicknessOuter < 0.001 || airThicknessInner < 0.001)
                throw new ArgumentOutOfRangeException("Air thickness can't be negative or zero");

            this._glassPanelOuter = glassPanelOuter ?? throw new ArgumentException("Outer Glass panel can't be null");
            this._glassPanelCentral = glassPanelCentral ?? throw new ArgumentException("Central Glass panel can't be null");
            this._glassPanelInner = glassPanelInner ?? throw new ArgumentException("Inner Glass panel can't be null");

            this._airThicknessOuter = airThicknessOuter;
            this._airThicknessInner = airThicknessInner;
        }

        public TripleInsulatingGlass(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            _glassPanelOuter = (IGlassPanel)info.GetValue("GlassPanelOuter", typeof(IGlassPanel));
            _glassPanelCentral = (IGlassPanel)info.GetValue("GlassPanelCentral", typeof(IGlassPanel));
            _glassPanelInner = (IGlassPanel)info.GetValue("GlassPanelInner", typeof(IGlassPanel));
            _airThicknessOuter = info.GetDouble("AirThicknessOuter");
            _airThicknessInner = info.GetDouble("AirThicknessInner");
        }

        #endregion Public constructor

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

        #endregion PUBLIC METHODS
    }
}