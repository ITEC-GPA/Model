using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.FEM.FiniteElements;
using GPC.Geometry;
using GPC.Model.Maths.GaussIntegrations;

namespace MathTest
{
    [TestClass]
    public class GaussIntegrationTest
    {
		#region Old Gauss Integration

		/*[TestMethod]
        public void Test1()
        {
            int nrpoints = 9;

            var pts1 = GaussIntegration.GetPointsRectangular(nrpoints);
            var pts2 = GaussIntegration.GetPointsRectangular2(nrpoints);

            for (int i = 0; i < nrpoints; i++)
            {
                bool found = false;
                for (int j = 0; j < nrpoints; j++)
                {
                    if (pts2[i].Point.X == pts1[j].Point.X && pts2[i].Point.Y == pts1[j].Point.Y && pts2[i].Point.Z == pts1[j].Point.Z && Math.Round(pts2[i].Weight,10) == Math.Round(pts1[j].Weight,10))
                    {
                        found = true;
                    }
                }
                Assert.IsTrue(true == found);
            }
        }*/

		//costant 1 pt gauss
		[TestMethod]
        public void GaussIntegrationTest1()
        {
            int nrpoints = 1;

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(4.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0, ris[0, 0]);
        }

        //costant 4 pt gauss
        [TestMethod]
        public void GaussIntegrationTest2()
        {
            int nrpoints = 4;

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(4.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0, ris[0, 0]);
        }

        //costant 9 pt gauss
        [TestMethod]
        public void GaussIntegrationTest3()
        {
            int nrpoints = 9;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(4.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0, ris[0, 0]);
        }

        //linear 1 pt gauss
        [TestMethod]
        public void GaussIntegrationTest4()
        {
            int nrpoints = 1;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(0.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0 * 1.0 * 1.0 / 2.0, ris[0, 0]);
        }

        //linear 4 pt gauss
        [TestMethod]
        public void GaussIntegrationTest5()
        {
            int nrpoints = 4;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(0.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0 * 1.0 * 1.0 / 2.0, ris[0, 0], 0.000000000001);
        }

        //linear 9 pt gauss
        [TestMethod]
        public void GaussIntegrationTest6()
        {
            int nrpoints = 9;
            double constant = 5.0;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * 0.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * 1.0 * 1.0 * 1.0 / 2.0, ris[0, 0]);
        }

        //quadratic 4 pt gauss
        [TestMethod]
        public void GaussIntegrationTest7()
        {
            int nrpoints = 4;
            double constant = 5.0;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * x;
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 *-1.0)) / 3.0 * 2.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * x;
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * (1.0*1.0*1.0)/3.0 * 1.0, ris[0, 0], 0.0000000000001);
        }

        //quadratic 9 pt gauss
        [TestMethod]
        public void GaussIntegrationTest8()
        {
            int nrpoints = 9;
            double constant = 3.0;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * x;
                return m;
            };

            var j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 * -1.0)) / 3.0 * 2.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(1.0, 0.0, 0, "2");
            nds[2] = new Node(1.0, 1.0, 0, "3");
            nds[3] = new Node(0.0, 1.0, 0, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate2D("X", csi, eta, LinearShapeFunctionQuad4.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * x;
                return m;
            };

            j = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = OldGaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * (1.0 * 1.0 * 1.0) / 3.0 * 1.0, ris[0, 0],0.000000001);
        }

        //hesaedral 1 pt gauss - constant
        [TestMethod]
        public void GaussIntegrationTest9()
        {
            int nrpoints = 1;
            double constant = 3.0;
            Node[] nds = new Node[8];
            nds[0] = new Node(-1.0, -1.0, -1.0, "1");
            nds[1] = new Node(+1.0, -1.0, -1.0, "2");
            nds[2] = new Node(+1.0, +1.0, -1.0, "3");
            nds[3] = new Node(-1.0, +1.0, -1.0, "4");
            nds[4] = new Node(-1.0, -1.0, +1.0, "1");
            nds[5] = new Node(+1.0, -1.0, +1.0, "2");
            nds[6] = new Node(+1.0, +1.0, +1.0, "3");
            nds[7] = new Node(-1.0, +1.0, +1.0, "4");

            Func<double, double, double, mnl.Matrix<double>> F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant;
                return m;
            };

            var j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            var ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            double volume = 2.0 * 2.0 * 2.0;
            Assert.AreEqual(constant * volume, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0.0, "1");
            nds[1] = new Node(+2.0, 0.0, 0.0, "2");
            nds[2] = new Node(+2.0, 1.0, 0.0, "3");
            nds[3] = new Node(0.0, 1.0, 0.0, "4");

            nds[4] = new Node(0.0, 0.0, 3.0, "1");
            nds[5] = new Node(+2.0, 0.0, 3.0, "2");
            nds[6] = new Node(+2.0, 1.0, 3.0, "3");
            nds[7] = new Node(0.0, 1.0, 3.0, "4");

            volume = 2.0 * 1.0 * 3.0;
            j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            Assert.AreEqual(constant * volume, ris[0, 0], 0.00000000001);
        }

        //hesaedral 8 pt gauss - constant
        [TestMethod]
        public void GaussIntegrationTest10()
        {
            int nrpoints = 8;
            double constant = 3.0;
            Node[] nds = new Node[8];
            nds[0] = new Node(-1.0, -1.0, -1.0, "1");
            nds[1] = new Node(+1.0, -1.0, -1.0, "2");
            nds[2] = new Node(+1.0, +1.0, -1.0, "3");
            nds[3] = new Node(-1.0, +1.0, -1.0, "4");
            nds[4] = new Node(-1.0, -1.0, +1.0, "1");
            nds[5] = new Node(+1.0, -1.0, +1.0, "2");
            nds[6] = new Node(+1.0, +1.0, +1.0, "3");
            nds[7] = new Node(-1.0, +1.0, +1.0, "4");

            Func<double, double, double, mnl.Matrix<double>> F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant;
                return m;
            };

            var j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            var ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            double volume = 2.0 * 2.0 * 2.0;
            Assert.AreEqual(constant * volume, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0.0, "1");
            nds[1] = new Node(+2.0, 0.0, 0.0, "2");
            nds[2] = new Node(+2.0, 1.0, 0.0, "3");
            nds[3] = new Node(0.0, 1.0, 0.0, "4");

            nds[4] = new Node(0.0, 0.0, 3.0, "1");
            nds[5] = new Node(+2.0, 0.0, 3.0, "2");
            nds[6] = new Node(+2.0, 1.0, 3.0, "3");
            nds[7] = new Node(0.0, 1.0, 3.0, "4");

            volume = 2.0 * 1.0 * 3.0;
            j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            Assert.AreEqual(constant * volume, ris[0, 0], 0.00000000001);
        }

        //hesaedral 1 pt gauss - linear
        [TestMethod]
        public void GaussIntegrationTest11()
        {
            int nrpoints = 1;
            double constant = 2.0;
            Node[] nds = new Node[8];
            nds[0] = new Node(-1.0, -1.0, -1.0, "1");
            nds[1] = new Node(+1.0, -1.0, -1.0, "2");
            nds[2] = new Node(+1.0, +1.0, -1.0, "3");
            nds[3] = new Node(-1.0, +1.0, -1.0, "4");
            nds[4] = new Node(-1.0, -1.0, +1.0, "1");
            nds[5] = new Node(+1.0, -1.0, +1.0, "2");
            nds[6] = new Node(+1.0, +1.0, +1.0, "3");
            nds[7] = new Node(-1.0, +1.0, +1.0, "4");

            Func<double, double, double, mnl.Matrix<double>> F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate3D("X", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double y = FEMUtilities.GetLocalCoordinate3D("Y", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double z = FEMUtilities.GetLocalCoordinate3D("Z", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * y * z;
                return m;
            };

            var j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            var ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            
            Func<double, double, double> integral = (double start, double end) => { return 1.0 / 2.0 * (end * end - start * start); };

            Assert.AreEqual(constant * integral(-1,1) * integral(-1, 1) * integral(-1, 1), ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0.0, "1");
            nds[1] = new Node(+2.0, 0.0, 0.0, "2");
            nds[2] = new Node(+2.0, 1.0, 0.0, "3");
            nds[3] = new Node(0.0, 1.0, 0.0, "4");

            nds[4] = new Node(0.0, 0.0, 3.0, "1");
            nds[5] = new Node(+2.0, 0.0, 3.0, "2");
            nds[6] = new Node(+2.0, 1.0, 3.0, "3");
            nds[7] = new Node(0.0, 1.0, 3.0, "4");

            F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate3D("X", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double y = FEMUtilities.GetLocalCoordinate3D("Y", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double z = FEMUtilities.GetLocalCoordinate3D("Z", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * y * z;
                return m;
            };

            j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            Assert.AreEqual(constant * integral(0, 2) * integral(0, 1) * integral(0, 3), ris[0, 0], 0.00000000001);
        }

        //hesaedral 8 pt gauss - linear
        [TestMethod]
        public void GaussIntegrationTest12()
        {
            int nrpoints = 8;
            double constant = 2.0;
            Node[] nds = new Node[8];
            nds[0] = new Node(-1.0, -1.0, -1.0, "1");
            nds[1] = new Node(+1.0, -1.0, -1.0, "2");
            nds[2] = new Node(+1.0, +1.0, -1.0, "3");
            nds[3] = new Node(-1.0, +1.0, -1.0, "4");
            nds[4] = new Node(-1.0, -1.0, +1.0, "1");
            nds[5] = new Node(+1.0, -1.0, +1.0, "2");
            nds[6] = new Node(+1.0, +1.0, +1.0, "3");
            nds[7] = new Node(-1.0, +1.0, +1.0, "4");

            Func<double, double, double, mnl.Matrix<double>> F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate3D("X", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double y = FEMUtilities.GetLocalCoordinate3D("Y", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double z = FEMUtilities.GetLocalCoordinate3D("Z", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * y * z;
                return m;
            };

            var j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            var ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);

            Func<double, double, double> integral = (double start, double end) => { return 1.0 / 2.0 * (end * end - start * start); };

            Assert.AreEqual(constant * integral(-1, 1) * integral(-1, 1) * integral(-1, 1), ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0.0, "1");
            nds[1] = new Node(+2.0, 0.0, 0.0, "2");
            nds[2] = new Node(+2.0, 1.0, 0.0, "3");
            nds[3] = new Node(0.0, 1.0, 0.0, "4");

            nds[4] = new Node(0.0, 0.0, 3.0, "1");
            nds[5] = new Node(+2.0, 0.0, 3.0, "2");
            nds[6] = new Node(+2.0, 1.0, 3.0, "3");
            nds[7] = new Node(0.0, 1.0, 3.0, "4");

            F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate3D("X", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double y = FEMUtilities.GetLocalCoordinate3D("Y", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double z = FEMUtilities.GetLocalCoordinate3D("Z", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                m[0, 0] = constant * x * y * z;
                return m;
            };

            j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            Assert.AreEqual(constant * integral(0, 2) * integral(0, 1) * integral(0, 3), ris[0, 0], 0.00000000001);
        }

        //hesaedral 8 pt gauss - quadratic
        [TestMethod]
        public void GaussIntegrationTest13()
        {
            int nrpoints = 8;
            double constant = 2.0;
            Node[] nds = new Node[8];
            nds[0] = new Node(-1.0, -1.0, -1.0, "1");
            nds[1] = new Node(+1.0, -1.0, -1.0, "2");
            nds[2] = new Node(+1.0, +1.0, -1.0, "3");
            nds[3] = new Node(-1.0, +1.0, -1.0, "4");
            nds[4] = new Node(-1.0, -1.0, +1.0, "1");
            nds[5] = new Node(+1.0, -1.0, +1.0, "2");
            nds[6] = new Node(+1.0, +1.0, +1.0, "3");
            nds[7] = new Node(-1.0, +1.0, +1.0, "4");

            Func<double, double, double, mnl.Matrix<double>> F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate3D("X", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double y = FEMUtilities.GetLocalCoordinate3D("Y", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double z = FEMUtilities.GetLocalCoordinate3D("Z", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                m[0, 0] = constant * x*x * y*y * z*z;
                return m;
            };

            var j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            var ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);

            Func<double, double, double> integral = (double start, double end) => { return 1.0 / 3.0 * (end * end * end - start * start * start); };

            Assert.AreEqual(constant * integral(-1, 1) * integral(-1, 1) * integral(-1, 1), ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0.0, "1");
            nds[1] = new Node(+2.0, 0.0, 0.0, "2");
            nds[2] = new Node(+2.0, 1.0, 0.0, "3");
            nds[3] = new Node(0.0, 1.0, 0.0, "4");

            nds[4] = new Node(0.0, 0.0, 3.0, "1");
            nds[5] = new Node(+2.0, 0.0, 3.0, "2");
            nds[6] = new Node(+2.0, 1.0, 3.0, "3");
            nds[7] = new Node(0.0, 1.0, 3.0, "4");

            F = (double csi, double eta, double zeta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                double x = FEMUtilities.GetLocalCoordinate3D("X", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double y = FEMUtilities.GetLocalCoordinate3D("Y", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                double z = FEMUtilities.GetLocalCoordinate3D("Z", csi, eta, zeta, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction, nds);
                m[0, 0] = constant * x*x * y*y * z*z;
                return m;
            };

            j = FEMUtilities.J3D(TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, nds);
            ris = OldGaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            Assert.AreEqual(constant * integral(0, 2) * integral(0, 1) * integral(0, 3), ris[0, 0], 0.00000000001);
        }

        #endregion

        #region New Gauss Integration

        #region Linear Shape Function

        [TestMethod]
        public void Line2Test1LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(1.0, 1.0, 0), new Point3d(10.0, 1.0, 0) };
            double expValue = 999;
            Func<double, double, double> func = (x, y) => constant * x * x;

            int nrGaussPoints = 3;
            double result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 9;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 16;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 32;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 20;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Line2Test2LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(1.0, 1.0, 0), new Point3d(10.0, 1.0, 0) };
            double expValue = 7499.25;
            Func<double, double, double> func = (x, y) => constant * x * x * x;

            int nrGaussPoints = 3;
            double result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 9;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 16;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 32;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 20;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test1LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(-1.0, -1.0, 0), new Point3d(+1.0, -1.0, 0), new Point3d(+1.0, +1.0, 0), new Point3d(-1.0, +1.0, 0) };
            double expValue = constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 * -1.0)) / 3.0 * 2.0;
            Func<double, double, double> func = (x, y) => constant * x * x;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test2LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(0.0, 0.0, 0), new Point3d(10, 0.0, 0), new Point3d(10, 10, 0), new Point3d(0.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * x;
            double expValue = 10000;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test3LSF()
        {
            double constant = 3.0;
            double expValue = 4218.75;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * y;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test4LSF()
		{
            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(12, 12, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x ;
            double expValue = 241.5;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test5LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 336;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test6LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * y * x;
            double expValue = 4218.75;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test1LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 85.5;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test2LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 126;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test3LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 354.66666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test4LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) =>  y ;
            double expValue = 466.6666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        #endregion

        #region Quadratic Shape Function

        [TestMethod]
        public void Quad4Test1QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(-1.0, -1.0, 0), new Point3d(+1.0, -1.0, 0), new Point3d(+1.0, +1.0, 0), new Point3d(-1.0, +1.0, 0) };
            double expValue = constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 * -1.0)) / 3.0 * 2.0;
            Func<double, double, double> func = (x, y) => constant * x * x;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test2QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(0.0, 0.0, 0), new Point3d(10, 0.0, 0), new Point3d(10, 10, 0), new Point3d(0.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * x;
            double expValue = 10000;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test3QSF()
        {
            double constant = 3.0;
            double expValue = 4218.75;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * y;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test4QSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(12, 12, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 241.5;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test5QSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 336;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test6QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * y * x;
            double expValue = 4218.75;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test1QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 85.5;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 33;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test2QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 126;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test3QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 354.66666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test4QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => y;
            double expValue = 466.6666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        #endregion

        #endregion
    }
}