using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    /// <summary>
    /// A <see cref="LoadCase"/> with the additional type of load required by the standard EN 16612:2020 (duration and kind of the load for the glass)
    /// </summary>
    [Serializable]
    public class LoadCaseEn16612 : LoadCase, ISerializable
    {
        #region PUBLIC ENUMS

        /// <summary>
        /// The types of load of EN 16612
        /// </summary>
        public enum LoadCaseEn16612Types
        {
            /// <summary>Wind gust load, Mediterranean area</summary>
            [Description("Wind gust load mediterranean")] WindGustLoadMediterranean,
            /// <summary>Wind gust load, other areas</summary>
            [Description("Wind gust load other")] WindGustLoadOther,
            /// <summary>Wind storm load, Mediterranean area</summary>
            [Description("Wind storm load mediterranean")] WindStormLoadMediterranean,
            /// <summary>Wind storm load, other areas</summary>
            [Description("Wind storm load other")] WindStormLoadOther,
            /// <summary>Balustrade load, duty</summary>
            [Description("Balaustrade duty")] BalustradeDuty,
            /// <summary>Balustrade load, crowds</summary>
            [Description("Balaustrade crowds")] BalustradeCrowds,
            /// <summary>Maintenance load</summary>
            [Description("Maintenance")] Maintenance,
            /// <summary>Snow on canopies</summary>
            [Description("Snow canopies")] SnowCanopies,
            /// <summary>Snow on roofs</summary>
            [Description("Snow roofs")] SnowRoofs,
            /// <summary>Permanent load</summary>
            [Description("Permanent")] Permanent
            //[Description("Climatic summer")] ClimaticSummer,
            //[Description("Climatic winter")] ClimaticWinter,
        }

        #endregion

        #region Variables

        /// <summary>
        /// The type of load of EN 16612
        /// </summary>
        private LoadCaseEn16612Types _loadCaseEn16612Type;

        #endregion

        #region Properties

        /// <summary>
        /// The type of load of EN 16612
        /// </summary>
        public LoadCaseEn16612Types LoadCaseEn16612Type { get => _loadCaseEn16612Type; set => _loadCaseEn16612Type = value; }

        #endregion

        #region PUBLIC CONSTRUCTOR

        /// <summary>
        /// Creates a load case with a new Guid
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <param name="loadCaseType">The type of load case</param>
        /// <param name="loadCaseEn16612Type">The type of load of EN 16612</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public LoadCaseEn16612(string name, LoadCaseTypes loadCaseType, LoadCaseEn16612Types loadCaseEn16612Type)
            : base(name, loadCaseType)
        {
            _loadCaseEn16612Type = loadCaseEn16612Type;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="LoadCase"/> and the type of EN 16612
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected LoadCaseEn16612(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCaseEn16612Type = (LoadCaseEn16612Types)info.GetValue("LoadCaseEn16612Type", typeof(LoadCaseEn16612Types));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The type of load of EN 16612
        /// </summary>
        /// <returns>The type</returns>
        public LoadCaseEn16612Types GetLoadCasePrEnType() => _loadCaseEn16612Type;

        /// <summary>
        /// Serializes the data of <see cref="LoadCase"/> and the type of EN 16612
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCaseEn16612Type", _loadCaseEn16612Type);
        }

        /// <summary>
        /// Equality of name, type and type of EN 16612
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal load case</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is LoadCaseEn16612 objCasted) && _loadCaseEn16612Type.Equals(objCasted._loadCaseEn16612Type) && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of name, type and type of EN 16612
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _loadCaseEn16612Type.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null load cases are equal
        /// </summary>
        /// <param name="obj1">The first load case</param>
        /// <param name="obj2">The second load case</param>
        /// <returns>True if the load cases are equal</returns>
        public static bool operator ==(LoadCaseEn16612 obj1, LoadCaseEn16612 obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first load case</param>
        /// <param name="obj2">The second load case</param>
        /// <returns>True if the load cases are different</returns>
        public static bool operator !=(LoadCaseEn16612 obj1, LoadCaseEn16612 obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}