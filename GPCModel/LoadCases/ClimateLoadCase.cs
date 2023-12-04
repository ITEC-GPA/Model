using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    [Serializable]
    public class ClimateLoadCase : LoadCaseBase
    {
        #region Public Enums

        public enum Seasons
        {
            Summer,
            Winter
        }

        public enum ClimateTypes
        {
            [Description("Climate delta H")] DeltaH,
            [Description("Climate delta P")] DeltaP,
            [Description("Climate delta T")] DeltaT
        }

        #endregion

        #region Class Variables

        private Seasons _season;
        private ClimateTypes _climateType;
        private double _manufactoring;
        private double _installation;

        #endregion

        #region Properties

        public Seasons Season { get => _season; set => _season = value; }

        public ClimateTypes ClimateType { get => _climateType; set => _climateType = value; }

        public double Manufactoring { get => _manufactoring; set => _manufactoring = value; }

        public double Installation { get => _installation; set => _installation = value; }

        #endregion

        #region Constructors

        public ClimateLoadCase(string name, Seasons season, ClimateTypes climateType, double manufactoring, double installation)
            : base(name)
        {
            _season = season;
            _climateType = climateType;
            _manufactoring = manufactoring;
            _installation = installation;
        }

        protected ClimateLoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _season = (Seasons)info.GetValue("Season", typeof(Seasons));
            _climateType = (ClimateTypes)info.GetValue("ClimateType", typeof(ClimateTypes));
            _manufactoring = info.GetDouble("Manufactoring");
            _installation = info.GetDouble("Installation");
        }

        #endregion

        #region Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Season", _season, typeof(Seasons));
            info.AddValue("ClimateType", _climateType, typeof(ClimateTypes));
            info.AddValue("Manufactoring", _manufactoring);
            info.AddValue("Installation", _installation);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return !(!(obj is ClimateLoadCase clc)) &&
                base.Equals(obj) &&
                _season == clc._season &&
                _climateType == clc._climateType &&
                _manufactoring == clc._manufactoring &&
                _installation == clc._installation;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _season.GetHashCode();
                hashCode = hashCode * -17 + _climateType.GetHashCode();
                hashCode = hashCode * -17 + _manufactoring.GetHashCode();
                hashCode = hashCode * -17 + _installation.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
