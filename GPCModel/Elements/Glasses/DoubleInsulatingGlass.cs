using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// This represent a double glazing panel composed by two glass panels separated by air.
    /// </summary>
    [Serializable]
    public class DoubleInsulatingGlass : GlassProperty, IInsulatingGlass
    {
        #region Variables

        protected readonly IGlassPanel _glassPanelOuter;
        protected readonly double _airThickness;
        protected readonly IGlassPanel _glassPanelInner;

        #endregion Variables

        #region Properties

        public IGlassPanel GlassPanelOuter => _glassPanelOuter;
        public double AirThickness => _airThickness;
        public IGlassPanel GlassPanelInner => _glassPanelInner;

        #endregion Properties

        #region Public constructor

        /// <summary>
        ///
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="airThickness">air gap</param>
        public DoubleInsulatingGlass(IGlassPanel glassPanelOuter, IGlassPanel glassPanelInner, double airThickness)
            : this(glassPanelOuter, glassPanelInner, airThickness, Guid.Empty)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Outer glass panel</param>
        /// <param name="airThickness">air gap</param>
        /// <param name="guid">The guid of the objec</param>
        public DoubleInsulatingGlass(IGlassPanel glassPanelOuter, IGlassPanel glassPanelInner, double airThickness, Guid guid)
            : base(guid)
        {
            if (airThickness <= 0.001)
                throw new ArgumentOutOfRangeException("Air thickness can't be negative or zero");

            this._glassPanelOuter = glassPanelOuter ?? throw new ArgumentException("Outer Glass panel can't be null");
            this._glassPanelInner = glassPanelInner ?? throw new ArgumentException("Inner Glass panel can't be null");

            this._airThickness = airThickness;
        }

        public DoubleInsulatingGlass(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            _glassPanelOuter = (IGlassPanel)info.GetValue("GlassPanelOuter", typeof(IGlassPanel));
            _glassPanelInner = (IGlassPanel)info.GetValue("GlassPanelInner", typeof(IGlassPanel));
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