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
            /*double[] csiGauss = new []   { 1.0 / 6.0, 2.0 / 3.0, 1.0 / 6.0 };
            double[] etaGauss = new[]    { 1.0 / 6.0, 1.0 / 6.0, 2.0 / 3.0 };
            double[] weightGauss = new[] { 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0 };*/

            double[] csiGauss = new[]   {
                1.0 / 2.0,
                0.0,
                1.0 / 2.0
            };
            double[] etaGauss = new[] {
                1.0 / 2.0,
                1.0 / 2.0,
                0.0
            };
            double[] weightGauss = new[] {
                1.0 / 6.0,
                1.0 / 6.0,
                1.0 / 6.0
            };

            /*double[] csiGauss = new []   { 1.0 / 3.0, 1.0 / 5.0, 3.0 / 5.0, 1.0 / 5.0 }; //unnecessary integration over 4 points
            double[] etaGauss = new[]    { 1.0 / 3.0, 1.0 / 5.0, 1.0 / 5.0, 3.0 / 5.0 };
            double[] weightGauss = new[] { -27.0 / 96.0, 25.0 / 96.0, 25.0 / 96.0, 25.0 / 96.0 };*/

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(9, 9);
            for (int i = 0; i < csiGauss.Length; i++) //trhough the 3 gauss points
            {
                double csi = csiGauss[i];
                double eta = etaGauss[i];
                mnl.Matrix<double> b = B(csi, eta);
                mnl.Matrix<double> m = b.Transpose() * _d * b;
                Console.WriteLine("B(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2") + ")^T * D * B(csi=" + csi.ToString("F2") + ",eta=");
                for (int row = 0; row < m.RowCount; row++)
                {
                    for (int col = 0; col < m.RowCount; col++)
                    {
                        Console.Write(m[row, col] +" ");
                    }
                    Console.WriteLine();
                }
                _kElementLocalCoord = _kElementLocalCoord + weightGauss[i] * m;
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
            double l31 = Math.Sqrt(x31 * x31 + y31 * y31);

            double x12 = node1.Position.X - node2.Position.X;
            double y12 = node1.Position.Y - node2.Position.Y;
            double l12 = Math.Sqrt(x12 * x12 + y12 * y12); 

            double x23 = node2.Position.X - node3.Position.X;
            double y23 = node2.Position.Y - node3.Position.Y;
            double l23 = Math.Sqrt(x23 * x23 + y23 * y23);

            _areaElement = (x31 * y12 - x12 * y31) / 2.0;

            //create vector of derivative of "new shape function"
            #region formuleFornite
            /*
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
            hxCsi[5] = -2.0 + 6.0 * csi + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6);
            hxCsi[6] = -eta * (P5 + P4);
            hxCsi[7] = eta * (q4 - q5);
            hxCsi[8] = -eta * (r5 - r4);
            Console.WriteLine("Hx,Csi(csi="+csi.ToString("F2")+" ,eta="+eta.ToString("F2")+") = " + hxCsi);

            mnl.Vector<double> hyCsi = mnl.Vector<double>.Build.Dense(9);
            hyCsi[0] = t6 * (1.0 - 2.0 * csi) + eta * (t5 - t6);
            hyCsi[1] = 1.0 + r6 * (1.0 - 2.0 * csi) - eta * (r5 + r6);
            hyCsi[2] = -q6 * (1.0 - 2.0 * csi) + eta * (q5 + q6); /// <<<<<------------ -eta instead of + eta
            hyCsi[3] = -t6 * (1.0 - 2.0 * csi) + eta * (t4 + t6);
            hyCsi[4] = -1.0 + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6); ///<-------------- -eta instead of + eta
            hyCsi[5] = -q6 * (1.0 - 2.0 * csi) - eta * (q4 - q6); ///<---------- +eta instead of -eta
            hyCsi[6] = -eta * (t4 + t5);
            hyCsi[7] = eta * (r4 - r5);
            hyCsi[8] = -eta * (q4 - q5);
            Console.WriteLine("Hy,Csi(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hyCsi);

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
            Console.WriteLine("Hx,Eta(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hxEta);

            mnl.Vector<double> hyEta = mnl.Vector<double>.Build.Dense(9);
            hyEta[0] = -t5 * (1.0 - 2.0 * eta) - csi * (t6 - t5);
            hyEta[1] = 1.0 + r5 * (1.0 - 2.0 * eta) - csi * (r5 + r6);
            hyEta[2] = -q5 * (1.0 - 2.0 * eta) + csi * (q5 + q6);
            hyEta[3] = csi * (t4 + t6);
            hyEta[4] = csi * (r4 - r6);
            hyEta[5] = -csi * (q4 - q6);
            hyEta[6] = t5 * (1.0 - 2.0 * eta) - csi * (t4 + t5);
            hyEta[7] = -1.0 + r5 * (1.0 - 2.0 * eta) + csi * (r4 - r5);
            hyEta[8] = -q5 * (1.0 - 2.0 * eta) - csi * (q4 - q5);  ///<----------- + csi instead of - csi
            Console.WriteLine("Hy,Eta(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hyEta);
            */

            /*mnl.Vector<double> r0 = y31 * hxCsi + y12 * hxEta;
            mnl.Vector<double> r1 = -x31 * hyCsi - x12 * hyEta;
            mnl.Vector<double> r2 = -x31 * hxCsi - x12 * hxEta + y31 * hyCsi + y12 * hyEta;*/
            #endregion

            #region Derivatives
            double a4 = -x23 / Math.Pow(l23, 2.0);
            double a5 = -x31 / Math.Pow(l31, 2.0);
            double a6 = -x12 / Math.Pow(l12, 2.0);
            Console.WriteLine("a4 = " + a4);
            Console.WriteLine("a5 = " + a5);
            Console.WriteLine("a6 = " + a6);

            double b4 = 3.0 / 4.0 * x23 * y23 / Math.Pow(l23, 2.0);
            double b5 = 3.0 / 4.0 * x31 * y31 / Math.Pow(l31, 2.0);
            double b6 = 3.0 / 4.0 * x12 * y12 / Math.Pow(l12, 2.0);
            Console.WriteLine("b4 = " + b4);
            Console.WriteLine("b5 = " + b5);
            Console.WriteLine("b6 = " + b6);

            double c4 = (1.0 / 4.0 * Math.Pow(x23, 2.0) - 1.0 / 2.0 * Math.Pow(y23, 2.0)) / Math.Pow(l23, 2.0);
            double c5 = (1.0 / 4.0 * Math.Pow(x31, 2.0) - 1.0 / 2.0 * Math.Pow(y31, 2.0)) / Math.Pow(l31, 2.0);
            double c6 = (1.0 / 4.0 * Math.Pow(x12, 2.0) - 1.0 / 2.0 * Math.Pow(y12, 2.0)) / Math.Pow(l12, 2.0);
            Console.WriteLine("c4 = " + c4);
            Console.WriteLine("c5 = " + c5);
            Console.WriteLine("c6 = " + c6);

            double d4 = -y23 / Math.Pow(l23, 2.0);
            double d5 = -y31 / Math.Pow(l31, 2.0);
            double d6 = -y12 / Math.Pow(l12, 2.0);
            Console.WriteLine("d4 = " + d4);
            Console.WriteLine("d5 = " + d5);
            Console.WriteLine("d6 = " + d6);

            double e4 = (1.0 / 4.0 * Math.Pow(y23, 2.0) - 1.0 / 2.0 * Math.Pow(x23, 2.0)) / Math.Pow(l23, 2.0);
            double e5 = (1.0 / 4.0 * Math.Pow(y31, 2.0) - 1.0 / 2.0 * Math.Pow(x31, 2.0)) / Math.Pow(l31, 2.0);
            double e6 = (1.0 / 4.0 * Math.Pow(y12, 2.0) - 1.0 / 2.0 * Math.Pow(x12, 2.0)) / Math.Pow(l12, 2.0);
            Console.WriteLine("e4 = " + e4);
            Console.WriteLine("e5 = " + e5);
            Console.WriteLine("e6 = " + e6);

            mnl.Vector<double> hxdCsi = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hydCsi = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hxdEta = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hydEta = mnl.Vector<double>.Build.Dense(9);

            hxdCsi[1 - 1] = 1.5 * (a6 * dNdCsi(6, csi, eta) - a5 * dNdCsi(5, csi, eta));
            hxdCsi[2 - 1] = b5 * dNdCsi(5, csi, eta) + b6 * dNdCsi(6, csi, eta);
            hxdCsi[3 - 1] = dNdCsi(1, csi, eta) - c5 * dNdCsi(5, csi, eta) - c6 * dNdCsi(6, csi, eta);
            
            hxdCsi[4 - 1] = 1.5 * (a4 * dNdCsi(4, csi, eta) - a6 * dNdCsi(6, csi, eta));
            hxdCsi[5 - 1] = (b6 * dNdCsi(6, csi, eta) + b4 * dNdCsi(4, csi, eta));
            hxdCsi[6 - 1] = dNdCsi(2, csi, eta) - c6 * dNdCsi(6, csi, eta) - c4 * dNdCsi(4, csi, eta);

            hxdCsi[7 - 1] = 1.5 * (a5 * dNdCsi(5, csi, eta) - a4 * dNdCsi(4, csi, eta));
            hxdCsi[8 - 1] = (b4 * dNdCsi(4, csi, eta) + b5 * dNdCsi(5, csi, eta));
            hxdCsi[9 - 1] = dNdCsi(3, csi, eta) - c4 * dNdCsi(4, csi, eta) - c5 * dNdCsi(5, csi, eta);

            //////////////////////////////////////////////////////////////////////////////////////////////////////
            
            hxdEta[1 - 1] = 1.5 * (a6 * dNdEta(6, csi, eta) - a5 * dNdEta(5, csi, eta));
            hxdEta[2 - 1] = b5 * dNdEta(5, csi, eta) + b6 * dNdEta(6, csi, eta);
            hxdEta[3 - 1] = dNdEta(1, csi, eta) - c5 * dNdEta(5, csi, eta) - c6 * dNdEta(6, csi, eta);

            hxdEta[4 - 1] = 1.5 * (a4 * dNdEta(4, csi, eta) - a6 * dNdEta(6, csi, eta));
            hxdEta[5 - 1] = b6 * dNdEta(6, csi, eta) + b4 * dNdEta(4, csi, eta);
            hxdEta[6 - 1] = dNdEta(2, csi, eta) - c6 * dNdEta(6, csi, eta) - c4 * dNdEta(4, csi, eta);

            hxdEta[7 - 1] = 1.5 * (a5 * dNdEta(5, csi, eta) - a4 * dNdEta(4, csi, eta));
            hxdEta[8 - 1] = (b4 * dNdEta(4, csi, eta) + b5 * dNdEta(5, csi, eta));
            hxdEta[9 - 1] = dNdEta(3, csi, eta) - c4 * dNdEta(4, csi, eta) - c5 * dNdEta(5, csi, eta);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            
            hydCsi[1 - 1] = 1.5 * (d6 * dNdCsi(6, csi, eta) - d5 * dNdCsi(5, csi, eta));
            hydCsi[2 - 1] = -dNdCsi(1, csi, eta) + e5 * dNdCsi(5, csi, eta) + e6 * dNdCsi(6, csi, eta);
            hydCsi[3 - 1] = -b5 * dNdCsi(5, csi, eta) - b6 * dNdCsi(6, csi, eta);

            hydCsi[4 - 1] = 1.5 * (d4 * dNdCsi(4, csi, eta) - d6 * dNdCsi(6, csi, eta));
            hydCsi[5 - 1] = -dNdCsi(2, csi, eta) + e6 * dNdCsi(6, csi, eta) + e4 * dNdCsi(4, csi, eta);
            hydCsi[6 - 1] = -b6 * dNdCsi(6, csi, eta) - b4 * dNdCsi(4, csi, eta);

            hydCsi[7 - 1] = 1.5 * (d5 * dNdCsi(5, csi, eta) - d4 * dNdCsi(4, csi, eta));
            hydCsi[8 - 1] = -dNdCsi(3, csi, eta) + e4 * dNdCsi(4, csi, eta) + e5 * dNdCsi(5, csi, eta);
            hydCsi[9 - 1] = -b4 * dNdCsi(4, csi, eta) - b5 * dNdCsi(5, csi, eta);

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            hydEta[1 - 1] = 1.5 * (d6 * dNdEta(6, csi, eta) - d5 * dNdEta(5, csi, eta));
            hydEta[2 - 1] = -dNdEta(1, csi, eta) + e5 * dNdEta(5, csi, eta) + e6 * dNdEta(6, csi, eta);
            hydEta[3 - 1] = -b5 * dNdEta(5, csi, eta) - b6 * dNdEta(6, csi, eta);

            hydEta[4 - 1] = 1.5 * (d4 * dNdEta(4, csi, eta) - d6 * dNdEta(6, csi, eta));
            hydEta[5 - 1] = -dNdEta(2, csi, eta) + e6 * dNdEta(6, csi, eta) + e4 * dNdEta(4, csi, eta);
            hydEta[6 - 1] = -b6 * dNdEta(6, csi, eta) - b4 * dNdEta(4, csi, eta);

            hydEta[7 - 1] = 1.5 * (d5 * dNdEta(5, csi, eta) - d4 * dNdEta(4, csi, eta));
            hydEta[8 - 1] = -dNdEta(3, csi, eta) + e4 * dNdEta(4, csi, eta) + e5 * dNdEta(5, csi, eta);
            hydEta[9 - 1] = -b4 * dNdEta(4, csi, eta) - b5 * dNdEta(5, csi, eta);

            mnl.Vector<double> r0 = y31 * hxdCsi + y12 * hxdEta;
            mnl.Vector<double> r1 = -x31 * hydCsi - x12 * hydEta;
            mnl.Vector<double> r2 = -x31 * hxdCsi - x12 * hxdEta + y31 * hydCsi + y12 * hydEta;
            #endregion

            mnl.Matrix<double> b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0,r1,r2);
            b = 1.0 / (2.0 * _areaElement) * b;
            Console.WriteLine("B(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + b);
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

        private double dNdCsi(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 4.0 * (csi + eta - 3.0 / 4.0);
                case 2:
                    return 4.0 * csi - 1.0;
                case 3:
                    return 0.0;
                case 4:
                    return 4.0 * eta;
                case 5:
                    return -4.0 * eta;
                case 6:
                    return -4.0 * (2.0 * csi + eta - 1.0);
                default:
                    throw new Exception();
            }
        }

        private double dNdEta(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 4.0 * (csi + eta - 3.0 / 4.0);
                case 2:
                    return 0.0;
                case 3:
                    return 4.0 * eta - 1.0;
                case 4:
                    return 4.0 * csi;
                case 5:
                    return -4.0 * (csi + 2.0 * eta - 1.0);
                case 6:
                    return -4.0 * csi;
                default:
                    throw new Exception();
            }
        }
        #endregion
    }
}