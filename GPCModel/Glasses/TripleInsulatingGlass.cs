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
        private AirChamber _airChamberOuter;
        private IGlassPanel _glassPanelCentral;
        private AirChamber _airChamberInner;
        private IGlassPanel _glassPanelInner;

        #endregion

        #region Properties

        public IGlassPanel GlassPanelOuter => _glassPanelOuter;
        public AirChamber AirChamberOuter => _airChamberOuter;
        public IGlassPanel GlassPanelCentral => _glassPanelCentral;
        public AirChamber AirChamberInner => _airChamberInner;
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
            _airChamberOuter = null;
            _airChamberInner = null;
        }


        /// <param name="name"></param>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="glassPanelCentral">Central glass panel</param>
        /// <param name="airChamberOuter">Outer air thickness</param>
        /// <param name="airChamberInner">Inner air thickness</param>
        public TripleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelCentral, IGlassPanel glassPanelInner, AirChamber airChamberOuter, AirChamber airChamberInner)
            : this(name, glassPanelOuter, glassPanelCentral, glassPanelInner, airChamberOuter, airChamberInner, Guid.NewGuid())
        {

        }

        /// <param name="name"></param>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="glassPanelCentral">Central glass panel</param>
        /// <param name="airChamberOuter">Outer air thickness</param>
        /// <param name="airChamberInner">Inner air thickness</param>
        /// <param name="guid"></param>
        public TripleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelCentral, IGlassPanel glassPanelInner, AirChamber airChamberOuter, AirChamber airChamberInner, Guid guid)
            : base(guid, name)
        {
            this._glassPanelOuter = glassPanelOuter ?? throw new ArgumentException("Outer Glass panel can't be null");
            this._glassPanelCentral = glassPanelCentral ?? throw new ArgumentException("Central Glass panel can't be null");
            this._glassPanelInner = glassPanelInner ?? throw new ArgumentException("Inner Glass panel can't be null");

            this._airChamberInner = airChamberInner ?? throw new ArgumentException("Inner AirChamber panel can't be null");
            this._airChamberOuter = airChamberOuter ?? throw new ArgumentException("Outer AirChamber panel can't be null");
        }


        public TripleInsulatingGlass(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            _glassPanelOuter = (IGlassPanel)info.GetValue("GlassPanelOuter", typeof(IGlassPanel));
            _glassPanelCentral = (IGlassPanel)info.GetValue("GlassPanelCentral", typeof(IGlassPanel));
            _glassPanelInner = (IGlassPanel)info.GetValue("GlassPanelInner", typeof(IGlassPanel));
            _airChamberInner = (AirChamber)info.GetValue("AirChamberInner", typeof(AirChamber));
            _airChamberOuter = (AirChamber)info.GetValue("AirChamberOuter", typeof(AirChamber));
        }

        #endregion


        /// <remarks>Order of the glass panels is from external to internal</remarks>
        public IGlassPackage[][] GetGlassPackage()
        {
            IGlassPackage[][] package = new IGlassPackage[5][];

            package[0] = _glassPanelOuter.GetGlassPackage();
            package[1] = new[] { _airChamberOuter };
            package[2] = _glassPanelCentral.GetGlassPackage();
            package[3] = new[] { _airChamberInner };
            package[4] = _glassPanelInner.GetGlassPackage();

            return package;
        }


        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassPanelOuter", _glassPanelOuter);
            info.AddValue("GlassPanelCentral", _glassPanelCentral);
            info.AddValue("GlassPanelInner", _glassPanelInner);
            info.AddValue("AirChamberInner", _airChamberInner);
            info.AddValue("AirChamberOuter", _airChamberOuter);
        }

        public bool Equals(TripleInsulatingGlass other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._glassPanelOuter.Equals(_glassPanelOuter)
                                    && other._airChamberInner.Equals(_airChamberInner)
                                    && other._glassPanelCentral.Equals(_glassPanelCentral)
                                    && other._airChamberOuter.Equals(_airChamberOuter)
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
            hashCode = hashCode * -17 + EqualityComparer<AirChamber>.Default.GetHashCode(_airChamberInner);
            hashCode = hashCode * -17 + EqualityComparer<IGlassPanel>.Default.GetHashCode(_glassPanelCentral);
            hashCode = hashCode * -17 + EqualityComparer<AirChamber>.Default.GetHashCode(_airChamberOuter);
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
