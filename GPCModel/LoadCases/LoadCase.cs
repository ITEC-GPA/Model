using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class LoadCase : LoadCaseBase
    {
        #region PUBLIC ENUMS

        public enum LoadCaseTypes
        {
            [Description("Self weight")] SelfWeight,
            [Description("Superimposed dead load")] SuperImposedDeadLoad,
            [Description("Prestress")] Prestress,
            [Description("Live load")] LiveLoad,
            [Description("Wind pressure")] WindPressure,
            [Description("Wind suction")] WindSuction,
            [Description("Snow")] Snow,
            [Description("Maintenance")] Maintenance,
            [Description("Earthquake")] Earthquake,
            [Description("Temperature")] Temperature,
        }

        #endregion

        #region VARIABLES

        private readonly LoadCaseTypes _loadCaseType;

        #endregion 

        public LoadCaseTypes LoadCaseType => _loadCaseType;

        #region PUBLIC CONSTRUCTORS

        public LoadCase(string name, LoadCaseTypes loadCaseType)
            : this(name, loadCaseType, Guid.NewGuid())
        {

        }

        public LoadCase(string name, LoadCaseTypes loadCaseType, Guid guid)
            : base(name, guid)
        {
            if (String.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Loadcase name cannot be empty");

            _loadCaseType = loadCaseType;
        }

        protected LoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCaseType = (LoadCaseTypes)info.GetValue("LoadCaseType", typeof(LoadCaseTypes));
        }

        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCaseType", _loadCaseType);
        }

        /// <returns><see langword="True"/> if <paramref name="obj"/> have the same <see cref="_loadCaseType"/> and <see cref="ModelObject.Name"/> of this object </returns>
        public override bool Equals(object obj)
        {
            LoadCase lc = obj as LoadCase;

            return lc != null && base.Equals(lc) && _loadCaseType.Equals(lc._loadCaseType);
        }

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

        public static bool operator ==(LoadCase obj1, LoadCase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(LoadCase obj1, LoadCase obj2)
        {
            return !(obj1 == obj2);
        }

        private string GetDebuggerDisplay()
        {
            return $"LoadCase {Name} {_loadCaseType}";
        }
    }
}