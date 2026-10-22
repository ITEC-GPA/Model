using System.Text.Json;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Results.Processing;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

public partial class Straus7AssignmentImportTest
{
    // Fresh native R31 API results captured on 2026-10-22, not pre-existing baselines.
    // SetBeamSectionGeometry -> SetBeamMirrorOption -> CalculateBeamSectionProperties
    // -> GetBeamSectionPropertyData, mm/N. Separate rectangle integrals validate A/I.
    private static Straus7Property PairProperty(bool channel, int mirror)
    {
        using var data = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "straus7-identical-pairs.json")));
        var row = data.RootElement.EnumerateArray().Single(r => r.GetProperty("kind").GetString() == (channel ? "C-standard" : "L") && r.GetProperty("mirror").GetInt32() == mirror);
        double[] values = row.GetProperty("properties").EnumerateArray().Select(v => v.GetDouble()).ToArray();
        // The transport double works in m, kN, kPa.
        values[0] /= 1e6; values[1] /= 1e12; values[2] /= 1e12; values[3] /= 1e12;
        for (int k = 4; k < 6; k++) values[k] /= 1000;
        for (int k = 6; k < 8; k++) values[k] /= 1e6;
        values[8] /= 1000; values[9] /= 1000;
        return new() { Number = 2, Name = channel ? "2C native" : "2L native", Formulation = 6,
            SectionType = channel ? 5 : 9, Geometry = row.GetProperty("geometry").EnumerateArray().Select(v => v.GetDouble() / 1000).ToArray(),
            MirrorType = mirror, MirrorGaps = new[] { .01, 0d }, CompatibleTwist = true,
            SectionProperties = values, Material = new[] { 210e6, 80.77e6, .3, 7850, 1.2e-5 }, MaterialName = "Steel S355" };
    }

    [DataTestMethod]
    [DataRow(false, 1)] [DataRow(false, 2)] [DataRow(false, 3)] [DataRow(false, 4)]
    [DataRow(true, 1)] [DataRow(true, 2)] [DataRow(true, 3)] [DataRow(true, 4)]
    public void NativePairPropertiesMatchIndependentIntegralsAndImportedGeometricAxes(bool channel, int mirror)
    {
        var api = new AssignmentApi(); var property = PairProperty(channel, mirror); api.BeamProperties[2] = property;
        var reference = IdenticalPairReference.Integrate(channel, channel ? 200 : 150, channel ? 75 : 100, 10, 12, 10, mirror);
        double angle = property.SectionProperties[10], c = Math.Cos(angle), s = Math.Sin(angle);
        void Close(double a, double b) => Assert.AreEqual(a, b, Math.Max(1, Math.Abs(a)) * 1e-9);
        Close(reference.Area, property.SectionProperties[0] * 1e6);
        Close(c * c * reference.Ixx + s * s * reference.Iyy, property.SectionProperties[1] * 1e12);
        Close(s * s * reference.Ixx + c * c * reference.Iyy, property.SectionProperties[2] * 1e12);
        var report = Import(api); Partial(report);
        var beam = report.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "2");
        var pair = (SectionBuiltUp)((SteelSection)beam.BeamProperty).SectionShape;
        Assert.AreSame(pair.Parts[0].Section, pair.Parts[1].Section);
        Assert.AreEqual(2, pair.GetCurveOutlines().Count(), "Both transformed contours are available to the preview.");
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "SteelSectionTypeAssumed"));
        Close(reference.Area, pair.Area); Close(reference.Ixx, pair.Jxx); Close(reference.Iyy, pair.Jyy); Close(0, pair.Jxy);
        Close(1, beam.CoordinateSystem.V1.Y); Close(1, beam.CoordinateSystem.V2.Z);
        Close(c, beam.Assignments.SectionAxes.V1.Y); Close(-s, beam.Assignments.SectionAxes.V1.Z);
        Close(s, beam.Assignments.SectionAxes.V2.Y); Close(c, beam.Assignments.SectionAxes.V2.Z);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "Straus7IdenticalPairGeometry"));
        var restored = ModelArchive.CopyValue(pair);
        Close(pair.J11, restored.J11); Assert.AreSame(restored.Parts[0].Section, restored.Parts[1].Section);
    }

    [TestMethod]
    public void RotatedPairKeepsNativeLoadFrameAndConvertsOnlyGeometricOffsetComponents()
    {
        var api = new AssignmentApi(); api.BeamProperties[2] = PairProperty(false, 4); // native angle = pi/2
        api.Add(Straus7Entity.Beam, 2, Straus7Attributes.BeamOffset); api.BeamOffsets[2] = new[] { .05, .1 };
        api.Point(2, 1, 1, false, false, 0, -20, 0, .5);
        var report = Import(api); Partial(report);
        var beam = report.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "2");
        Assert.AreEqual(-100, beam.Assignments.SectionCentroidOffset.X, 1e-9);
        Assert.AreEqual(50, beam.Assignments.SectionCentroidOffset.Y, 1e-9);
        var sum = Equilibrium.BeamLoad(beam, beam.Assignments.Loads.Single());
        Assert.AreEqual(0, sum.Force.Y, 1e-8); Assert.AreEqual(-20000, sum.Force.Z, 1e-8);
        Assert.AreEqual(2050 * -20000, sum.Moment.X + sum.Point.Y * sum.Force.Z - sum.Point.Z * sum.Force.Y, 1e-5, "Offset force is applied once in the native frame.");
    }

    [DataTestMethod]
    [DataRow("gap")] [DataRow("four")] [DataRow("inertia")] [DataRow("angle")] [DataRow("bgl")]
    public void AmbiguousOrInconsistentPairCannotBecomeAQualifiedOutline(string problem)
    {
        var api = new AssignmentApi(); var p = PairProperty(false, 3); api.BeamProperties[2] = p;
        if (problem == "gap") p.MirrorGaps = null;
        if (problem == "four") p.MirrorType = 5;
        if (problem == "inertia") p.SectionProperties[1] *= 1.1;
        if (problem == "angle") p.SectionProperties[10] = .2;
        if (problem == "bgl") { p.SectionType = 17; p.BglShape = 5; p.BglDimensions = new double[16]; }
        var report = Import(api); Partial(report);
        Assert.IsNull(report.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "2").BeamProperty);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "Straus7MirroredSectionByValues"));
    }
}
