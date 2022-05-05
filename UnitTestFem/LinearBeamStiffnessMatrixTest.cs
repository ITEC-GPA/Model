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

            Console.WriteLine(sec.Material.E);
            Console.WriteLine(sec.Material.Ni);
            Console.WriteLine(sec.Material.GetShearModule());

            Console.WriteLine(sec.Jxx);
            Console.WriteLine(sec.Jyy);
            Console.WriteLine(sec.Jt);

            Console.WriteLine(matrix.ToStringKeyMatrix());
            
            // Assert
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
            Assert.AreEqual(777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY)), tol);

            Assert.AreEqual(583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node1, dofRZ)), tol);

            Assert.AreEqual(-777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofDY)), tol);

            Assert.AreEqual(583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofRZ)), tol);


            // FZ
            Assert.AreEqual(31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ)), tol);

            Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol);

            Assert.AreEqual(-31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofDZ)), tol);

            Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol);


            // MX
            Assert.AreEqual(7862253415, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node1, dofRX)), tol * 10E10);


            // MY
            Assert.AreEqual(23333333333, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node1, dofRY)), tol * 100);
            Assert.AreEqual(11666666667, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol * 100);

            Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node1, dofDZ)), tol * 100);
            Assert.AreEqual(+23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node2, dofDZ)), tol * 100);


            // MZ
            Assert.AreEqual(+5.8333333333E+11, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node1, dofRZ)), tol * 1000);
            Assert.AreEqual(+583333333.3, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node1, dofDY)), tol * 1000);

            Assert.AreEqual(-583333333.3, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node2, dofDY)), tol * 1000);

            Assert.AreEqual(+2.91666666666E+11, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node2, dofRZ)), tol * 1000);



        }



        [TestMethod]
        public void EulerBernulliParallelY()
        {

            SectionRectangular sec = GetSectionRectangular();

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 1500, 0));

            EulerBeam beam = new EulerBeam(node1, node2);
            beam.SetProperty(sec);

            ElementStiffnessMatrix matrix = beam.GetGlobalStiffnessMatrix();


            Console.WriteLine(matrix.ToStringKeyMatrix());

            // Assert
            Assert.IsTrue(matrix.IsSymmetric());

            var dofDX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofDY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofDZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            var dofRX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofRY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofRZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            double tol = 0.01;

            // FX
            Assert.AreEqual(777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX)), tol);
            Assert.AreEqual(-583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node1, dofRZ)), tol);
            Assert.AreEqual(-777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node2, dofDX)), tol);
            Assert.AreEqual(-583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node2, dofRZ)), tol);


            // FX
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY)));
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofDY)));


            // FZ
            Assert.AreEqual(31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ)), tol);
            Assert.AreEqual(23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofRX)), tol);
            Assert.AreEqual(-31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofDZ)), tol);
            Assert.AreEqual(23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofRX)), tol);


            // MY
            Assert.AreEqual(7862253415, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node1, dofRY)), tol * 10E10);


            // MY
            Assert.AreEqual(23333333333, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node1, dofRX)), tol * 100);
            Assert.AreEqual(11666666667, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node2, dofRX)), tol * 100);
            Assert.AreEqual(+23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node1, dofDZ)), tol * 100);
            Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node2, dofDZ)), tol * 100);


            // MZ
            Assert.AreEqual(+5.8333333333E+11, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node1, dofRZ)), tol * 1000);
            Assert.AreEqual(-583333333.3, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node1, dofDX)), tol * 1000);
            Assert.AreEqual(+583333333.3, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node2, dofDX)), tol * 1000);
            Assert.AreEqual(+2.91666666666E+11, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node2, dofRZ)), tol * 1000);



        }



        [TestMethod]
        public void EulerBernulliParallelZ()
        {

            SectionRectangular sec = GetSectionRectangular();

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(0, 0, 1500));

            EulerBeam beam = new EulerBeam(node1, node2);
            beam.SetProperty(sec);

            ElementStiffnessMatrix matrix = beam.GetGlobalStiffnessMatrix();


            Console.WriteLine(matrix.ToStringKeyMatrix());

            // Assert
            Assert.IsTrue(matrix.IsSymmetric());

            var dofDX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofDY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofDZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            var dofRX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofRY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofRZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            double tol = 0.01;

            // FX   
            Assert.AreEqual(31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX)), tol);
            Assert.AreEqual(23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol);
            Assert.AreEqual(-31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node2, dofDX)), tol);
            Assert.AreEqual(23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol);


            // FZ
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ)));
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofDZ)));


            // FY
            Assert.AreEqual(777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY)), tol);
            Assert.AreEqual(-583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node1, dofRX)), tol);
            Assert.AreEqual(-777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofDY)), tol);
            Assert.AreEqual(-583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofRX)), tol);


            // MZ
            Assert.AreEqual(7862253415, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRZ), new NodalGlobalDegreeOfFreedom(node1, dofRZ)), tol * 10E10);


            // MY
            Assert.AreEqual(23333333333, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node1, dofRY)), tol * 100);
            Assert.AreEqual(11666666667, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node2, dofRY)), tol * 100);
            Assert.AreEqual(+23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node1, dofDX)), tol * 100);
            Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRY), new NodalGlobalDegreeOfFreedom(node2, dofDX)), tol * 100);


            // MX
            Assert.AreEqual(+5.8333333333E+11, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node1, dofRX)), tol * 1000);
            Assert.AreEqual(-583333333.3, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node1, dofDY)), tol * 1000);
            Assert.AreEqual(+583333333.3, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node2, dofDY)), tol * 1000);
            Assert.AreEqual(+2.91666666666E+11, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofRX), new NodalGlobalDegreeOfFreedom(node2, dofRX)), tol * 1000);



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

            var dofDX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofDY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofDZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            Console.Write(matrix.ToStringKeyMatrix());

            Assert.IsTrue(matrix.IsSymmetric());

            Assert.AreEqual(7000000, matrix[0, 0]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX)));

            Assert.AreEqual(-7000000, matrix[0, 3]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX), new NodalGlobalDegreeOfFreedom(node2, dofDX)));

            Assert.AreEqual(7000000, matrix[3, 3]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node2, dofDX)));


            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY)));

            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ)));

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

            var dofDX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofDY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofDZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            Console.Write(matrix.ToStringKeyMatrix());

            Assert.IsTrue(matrix.IsSymmetric());

            Assert.AreEqual(7000000, matrix[1, 1]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY)));

            Assert.AreEqual(-7000000, matrix[1, 4]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY), new NodalGlobalDegreeOfFreedom(node2, dofDY)));

            Assert.AreEqual(7000000, matrix[4, 4]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node2, dofDY)));


            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX)));

            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ)));

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

            var dofDX = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X);
            var dofDY = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y);
            var dofDZ = new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z);

            Console.Write(matrix.ToStringKeyMatrix());

            Assert.IsTrue(matrix.IsSymmetric());

            Assert.AreEqual(7000000, matrix[2, 2]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ)));

            Assert.AreEqual(-7000000, matrix[2, 5]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDZ), new NodalGlobalDegreeOfFreedom(node2, dofDZ)));

            Assert.AreEqual(7000000, matrix[5, 5]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node2, dofDZ)));


            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDY)));

            Assert.AreEqual(0, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, dofDX)));

        }

    }
}
