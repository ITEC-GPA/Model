using System;
using GPC.Geometry;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Fem.ElementStiffnessMatrices;
using GPC.Model.Materials;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Fem;

namespace UnitTestFem
{
    [TestClass]
    public class LinearBeamStiffnessMatrixTest
    {



        [TestMethod]
        public void EulerBernulli1()
        {

            SectionRectangular sec = new SectionRectangular(500, 100, SteelMaterial.S355);

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(1500, 0, 0));


            EulerBeam beam = new EulerBeam(node1, node2);
            beam.SetProperty(sec);

            ElementStiffnessMatrix matrix = beam.GetGlobalStiffnessMatrix();

            Console.Write(matrix);

            Assert.IsTrue(matrix.IsSymmetric());

            // FX
            Assert.AreEqual(7000000, matrix[0, 0]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, 
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X))));
            Assert.AreEqual(-7000000, matrix[0, 6]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, 
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X)), 
                new NodalGlobalDegreeOfFreedom(node2, 
                new GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X))));

            //// FY
            //double tol = 0.01;
            //Assert.AreEqual(777777.7778, matrix[1, 1], tol);
            //Assert.AreEqual(777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY)), tol);

            //Assert.AreEqual(583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY),
            //    new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RZ)), tol);

            //Assert.AreEqual(-777777.7778, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY),
            //    new NodalGlobalDegreeOfFreedom(node2, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY)), tol);

            //Assert.AreEqual(583333333.334, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY),
            //    new NodalGlobalDegreeOfFreedom(node2, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RZ)), tol);


            //// FZ
            //Assert.AreEqual(31111.1111, matrix[2, 2], tol);
            //Assert.AreEqual(31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ)), tol);

            //Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ),
            //    new NodalGlobalDegreeOfFreedom(node2, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RY)), tol);

            //Assert.AreEqual(-31111.1111, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ),
            //    new NodalGlobalDegreeOfFreedom(node2, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ)), tol);

            //Assert.AreEqual(-23333333.33, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ),
            //    new NodalGlobalDegreeOfFreedom(node2, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RY)), tol);


            //// MX
            //Assert.AreEqual(137222208.7198, matrix[3, 3]);
            //Assert.AreEqual(137222208.7198, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RX)));
            //Assert.AreEqual(-137222208.7198, matrix[0, 6]);
            //Assert.AreEqual(-137222208.7198, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RX),
            //    new NodalGlobalDegreeOfFreedom(node2, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RX)));


            // Non c'è coerenza fra assi locali beam e chiavi dentro la matrice, ma riordinata la matrice i l'arrai di dof
        }




        [TestMethod]
        public void TrussParallelX()
        {

            SectionRectangular sec = new SectionRectangular(500, 100, SteelMaterial.S355);

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

            SectionRectangular sec = new SectionRectangular(500, 100, SteelMaterial.S355);

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

            SectionRectangular sec = new SectionRectangular(500, 100, SteelMaterial.S355);

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
