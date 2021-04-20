using GPC.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Glasses
{
    /// <summary>
    /// This represent a double glazing panel composed by two glass panels separated by air.
    /// </summary>
    [Serializable]
    [UI(Description = "Double insulating", Group = "Glasses", Kind = "Glass")]
    public sealed class DoubleInsulatingGlass : Glass, IInsulatingGlass, IEquatable<DoubleInsulatingGlass>
    {
        #region Variables

        private IGlassPanel _glassPanelOuter;
        private AirChamber _airChamber;
        private IGlassPanel _glassPanelInner;

        #endregion 

        #region Properties

        public IGlassPanel GlassPanelOuter => _glassPanelOuter;
        public AirChamber AirChamber => _airChamber;
        public IGlassPanel GlassPanelInner => _glassPanelInner;

        #endregion 

        #region Public constructor

        /// <summary>
        /// Initialize the empty double insulating glass 
        /// Used in UI to create an empty laminated that the user will interactively define.
        /// </summary>
        /// <param name="name"></param>
        public DoubleInsulatingGlass(string name)
            : base (Guid.NewGuid(), name)
        {
            _glassPanelOuter = null;
            _glassPanelInner = null;
            _airChamber = null;
        }


        /// <param name="name"></param>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="airChamber">air gap</param>
        public DoubleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelInner, AirChamber airChamber)
            : this(name, glassPanelOuter, glassPanelOuter, airChamber, Guid.NewGuid())
        {

        }

        /// <param name="name"></param>
        /// <param name="glassPanelOuter">Outer glass panel</param>
        /// <param name="glassPanelInner">Inner glass panel</param>
        /// <param name="airChamber">air gap</param>
        /// <param name="guid"></param>
        public DoubleInsulatingGlass(string name, IGlassPanel glassPanelOuter, IGlassPanel glassPanelInner, AirChamber airChamber, Guid guid)
            : base(guid, name)
        {
            _glassPanelOuter = glassPanelOuter ?? throw new ArgumentException("Outer Glass panel can't be null");
            _glassPanelInner = glassPanelInner ?? throw new ArgumentException("Inner Glass panel can't be null");
            _airChamber = airChamber ?? throw new ArgumentException("AirChamber panel can't be null");
        }


        public DoubleInsulatingGlass(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            _glassPanelOuter = (IGlassPanel)info.GetValue("GlassPanelOuter", typeof(IGlassPanel));
            _glassPanelInner = (IGlassPanel)info.GetValue("GlassPanelInner", typeof(IGlassPanel));
            _airChamber = (AirChamber)info.GetValue("AirChamber", typeof(AirChamber));
        }

        #endregion

        public IGlassPackage[][] GetGlassPackage()
        {

            IGlassPackage[][] package = new IGlassPackage[3][];

            package[0] = _glassPanelOuter.GetGlassPackage();
            package[1] = new[] { _airChamber };
            package[2] = _glassPanelInner.GetGlassPackage();

            return package;
        }



        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassPanelOuter", _glassPanelOuter);
            info.AddValue("GlassPanelInner", _glassPanelInner);
            info.AddValue("AirChamber", _airChamber);
        }

        public bool Equals(DoubleInsulatingGlass other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._glassPanelOuter.Equals(_glassPanelOuter) 
                                    && other._airChamber.Equals(_airChamber)
                                    && other._glassPanelInner.Equals(_glassPanelInner)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DoubleInsulatingGlass);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<IGlassPanel>.Default.GetHashCode(_glassPanelOuter);
            hashCode = hashCode * -17 + EqualityComparer<AirChamber>.Default.GetHashCode(_airChamber);
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
