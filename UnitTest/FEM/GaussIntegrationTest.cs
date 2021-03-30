using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace FemTest.Solver
{
    [TestClass]
    public class GaussIntegrationTest
    {
        //costant 1 pt gauss
        [TestMethod]
        public void GaussIntegrationTest1()
        {
            int nrpoints = 1;

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(4.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0, ris[0, 0]);
        }

        //costant 4 pt gauss
        [TestMethod]
        public void GaussIntegrationTest2()
        {
            int nrpoints = 4;

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(4.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0, ris[0, 0]);
        }

        //costant 9 pt gauss
        [TestMethod]
        public void GaussIntegrationTest3()
        {
            int nrpoints = 9;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(4.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = 1.0;
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0, ris[0, 0]);
        }

        //linear 1 pt gauss
        [TestMethod]
        public void GaussIntegrationTest4()
        {
            int nrpoints = 1;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, Node[], double> X = (double csi, double eta, Node[] nodi) =>
            {
                double x = 0;
                for (int i = 1; i <= nds.Length; i++)
                {
                    x = x + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodi[i - 1].Position.X;
                }
                return x;
            };

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = X(csi, eta, nds);
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(0.0, ris[0, 0]);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = X(csi, eta, nds);
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0 * 1.0 * 1.0 / 2.0, ris[0, 0]);
        }

        //linear 4 pt gauss
        [TestMethod]
        public void GaussIntegrationTest5()
        {
            int nrpoints = 4;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, Node[], double> X = (double csi, double eta, Node[] nodi) =>
            {
                double x = 0;
                for (int i = 1; i <= nds.Length; i++)
                {
                    x = x + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodi[i - 1].Position.X;
                }
                return x;
            };

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = X(csi, eta, nds);
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(0.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = X(csi, eta, nds);
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(1.0 * 1.0 * 1.0 / 2.0, ris[0, 0]);
        }

        //linear 9 pt gauss
        [TestMethod]
        public void GaussIntegrationTest6()
        {
            int nrpoints = 9;
            double constant = 5.0;
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, Node[], double> X = (double csi, double eta, Node[] nodi) =>
            {
                double x = 0;
                for (int i = 1; i <= nds.Length; i++)
                {
                    x = x + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodi[i - 1].Position.X;
                }
                return x;
            };

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * X(csi, eta, nds);
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * 0.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * X(csi, eta, nds);
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

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
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, Node[], double> X = (double csi, double eta, Node[] nodi) =>
            {
                double x = 0;
                for (int i = 1; i <= nds.Length; i++)
                {
                    x = x + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodi[i - 1].Position.X;
                }
                return x;
            };

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * X(csi, eta, nds) * X(csi, eta, nds);
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 *-1.0)) / 3.0 * 2.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * X(csi, eta, nds) * X(csi, eta, nds);
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

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
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Func<double, double, Node[], double> X = (double csi, double eta, Node[] nodi) =>
            {
                double x = 0;
                for (int i = 1; i <= nds.Length; i++)
                {
                    x = x + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodi[i - 1].Position.X;
                }
                return x;
            };

            Func<double, double, mnl.Matrix<double>> F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * X(csi, eta, nds) * X(csi, eta, nds);
                return m;
            };

            var j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            var ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 * -1.0)) / 3.0 * 2.0, ris[0, 0], 0.00000000001);

            nds[0] = new Node(0.0, 0.0, 0, 1, "1");
            nds[1] = new Node(1.0, 0.0, 0, 2, "2");
            nds[2] = new Node(1.0, 1.0, 0, 3, "3");
            nds[3] = new Node(0.0, 1.0, 0, 4, "4");

            F = (double csi, double eta) => {

                mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(1, 1);
                m[0, 0] = constant * X(csi, eta, nds) * X(csi, eta, nds);
                return m;
            };

            j = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nds);

            ris = GaussIntegration.IntegrationQuadrilateral(F, j, nrpoints);
            Assert.AreEqual(constant * (1.0 * 1.0 * 1.0) / 3.0 * 1.0, ris[0, 0]);
        }
    }
}