using System.IO.Compression;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Compatibility;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;
using GPC.Model.Core.Coordinates;
using GPC.Model.Checking.Preparation;

namespace UnitTest;

/// <summary>Opt-in native tests (installed and licensed Straus7 R3 x64): the versioned fixture of generate_native_elements_fixture.py, the manufacturer's
/// plate example and real BridGHe models. Every source is copied into a new temporary folder before the read-only API session.</summary>
[TestClass]
public class Straus7NativeElementsTest
{
    private static string Source(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(value)) Assert.Inconclusive("Set " + variable + " (see POST_PROCESSING.md, Straus7 native tests).");
        return value!;
    }
    private static string Copy(params string[] files)
    {
        var folder = Path.Combine(Path.GetTempPath(), "gpc-straus7-native-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        foreach (var file in files) File.Copy(file, Path.Combine(folder, Path.GetFileName(file)));
        return folder;
    }
    private static string Elements()
    {
        var value = Source("GPC_STRAUS7_ELEMENTS");
        if (value != "versioned") return Copy(Path.Combine(value, "elements.st7"), Path.Combine(value, "elements.LSA"));
        var folder = Path.Combine(Path.GetTempPath(), "gpc-straus7-native-" + Guid.NewGuid().ToString("N"));
        ZipFile.ExtractToDirectory(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Straus7", "native-elements-r31.zip"), folder);
        return folder;
    }
    private static T Find<T>(GPC.Model.Models.Model m, EntityFamily family, string id) where T : class =>
        (T)(object)m.FindBySource(new GPC.Model.Core.Identity.SourceIdentity("Straus7", m.AnalysisSource.ModelRevision, family, id))!;
    private static void Print(IEnumerable<ModelDiagnostic> diagnostics) { foreach (var d in diagnostics) Console.WriteLine(d.Severity + " " + d.Code + " [" + d.Record + "] " + d.Message); }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeElements_ReleasesSpringsLinksCombinationsAndPlateCorners()
    {
        var folder = Elements(); var path = Path.Combine(folder, "elements.st7"); var results = Path.Combine(folder, "elements.LSA");
        var converter = new Straus7ApiConverter();
        var geometry = converter.Import(new Straus7ImportRequest { ModelPath = path, ModelRevision = "native-elements-v1", AnalysisId = "lsa-1" });
        Print(geometry.Diagnostics);
        Assert.AreEqual(ImportStatus.Partial, geometry.Status);
        var m = geometry.Model;
        Assert.AreEqual(22, m.NodesElements.Count); Assert.AreEqual(4, m.BeamElements.Count); Assert.AreEqual(2, m.AreaElements.Count);
        Assert.IsTrue(geometry.Diagnostics.Any(d => d.Code == "Straus7BricksPreserved"));
        var hinge = Find<BeamElement>(m, EntityFamily.Beam, "1").Attributes.Values.OfType<BeamReleasesAttribute>().Single();
        Assert.AreEqual(BeamConnectionKind.Released, hinge.J[3].Kind); Assert.AreEqual(BeamConnectionKind.Continuous, hinge.J[4].Kind);
        Assert.IsTrue(hinge.I.All(d => d.Kind == BeamConnectionKind.Continuous));
        var partial = Find<BeamElement>(m, EntityFamily.Beam, "2").Attributes.Values.OfType<BeamReleasesAttribute>().Single();
        Assert.AreEqual(BeamConnectionKind.AbsoluteStiffness, partial.J[3].Kind); Assert.AreEqual(1e9, partial.J[3].Value, 1);
        var truss = Find<BeamElement>(m, EntityFamily.Beam, "3"); var cable = Find<BeamElement>(m, EntityFamily.Beam, "4");
        Assert.AreEqual(BeamFormulation.Truss, truss.Assignments.Formulation); Assert.IsInstanceOfType(((SteelSection)truss.BeamProperty).SectionShape, typeof(GPC.Model.Sections.SectionCHS));
        Assert.AreEqual(BeamFormulation.Cable, cable.Assignments.Formulation);
        var spring = Find<NodeElement>(m, EntityFamily.Node, "1").Assignments.Links.Single(l => l.Spring != null).Spring;
        Assert.AreEqual(2000, spring[2, 2], 1e-9); Assert.AreEqual(-1, spring.CoordinateSystem.V3.Z, 1e-12, "Axial along the spring element 1 -> 6 (down).");
        var ground = Find<NodeElement>(m, EntityFamily.Node, "2").Assignments.GroundSprings.Single();
        Assert.AreEqual(500, ground[2, 2], 1e-9); Assert.AreEqual(0, ground[0, 0]);
        Assert.AreEqual(1, Find<NodeElement>(m, EntityFamily.Node, "2").Assignments.Links.Count(l => l.KinematicCoefficients?.Length == 72), "Rigid XYZ link 2-7.");
        Assert.AreEqual(1, Find<NodeElement>(m, EntityFamily.Node, "7").Assignments.Links.Count(l => l.KinematicCoefficients?.Length == 72), "Master-slave 7-8, coincident.");
        var uls = CombinationExpansion.Definitions(m).Single();
        Assert.AreEqual("ULS", uls.Name); Assert.AreEqual(1.35, uls.Terms.Single(t => t.Name == Straus7ApiConverter.CaseName(1, "Dead")).Factor);
        Assert.AreEqual(1.5, uls.Terms.Single(t => t.Name == Straus7ApiConverter.CaseName(2, "Live")).Factor);
        Assert.IsTrue(m.Combinations.ContainsKey("ULS"));

        var native = converter.ReadResults(m, ResultFilter.All(m), path, results, includePlateNodeResults: true);
        Print(native.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, native.Status);
        var imported = Straus7LinearStaticResults.Import(m, native, "native-elements");
        Print(imported.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, imported.Status);
        var dead = Straus7ApiConverter.CaseName(1, "Dead"); var live = Straus7ApiConverter.CaseName(2, "Live");
        foreach (var plate in m.AreaElements.Values)
            foreach (var name in new[] { dead, live })
            {
                var samples = plate.Results.SelectMany(r => r.Results).OfType<PointResultPlateForces>().Where(s => s.Case?.Name == name).ToArray();
                var centre = samples.Single(s => s.PointKind == ShellResultPointKind.Centroid).Forces;
                var corners = samples.Where(s => s.PointKind == ShellResultPointKind.ElementNodeExtrapolated).ToArray();
                Assert.AreEqual(4, corners.Length); CollectionAssert.AreEquivalent(plate.Nodes.Select(n => (int?)n.Id).ToArray(), corners.Select(c => c.SourceNodeId).ToArray());
                // Bilinear extrapolation from the 2x2 Gauss points: the mean of the corner values is the centre value.
                Assert.AreEqual(centre.Mxx, corners.Average(c => c.Forces.Mxx), 1e-6 * Math.Max(1, Math.Abs(centre.Mxx)) + 1e-6, name);
                Assert.AreEqual(centre.Fxx, corners.Average(c => c.Forces.Fxx), 1e-6 * Math.Max(1, Math.Abs(centre.Fxx)) + 1e-6, name);
                if (name == dead) Assert.IsTrue(corners.Max(c => c.Forces.Mxx) - corners.Min(c => c.Forces.Mxx) > 1, "Cantilever moments vary along the plate.");
                foreach (var c in corners)
                {
                    var node = plate.Nodes.Single(n => n.Id == c.SourceNodeId); Vector3d d = node.Position - plate.CoordinateSystem.Origin;
                    Assert.AreEqual(Axes.Dot(d, plate.CoordinateSystem.V1), c.Location.X, 1e-6); Assert.AreEqual(Axes.Dot(d, plate.CoordinateSystem.V2), c.Location.Y, 1e-6);
                }
            }
        // The hinge: no bending about principal axis 1 at End 2 of beam 1.
        var end = Verification.BeamSamples(Find<BeamElement>(m, EntityFamily.Beam, "1"), "native-elements", live).OrderBy(s => s.ParametricDistance).Last();
        Assert.AreEqual(1, end.ParametricDistance, 1e-12); Assert.AreEqual(0, end.ResultBeamForces.M1, 1e-3);
        // Equilibrium of case Dead: applied loads (imported) + support reactions + node spring force at node 2.
        double applied = m.NodesElements.Values.SelectMany(n => n.Loads.Values.OfType<PointLoad>()).Where(l => l.LoadCase.Name == dead).Sum(l => l.F3)
            + m.BeamElements.Values.Sum(b => b.Assignments.Loads.Where(l => (l.Concentrated?.LoadCase ?? l.StartIntensity.LoadCase).Name == dead).Sum(l => Equilibrium.BeamLoad(b, l).Force.Z));
        Assert.AreEqual(-10000, applied, 1e-6);
        double reactions = m.NodesElements.Values.SelectMany(n => n.Results.SelectMany(r => r.Results)).OfType<NodeResultForces>()
            .Where(r => r.Case?.Name == dead && r.Kind == NodalForceKind.SupportReaction).Sum(r => r.Fz);
        var uz = m.NodesElements.Values.Single(n => n.Source.OriginalId == "2").Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().Single(r => r.Case?.Name == dead).ResultDisplacement.D3;
        Console.WriteLine($"applied {applied}, reactions {reactions}, spring {-500 * uz}");
        Assert.AreEqual(0, applied + reactions, 1e-6, "The solver reactions balance the imported loads.");
        // Straus7 reports the force of a node spring as the reaction of its node: -k uz at node 2, which has no restraint.
        var springReaction = m.NodesElements.Values.Single(n => n.Source.OriginalId == "2").Results.SelectMany(r => r.Results).OfType<NodeResultForces>()
            .Single(r => r.Case?.Name == dead && r.Kind == NodalForceKind.SupportReaction).Fz;
        Assert.AreEqual(-500 * uz, springReaction, 1e-6);
    }

    /// <summary>The manufacturer's plate example (C:\ProgramData\Straus7 R31\API Examples\Matlab\Plates.st7): geometry, properties and axes.</summary>
    [TestMethod, TestCategory("Straus7Native")]
    public void NativeManufacturerPlates_AreImported()
    {
        var folder = Copy(Source("GPC_STRAUS7_PLATES"));
        var report = new Straus7ApiConverter().Import(new Straus7ImportRequest { ModelPath = Directory.GetFiles(folder).Single(), ModelRevision = "plates", AnalysisId = "none" });
        Print(report.Diagnostics.Take(20));
        Assert.AreEqual(ImportStatus.Partial, report.Status);
        Assert.AreEqual(5175, report.Model.NodesElements.Count); Assert.AreEqual(4928, report.Model.AreaElements.Count);
        Assert.IsTrue(report.Model.AreaElements.Values.All(a => a.CoordinateSystem != null));
    }

    /// <summary>Real BridGHe models (GPC_STRAUS7_BRIDGHE: a folder with 0x-Straus.st7 and .LSA copies): every model and its static results are read,
    /// rigid links and LSA combinations included, and the imported loads balance the solver reactions of every case.</summary>
    [TestMethod, TestCategory("Straus7Native")]
    public void NativeBridgheModels_AreReadWithTheirResultsAndBalance()
    {
        var source = Source("GPC_STRAUS7_BRIDGHE"); int models = 0;
        foreach (var file in Directory.GetFiles(source, "*.st7").OrderBy(f => f))
        {
            var lsa = Path.ChangeExtension(file, ".LSA"); if (!File.Exists(lsa)) continue;
            var folder = Copy(file, lsa); var path = Path.Combine(folder, Path.GetFileName(file)); var results = Path.Combine(folder, Path.GetFileName(lsa));
            var converter = new Straus7ApiConverter();
            var geometry = converter.Import(new Straus7ImportRequest { ModelPath = path, ModelRevision = Path.GetFileNameWithoutExtension(file), AnalysisId = "lsa" });
            Assert.AreEqual(ImportStatus.Partial, geometry.Status, Path.GetFileName(file) + ": " + string.Join("; ", geometry.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Message)));
            var m = geometry.Model;
            // Only the solved load cases have results (BridGHe leaves the default case "Dead" out of the solution).
            var listing = converter.ReadResults(new Straus7ResultsRequest { ModelPath = path, ResultPath = results });
            Assert.AreEqual(ImportStatus.Completed, listing.Status, string.Join("; ", listing.Diagnostics.Select(d => d.Message)));
            var solved = Straus7LinearStaticResults.CaseMap(m, listing.Cases).Values.ToArray();
            var native = converter.ReadResults(m, new ResultFilter { StaticCases = solved }.Resolve(m), path, results);
            Assert.AreEqual(ImportStatus.Completed, native.Status, string.Join("; ", native.Diagnostics.Select(d => d.Message)));
            var imported = Straus7LinearStaticResults.Import(m, native, "bridghe");
            Assert.AreEqual(ImportStatus.Completed, imported.Status, string.Join("; ", imported.Diagnostics.Select(d => d.Message)));
            Console.WriteLine($"{Path.GetFileName(file)}: nodes {m.NodesElements.Count}, beams {m.BeamElements.Count} ({m.BeamElements.Values.Count(b => b.BeamProperty is not null)} with property), shells {m.AreaElements.Count},"
                + $" links {m.NodesElements.Values.Sum(n => n.Assignments.Links.Count)}, releases {m.BeamElements.Values.Count(b => b.Attributes.Values.OfType<BeamReleasesAttribute>().Any())},"
                + $" cases {m.LoadCases.Count}, combinations {CombinationExpansion.Definitions(m).Count}, samples {imported.ImportedSamples} | "
                + string.Join(", ", geometry.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).GroupBy(d => d.Code).Select(g => g.Key + "x" + g.Count())));
            foreach (var c in native.Cases)
            {
                var name = Straus7LinearStaticResults.CaseMap(m, native.Cases)[c.Number];
                var contributions = new List<EquilibriumContribution>();
                foreach (var node in m.NodesElements.Values) contributions.AddRange(node.Loads.Values.OfType<PointLoad>().Where(l => l.LoadCase.Name == name).Select(l => Equilibrium.PointLoad(l, "node")));
                foreach (var beam in m.BeamElements.Values)
                    contributions.AddRange(beam.Assignments.Loads.Where(l => (l.Concentrated?.LoadCase ?? l.StartIntensity.LoadCase).Name == name).Select(l => Equilibrium.BeamLoad(beam, l)));
                contributions.AddRange(m.ModelLoads.Values.OfType<ModelGravityLoad>().Where(g => g.LoadCase.Name == name).Select(g => Equilibrium.Gravity(m, g, "gravity")));
                double applied = contributions.Sum(x => x.Force.Z);
                double reactions = m.NodesElements.Values.SelectMany(n => n.Results.SelectMany(r => r.Results)).OfType<NodeResultForces>()
                    .Where(r => r.Case?.Name == name && r.Kind == NodalForceKind.SupportReaction).Sum(r => r.Fz);
                Console.WriteLine($"  {name}: applied Z {applied:F3} N, reactions Z {reactions:F3} N");
                Assert.AreEqual(0, applied + reactions, 1e-6 * Math.Max(1, Math.Abs(applied)), name);
            }
            models++;
        }
        Assert.IsTrue(models >= 2, "At least two BridGHe models with results are required.");
    }
}
