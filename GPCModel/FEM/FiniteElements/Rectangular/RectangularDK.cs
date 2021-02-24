using System;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Discrete Kirchoff Rectangular - Evaluation of new quadrilateral thin plate bending element - Jean-Louis Batoz
    /// International Jurnal for numerical methods in engineering, vol 18, 1655-1977 (1982)
    /// </summary>
    public class RectangularDK : FiniteElement
    {
        public RectangularDK(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            DOF.Add(LinearSolver.DOF.DX);
            DOF.Add(LinearSolver.DOF.DY);
            DOF.Add(LinearSolver.DOF.DZ);
            //displacement w il local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(LinearSolver.DOF.RX);
            DOF.Add(LinearSolver.DOF.RY);
            DOF.Add(LinearSolver.DOF.RZ);
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
            //Console.WriteLine("D = " + _d.ToString());
            #endregion

            //4 Gauss Integration points
            double[] csiGauss = new [] { -1.0/Math.Pow(3,0.5), 1.0 / Math.Pow(3, 0.5)};
            double[] etaGauss = new[] { -1.0 / Math.Pow(3, 0.5), 1.0 / Math.Pow(3, 0.5) };
            double[] weightGauss = new[] { 1.0, 1.0};

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(12, 12);
            for (int i = 0; i < csiGauss.Length; i++)
            {
                double csi = csiGauss[i];
                for (int j = 0; j < etaGauss.Length; j++) {
                    double eta = etaGauss[j];
                    mnl.Matrix<double> b = B(csi, eta);
                    mnl.Matrix<double> m = weightGauss[i] * weightGauss[j] * b.Transpose() * _d * b * detJ(csi, eta);
                    _kElementLocalCoord = _kElementLocalCoord + m;
                }
            }
            
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            throw new System.NotImplementedException();
        }

        protected mnl.Matrix<double> B(double csi, double eta)
        {
            //LocalNodes(GlobalNodesElement[0], GlobalNodesElement[1], GlobalNodesElement[2], out Node node1, out Node node3, out Node node2); //node 1 is origin, node 3 is in (0,y2), node2 is in (x2,y2)
            Node node1 = new Node(-1, -1, 0, 0);
            Node node2 = new Node( 1, -1, 0, 0);
            Node node3 = new Node( 1,  1, 0, 0);
            Node node4 = new Node(-1,  1, 0, 0);

            double x12 = node1.Position.X - node2.Position.X;
            double y12 = node1.Position.Y - node2.Position.Y;
            double l12 = node1.Position.DistanceTo(node2.Position);

            double x21 = node2.Position.X - node1.Position.X;
            double y21 = node2.Position.Y - node1.Position.Y;

            double x23 = node2.Position.X - node3.Position.X;
            double y23 = node2.Position.Y - node3.Position.Y;
            double l23 = node2.Position.DistanceTo(node3.Position);

            double x34 = node3.Position.X - node4.Position.X;
            double y34 = node3.Position.Y - node4.Position.Y;
            double l34 = node3.Position.DistanceTo(node4.Position);

            double x32 = node3.Position.X - node2.Position.X;
            double y32 = node3.Position.Y - node2.Position.Y;

            double x41 = node4.Position.X - node1.Position.X;
            double y41 = node4.Position.Y - node1.Position.Y;
            double l41 = node4.Position.DistanceTo(node1.Position);

            double x31 = node3.Position.X - node1.Position.X;
            double y31 = node3.Position.Y - node1.Position.Y;
            
            double x42 = node4.Position.X - node2.Position.X;
            double y42 = node4.Position.Y - node2.Position.Y;

            double detJ = 1.0 / 8.0 * (y42 * x31 - y31 * x42) + csi / 8.0 * (y34 * x21 - y21 * x34) + eta / 8.0 * (y41 * x32 - y32 * x41);
            if (detJ < 0)
            {
                Console.WriteLine("absolute value?!");
            }

            double j11 = 1.0 / detJ * (y32 + y41 + csi * (y12 + y34));
            double j12 = -1.0 / detJ * (y21 + y34 + eta * (y12 + y34));
            double j21 = -1.0 / detJ * (x32 + x41 + csi * (x12 + x34));
            double j22 = 1.0 / detJ * (y32 + y41 + csi * (y12 + y34));

            mnl.Vector<double> hxCsi = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hyCsi = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hxEta = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hyEta = mnl.Vector<double>.Build.Dense(12, 1);

            double a5 = -x12 / Math.Pow(l12, 2.0);
            double a6 = -x23 / Math.Pow(l23, 2.0);
            double a7 = -x34 / Math.Pow(l34, 2.0);
            double a8 = -x41 / Math.Pow(l41, 2.0);

            double b5 = 3.0 / 4.0 * x12 * y12 / Math.Pow(l12, 2.0);
            double b6 = 3.0 / 4.0 * x23 * y12 / Math.Pow(l23, 2.0);
            double b7 = 3.0 / 4.0 * x34 * y12 / Math.Pow(l34, 2.0);
            double b8 = 3.0 / 4.0 * x41 * y12 / Math.Pow(l41, 2.0);

            double c5 = (1.0 / 4.0 * Math.Pow(x12, 2.0) - 1.0 / 2.0 * Math.Pow(y12, 2.0)) / Math.Pow(l12, 2.0);
            double c6 = (1.0 / 4.0 * Math.Pow(x23, 2.0) - 1.0 / 2.0 * Math.Pow(y23, 2.0)) / Math.Pow(l23, 2.0);
            double c7 = (1.0 / 4.0 * Math.Pow(x34, 2.0) - 1.0 / 2.0 * Math.Pow(y34, 2.0)) / Math.Pow(l34, 2.0);
            double c8 = (1.0 / 4.0 * Math.Pow(x41, 2.0) - 1.0 / 2.0 * Math.Pow(y41, 2.0)) / Math.Pow(l41, 2.0);

            double d5 = -y12 / Math.Pow(l12, 2.0);
            double d6 = -y23 / Math.Pow(l23, 2.0);
            double d7 = -y34 / Math.Pow(l34, 2.0);
            double d8 = -y41 / Math.Pow(l41, 2.0);

            double e5 = (-1.0 / 2.0 * Math.Pow(x12, 2.0) + 1.0 / 4.0 * Math.Pow(y12, 2.0)) / Math.Pow(l12, 2.0);
            double e6 = (-1.0 / 2.0 * Math.Pow(x23, 2.0) + 1.0 / 4.0 * Math.Pow(y23, 2.0)) / Math.Pow(l23, 2.0);
            double e7 = (-1.0 / 2.0 * Math.Pow(x34, 2.0) + 1.0 / 4.0 * Math.Pow(y34, 2.0)) / Math.Pow(l34, 2.0);
            double e8 = (-1.0 / 2.0 * Math.Pow(x41, 2.0) + 1.0 / 4.0 * Math.Pow(y41, 2.0)) / Math.Pow(l41, 2.0);

            hxCsi[1 - 1] = 3.0 / 2.0 * (a5 * dNdCsi(5, csi, eta) - a8 * dNdCsi(8, csi, eta));
            hxCsi[2 - 1] = b5 * dNdCsi(5, csi, eta) - a8 * dNdCsi(8, csi, eta);
            hxCsi[3 - 1] = dNdCsi(1, csi, eta) - c5 * dNdCsi(5, csi, eta) - c8 * dNdCsi(8, csi, eta);

            hxCsi[4 - 1] = 3.0 / 2.0 * (a6 * dNdCsi(6, csi, eta) - a5 * dNdCsi(5, csi, eta));
            hxCsi[5 - 1] = b6 * dNdCsi(6, csi, eta) - a5 * dNdCsi(5, csi, eta);
            hxCsi[6 - 1] = dNdCsi(2, csi, eta) - c6 * dNdCsi(6, csi, eta) - c5 * dNdCsi(5, csi, eta);

            hxCsi[7 - 1] = 3.0 / 2.0 * (a7 * dNdCsi(7, csi, eta) - a6 * dNdCsi(6, csi, eta));
            hxCsi[8 - 1] = b7 * dNdCsi(7, csi, eta) - a6 * dNdCsi(6, csi, eta);
            hxCsi[9 - 1] = dNdCsi(3, csi, eta) - c7 * dNdCsi(7, csi, eta) - c6 * dNdCsi(6, csi, eta);

            hxCsi[10 - 1] = 3.0 / 2.0 * (a8 * dNdCsi(8, csi, eta) - a7 * dNdCsi(7, csi, eta));
            hxCsi[11 - 1] = b8 * dNdCsi(8, csi, eta) - a7 * dNdCsi(7, csi, eta);
            hxCsi[12 - 1] = dNdCsi(4, csi, eta) - c8 * dNdCsi(8, csi, eta) - c7 * dNdCsi(7, csi, eta);

            /////////////////////////////////////////////////////////////////////////////////////////////////
            
            hyCsi[1 - 1] = 3.0 / 2.0 * (d5 * dNdCsi(5, csi, eta) - d8 * dNdCsi(8, csi, eta));
            hyCsi[2 - 1] = -dNdCsi(1, csi, eta) + e5 * dNdCsi(5, eta, csi) + e8 * dNdCsi(8, csi, eta);
            hyCsi[3 - 1] = -b5 * dNdCsi(5, csi, eta) - b8 * dNdCsi(8, csi, eta);

            hyCsi[4 - 1] = 3.0 / 2.0 * (d6 * dNdCsi(6, csi, eta) - d5 * dNdCsi(5, csi, eta));
            hyCsi[5 - 1] = -dNdCsi(2, csi, eta) + e6 * dNdCsi(6, eta, csi) + e5 * dNdCsi(5, csi, eta);
            hyCsi[6 - 1] = -b6 * dNdCsi(6, csi, eta) - b5 * dNdCsi(5, csi, eta);

            hyCsi[7 - 1] = 3.0 / 2.0 * (d7 * dNdCsi(7, csi, eta) - d6 * dNdCsi(6, csi, eta));
            hyCsi[8 - 1] = -dNdCsi(3, csi, eta) + e7 * dNdCsi(7, eta, csi) + e6 * dNdCsi(6, csi, eta);
            hyCsi[9 - 1] = -b7 * dNdCsi(7, csi, eta) - b6 * dNdCsi(6, csi, eta);

            hyCsi[10 - 1] = 3.0 / 2.0 * (d8 * dNdCsi(8, csi, eta) - d7 * dNdCsi(7, csi, eta));
            hyCsi[11 - 1] = -dNdCsi(4, csi, eta) + e8 * dNdCsi(8, eta, csi) + e7 * dNdCsi(7, csi, eta);
            hyCsi[12 - 1] = -b8 * dNdCsi(8, csi, eta) - b7 * dNdCsi(7, csi, eta);

            /////////////////////////////////////////////////////////////////////////////////////////////////

            hxEta[1 - 1] = 3.0 / 2.0 * (a5 * dNdEta(5, csi, eta) - a8 * dNdEta(8, csi, eta));
            hxEta[2 - 1] = b5 * dNdEta(5, csi, eta) - a8 * dNdEta(8, csi, eta);
            hxEta[3 - 1] = dNdEta(1, csi, eta) - c5 * dNdEta(5, csi, eta) - c8 * dNdEta(8, csi, eta);

            hxEta[4 - 1] = 3.0 / 2.0 * (a6 * dNdEta(6, csi, eta) - a5 * dNdEta(5, csi, eta));
            hxEta[5 - 1] = b6 * dNdEta(6, csi, eta) - a5 * dNdEta(5, csi, eta);
            hxEta[6 - 1] = dNdEta(2, csi, eta) - c6 * dNdEta(6, csi, eta) - c5 * dNdEta(5, csi, eta);

            hxEta[7 - 1] = 3.0 / 2.0 * (a7 * dNdEta(7, csi, eta) - a6 * dNdEta(6, csi, eta));
            hxEta[8 - 1] = b7 * dNdEta(7, csi, eta) - a6 * dNdEta(6, csi, eta);
            hxEta[9 - 1] = dNdEta(3, csi, eta) - c7 * dNdEta(7, csi, eta) - c6 * dNdEta(6, csi, eta);

            hxEta[10 - 1] = 3.0 / 2.0 * (a8 * dNdEta(8, csi, eta) - a7 * dNdEta(7, csi, eta));
            hxEta[11 - 1] = b8 * dNdEta(8, csi, eta) - a7 * dNdEta(7, csi, eta);
            hxEta[12 - 1] = dNdEta(4, csi, eta) - c8 * dNdEta(8, csi, eta) - c7 * dNdEta(7, csi, eta);

            /////////////////////////////////////////////////////////////////////////////////////////////////

            hyEta[1 - 1] = 3.0 / 2.0 * (d5 * dNdEta(5, csi, eta) - d8 * dNdEta(8, csi, eta));
            hyEta[2 - 1] = -dNdEta(1, csi, eta) + e5 * dNdEta(5, eta, csi) + e8 * dNdEta(8, csi, eta);
            hyEta[3 - 1] = -b5 * dNdEta(5, csi, eta) - b8 * dNdEta(8, csi, eta);

            hyEta[4 - 1] = 3.0 / 2.0 * (d6 * dNdEta(6, csi, eta) - d5 * dNdEta(5, csi, eta));
            hyEta[5 - 1] = -dNdEta(2, csi, eta) + e6 * dNdEta(6, eta, csi) + e5 * dNdEta(5, csi, eta);
            hyEta[6 - 1] = -b6 * dNdEta(6, csi, eta) - b5 * dNdEta(5, csi, eta);

            hyEta[7 - 1] = 3.0 / 2.0 * (d7 * dNdEta(7, csi, eta) - d6 * dNdEta(6, csi, eta));
            hyEta[8 - 1] = -dNdEta(3, csi, eta) + e7 * dNdEta(7, eta, csi) + e6 * dNdEta(6, csi, eta);
            hyEta[9 - 1] = -b7 * dNdEta(7, csi, eta) - b6 * dNdEta(6, csi, eta);

            hyEta[10 - 1] = 3.0 / 2.0 * (d8 * dNdEta(8, csi, eta) - d7 * dNdEta(7, csi, eta));
            hyEta[11 - 1] = -dNdCsi(4, csi, eta) + e8 * dNdEta(8, eta, csi) + e7 * dNdEta(7, csi, eta);
            hyEta[12 - 1] = -b8 * dNdEta(8, csi, eta) - b7 * dNdEta(7, csi, eta);

            mnl.Vector<double> r0 = j11 * hxCsi + j12 * hxEta;
            mnl.Vector<double> r1 = j21 * hyCsi + j22 * hyEta;
            mnl.Vector<double> r2 = j11 * hyCsi + j12 * hyEta + j21 * hxCsi + j22 * hxEta;
            
            _b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0, r1, r2);
            return _b;
        }

        private double detJ(double csi, double eta)
        {
            Node node1 = new Node(-1, -1, 0, 0);
            Node node2 = new Node(1, -1, 0, 0);
            Node node3 = new Node(1, 1, 0, 0);
            Node node4 = new Node(-1, 1, 0, 0);

            double x21 = node2.Position.X - node1.Position.X;
            double y21 = node2.Position.Y - node1.Position.Y;

            double x34 = node3.Position.X - node4.Position.X;
            double y34 = node3.Position.Y - node4.Position.Y;
    
            double x32 = node3.Position.X - node2.Position.X;
            double y32 = node3.Position.Y - node2.Position.Y;

            double x41 = node4.Position.X - node1.Position.X;
            double y41 = node4.Position.Y - node1.Position.Y;

            double x31 = node3.Position.X - node1.Position.X;
            double y31 = node3.Position.Y - node1.Position.Y;

            double x42 = node4.Position.X - node2.Position.X;
            double y42 = node4.Position.Y - node2.Position.Y;

            return 1.0 / 8.0 * (y42 * x31 - y31 * x42) + csi / 8.0 * (y34 * x21 - y21 * x34) + eta / 8.0 * (y41 * x32 - y32 * x41);
        }

        #region ShapeFunction
        /*private double N(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return -1.0 / 4.0 * (1.0 - csi) * (1.0 - eta) * (1.0 + csi + eta);
                    break;
                case 2:
                    return 0;
                    break;
                default:
                    return 0;
                    break;
            }
        }*/

            private double dNdCsi(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (2.0 * csi + eta) * (1.0 - eta);
                case 2:
                    return 1.0 / 4.0 * (2.0 * csi - eta) * (1.0 - eta);
                case 3:
                    return 1.0 / 4.0 * (2.0 * csi + eta) * (1.0 + eta);
                case 4:
                    return 1.0 / 4.0 * (2.0 * csi - eta) * (1.0 + eta);
                case 5:
                    return -csi * (1.0 - eta);
                case 6:
                    return 1.0 / 2.0 * (1.0 - eta * eta);
                case 7:
                    return -csi * (1.0 + eta);
                case 8:
                    return -1.0 / 2.0 * (1.0 - eta * eta);
                default:
                    throw new Exception();
            }
        }

        private double dNdEta(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (2.0 * eta + csi) * (1.0 - csi);
                case 2:
                    return 1.0 / 4.0 * (2.0 * eta - csi) * (1.0 + csi);
                case 3:
                    return 1.0 / 4.0 * (2.0 * eta + csi) * (1.0 + csi);
                case 4:
                    return 1.0 / 4.0 * (2.0 * eta - csi) * (1.0 - csi);
                case 5:
                    return -1.0 / 2.0 * (1.0 - csi * csi);
                case 6:
                    return -eta * (1.0 + csi);
                case 7:
                    return 1.0 / 2.0 * (1.0 - eta * eta);
                case 8:
                    return -eta * (1.0 - csi);
                default:
                    throw new Exception();
            }
        }
        #endregion
    }
}
