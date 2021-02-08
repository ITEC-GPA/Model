using System;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public class ClimatePLoadCase : LoadCase, IClimate, ISerializable, IEquatable<ClimatePLoadCase>
    {
        private double _manufactoringPressure;
        private double _installationPressure;

        public double ManufactoringPressure => _manufactoringPressure;
        public double InstallationPressure => _installationPressure;


        public ClimatePLoadCase(string name, double manufactoringPressure, double installationPressure, LoadCaseType loadCaseType, Guid guid)
            : base(name, loadCaseType, guid)
        {
            if (loadCaseType != LoadCaseType.ClimateSummer || loadCaseType != LoadCaseType.ClimateWinter)
                throw new ArgumentException("Load case type must be climate");


            this._manufactoringPressure = manufactoringPressure;
            this._installationPressure = installationPressure;
        }

        public ClimatePLoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _manufactoringPressure = info.GetDouble("ManufactoringPressure");
            _installationPressure = info.GetDouble("InstallationPressure");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ManufactoringPressure", _manufactoringPressure);
            info.AddValue("InstallationPressure", _installationPressure);
        }

        public bool Equals(ClimatePLoadCase other)
        {
            return !(other is null) &&
                    base.Equals(other) &&
                    _manufactoringPressure == other._manufactoringPressure &&
                    _installationPressure == other._installationPressure;
        }
    }
}
