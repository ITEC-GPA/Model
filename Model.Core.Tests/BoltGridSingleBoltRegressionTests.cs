using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using GPC.Model.Sections.Bolt;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPC.Model.Core.Tests;

[TestClass]
public class BoltGridSingleBoltRegressionTests
{
    private static RectangularBoltGrid Single() => new(Array.Empty<double>(), Array.Empty<double>(),
        16, BoltMaterialEN1993Data.Class8_8, new Point2d(10,20));

    [TestMethod]
    public void RotatedLoadAxesAreTransformedBeforeReturningSingleBoltShear()
    {
        var grid = Single();
        // Local axes e1=globalY, e2=-globalX: local(3000,4000) -> global(-4000,3000).
        var system = new CoordinateSystem(new Point3d(10,20,0), new Vector3d(0,1,0), new Vector3d(-1,0,0));
        var applied = new ResultBeamForces(0,3000,4000,0,0,0,system,37);
        var transmitted = grid.CalculateShearForcesElastic(applied).Single().Value;
        Assert.AreEqual(-4000.0, transmitted.V1, 1e-10);
        Assert.AreEqual(3000.0, transmitted.V2, 1e-10);
        Assert.AreEqual(37, transmitted.Id);
        Assert.IsTrue(grid.CheckShearForcesElastic(grid.CalculateShearForcesElastic(applied),applied));
    }

    [DataTestMethod]
    [DataRow(1.0)] [DataRow(-1.0)] [DataRow(1e-12)] [DataRow(1e-200)]
    public void FreeTorsionCannotBeSilentlyDroppedForSinglePointBolt(double torque)
    {
        var grid=Single();
        var applied=new ResultBeamForces(0,3000,4000,torque,0,0,
            new CoordinateSystem(new Point3d(10,20,0),Vector3d.XAxis,Vector3d.YAxis));
        Assert.ThrowsException<InvalidOperationException>(() => grid.CalculateShearForcesElastic(applied));
    }

    [TestMethod]
    public void EccentricShearWithUnbalancedTorqueIsRejected()
    {
        var grid=Single();
        // r=(10,0), F=(0,100): torque at bolt =1000 Nmm.
        var applied=new ResultBeamForces(0,0,100,0,0,0,
            new CoordinateSystem(new Point3d(20,20,0),Vector3d.XAxis,Vector3d.YAxis));
        Assert.ThrowsException<InvalidOperationException>(() => grid.CalculateShearForcesElastic(applied));
    }

    [TestMethod]
    public void AppliedTorqueCanCancelEccentricityExactly()
    {
        var grid=Single();
        var applied=new ResultBeamForces(0,0,100,-1000,0,0,
            new CoordinateSystem(new Point3d(20,20,0),Vector3d.XAxis,Vector3d.YAxis));
        var forces=grid.CalculateShearForcesElastic(applied);
        Assert.AreEqual(0.0,forces.Single().Value.V1);
        Assert.AreEqual(100.0,forces.Single().Value.V2);
        Assert.IsTrue(grid.CheckShearForcesElastic(forces,applied));
    }

    [DataTestMethod]
    [DataRow(0.0,0.0)] [DataRow(3.0,4.0)] [DataRow(1e-12,-1e-12)]
    public void CentredShearKeepsEveryActualDemand(double fx,double fy)
    {
        var grid=Single();
        var applied=new ResultBeamForces(0,fx,fy,0,0,0,
            new CoordinateSystem(new Point3d(10,20,0),Vector3d.XAxis,Vector3d.YAxis));
        var result=grid.CalculateShearForcesElastic(applied).Single().Value;
        Assert.AreEqual(fx,result.V1); Assert.AreEqual(fy,result.V2);
    }
}
