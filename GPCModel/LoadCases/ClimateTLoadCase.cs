using System;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public class ClimateTLoadCase : LoadCase, IClimate, ISerializable
    {
        private double _manufactoringTemperature;
        private double _installationTemperature;

        public double ManufactoringTemperature => _manufactoringTemperature;
        public double InstallationTemperature => _installationTemperature;

        public ClimateTLoadCase(string name, double manufactoringTemperature, double installationTemperature, LoadCaseType loadCaseType, Guid guid)
            : base(name, loadCaseType, guid)
        {
            if (loadCaseType != LoadCaseType.ClimateSummerDeltaH || loadCaseType != LoadCaseType.ClimateSummerDeltaP || loadCaseType != LoadCaseType.ClimateSummerDeltaT ||
                loadCaseType != LoadCaseType.ClimateWinterDeltaH || loadCaseType != LoadCaseType.ClimateWinterDeltaP || loadCaseType != LoadCaseType.ClimateWinterDeltaT)
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

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ClimateTLoadCase objCasted = obj as ClimateTLoadCase;
            return !(objCasted is null) && _manufactoringTemperature == objCasted._manufactoringTemperature &&
                                            _installationTemperature == objCasted._installationTemperature &&
                                            base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _manufactoringTemperature.GetHashCode();
            hashCode = hashCode * -17 + _installationTemperature.GetHashCode();
            return hashCode;
        }
    }
}
