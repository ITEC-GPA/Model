using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    [Serializable]
    public class LoadCase : ModelObject, ISerializable, IEquatable<LoadCase>
    {
        #region PUBLIC ENUMS

        [Serializable]
        public enum LoadCaseType
        {
            [Description("Self weigth")] SelfWeight = 0,
            [Description("Superimposed dead load")] SuperImposedDeadLoad = 1,
            [Description("Live load")] LiveLoad = 2,
            [Description("Wind")] Wind = 3,
            [Description("Snow")] Snow = 4,
            [Description("Maintenance")] Maintenance = 5,
            [Description("Earthquake")] Earthquake = 6,
            [Description("Temperature")] Temperature = 7,
            [Description("Climate Summer")] ClimateSummer = 8,
            [Description("Climate Winter")] ClimateWinter = 9,
        }

        #endregion

        #region VARIABLES

        private LoadCaseType? _loadCaseType;

        #endregion 

        #region PUBLIC CONSTRUCTOR

        public LoadCase(string name, LoadCaseType? loadCaseType)
            : this(name, loadCaseType, Guid.NewGuid())
        {

        }

        public LoadCase(string name, LoadCaseType? loadCaseType, Guid guid)
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

        public LoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCaseType = (LoadCaseType?)info.GetValue("LoadCaseType", typeof(LoadCaseType?));
        }

        #endregion 

        public LoadCaseType? GetLoadCaseType() => _loadCaseType;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCaseType", _loadCaseType);
        }

        public bool Equals(LoadCase other)
        {
            return !(other is null) && _loadCaseType.Equals(other._loadCaseType)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as LoadCase);
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
    }
}