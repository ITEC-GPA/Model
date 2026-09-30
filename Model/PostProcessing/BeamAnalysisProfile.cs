using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.PostProcessing
{
    public enum ProfileInterpolation { Constant, Linear }

    /// <summary>Analysis rigidities EA, GA1, GA2 (N), GJ, EI1, EI2 (N mm²).
    /// These values never replace the physical section or its resistance material.</summary>
    [Serializable]
    public sealed class BeamAnalysisStation
    {
        public double Station { get; set; }
        public double EA { get; set; } = double.NaN;
        public double GA1 { get; set; } = double.NaN;
        public double GA2 { get; set; } = double.NaN;
        public double GJ { get; set; } = double.NaN;
        public double EI1 { get; set; } = double.NaN;
        public double EI2 { get; set; } = double.NaN;
        public double MassPerLength { get; set; } = double.NaN;
        internal double[] Values => new[] { EA, GA1, GA2, GJ, EI1, EI2, MassPerLength };
        internal static BeamAnalysisStation From(double station, double[] v) => new BeamAnalysisStation
        { Station = station, EA = v[0], GA1 = v[1], GA2 = v[2], GJ = v[3], EI1 = v[4], EI2 = v[5], MassPerLength = v[6] };
    }

    /// <summary>Dimensionless modifiers, deliberately separate from physical resistance properties.</summary>
    [Serializable]
    public sealed class BeamStiffnessModifiers
    {
        public double Axial { get; set; } = 1;
        public double Shear1 { get; set; } = 1;
        public double Shear2 { get; set; } = 1;
        public double Torsion { get; set; } = 1;
        public double Bending1 { get; set; } = 1;
        public double Bending2 { get; set; } = 1;
        public double Mass { get; set; } = 1;
        internal double[] Values => new[] { Axial, Shear1, Shear2, Torsion, Bending1, Bending2, Mass };
    }

    [Serializable]
    public sealed class BeamAnalysisProfile
    {
        public string StationDomain { get; set; } = "OffsetToOffset";
        public ProfileInterpolation Interpolation { get; set; }
        public List<BeamAnalysisStation> Stations { get; private set; } = new List<BeamAnalysisStation>();
        public BeamStiffnessModifiers Modifiers { get; set; } = new BeamStiffnessModifiers();
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
                if (s.Station <= last) throw new ArgumentException("UnorderedAnalysisProfile");
                last = s.Station; ValidateValues(s.Values);
            }
            ValidateValues(Modifiers.Values);
        }
        private static void ValidateValues(double[] values)
        {
            foreach (var v in values) if (NumericGuard.Finite(v, "analysis property") < 0) throw new ArgumentException("NegativeAnalysisProperty");
        }
        /// <summary>Piecewise constant (left/right limits explicit) or piecewise linear. No extrapolation.</summary>
        public BeamAnalysisStation At(double station, SectionSide side = SectionSide.Unspecified)
        {
            Validate(); NumericGuard.Station(station);
            int i = Stations.FindLastIndex(s => s.Station <= station);
            if (i > 0 && station == Stations[i].Station && station != 1 && Interpolation == ProfileInterpolation.Constant)
            {
                if (side == SectionSide.Unspecified) throw new InvalidOperationException("AmbiguousAnalysisProfileSide");
                if (side == SectionSide.Left) i--;
            }
            if (i == Stations.Count - 1) i--;
            var a = Stations[i]; var b = Stations[i + 1];
            double t = Interpolation == ProfileInterpolation.Constant ? 0 : (station - a.Station) / (b.Station - a.Station);
            var av = a.Values; var bv = b.Values; var m = Modifiers.Values;
            var values = av.Select((v, k) => NumericGuard.Finite(((1 - t) * v + t * bv[k]) * m[k], "modified rigidity")).ToArray();
            return BeamAnalysisStation.From(station, values);
        }
    }
}
