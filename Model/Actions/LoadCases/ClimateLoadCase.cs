using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    /// <summary>
    /// A climatic load case of an insulating glass unit (EN 16612): variation of altitude, pressure or temperature between the manufacturing
    /// and the installation, in summer or in winter
    /// </summary>
    [Serializable]
    public class ClimateLoadCase : LoadCaseBase
    {
        #region Public Enums

        /// <summary>
        /// The seasons of the climatic actions
        /// </summary>
        public enum Seasons
        {
            /// <summary>Summer conditions</summary>
            Summer,
            /// <summary>Winter conditions</summary>
            Winter
        }

        /// <summary>
        /// The climatic parameters
        /// </summary>
        public enum ClimateTypes
        {
            /// <summary>Difference of altitude</summary>
            [Description("Climate delta H")] DeltaH,
            /// <summary>Difference of atmospheric pressure</summary>
            [Description("Climate delta P")] DeltaP,
            /// <summary>Difference of temperature</summary>
            [Description("Climate delta T")] DeltaT
        }

        #endregion

        #region Class Variables

        /// <summary>
        /// The season
        /// </summary>
        private Seasons _season;
        /// <summary>
        /// The climatic parameter
        /// </summary>
        private ClimateTypes _climateType;
        /// <summary>
        /// The value of the parameter at the manufacturing
        /// </summary>
        private double _manufactoring;
        /// <summary>
        /// The value of the parameter at the installation
        /// </summary>
        private double _installation;

        #endregion

        #region Properties

        /// <summary>
        /// The season
        /// </summary>
        public Seasons Season { get => _season; set => _season = value; }

        /// <summary>
        /// The climatic parameter
        /// </summary>
        public ClimateTypes ClimateType { get => _climateType; set => _climateType = value; }

        /// <summary>
        /// The value of the parameter at the manufacturing
        /// </summary>
        public double Manufactoring { get => _manufactoring; set => _manufactoring = value; }

        /// <summary>
        /// The value of the parameter at the installation
        /// </summary>
        public double Installation { get => _installation; set => _installation = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a climatic load case with a new Guid
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <param name="season">The season</param>
        /// <param name="climateType">The climatic parameter</param>
        /// <param name="manufactoring">The value at the manufacturing</param>
        /// <param name="installation">The value at the installation</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public ClimateLoadCase(string name, Seasons season, ClimateTypes climateType, double manufactoring, double installation)
            : base(name)
        {
            _season = season;
            _climateType = climateType;
            _manufactoring = manufactoring;
            _installation = installation;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="LoadCaseBase"/>, season, parameter and values
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Serializes the data of <see cref="LoadCaseBase"/>, season, parameter and values
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Season", _season, typeof(Seasons));
            info.AddValue("ClimateType", _climateType, typeof(ClimateTypes));
            info.AddValue("Manufactoring", _manufactoring);
            info.AddValue("Installation", _installation);
        }

        /// <summary>
        /// Equality of name, season, parameter and values (exact)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal climatic load case</returns>
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

        /// <summary>
        /// The hash code of name, season, parameter and values
        /// </summary>
        /// <returns>The hash code</returns>
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
