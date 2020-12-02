using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// This represent a double glazing panel composed by two glass panels separated by air.
    /// </summary>
    [Serializable]
    public class DoubleInsulatingGlass : GlassProperty, IInsulatingGlassProperty
    {
        #region Variables

        protected readonly IGlassPanelProperty _glassPanelOuter;
        protected readonly double _airThickness;
        protected readonly IGlassPanelProperty _glassPanelInner;

        #endregion Variables

        #region Properties

        public IGlassPanelProperty GlassPanelOuter => _glassPanelOuter;
        public double AirThickness => _airThickness;
        public IGlassPanelProperty GlassPanelInner => _glassPanelInner;

        #endregion Properties

        #region Public constructor

        /// <summary>
        ///
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="airThickness">air gap</param>
        public DoubleInsulatingGlass(string name, IGlassPanelProperty glassPanelOuter, IGlassPanelProperty glassPanelInner, double airThickness)
            : this(name, glassPanelOuter, glassPanelInner, airThickness, Guid.Empty)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Outer glass panel</param>
        /// <param name="airThickness">air gap</param>
        /// <param name="guid">The guid of the objec</param>
        public DoubleInsulatingGlass(string name, IGlassPanelProperty glassPanelOuter, IGlassPanelProperty glassPanelInner, double airThickness, Guid guid)
            : base(guid, name)
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
            _glassPanelOuter = (IGlassPanelProperty)info.GetValue("GlassPanelOuter", typeof(IGlassPanelProperty));
            _glassPanelInner = (IGlassPanelProperty)info.GetValue("GlassPanelInner", typeof(IGlassPanelProperty));
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