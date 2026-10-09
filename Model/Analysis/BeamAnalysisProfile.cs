using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Results.Locations;

namespace GPC.Model.Analysis
{
    public enum ProfileInterpolation
    {
        Constant,
        Linear
    }

    /// <summary>Analysis rigidities EA, GA1, GA2 (N), GJ, EI1, EI2 (N mm²).
    /// These values never replace the physical section or its resistance material.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamAnalysisStation
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Station>k__BackingField", IsRequired = true)]
        public double Station { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<EA>k__BackingField", IsRequired = true)]
        public double EA { get; set; } = double.NaN;

        [field: System.Runtime.Serialization.DataMember(Name = "<GA1>k__BackingField", IsRequired = true)]
        public double GA1 { get; set; } = double.NaN;

        [field: System.Runtime.Serialization.DataMember(Name = "<GA2>k__BackingField", IsRequired = true)]
        public double GA2 { get; set; } = double.NaN;

        [field: System.Runtime.Serialization.DataMember(Name = "<GJ>k__BackingField", IsRequired = true)]
        public double GJ { get; set; } = double.NaN;

        [field: System.Runtime.Serialization.DataMember(Name = "<EI1>k__BackingField", IsRequired = true)]
        public double EI1 { get; set; } = double.NaN;

        [field: System.Runtime.Serialization.DataMember(Name = "<EI2>k__BackingField", IsRequired = true)]
        public double EI2 { get; set; } = double.NaN;

        [field: System.Runtime.Serialization.DataMember(Name = "<MassPerLength>k__BackingField", IsRequired = true)]
        public double MassPerLength { get; set; } = double.NaN;
        internal double[] Values => new[]
        {
            EA,
            GA1,
            GA2,
            GJ,
            EI1,
            EI2,
            MassPerLength
        };

        internal static BeamAnalysisStation From(double station, double[] v) => new BeamAnalysisStation
        {
            Station = station,
            EA = v[0],
            GA1 = v[1],
            GA2 = v[2],
            GJ = v[3],
            EI1 = v[4],
            EI2 = v[5],
            MassPerLength = v[6]
        };
    }

    /// <summary>Dimensionless modifiers, deliberately separate from physical resistance properties.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamStiffnessModifiers
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Axial>k__BackingField", IsRequired = true)]
        public double Axial { get; set; } = 1;

        [field: System.Runtime.Serialization.DataMember(Name = "<Shear1>k__BackingField", IsRequired = true)]
        public double Shear1 { get; set; } = 1;

        [field: System.Runtime.Serialization.DataMember(Name = "<Shear2>k__BackingField", IsRequired = true)]
        public double Shear2 { get; set; } = 1;

        [field: System.Runtime.Serialization.DataMember(Name = "<Torsion>k__BackingField", IsRequired = true)]
        public double Torsion { get; set; } = 1;

        [field: System.Runtime.Serialization.DataMember(Name = "<Bending1>k__BackingField", IsRequired = true)]
        public double Bending1 { get; set; } = 1;

        [field: System.Runtime.Serialization.DataMember(Name = "<Bending2>k__BackingField", IsRequired = true)]
        public double Bending2 { get; set; } = 1;

        [field: System.Runtime.Serialization.DataMember(Name = "<Mass>k__BackingField", IsRequired = true)]
        public double Mass { get; set; } = 1;
        internal double[] Values => new[]
        {
            Axial,
            Shear1,
            Shear2,
            Torsion,
            Bending1,
            Bending2,
            Mass
        };
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamAnalysisProfile
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<StationDomain>k__BackingField", IsRequired = true)]
        public string StationDomain { get; set; } = "OffsetToOffset";

        [field: System.Runtime.Serialization.DataMember(Name = "<Interpolation>k__BackingField", IsRequired = true)]
        public ProfileInterpolation Interpolation { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Stations>k__BackingField", IsRequired = true)]
        public List<BeamAnalysisStation> Stations { get; private set; } = new List<BeamAnalysisStation>();

        [field: System.Runtime.Serialization.DataMember(Name = "<Modifiers>k__BackingField", IsRequired = true)]
        public BeamStiffnessModifiers Modifiers { get; set; } = new BeamStiffnessModifiers();

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(ProfileInterpolation), Interpolation) || Modifiers == null || Stations == null || Stations.Count < 2)
                throw new ArgumentException("IncompleteAnalysisProfile");
            if (StationDomain != "NodeToNode" && StationDomain != "OffsetToOffset" && StationDomain != "Deformable")
                throw new ArgumentException("UnsupportedStationDomain");
            if (Stations.Any(s => s == null) || Stations[0].Station != 0 || Stations[Stations.Count - 1].Station != 1)
                throw new ArgumentException("IncompleteAnalysisProfileCoverage");
            double last = -1;
            foreach (var s in Stations)
            {
                NumericGuard.Station(s.Station);
                if (s.Station <= last)
                    throw new ArgumentException("UnorderedAnalysisProfile");
                last = s.Station;
                ValidateValues(s.Values);
            }

            ValidateValues(Modifiers.Values);
        }

        private static void ValidateValues(double[] values)
        {
            foreach (var v in values)
                if (NumericGuard.Finite(v, "analysis property") < 0)
                    throw new ArgumentException("NegativeAnalysisProperty");
        }

        /// <summary>Piecewise constant (left/right limits explicit) or piecewise linear. No extrapolation.</summary>
        public BeamAnalysisStation At(double station, SectionSide side = SectionSide.Unspecified)
        {
            Validate();
            NumericGuard.Station(station);
            int i = Stations.FindLastIndex(s => s.Station <= station);
            if (i > 0 && station == Stations[i].Station && station != 1 && Interpolation == ProfileInterpolation.Constant)
            {
                if (side == SectionSide.Unspecified)
                    throw new InvalidOperationException("AmbiguousAnalysisProfileSide");
                if (side == SectionSide.Left)
                    i--;
            }

            if (i == Stations.Count - 1)
                i--;
            var a = Stations[i];
            var b = Stations[i + 1];
            double t = Interpolation == ProfileInterpolation.Constant ? 0 : (station - a.Station) / (b.Station - a.Station);
            var av = a.Values;
            var bv = b.Values;
            var m = Modifiers.Values;
            var values = av.Select((v, k) => NumericGuard.Finite(((1 - t) * v + t * bv[k]) * m[k], "modified rigidity")).ToArray();
            return BeamAnalysisStation.From(station, values);
        }
    }
}
