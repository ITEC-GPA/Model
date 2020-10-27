using System;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    public class ClimateHLoadCase : LoadCase, IClimate
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
    }
}