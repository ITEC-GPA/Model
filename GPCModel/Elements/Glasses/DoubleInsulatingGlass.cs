using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// This represent a double glazing panel composed by two glass panels separated by air.
    /// </summary>
    [Serializable]
    public class DoubleInsulatingGlass : Glass, IInsulatingGlass, IEquatable<DoubleInsulatingGlass>
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
        public DoubleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelInner, double airThickness)
            : this(name, glassPanelOuter, glassPanelInner, airThickness, Guid.NewGuid())
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Outer glass panel</param>
        /// <param name="airThickness">air gap</param>
        /// <param name="guid">The guid of the objec</param>
        public DoubleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelInner, double airThickness, Guid guid)
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

        public bool Equals(DoubleInsulatingGlass other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._glassPanelOuter.Equals(_glassPanelOuter) 
                                    && other._airThickness.Equals(_airThickness)
                                    && other._glassPanelInner.Equals(_glassPanelInner);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as DoubleInsulatingGlass);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<IGlassPanel>.Default.GetHashCode(_glassPanelOuter);
            hashCode = hashCode * -17 + _airThickness.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<IGlassPanel>.Default.GetHashCode(_glassPanelInner);
            return hashCode;
        }

        public static bool operator ==(DoubleInsulatingGlass obj1, DoubleInsulatingGlass obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }
        public static bool operator !=(DoubleInsulatingGlass obj1, DoubleInsulatingGlass obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

    }
}