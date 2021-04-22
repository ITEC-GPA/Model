using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class LoadCase : ModelObject, ISerializable, ILoadCase
    {
        #region PUBLIC ENUMS

        [Serializable]
        public enum LoadCaseTypes
        {
            [Description("Self weigth")] SelfWeight,
            [Description("Superimposed dead load")] SuperImposedDeadLoad,
            [Description("Prestress")] Prestress,
            [Description("Live load")] LiveLoad,
            [Description("Wind pressure")] WindPressure,
            [Description("Wind suction")] WindSuction,
            [Description("Snow")] Snow,
            [Description("Maintenance")] Maintenance,
            [Description("Earthquake")] Earthquake,
            [Description("Temperature")] Temperature,
            [Description("Climate Summer delta H")] ClimateSummerDeltaH,
            [Description("Climate Summer delta P")] ClimateSummerDeltaP,
            [Description("Climate Summer delta T")] ClimateSummerDeltaT,
            [Description("Climate Winter delta H")] ClimateWinterDeltaH,
            [Description("Climate Winter delta P")] ClimateWinterDeltaP,
            [Description("Climate Winter delta T")] ClimateWinterDeltaT,
        }

        #endregion

        #region VARIABLES

        private LoadCaseTypes? _loadCaseType;

        #endregion 

        public LoadCaseTypes? LoadCaseType => _loadCaseType;


        #region PUBLIC CONSTRUCTOR

        public LoadCase(string name, LoadCaseTypes? loadCaseType)
            : this(name, loadCaseType, Guid.NewGuid())
        {

        }

        public LoadCase(string name, LoadCaseTypes? loadCaseType, Guid guid)
            : base(guid, name)
        {
            if (String.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Loadcase name cannot be empty");

            this._loadCaseType = loadCaseType;
        }

        public LoadCase(string name, Guid guid)
            : this(name, null, guid)
        {

        }
        public LoadCase(string name)
            : this(name, null, Guid.NewGuid())
        {

        }

        public LoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCaseType = (LoadCaseTypes?)info.GetValue("LoadCaseType", typeof(LoadCaseTypes?));
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
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            LoadCase objCasted = obj as LoadCase;
            return !(objCasted is null) && _loadCaseType.Equals(objCasted._loadCaseType) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _loadCaseType.GetHashCode();
            return hashCode;
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
