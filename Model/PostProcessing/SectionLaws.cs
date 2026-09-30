using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Persistence;

namespace GPC.Model.PostProcessing
{
    [Serializable]
    public sealed class BeamSectionStation
    {
        public double Station { get; set; }
        public SectionSide Side { get; set; }
        public ReinforcedConcreteSection Section { get; set; }
    }
    public static class SectionLaws
    {
        public static ReinforcedConcreteSection Evaluate(BeamSectionAssignment a, double station, SectionSide side)
        {
            NumericGuard.Station(station);
            if (a == null || ! (a.Start < a.End) || station < a.Start || station > a.End) throw new ArgumentException("InvalidSectionInterval");
            if (a.Law == "Constant") return a.Section;
            if (a.Law == "Tabulated")
            {
                var samples = a.Stations.Where(s => s.Station == station && (s.Side == side || s.Side == SectionSide.Unspecified)).ToArray();
                if (samples.Length != 1 || samples[0].Section == null) throw new InvalidOperationException("MissingOrAmbiguousTabulatedSection");
                return samples[0].Section;
            }
            if (a.Law != "LinearRectangular") throw new NotSupportedException("UnsupportedSectionInterpolation");
            var first = a.Section; var last = a.EndSection;
            if (!(first?.SectionShape is SectionRectangular x) || !(last?.SectionShape is SectionRectangular y)
                || x.Angle != 0 || y.Angle != 0 || first.SteelSections.Count != 0 || last.SteelSections.Count != 0
                || ModelArchive.Fingerprint(new object[] { first.ConcreteMaterial }) != ModelArchive.Fingerprint(new object[] { last.ConcreteMaterial }))
                throw new NotSupportedException("IncompatibleRectangularSectionLaw");
            double t = (station - a.Start) / (a.End - a.Start);
            var result = new ReinforcedConcreteSection(new SectionRectangular((1 - t) * x.Height + t * y.Height, (1 - t) * x.Width + t * y.Width), first.ConcreteMaterial);
            var bars = first.Rebars.OrderBy(b => b.Id).ToArray(); var ends = last.Rebars.OrderBy(b => b.Id).ToArray();
            if (bars.Length != ends.Length) throw new NotSupportedException("IncompatibleRebarTopology");
            for (int i = 0; i < bars.Length; i++)
            {
                var b = bars[i]; var e = ends[i];
                if (b.Id != e.Id || b.EpsilonP != 0 || e.EpsilonP != 0 || ModelArchive.Fingerprint(new object[] { b.RebarSection }) != ModelArchive.Fingerprint(new object[] { e.RebarSection }))
                    throw new NotSupportedException("IncompatibleRebarTopologyOrPrestress");
                result.AddRebar(new ReinforcedConcreteRebar(b.RebarSection,
                    new Point2d((1 - t) * b.Position.X + t * e.Position.X, (1 - t) * b.Position.Y + t * e.Position.Y), id: b.Id));
            }
            if (AssignmentValidation.ValidateRebars(result).Count != 0) throw new ArgumentException("InvalidInterpolatedReinforcement");
            return result;
        }
    }
}
