using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    /// <summary>
    /// A load case with a type (self weight, live load, wind...), used by the combinations of the standards
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class LoadCase : LoadCaseBase
    {
        #region PUBLIC ENUMS

        /// <summary>
        /// The types of load case
        /// </summary>
        public enum LoadCaseTypes
        {
            /// <summary>Self weight of the structure</summary>
            [Description("Self weight")] SelfWeight,
            /// <summary>Superimposed dead load (permanent non structural loads)</summary>
            [Description("Superimposed dead load")] SuperImposedDeadLoad,
            /// <summary>Prestress</summary>
            [Description("Prestress")] Prestress,
            /// <summary>Live (imposed) load</summary>
            [Description("Live load")] LiveLoad,
            /// <summary>Wind pressure</summary>
            [Description("Wind pressure")] WindPressure,
            /// <summary>Wind suction</summary>
            [Description("Wind suction")] WindSuction,
            /// <summary>Snow</summary>
            [Description("Snow")] Snow,
            /// <summary>Maintenance load</summary>
            [Description("Maintenance")] Maintenance,
            /// <summary>Earthquake</summary>
            [Description("Earthquake")] Earthquake,
            /// <summary>Temperature</summary>
            [Description("Temperature")] Temperature,
        }

        #endregion

        #region VARIABLES

        /// <summary>
        /// The type of load case
        /// </summary>
        private LoadCaseTypes _loadCaseType;

        #endregion

        #region Properties

        /// <summary>
        /// The type of load case
        /// </summary>
        public LoadCaseTypes LoadCaseType { get => _loadCaseType; set => _loadCaseType = value; }

        #endregion

        #region PUBLIC CONSTRUCTORS

        /// <summary>
        /// Creates a load case with a new Guid
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <param name="loadCaseType">The type</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public LoadCase(string name, LoadCaseTypes loadCaseType)
            : base(name)
        {
            if (String.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Loadcase name cannot be empty");

            _loadCaseType = loadCaseType;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="ModelObject"/> and the type
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected LoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCaseType = (LoadCaseTypes)info.GetValue("LoadCaseType", typeof(LoadCaseTypes));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes the data of <see cref="ModelObject"/> and the type
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCaseType", _loadCaseType);
        }

        /// <summary>
        /// Equality of name and type
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns><see langword="True"/> if <paramref name="obj"/> have the same <see cref="_loadCaseType"/> and <see cref="ModelObject.Name"/> of this object</returns>
        public override bool Equals(object obj)
        {
            LoadCase lc = obj as LoadCase;
            return lc != null && base.Equals(lc) && _loadCaseType.Equals(lc._loadCaseType);
        }

        /// <summary>
        /// The hash code of name and type
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _loadCaseType.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null load cases are equal
        /// </summary>
        /// <param name="obj1">The first load case</param>
        /// <param name="obj2">The second load case</param>
        /// <returns>True if the load cases are equal</returns>
        public static bool operator ==(LoadCase obj1, LoadCase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first load case</param>
        /// <param name="obj2">The second load case</param>
        /// <returns>True if the load cases are different</returns>
        public static bool operator !=(LoadCase obj1, LoadCase obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>"LoadCase ", the name and the type</returns>
        private string GetDebuggerDisplay()
        {
            return $"LoadCase {Name} {_loadCaseType}";
        }

        #endregion
    }
}