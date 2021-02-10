using System;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public sealed class ClimateHLoadCase : LoadCase, IClimate, ISerializable, IEquatable<ClimateHLoadCase>
    {
        private double _manufactoringHeight;
        private double _installationHeight;

        public double ManufactoringHeight => _manufactoringHeight;
        public double InstallationHeight => _installationHeight;


        public ClimateHLoadCase(string name, double manufactoringHeight, double installationHeight, LoadCaseType loadCaseType, Guid guid)
            : base(name, loadCaseType, guid)
        {
            if (loadCaseType != LoadCaseType.ClimateSummer || loadCaseType != LoadCaseType.ClimateWinter)
                throw new ArgumentException("Load case type must be climate");

            this._manufactoringHeight = manufactoringHeight;
            this._installationHeight = installationHeight;
        }

        public ClimateHLoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _manufactoringHeight = info.GetDouble("ManufactoringHeight");
            _installationHeight = info.GetDouble("InstallationHeight");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ManufactoringHeight", _manufactoringHeight);
            info.AddValue("InstallationHeight", _installationHeight);
        }

        public bool Equals(ClimateHLoadCase other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return !(other is null) &&
                    _installationHeight == other._installationHeight &&
                    _manufactoringHeight == other._manufactoringHeight &&
                    base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ClimateHLoadCase objCasted = obj as ClimateHLoadCase;
            return !(objCasted is null) && _installationHeight == objCasted._installationHeight &&
                                        _manufactoringHeight == objCasted._manufactoringHeight &&
                                        base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _manufactoringHeight.GetHashCode();
            hashCode = hashCode * -17 + _installationHeight.GetHashCode();
            return hashCode;
        }
    }
}