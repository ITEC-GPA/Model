using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// This represent a triple glazing panel composed by three glass panels separated by air.
    /// </summary>
    [Serializable]
    public class TripleInsulatingGlass : GlassProperty, IInsulatingGlassProperty
    {
        #region Variables

        protected readonly IGlassPanelProperty _glassPanelOuter;
        protected readonly double _airThicknessOuter;
        protected readonly IGlassPanelProperty _glassPanelCentral;
        protected readonly double _airThicknessInner;
        protected readonly IGlassPanelProperty _glassPanelInner;

        #endregion Variables

        #region Properties

        public IGlassPanelProperty GlassPanelOuter => _glassPanelOuter;
        public double AirThicknessOuter => _airThicknessOuter;
        public IGlassPanelProperty GlassPanelCentral => _glassPanelCentral;
        public double AirThicknessInner => _airThicknessInner;
        public IGlassPanelProperty GlassPanelInner => _glassPanelInner;

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
        public TripleInsulatingGlass(IGlassPanelProperty glassPanelOuter, IGlassPanelProperty glassPanelCentral, IGlassPanelProperty glassPanelInner, double airThicknessOuter, double airThicknessInner)
            : this(glassPanelOuter, glassPanelCentral, glassPanelInner, airThicknessOuter, airThicknessInner, Guid.Empty)
        {

        }

        public TripleInsulatingGlass(IGlassPanelProperty glassPanelOuter, IGlassPanelProperty glassPanelCentral, IGlassPanelProperty glassPanelInner, double airThicknessOuter, double airThicknessInner, Guid guid)
            : base(guid)
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
            _glassPanelOuter = (IGlassPanelProperty)info.GetValue("GlassPanelOuter", typeof(IGlassPanelProperty));
            _glassPanelCentral = (IGlassPanelProperty)info.GetValue("GlassPanelCentral", typeof(IGlassPanelProperty));
            _glassPanelInner = (IGlassPanelProperty)info.GetValue("GlassPanelInner", typeof(IGlassPanelProperty));
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