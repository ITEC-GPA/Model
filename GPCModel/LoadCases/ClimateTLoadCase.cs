using System;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public class ClimateTLoadCase : LoadCase, IClimate, ISerializable, IEquatable<ClimateTLoadCase>
    {
        private double _manufactoringTemperature;
        private double _installationTemperature;

        public double ManufactoringTemperature => _manufactoringTemperature;
        public double InstallationTemperature => _installationTemperature;

        public ClimateTLoadCase(string name, double manufactoringTemperature, double installationTemperature, LoadCaseType loadCaseType, Guid guid)
            : base(name, loadCaseType, guid)
        {
            if (loadCaseType != LoadCaseType.ClimateSummer || loadCaseType != LoadCaseType.ClimateWinter)
                throw new ArgumentException("Load case type must be climate");


            this._manufactoringTemperature = manufactoringTemperature;
            this._installationTemperature = installationTemperature;
        }

        public ClimateTLoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _manufactoringTemperature = info.GetDouble("ManufactoringTemperature");
            _installationTemperature = info.GetDouble("InstallationTemperature");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ManufactoringTemperature", _manufactoringTemperature);
            info.AddValue("InstallationTemperature", _installationTemperature);
        }
        public bool Equals(ClimateTLoadCase other)
        {
            return !(other is null) &&
                    base.Equals(other) &&
                    _manufactoringTemperature == other._manufactoringTemperature &&
                    _installationTemperature == other._installationTemperature;
        }
    }
}