using System;
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

        #endregion PUBLIC ENUMS

        #region VARIABLES

        private LoadCaseType? _loadCaseType;

        #endregion VARIABLES

        #region PUBLIC CONSTRUCTOR

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
            _name = info.GetString("Name");
            _loadCaseType = (LoadCaseType?)info.GetValue("LoadCaseType", typeof(LoadCaseType?));
        }

        #endregion PUBLIC CONSTRUCTOR

        public LoadCaseType? GetLoadCaseType() => _loadCaseType;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Name", _name);
            info.AddValue("LoadCaseType", _loadCaseType);
        }

        public bool Equals(LoadCase other)
        {
            return !(other is null) && 
                    _name == other._name && 
                    _loadCaseType == other._loadCaseType &&
                    _guid == other._guid;
        }
    }
}