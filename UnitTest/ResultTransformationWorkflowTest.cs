using GPC.Examples;
using GPC.Geometry;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class ResultTransformationWorkflowTest
{
    [TestMethod]
    public void Beam_RotationAndRoundTrip_PreserveSourceAndSignedMoments()
    {
        var model = MixedModelFactory.Create();
        var source = Verification.BeamSample(model.BeamElements[250], "synthetic-static", "P+", .5, SectionSide.Unspecified);
        var original = source.ResultBeamForces; var target = new CoordinateSystem(original.CoordinateSystem.Origin, new Vector3d(0,1,0), new Vector3d(-1,0,0));
        var rotated = ResultTransformations.RotateBeam(source, target);
        Assert.AreEqual(1000, rotated.ResultBeamForces.V1, 1e-9); Assert.AreEqual(1e6, rotated.ResultBeamForces.M2, 1e-9);
        Assert.AreEqual(-1e6, source.ResultBeamForces.M1); Assert.AreEqual(ActionBody.PositiveSectionFace, rotated.Body);
        Assert.AreNotSame(source.State, rotated.State); Assert.AreNotSame(source.State.Components, rotated.State.Components);
        var restored = ResultTransformations.RotateBeam(rotated, original.CoordinateSystem);
        Assert.AreEqual(original.M1, restored.ResultBeamForces.M1, 1e-9); Assert.AreEqual(original.V2, restored.ResultBeamForces.V2, 1e-9);
    }

    [TestMethod]
    public void Beam_MomentTransportRequiresExplicitOperation()
    {
        var model = MixedModelFactory.Create();
        var source = Verification.BeamSample(model.BeamElements[250], "synthetic-static", "P+", .5, SectionSide.Unspecified);
        Assert.ThrowsException<NotSupportedException>(() => ResultTransformations.RotateBeam(source, CoordinateSystem.Global));
        var moved = ResultTransformations.TransportBeam(source, CoordinateSystem.Global);
        Assert.AreEqual(-2e6, moved.ResultBeamForces.M1, 1e-8);
        Assert.AreEqual(source.ParametricDistance, moved.ParametricDistance); // Original cut, different reduction point.
    }

    [TestMethod]
    public void Shell_InPlaneTensorRotationAndRoundTrip_KeepPointIdentity()
    {
        var model = MixedModelFactory.Create(); var shell = model.AreaElements[1090];
        var source = (PointResultPlateForces)shell.Results[0].Results[0]; var axes = source.Forces.CoordinateSystem;
        var target = new CoordinateSystem(axes.Origin, axes.V2, axes.V1 * -1, axes.V3);
        var rotated = ResultTransformations.RotateShell(source, target);
        Assert.AreEqual(source.Forces.Fyy, rotated.Forces.Fxx, 1e-8); Assert.AreEqual(-source.Forces.Fxy, rotated.Forces.Fxy, 1e-8);
        Assert.AreEqual(source.Forces.Myy, rotated.Forces.Mxx, 1e-8); Assert.AreEqual(source.Forces.Fyz, rotated.Forces.Fxz, 1e-8);
        var restored = ResultTransformations.RotateShell(rotated, axes);
        Assert.AreEqual(source.Forces.Mxy, restored.Forces.Mxy, 1e-8); Assert.AreEqual(source.Location.X, rotated.Location.X);
        Assert.AreEqual(source.PointKind, rotated.PointKind);
    }

    [TestMethod]
    public void Rotations_RejectMissingComponentsAndShellNormalReversal()
    {
        var model = MixedModelFactory.Create(); var source = (PointResultPlateForces)model.AreaElements[1090].Results[0].Results[0];
        var axes = source.Forces.CoordinateSystem;
        Assert.ThrowsException<NotSupportedException>(() => ResultTransformations.RotateShell(source, new CoordinateSystem(axes.Origin, axes.V1, axes.V2 * -1, axes.V3 * -1)));
        source.State.Components[2] = ComponentAvailability.NotExported;
        Assert.ThrowsException<ArgumentException>(() => ResultTransformations.RotateShell(source, axes));
    }
}
