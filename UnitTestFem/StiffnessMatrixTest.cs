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
    public class StiffnessMatrixTest
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

            Assert.AreEqual(7000000, matrix[0, 0]);
            Assert.AreEqual(7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DX)));
            Assert.AreEqual(-7000000, matrix[0, 6]);
            Assert.AreEqual(-7000000, matrix.GetElementAt(new NodalGlobalDegreeOfFreedom(node1, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DX), 
                new NodalGlobalDegreeOfFreedom(node2, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DX)));



        }
    }
}
