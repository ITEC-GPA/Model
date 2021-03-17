using GPC.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Glasses
{
    /// <summary>
    /// This represent a triple glazing panel composed by three glass panels separated by air.
    /// </summary>
    [Serializable]
    [UI(Description = "Triple insulating", Group = "Glasses", Kind = "Glass")]
    public sealed class TripleInsulatingGlass : Glass, IInsulatingGlass, IEquatable<TripleInsulatingGlass>
    {
        #region Variables

        private IGlassPanel _glassPanelOuter;
        private double _airThicknessOuter;
        private IGlassPanel _glassPanelCentral;
        private double _airThicknessInner;
        private IGlassPanel _glassPanelInner;

        #endregion

        #region Properties

        public IGlassPanel GlassPanelOuter => _glassPanelOuter;
        public double AirThicknessOuter => _airThicknessOuter;
        public IGlassPanel GlassPanelCentral => _glassPanelCentral;
        public double AirThicknessInner => _airThicknessInner;
        public IGlassPanel GlassPanelInner => _glassPanelInner;

        #endregion

        #region Public constructor

        /// <summary>
        /// Initialize the empty triple insulating glass 
        /// Used in UI to create an empty laminated that the user will interactively define.
        /// </summary>
        /// <param name="name"></param>
        public TripleInsulatingGlass(string name)
            : base (Guid.NewGuid(), name)
        {
            _glassPanelOuter = null;
            _glassPanelCentral = null;
            _glassPanelInner = null;
            _airThicknessOuter = 0;
            _airThicknessInner = 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="glassPanelCentral">Central glass panel</param>
        /// <param name="airThicknessOuter">Outer air thickness</param>
        /// <param name="airThicknessInner">Inner air thickness</param>
        public TripleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelCentral, IGlassPanel glassPanelInner, double airThicknessOuter, double airThicknessInner)
            : this(name, glassPanelOuter, glassPanelCentral, glassPanelInner, airThicknessOuter, airThicknessInner, Guid.NewGuid())
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

        public bool Equals(TripleInsulatingGlass other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._glassPanelOuter.Equals(_glassPanelOuter)
                                    && other._airThicknessOuter.Equals(_airThicknessOuter)
                                    && other._glassPanelCentral.Equals(_glassPanelCentral)
                                    && other._airThicknessInner.Equals(_airThicknessInner)
                                    && other._glassPanelInner.Equals(_glassPanelInner)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TripleInsulatingGlass);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<IGlassPanel>.Default.GetHashCode(_glassPanelOuter);
            hashCode = hashCode * -17 + _airThicknessOuter.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<IGlassPanel>.Default.GetHashCode(_glassPanelCentral);
            hashCode = hashCode * -17 + _airThicknessInner.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<IGlassPanel>.Default.GetHashCode(_glassPanelInner);
            return hashCode;
        }

        public static bool operator ==(TripleInsulatingGlass obj1, TripleInsulatingGlass obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(TripleInsulatingGlass obj1, TripleInsulatingGlass obj2)
        {
            return !(obj1 == obj2);
        }


        #endregion
    }
}