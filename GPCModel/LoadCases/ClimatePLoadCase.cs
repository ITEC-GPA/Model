using System;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public class ClimatePLoadCase : LoadCase, IClimate, ISerializable
    {
        private double _manufactoringPressure;
        private double _installationPressure;

        public double ManufactoringPressure => _manufactoringPressure;
        public double InstallationPressure => _installationPressure;


        public ClimatePLoadCase(string name, double manufactoringPressure, double installationPressure, LoadCaseTypes loadCaseType, Guid guid)
            : base(name, loadCaseType, guid)
        {
            if (loadCaseType != LoadCaseTypes.ClimateSummerDeltaH || loadCaseType != LoadCaseTypes.ClimateSummerDeltaP || loadCaseType != LoadCaseTypes.ClimateSummerDeltaT ||
                loadCaseType != LoadCaseTypes.ClimateWinterDeltaH || loadCaseType != LoadCaseTypes.ClimateWinterDeltaP || loadCaseType != LoadCaseTypes.ClimateWinterDeltaT)
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

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ClimatePLoadCase objCasted = obj as ClimatePLoadCase;
            return !(objCasted is null) && _manufactoringPressure == objCasted._manufactoringPressure &&
                                            _installationPressure == objCasted._installationPressure &&
                                            base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _manufactoringPressure.GetHashCode();
            hashCode = hashCode * -17 + _installationPressure.GetHashCode();
            return hashCode;
        }
    }
}
