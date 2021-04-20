using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.FEM.FiniteElements;

namespace FemTest.Solver
{
    [TestClass]
    public class GaussIntegrationTest
    {
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
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
            var ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
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
            ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
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
            var ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
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
            ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
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
            var ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            
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
            ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
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
            var ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);

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
            ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
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
            var ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);

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
            ris = GaussIntegration.IntegrationHexaedron(F, j, nrpoints);
            Assert.AreEqual(constant * integral(0, 2) * integral(0, 1) * integral(0, 3), ris[0, 0], 0.00000000001);
        }
    }
}