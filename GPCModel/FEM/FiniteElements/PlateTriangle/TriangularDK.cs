using System;
using GPC.Model.FEM.Properties;
using GPC.Geometry;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Discrete Kirchoff Triangle - A study of three-node triangular plate bending elements - Jean-Louis Batoz
    /// International Jurnal for numerical methods in engineering, vol 15, 1771-1812 (1980)
    /// </summary>
    public class TriangularDK : TriangleElement
    {
        #region variables
        double _x31;
        double _y31;
        double _l31;

        double _x12;
        double _y12;
        double _l12;

        double _x23;
        double _y23;
        double _l23;
        //private Func<double, double, double>[] _shapeFunctions;
        #endregion

        public TriangularDK(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            DOF.Add(LinearSolver.DOF.DX);
            DOF.Add(LinearSolver.DOF.DY);
            DOF.Add(LinearSolver.DOF.DZ);
            //displacement w il local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(LinearSolver.DOF.RX);
            DOF.Add(LinearSolver.DOF.RY);
            DOF.Add(LinearSolver.DOF.RZ);

            //Global : 3 nodes x 6 (DX, DY, DZ, RX, RY, RZ) DOF each = matrix 18x18
            //Local  : 3 nodes x 3 (dZ+rX+rZ) DOF each = matrix 9x9

            //DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //   [18x9]             [9x9]     [9x18]

            /*_shapeFunctions = new Func<double, double, double>[6];
            _shapeFunctions[0] = N1;
            _shapeFunctions[1] = N2;
            _shapeFunctions[2] = N3;
            _shapeFunctions[3] = N4;
            _shapeFunctions[4] = N5;
            _shapeFunctions[5] = N6;*/
        }

        public override void BuildMatrix()
        {
            #region calculationLocalAxisAndLocalCoordinates
            Node[] localNodes = LocalNodes(); //node 1 is origin, node 2 is in (0,y2), node3 is in (x2,y2)
            Node node1 = localNodes[0];
            Node node2 = localNodes[1];
            Node node3 = localNodes[2];
            Console.WriteLine("node 1: " + node1.Name + "==" + node1.Id + " " + node1.ToString());
            Console.WriteLine("node 2: " + node2.Name + "==" + node2.Id + " " + node2.ToString());
            Console.WriteLine("node 3: " + node3.Name + "==" + node3.Id + " " + node3.ToString());
            #endregion

            #region calculationVariablesForStiffnessEtcetera
            _x31 = node3.Position.X - node1.Position.X;
            _y31 = node3.Position.Y - node1.Position.Y;
            _l31 = Math.Sqrt(_x31 * _x31 + _y31 * _y31);

            _x12 = node1.Position.X - node2.Position.X;
            _y12 = node1.Position.Y - node2.Position.Y;
            _l12 = Math.Sqrt(_x12 * _x12 + _y12 * _y12);

            _x23 = node2.Position.X - node3.Position.X;
            _y23 = node2.Position.Y - node3.Position.Y;
            _l23 = Math.Sqrt(_x23 * _x23 + _y23 * _y23);

            _areaElement = (_x31 * _y12 - _x12 * _y31) / 2.0;
            #endregion

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            _dofGlobalToLocal = mnl.Matrix<double>.Build.Dense(9, 18);

            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(18, 9);

            Vector3d globalX = new Vector3d(1.0, 0.0, 0.0);
            Vector3d globalY = new Vector3d(0.0, 1.0, 0.0);
            Vector3d globalZ = new Vector3d(0.0, 0.0, 1.0);

            Vector3d localX = LocalCoordinateSystem.V1;
            Vector3d localY = LocalCoordinateSystem.V2;
            Vector3d localZ = LocalCoordinateSystem.V3;

            #region localToGlobalNode1
            //local node1 z-displacement in global coordinate
            dofGlobalToLocalTranspose[0, 0] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[1, 0] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[2, 0] = localZ.DotProduct(globalZ);

            //local node1 rx-rotation in global coordinate
            dofGlobalToLocalTranspose[3, 1] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[3, 2] = localY.DotProduct(globalX);

            dofGlobalToLocalTranspose[4, 1] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[4, 2] = localY.DotProduct(globalY);

            dofGlobalToLocalTranspose[5, 1] = localX.DotProduct(globalZ);
            dofGlobalToLocalTranspose[5, 2] = localY.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode2
            //local node2 z-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 3] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 3] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 3] = localZ.DotProduct(globalZ);

            //local node2 rx-rotation in global coordinate
            dofGlobalToLocalTranspose[9, 4] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[9, 5] = localY.DotProduct(globalX);

            dofGlobalToLocalTranspose[10, 4] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[10, 5] = localY.DotProduct(globalY);

            dofGlobalToLocalTranspose[11, 4] = localX.DotProduct(globalZ);
            dofGlobalToLocalTranspose[11, 5] = localY.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode3
            //local node3 z-displacement in global coordinate
            dofGlobalToLocalTranspose[12, 6] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[13, 6] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[14, 6] = localZ.DotProduct(globalZ);

            //local node3 rx-rotation in global coordinate
            dofGlobalToLocalTranspose[15, 7] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[15, 8] = localY.DotProduct(globalX);

            dofGlobalToLocalTranspose[16, 7] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[16, 8] = localY.DotProduct(globalY);

            dofGlobalToLocalTranspose[17, 7] = localX.DotProduct(globalZ);
            dofGlobalToLocalTranspose[17, 8] = localY.DotProduct(globalZ);
            #endregion
            _dofGlobalToLocal = dofGlobalToLocalTranspose.Transpose();

            Console.WriteLine("dofGlobalToLocalTranspose.");
            for (int r = 0; r < dofGlobalToLocalTranspose.RowCount; r++)
            {
                for (int c = 0; c < dofGlobalToLocalTranspose.ColumnCount; c++)
                {
                    Console.Write(dofGlobalToLocalTranspose[r,c] + " ");
                }
                Console.WriteLine();
            }
            
            #endregion

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

            //3 Gauss Integration points
            /*double[] csiGauss = new [] { 1.0 / 6.0, 2.0 / 3.0, 1.0 / 6.0 };
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
                //Console.WriteLine("B(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2") + ")^T * D * B(csi=" + csi.ToString("F2") + ",eta=");
                /*for (int row = 0; row < m.RowCount; row++)
                {
                    for (int col = 0; col < m.RowCount; col++)
                    {
                        Console.Write(m[row, col] +" ");
                    }
                    Console.WriteLine();
                }*/
                _kElementLocalCoord = _kElementLocalCoord + weightGauss[i] * m;
            }
            _kElementLocalCoord = (2.0 * _areaElement) * _kElementLocalCoord;
            Console.WriteLine("kElementLocal:");
            for (int row = 0; row < _kElementLocalCoord.RowCount; row++)
            {
                for (int col = 0; col < _kElementLocalCoord.RowCount; col++)
                {
                    Console.Write(_kElementLocalCoord[row, col] +" ");
                }
                Console.WriteLine();
            }
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            Console.WriteLine("BuildFLocalCoord TriangularDKT not yet implemented");
            //throw new System.NotImplementedException();
            return mnl.Vector<double>.Build.Dense(9);
        }

        protected mnl.Matrix<double> B(double csi, double eta)
        {
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
            double a4 = -_x23 / Math.Pow(_l23, 2.0);
            double a5 = -_x31 / Math.Pow(_l31, 2.0);
            double a6 = -_x12 / Math.Pow(_l12, 2.0);
            /*Console.WriteLine("a4 = " + a4);
            Console.WriteLine("a5 = " + a5);
            Console.WriteLine("a6 = " + a6);*/

            double b4 = 3.0 / 4.0 * _x23 * _y23 / Math.Pow(_l23, 2.0);
            double b5 = 3.0 / 4.0 * _x31 * _y31 / Math.Pow(_l31, 2.0);
            double b6 = 3.0 / 4.0 * _x12 * _y12 / Math.Pow(_l12, 2.0);
            /*Console.WriteLine("b4 = " + b4);
            Console.WriteLine("b5 = " + b5);
            Console.WriteLine("b6 = " + b6);*/

            double c4 = (1.0 / 4.0 * Math.Pow(_x23, 2.0) - 1.0 / 2.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0);
            double c5 = (1.0 / 4.0 * Math.Pow(_x31, 2.0) - 1.0 / 2.0 * Math.Pow(_y31, 2.0)) / Math.Pow(_l31, 2.0);
            double c6 = (1.0 / 4.0 * Math.Pow(_x12, 2.0) - 1.0 / 2.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0);
            /*Console.WriteLine("c4 = " + c4);
            Console.WriteLine("c5 = " + c5);
            Console.WriteLine("c6 = " + c6);*/

            double d4 = -_y23 / Math.Pow(_l23, 2.0);
            double d5 = -_y31 / Math.Pow(_l31, 2.0);
            double d6 = -_y12 / Math.Pow(_l12, 2.0);
            /*Console.WriteLine("d4 = " + d4);
            Console.WriteLine("d5 = " + d5);
            Console.WriteLine("d6 = " + d6);*/

            double e4 = (1.0 / 4.0 * Math.Pow(_y23, 2.0) - 1.0 / 2.0 * Math.Pow(_x23, 2.0)) / Math.Pow(_l23, 2.0);
            double e5 = (1.0 / 4.0 * Math.Pow(_y31, 2.0) - 1.0 / 2.0 * Math.Pow(_x31, 2.0)) / Math.Pow(_l31, 2.0);
            double e6 = (1.0 / 4.0 * Math.Pow(_y12, 2.0) - 1.0 / 2.0 * Math.Pow(_x12, 2.0)) / Math.Pow(_l12, 2.0);
            /*Console.WriteLine("e4 = " + e4);
            Console.WriteLine("e5 = " + e5);
            Console.WriteLine("e6 = " + e6);*/

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

            mnl.Vector<double> r0 = _y31 * hxdCsi + _y12 * hxdEta;
            mnl.Vector<double> r1 = -_x31 * hydCsi - _x12 * hydEta;
            mnl.Vector<double> r2 = -_x31 * hxdCsi - _x12 * hxdEta + _y31 * hydCsi + _y12 * hydEta;
            #endregion

            mnl.Matrix<double> b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0,r1,r2);
            b = 1.0 / (2.0 * _areaElement) * b;
            //Console.WriteLine("B(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + b);
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
        /// <summary>
        /// According to article, order of nodes are ANTICLOCKWISE
        /// </summary>
        /// <returns></returns>
        protected override Node[] LocalNodes()
        {
            #region CalculationOfLocalCoordinates
            //Search for 3 local axis
            Node nodeI = Nodes[0];
            Node nodeJ = Nodes[1];
            Node nodeK = Nodes[2];
            Vector3d x = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d vecx = new Vector3d(x);
            vecx.Unitize();

            Vector3d y = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);
            Vector3d vecy = new Vector3d(y);
            vecy.Unitize();

            Vector3d z = vecx.CrossProduct(vecy);
            Vector3d vecz = new Vector3d(z);
            vecz.Unitize();

            //recalculation of y that can be non-ortogonal
            y = z.CrossProduct(x);
            vecy = new Vector3d(y);
            vecy.Unitize();
            _localCoordinateSystem = new Geometry.CoordinateSystem(new Point3d(0, 0, 0), vecx, vecy);

            //move to local axis
            //calculation in local nodes
            Vector3d v12 = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d v13 = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);

            Node[] localNodes = new Node[3];
            localNodes[0] = new Node(0, 0, 0, nodeI.Id, nodeI.Name); //Origin GlobalNodes.ElementAt(1 - 1);
            localNodes[1] = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Id, nodeJ.Name); //Axis x GlobalNodes.ElementAt(2 - 1);
            localNodes[2] = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Id, nodeK.Name); //GlobalNodes.ElementAt(3 - 1);
            #endregion
            return localNodes;
        }
    }
}