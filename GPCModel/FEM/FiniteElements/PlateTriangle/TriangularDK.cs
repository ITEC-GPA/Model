using System;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Discrete Kirchoff Triangle - A study of three-node triangular plate bending elements - Jean-Louis Batoz
    /// International Jurnal for numerical methods in engineering, vol 15, 1771-1812 (1980)
    /// </summary>
    public class TriangularDK : TriangleElement
    {

        private Func<double, double, double>[] _shapeFunctions;

        public TriangularDK(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            DOF.Add(LinearSolver.DOF.DX);
            DOF.Add(LinearSolver.DOF.DY);
            DOF.Add(LinearSolver.DOF.DZ);
            //displacement w il local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(LinearSolver.DOF.RX);
            DOF.Add(LinearSolver.DOF.RY);
            DOF.Add(LinearSolver.DOF.RZ);

            _shapeFunctions = new Func<double, double, double>[6];
            _shapeFunctions[0] = N1;
            _shapeFunctions[1] = N2;
            _shapeFunctions[2] = N3;
            _shapeFunctions[3] = N4;
            _shapeFunctions[4] = N5;
            _shapeFunctions[5] = N6;
        }

        public override void BuildMatrix()
        {
            #region matrixD
            double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();
            double tb = ((PlateProperty)Property).BendingThickness;

            _d = mnl.Matrix<double>.Build.Dense(3, 3);
            _d[0, 0] = 1.0;
            _d[0, 1] = ni;
            _d[1, 0] = ni;
            _d[1, 1] = 1.0;
            _d[2, 2] = (1.0 - ni) / 2.0;
            _d = E * Math.Pow(tb, 3.0) / (12.0 * (1.0 - ni * ni)) * _d; //flexural rigidity
            Console.WriteLine("D = " + _d.ToString());
            #endregion

            //3 Gauss Integration points
            double[] csiGauss = new [] { 0.5, 0.5, 0 };
            double[] etaGauss = new[] { 0, 0.5, 0.5 };
            double[] weightGauss = new[] { 1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0 };

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(9, 9);
            for (int i = 0; i < csiGauss.Length; i++) //trhough the 3 gauss points
            {
                double csi = csiGauss[i];
                double eta = etaGauss[i];
                mnl.Matrix<double> b = B(csi, eta);
                mnl.Matrix<double> m = weightGauss[i] * b.Transpose() * _d * b; 
                _kElementLocalCoord = _kElementLocalCoord + m;
            }
            _kElementLocalCoord = (2.0 * _areaElement) * _kElementLocalCoord;
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            throw new System.NotImplementedException();
        }

        protected mnl.Matrix<double> B(double csi, double eta)
        {
            LocalNodes(Nodes[0], Nodes[1], Nodes[2], out Node node1, out Node node3, out Node node2); //node 1 is origin, node 3 is in (0,y2), node2 is in (x2,y2)

            double x31 = node3.Position.X - node1.Position.X; 
            double y31 = node3.Position.Y - node1.Position.Y;
            double l31 = node3.Position.DistanceTo(node1.Position);

            double x12 = node1.Position.X - node2.Position.X;
            double y12 = node1.Position.Y - node2.Position.Y;
            double l12 = node1.Position.DistanceTo(node2.Position);

            double x23 = node2.Position.X - node3.Position.X;
            double y23 = node2.Position.Y - node3.Position.Y;
            double l23 = node2.Position.DistanceTo(node3.Position);

            _areaElement = (x31 * y12 - x12 * y31) / 2.0;

            //create vector of derivative of "new shape function"
            double P4 = -6.0 * x23 / Math.Pow(l23, 2.0);
            double P5 = -6.0 * x31 / Math.Pow(l31, 2.0);
            double P6 = -6.0 * x12 / Math.Pow(l12, 2.0);

            double q4 = 3.0 * x23 * y23 / Math.Pow(l23, 2.0);
            double q5 = 3.0 * x31 * y31 / Math.Pow(l31, 2.0);
            double q6 = 3.0 * x12 * y12 / Math.Pow(l12, 2.0);

            double r4 = 3.0 * Math.Pow(y23, 2.0) / Math.Pow(l23, 2.0);
            double r5 = 3.0 * Math.Pow(y31, 2.0) / Math.Pow(l31, 2.0);
            double r6 = 3.0 * Math.Pow(y12, 2.0) / Math.Pow(l12, 2.0);

            double t4 = -6.0 * y23 / Math.Pow(l23, 2.0);
            double t5 = -6.0 * y31 / Math.Pow(l31, 2.0);
            double t6 = -6.0 * y12 / Math.Pow(l12, 2.0);

            mnl.Vector<double> hxCsi = mnl.Vector<double>.Build.Dense(9);
            hxCsi[0] = P6 * (1.0 - 2.0 * csi) + (P5 - P6) * eta;
            hxCsi[1] = q6 * (1.0 - 2.0 * csi) - (q5 + q6) * eta;
            hxCsi[2] = -4.0 + 6.0 * (csi + eta) + r6 * (1.0 - 2.0 * csi) - eta * (r5 + r6);
            hxCsi[3] = -P6 * (1.0 - 2.0 * csi) + eta * (P4 + P6);
            hxCsi[4] = q6 * (1.0 - 2.0 * csi) - eta * (q6 - q4);
            hxCsi[5] = -2.0 * 6.0 * csi + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6);
            hxCsi[6] = -eta * (P5 + P4);
            hxCsi[7] = eta * (q4 - q5);
            hxCsi[8] = -eta * (r5 - r4);

            mnl.Vector<double> hyCsi = mnl.Vector<double>.Build.Dense(9);
            hyCsi[0] = t6 * (1.0 - 2.0 * csi) + eta * (t5 - t6);
            hyCsi[1] = 1.0 + r6 * (1.0 - 2.0 * csi) - eta * (r5 + r6);
            hyCsi[2] = -q6 * (1.0 - 2.0 * csi) + eta * (q5 + q6);
            hyCsi[3] = -t6 * (1.0 - 2.0 * csi) + eta * (t4 + t6);
            hyCsi[4] = -1.0 + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6);
            hyCsi[5] = -q6 * (1.0 - 2.0 * csi) - eta * (q4 - q6);
            hyCsi[6] = -eta * (t4 + t5);
            hyCsi[7] = eta * (r4 - r5);
            hyCsi[8] = -eta * (q4 - q5);

            mnl.Vector<double> hxEta = mnl.Vector<double>.Build.Dense(9);
            hxEta[0] = -P5 * (1.0 - 2.0 * eta) - csi * (P6 - P5);
            hxEta[1] = q5 * (1.0 - 2.0 * eta) - csi * (q5 + q6);
            hxEta[2] = -4.0 + 6.0 * (csi + eta) + r5 * (1.0 - 2.0 * eta) - csi * (r5 + r6);
            hxEta[3] = csi * (P4 + P6);
            hxEta[4] = csi * (q4 - q6);
            hxEta[5] = -csi * (r6 - r4);
            hxEta[6] = P5 * (1.0 - 2.0 * eta) - csi * (P4 + P5);
            hxEta[7] = q5 * (1.0 - 2.0 * eta) + csi * (q4 - q5);
            hxEta[8] = -2.0 + 6.0 * eta + r5 * (1.0 - 2.0 * eta) + csi * (r4 - r5);

            mnl.Vector<double> hyEta = mnl.Vector<double>.Build.Dense(9);
            hyEta[0] = -t5 * (1.0 - 2.0 * eta) - csi * (t6 - t5);
            hyEta[1] = 1.0 + r5 * (1.0 - 2.0 * eta) - csi * (r5 + r6);
            hyEta[2] = -q5 * (1.0 - 2.0 * eta) + csi * (q5 + q6);
            hyEta[3] = csi * (t4 + t6);
            hyEta[4] = csi * (r4 - r6);
            hyEta[5] = -csi * (q4 - q6);
            hyEta[6] = t5 * (1.0 - 2.0 * eta) - csi * (t4 + t5);
            hyEta[7] = -1.0 + r5 * (1.0 - 2.0 * eta) + csi * (r4 - r5);
            hyEta[8] = -q5 * (1.0 - 2.0 * eta) - csi * (q4 - q5);

            mnl.Vector<double> r0 = y31 * hxCsi + y12 * hxEta;
            mnl.Vector<double> r1 = -x31 * hyCsi - x12 * hyEta;
            mnl.Vector<double> r2 = -x31 * hxCsi - x12 * hxEta + y31 * hyCsi + y12 * hyEta;

            mnl.Matrix<double> b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0,r1,r2);
            b = 1.0 / (2.0 * _areaElement) * b;
            return b;
        }

        #region ShapeFunction
        private double N1(double csi, double eta)
        {
            return 2.0 * (1.0 - csi - eta) * (0.5 - csi - eta);
        }

        private double N2(double csi, double eta)
        {
            return csi * (2.0 * csi - 1.0);
        }
        private double N3(double csi, double eta)
        {
            return eta * (2.0 * eta - 1.0);
        }
        private double N4(double csi, double eta)
        {
            return 4.0 * csi * eta;
        }
        private double N5(double csi, double eta)
        {
            return 4.0 * eta * (1.0 - csi - eta);
        }
        private double N6(double csi, double eta)
        {
            return 4.0 * csi * (1.0 - csi - eta);
        }
        #endregion
    }
}
