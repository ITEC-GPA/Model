using System;
using GPC.Geometry;
using GPC.Model.Fem;
using GPC.Model.Fem.ElementStiffnessMatrices;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.TestUtilities;
using GPC.Utilities.Extensions;
using GPC.Utilities.Maths;
using GPC.Utilities.Units;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFem
{
    [TestClass]
    public class CoordinateSystemTest : UnitTestBase
    {


        [TestMethod]
        public void LinearBeamVertical()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 0, 1000));

            EulerBeam beam = new EulerBeam(node1, node2);

            var v1 = beam.CoordinateSystem.V1;
            var v2 = beam.CoordinateSystem.V2;
            var v3 = beam.CoordinateSystem.V3;

            Assert.AreEqual(new Vector3d(1, 0, 0), v1);
            Assert.AreEqual(new Vector3d(0, 1, 0), v2);
            Assert.AreEqual(new Vector3d(0, 0, 1), v3);

        }

        [TestMethod]
        public void LinearBeamVerticalRotated()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 0, 1000));
            double angle = 45;
            EulerBeam beam = new EulerBeam(node1, node2, angle.ToRadians());

            var v1 = beam.CoordinateSystem.V1;
            var v2 = beam.CoordinateSystem.V2;
            var v3 = beam.CoordinateSystem.V3;

            Console.WriteLine($"V1: {v1}");
            Console.WriteLine($"V2: {v2}");
            Console.WriteLine($"V3: {v3}");

            Assert.AreEqual(new Vector3d(0.707106781186548, +0.707106781186548, 0), v1);
            Assert.AreEqual(new Vector3d(-0.707106781186548, 0.707106781186548, 0), v2);
            Assert.AreEqual(new Vector3d(0, 0, 1), v3);

        }


        [TestMethod]
        public void LinearBeamParallelX()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(1000, 0, 0));

            EulerBeam beam = new EulerBeam(node1, node2);

            Vector3d v1 = beam.CoordinateSystem.V1;
            Vector3d v2 = beam.CoordinateSystem.V2;
            Vector3d v3 = beam.CoordinateSystem.V3;

            Console.WriteLine($"V1: {v1}");
            Console.WriteLine($"V2: {v2}");
            Console.WriteLine($"V3: {v3}");

            Assert.AreEqual(new Vector3d(0, 0, -1), v1, "Axis 1");
            Assert.AreEqual(new Vector3d(0, 1, 0), v2, "Axis 2");
            Assert.AreEqual(new Vector3d(1, 0, 0), v3, "Axis 3");

        }


        [TestMethod]
        public void LinearBeamParallelXRotated()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(1000, 0, 0));

            double angle = 45;
            EulerBeam beam = new EulerBeam(node1, node2, angle.ToRadians());

            Vector3d v1 = beam.CoordinateSystem.V1;
            Vector3d v2 = beam.CoordinateSystem.V2;
            Vector3d v3 = beam.CoordinateSystem.V3;

            Console.WriteLine($"V1: {v1}");
            Console.WriteLine($"V2: {v2}");
            Console.WriteLine($"V3: {v3}");

            Assert.AreEqual(new Vector3d(0, +0.707106781186547, -0.707106781186547), v1, "Axis 1");
            Assert.AreEqual(new Vector3d(0, +0.707106781186547, +0.707106781186547), v2, "Axis 2");
            Assert.AreEqual(new Vector3d(1, 0, 0), v3, "Axis 3");

        }

        [TestMethod]
        public void LinearBeamParallelXNegative()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(-1000, 0, 0));

            EulerBeam beam = new EulerBeam(node1, node2);

            Vector3d v1 = beam.CoordinateSystem.V1;
            Vector3d v2 = beam.CoordinateSystem.V2;
            Vector3d v3 = beam.CoordinateSystem.V3;

            Assert.AreEqual(new Vector3d(+0, 0, -1), v1, "Axis 1");
            Assert.AreEqual(new Vector3d(+0, -1, 0), v2, "Axis 2");
            Assert.AreEqual(new Vector3d(-1, 0, 0), v3, "Axis 3");

        }

        [TestMethod]
        public void LinearBeamParallelY()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 1000, 0));

            EulerBeam beam = new EulerBeam(node1, node2);

            Vector3d v1 = beam.CoordinateSystem.V1;
            Vector3d v2 = beam.CoordinateSystem.V2;
            Vector3d v3 = beam.CoordinateSystem.V3;

            Assert.AreEqual(new Vector3d(0, 0, -1), v1, "Axis 1");
            Assert.AreEqual(new Vector3d(-1, 0, 0), v2, "Axis 2");
            Assert.AreEqual(new Vector3d(0, 1, 0), v3, "Axis 3");

        }


        [TestMethod]
        public void LinearBeamParallelYRotated()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 1000, 0));

            double angle = 45;
            EulerBeam beam = new EulerBeam(node1, node2, angle.ToRadians());

            Vector3d v1 = beam.CoordinateSystem.V1;
            Vector3d v2 = beam.CoordinateSystem.V2;
            Vector3d v3 = beam.CoordinateSystem.V3;

            Console.WriteLine($"V1: {v1}");
            Console.WriteLine($"V2: {v2}");
            Console.WriteLine($"V3: {v3}");

            Assert.AreEqual(new Vector3d(-0.707106781186547, 0, -0.707106781186547), v1, "Axis 1");
            Assert.AreEqual(new Vector3d(-0.707106781186547, 0, +0.707106781186547), v2, "Axis 2");
            Assert.AreEqual(new Vector3d(0, 1, 0), v3, "Axis 3");

        }

        [TestMethod]
        public void LinearBeamParallelYNegative()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, -1000, 0));

            EulerBeam beam = new EulerBeam(node1, node2);

            Vector3d v1 = beam.CoordinateSystem.V1;
            Vector3d v2 = beam.CoordinateSystem.V2;
            Vector3d v3 = beam.CoordinateSystem.V3;

            Assert.AreEqual(new Vector3d(0, 0, -1), v1, "Axis 1");
            Assert.AreEqual(new Vector3d(1, 0, 0), v2, "Axis 2");
            Assert.AreEqual(new Vector3d(0, -1, 0), v3, "Axis 3");

        }

        [TestMethod]
        public void LinearBeamPlaneZX()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(1000, 0, 1000));

            EulerBeam beam = new EulerBeam(node1, node2);

            Vector3d V1 = beam.CoordinateSystem.V1;
            Vector3d V2 = beam.CoordinateSystem.V2;
            Vector3d V3 = beam.CoordinateSystem.V3;

            Assert.AreEqual(new Vector3d(+0.707106781186548, 0, -0.707106781186548), V1, "Axis 1");
            Assert.AreEqual(new Vector3d(0, 1, 0), V2, "Axis 2");
            Assert.AreEqual(new Vector3d(+0.707106781186548, 0, 0.707106781186548), V3, "Axis 3");

        }

        [TestMethod]
        public void LinearBeamPlaneZY()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 1000, 1000));

            EulerBeam beam = new EulerBeam(node1, node2);

            Vector3d V1 = beam.CoordinateSystem.V1;
            Vector3d V2 = beam.CoordinateSystem.V2;
            Vector3d V3 = beam.CoordinateSystem.V3;

            Assert.AreEqual(new Vector3d(0, +0.707106781186548, -0.707106781186548), V1, "Axis 1");
            Assert.AreEqual(new Vector3d(-1, 0, 0), V2, "Axis 2");
            Assert.AreEqual(new Vector3d(0, +0.707106781186548, 0.707106781186548), V3, "Axis 3");

        }

        [TestMethod]
        public void LinearBeamSpace()
        {

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(1000, 1000, 1000));

            EulerBeam beam = new EulerBeam(node1, node2);

            Vector3d V1 = beam.CoordinateSystem.V1;
            Vector3d V2 = beam.CoordinateSystem.V2;
            Vector3d V3 = beam.CoordinateSystem.V3;

            Console.WriteLine($"V1: {V1}");
            Console.WriteLine($"V2: {V2}");
            Console.WriteLine($"V3: {V3}");

            Assert.AreEqual(new Vector3d(0.408248290463863, 0.408248290463863, -0.816496580927726), V1, "Axis 1");
            Assert.AreEqual(new Vector3d(-0.707106781186548, 0.707106781186548, 0), V2, "Axis 2");
            Assert.AreEqual(new Vector3d(0.577350269189626, 0.577350269189626, 0.577350269189626), V3, "Axis 3");

        }

    }
}
