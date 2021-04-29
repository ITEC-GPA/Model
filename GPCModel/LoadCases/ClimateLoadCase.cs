using GPC.Utilities.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
#if never
namespace GPC.Model.LoadCases
{
    public class ClimateLoadCase : LoadCaseBase
    {
#region Public Enums

        public enum Seasons
        {
            Summer,
            Winter
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum ClimateTypes
        {
            [Description("Climate delta H")] DeltaH,
            [Description("Climate delta P")] DeltaP,
            [Description("Climate delta T")] DeltaT
        }

#endregion

#region Class Variables

        private readonly Seasons _season;
        private readonly ClimateTypes _climateType;
        private readonly double _manufactoring;
        private readonly double _installation;

#endregion

#region Properties

        public Seasons Season => _season;

        public ClimateTypes ClimateType => _climateType;

        public double Manufactoring => _manufactoring;

        public double Installation => _installation;

#endregion

#region Constructors

        public ClimateLoadCase(string name, Seasons season, ClimateTypes climateType, double manufactoring, double installation, Guid guid)
            : base (name, guid)
        {
            _season = season;
            _climateType = climateType;
            _manufactoring = manufactoring;
            _installation = installation;
        }

        public ClimateLoadCase(string name, Seasons season, ClimateTypes climateType, double manufactoring, double installation)
            : this(name, season, climateType, manufactoring, installation, Guid.NewGuid())
        {
        }

        public ClimateLoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _season = (Seasons)info.GetValue("Season", typeof(Seasons));
            _climateType = (ClimateTypes)info.GetValue("ClimateType", typeof(ClimateTypes));
            _manufactoring = info.GetDouble("Manufactoring");
            _installation = info.GetDouble("Installation");
        }

#endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Season", _season, typeof(Seasons));
            info.AddValue("ClimateType", _climateType, typeof(ClimateTypes));
            info.AddValue("Manufactoring", _manufactoring);
            info.AddValue("Installation", _installation);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ClimateLoadCase clc = (ClimateLoadCase)obj;
            return !(clc is null)
                   && base.Equals(obj)
                   && _season == clc._season
                   && _climateType == clc._climateType
                   && _manufactoring == clc._manufactoring
                   && _installation == clc._installation;
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _season.GetHashCode();
            hashCode = hashCode * -17 + _climateType.GetHashCode();
            hashCode = hashCode * -17 + _manufactoring.GetHashCode();
            hashCode = hashCode * -17 + _installation.GetHashCode();
            return hashCode;
        }
    }
}
#endif