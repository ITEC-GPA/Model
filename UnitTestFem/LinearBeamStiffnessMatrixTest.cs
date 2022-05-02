using System;
using GPC.Geometry;
using GPC.Model.Fem;
using GPC.Model.Fem.ElementStiffnessMatrices;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFem
{
    [TestClass]
    public class LinearBeamStiffnessMatrixTest : UnitTestBase
    {

        private SectionRectangular GetSectionRectangular(double height = 500, double width = 100)
        {
            return new SectionRectangular(height, width, SteelMaterial.S355); ;
        }


        [TestMethod]
        public void EulerBernulliParallelX()
        {

            SectionRectangular sec = GetSectionRectangular();

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(1500, 0, 0));


            EulerBeam beam = new EulerBeam(node1, node2);
            beam.SetProperty(sec);

            ElementStiffnessMatrix matrix = beam.GetGlobalStiffnessMatrix();

            Console.Write(matrix);

            Assert.IsTrue(matrix.IsSymmetric());

            var dofDX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofDY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofDZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            var dofRX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofRY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofRZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            double tol = 0.01;

            // FX
            Assert.AreEqual(7000000, matrix[0, 0]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX)));
            Assert.AreEqual(-7000000, matrix[0, 6]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node2, dofDX)));

            // FY
            Assert.AreEqual(777777.7778, matrix[2, 2], tol);
            Assert.AreEqual(777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY)), tol);

            Assert.AreEqual(583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node1, dofRZ)), tol);

            Assert.AreEqual(-777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofDY)), tol);

            Assert.AreEqual(583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofRZ)), tol);


            // FZ
            Assert.AreEqual(31111.1111, matrix[1, 1], tol);
            Assert.AreEqual(31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ)), tol);

            Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol);

            Assert.AreEqual(-31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofDZ)), tol);

            Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol);


            // MX
            Assert.AreEqual(7862253415.109, matrix[3, 3]);
            Assert.AreEqual(7862253415.109, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX)), tol);

            Assert.AreEqual(-7862253415.109, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node2, dofRX)));


        }




        [TestMethod]
        public void TrussParallelX()
        {

            SectionRectangular sec = GetSectionRectangular();

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(1500, 0, 0));

            Truss truss = new Truss(node1, node2);
            truss.SetProperty(sec);

            ElementStiffnessMatrix matrix = truss.GetGlobalStiffnessMatrix();

            Console.Write(matrix);

            Assert.IsTrue(matrix.IsSymmetric());

            Assert.AreEqual(7000000, matrix[0, 0]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X))));

            Assert.AreEqual(-7000000, matrix[0, 3]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X)),
                new NodalGlobalDegreeOfFreedom(node2,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X))));

            Assert.AreEqual(7000000, matrix[3, 3]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node2,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X))));


            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y))));

            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z))));

        }


        [TestMethod]
        public void TrussParallelY()
        {

            SectionRectangular sec = GetSectionRectangular();

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 1500, 0));

            Truss truss = new Truss(node1, node2);
            truss.SetProperty(sec);

            ElementStiffnessMatrix matrix = truss.GetGlobalStiffnessMatrix();

            Console.Write(matrix);

            Assert.IsTrue(matrix.IsSymmetric());

            Assert.AreEqual(7000000, matrix[2, 2]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y))));

            Assert.AreEqual(-7000000, matrix[2, 5]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y)),
                new NodalGlobalDegreeOfFreedom(node2,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y))));

            Assert.AreEqual(7000000, matrix[5, 5]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node2,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y))));


            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X))));

            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z))));

        }



        [TestMethod]
        public void TrussParallelZ()
        {

            SectionRectangular sec = GetSectionRectangular();

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 0, 1500));

            Truss truss = new Truss(node1, node2);
            truss.SetProperty(sec);

            ElementStiffnessMatrix matrix = truss.GetGlobalStiffnessMatrix();

            Console.Write(matrix);

            Assert.IsTrue(matrix.IsSymmetric());

            Assert.AreEqual(7000000, matrix[1, 1]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z))));

            Assert.AreEqual(-7000000, matrix[1, 4]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z)),
                new NodalGlobalDegreeOfFreedom(node2,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z))));

            Assert.AreEqual(7000000, matrix[4, 4]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node2,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z))));


            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y))));

            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1,
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X))));

        }

    }
}
