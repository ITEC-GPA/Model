using System;
using GPC.Geometry;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Fem.StiffnessMatrix;
using GPC.Model.Materials;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFem
{
    [TestClass]
    public class StiffnessMatrixTest
    {



        [TestMethod]
        public void EulerBernulli1()
        {

            SectionRectangular sec = new SectionRectangular(100, 10, SteelMaterial.S355);

            Node node1 = new Node(new Point3d(0, 0, 0));
            Node node2 = new Node(new Point3d(20, 0, 0));


            EulerBeam beam = new EulerBeam(node1, node2);
            beam.SetProperty(sec);

            EulerBernulliStifnessMatrix stifnessMatrix = new EulerBernulliStifnessMatrix(beam);

            Console.Write(stifnessMatrix.LocalStiffnessMatrix);




        }
    }
}
