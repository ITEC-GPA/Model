using System.Globalization;
using GPC.Converter;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Results.Processing;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;

namespace UnitTest;

/// <summary>Canonical text of the structural content of a model, by source IDs and independent of the source program and records:
/// two imports of the same structure must give the same lines (numbers with 9 significant digits, N and mm).</summary>
internal static class ModelSignature
{
    public static string[] Lines(GPC.Model.Models.Model m, bool includeSprings = true)
    {
        var lines = new List<string>();
        string N(double v) => Math.Abs(v) < 1e-9 ? "0" : v.ToString("G9", CultureInfo.InvariantCulture);
        string V(Vector3d? v) => v == null ? "-" : "(" + N(v.X) + "," + N(v.Y) + "," + N(v.Z) + ")";
        string P(Point3d? p) => p == null ? "-" : "(" + N(p.X) + "," + N(p.Y) + "," + N(p.Z) + ")";
        string A(CoordinateSystem? a) => a == null ? "-" : "[" + V(a.V1) + V(a.V2) + V(a.V3) + "]";
        string Id(Element e) => e.Source?.OriginalId ?? "#" + e.Id;
        string Node(int id) => m.NodesElements.TryGetValue(id, out var n) ? Id(n) : "#" + id;
        string Material(GPC.Model.Materials.Material? material) => material == null ? "-" : material.Name + " E=" + N(material.ElasticModulusCompression) + " rho=" + material.Density.ToString("G4", CultureInfo.InvariantCulture);
        foreach (var n in m.NodesElements.Values)
        {
            lines.Add("node " + Id(n) + " " + P(n.Position) + (n.CoordinateSystem == null ? "" : " axes " + A(n.CoordinateSystem)));
            foreach (var r in n.Assignments.Restrains)
                lines.Add("restraint " + Id(n) + " " + string.Join("", r.Restrain.Restrains.Select(d => d.Dof.ToString()).OrderBy(s => s)) + " " + A(r.Restrain.CoordinateSystem));
            if (includeSprings)
                foreach (var s in n.Assignments.GroundSprings)
                    lines.Add("spring " + Id(n) + " " + string.Join(",", s.ToArray().Select(N)) + " " + A(s.CoordinateSystem));
            foreach (var link in n.Assignments.Links)
                lines.Add("link " + Id(n) + "-" + Node(link.OtherNodeId) + (link.Spring != null ? " spring " + string.Join(",", link.Spring.ToArray().Select(N)) + " " + A(link.Spring.CoordinateSystem)
                    : " rigid " + string.Join(",", link.KinematicCoefficients.Select(N)) + " arm " + V(link.LeverArm)));
            foreach (var l in n.Loads.Values.OfType<PointLoad>())
                lines.Add("nodeload " + Id(n) + " " + l.LoadCase.Name + " " + string.Join(",", new[] { l.F1, l.F2, l.F3, l.M1, l.M2, l.M3 }.Select(N)) + " " + A(l.CoordinateSystem));
        }
        foreach (var b in m.BeamElements.Values)
        {
            var a = b.Assignments; string property = "-";
            if (b.BeamProperty is SteelSection steel) property = "steel " + steel.SectionShape.GetType().Name + " A=" + N(steel.SectionShape.Area) + " J=" + N(steel.SectionShape.Jxx) + "/" + N(steel.SectionShape.Jyy) + " " + Material(steel.SteelMaterial);
            else if (b.BeamProperty is ReinforcedConcreteSection rc) property = "rc " + rc.SectionShape.GetType().Name + " A=" + N(rc.SectionShape.Area) + " " + Material(rc.ConcreteMaterial);
            else if (b.BeamProperty is not null) property = b.BeamProperty.GetType().Name;
            lines.Add("beam " + Id(b) + " " + Id(b.NodeI) + "-" + Id(b.NodeJ) + " " + a.Formulation + " " + A(a.SectionAxes) + " " + property
                + (b.BeamProperty is not null && a.SectionCentroidOffset != null ? " centroid (" + N(a.SectionCentroidOffset.X) + "," + N(a.SectionCentroidOffset.Y) + ")" : "")
                + " offsets " + V(a.OffsetI) + V(a.OffsetJ) + " " + A(a.OffsetAxes) + " rigid " + N(a.RigidLengthI) + "/" + N(a.RigidLengthJ));
            foreach (var r in b.Attributes.Values.OfType<BeamReleasesAttribute>())
                lines.Add("release " + Id(b) + " I " + string.Join(",", r.I.Select(d => d.Kind + ":" + N(d.Value))) + " J " + string.Join(",", r.J.Select(d => d.Kind + ":" + N(d.Value))) + " " + A(r.CoordinateSystem));
            foreach (var l in a.Loads)
            {
                var c = Equilibrium.BeamLoad(b, l); var loadCase = (l.Concentrated?.LoadCase ?? l.StartIntensity.LoadCase).Name;
                lines.Add("beamload " + Id(b) + " " + loadCase + " " + N(l.Start) + "-" + N(l.End) + " " + l.LengthConvention + " F" + V(c.Force) + " M" + V(c.Moment) + " at " + P(c.Point));
            }
        }
        foreach (var s in m.AreaElements.Values)
        {
            string property = s.PlateProperty switch
            {
                ConcretePlateProperty c => "concrete t=" + N(c.MembraneThickness) + "/" + N(c.BendingThickness) + " " + Material(c.ConcreteMaterial),
                GPC.Model.Sections.Steel.SteelPlateProperty st => "steel t=" + N(st.MembraneThickness) + "/" + N(st.BendingThickness) + " " + Material(st.SteelMaterial),
                null => "-", var other => other.GetType().Name
            };
            lines.Add("shell " + Id(s) + " " + string.Join("-", s.Nodes.Select(Id)) + " " + A(s.CoordinateSystem) + " " + property + " t=" + (s.Assignments.PhysicalThickness?.ToString("G9", CultureInfo.InvariantCulture) ?? "-")
                + " offset=" + (s.Assignments.Offset?.ToString("G9", CultureInfo.InvariantCulture) ?? "-"));
            foreach (var l in s.Loads.Values)
                lines.Add("shellload " + Id(s) + " " + l.LoadCase.Name + " " + l switch
                {
                    NormalAreaLoad n => "normal " + N(n.Pressure),
                    AreaLoad t => "traction " + V(t.CoordinateSystem.V1 * t.P1 + t.CoordinateSystem.V2 * t.P2 + t.CoordinateSystem.V3 * t.P3),
                    LineLoad e => "edge " + P(e.Line.Start) + P(e.Line.End) + " " + N(e.F1) + "," + N(e.F2) + "," + N(e.F3) + " " + A(e.CoordinateSystem),
                    NonUniformPlatePressure p => "varying " + string.Join(",", p.Pressures.Select(N)) + " " + V(p.GetGlobalLoadVector()),
                    _ => l.GetType().Name
                });
        }
        foreach (var c in m.LoadCases.Values) lines.Add("case " + c.Name + " " + c.GetType().Name);
        foreach (var g in m.ModelLoads.Values.OfType<ModelGravityLoad>()) lines.Add("gravity " + g.LoadCase.Name + " " + V(g.GravityVersor) + " " + N(g.Acceleration));
        foreach (var g in m.Groups.Keys) lines.Add("group " + g + " " + string.Join(",", m.GetGroupElements(g).Select(e => e.GetType().Name + ":" + Id(e)).OrderBy(x => x, StringComparer.Ordinal)));
        foreach (var c in m.Combinations.Keys) lines.Add("combination " + c);
        foreach (var d in CombinationExpansion.Definitions(m))
            lines.Add("definition " + d.Id + " " + d.Kind + " " + d.Status + " " + string.Join(";", d.Terms.Select(t => t.Analysis + ":" + t.Name + ":" + N(t.Factor))));
        foreach (var s in m.Stages.Values) lines.Add("stage " + s.Id + " " + s.Name + " " + (s.Definition?.GroupChanges.Count.ToString(CultureInfo.InvariantCulture) ?? "-"));
        return lines.OrderBy(l => l, StringComparer.Ordinal).ToArray();
    }

    /// <summary>Fails with the first differing lines of the two models.</summary>
    public static void AssertEqual(GPC.Model.Models.Model expected, GPC.Model.Models.Model actual, bool includeSprings = true)
    {
        var a = Lines(expected, includeSprings); var b = Lines(actual, includeSprings);
        var missing = a.Except(b).Take(8).ToArray(); var extra = b.Except(a).Take(8).ToArray();
        if (missing.Length + extra.Length != 0 || a.Length != b.Length)
            throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException("Models differ (" + a.Length + " vs " + b.Length + " lines).\nOnly in the first:\n  "
                + string.Join("\n  ", missing) + "\nOnly in the second:\n  " + string.Join("\n  ", extra));
    }
}
