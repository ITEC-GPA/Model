using System;
using GPC.Model.FEM.Properties;
using GPC.Geometry;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Materials;
using GPC.Utilities.Fem;
using System.Collections.Generic;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Discrete Kirchoff Triangle - A study of three-node triangular plate bending elements - Jean-Louis Batoz
    /// International Jurnal for numerical methods in engineering, vol 15, 1771-1812 (1980)
    /// </summary>
    public class Tri3DK : Plate
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

        double _areaElement;

        Dictionary<int, double> _aCoeff;
        Dictionary<int, double> _bCoeff;
        Dictionary<int, double> _cCoeff;
        Dictionary<int, double> _dCoeff;
        Dictionary<int, double> _eCoeff;
        #endregion

        public Tri3DK(Node[] nodes) : base(nodes)
        {
            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            //displacement out of local plane "w" in local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);

            //Global : 3 nodes x 6 (DX, DY, DZ, RX, RY, RZ) DOF each = matrix 18x18
            //Local  : 3 nodes x 3 (dZ+rX+rZ) DOF each = matrix 9x9

            //DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //   [18x9]             [9x9]     [9x18]

            #region calculationLocalAxisAndLocalCoordinates
            //Local axes calculater anticlockwise
            Node[] localNodes = Tri3Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);
            Node node1 = localNodes[0];
            Node node2 = localNodes[1];
            Node node3 = localNodes[2];
            /*Console.WriteLine("node 1: " + node1.Name + "==" + node1.Id + " " + node1.ToString());
            Console.WriteLine("node 2: " + node2.Name + "==" + node2.Id + " " + node2.ToString());
            Console.WriteLine("node 3: " + node3.Name + "==" + node3.Id + " " + node3.ToString());*/
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

            _aCoeff = new Dictionary<int, double>();
            _bCoeff = new Dictionary<int, double>();
            _cCoeff = new Dictionary<int, double>();
            _dCoeff = new Dictionary<int, double>();
            _eCoeff = new Dictionary<int, double>();

            _aCoeff.Add(4, -_x23 / Math.Pow(_l23, 2.0));
            _aCoeff.Add(5, -_x31 / Math.Pow(_l31, 2.0));
            _aCoeff.Add(6, -_x12 / Math.Pow(_l12, 2.0));

            _bCoeff.Add(4, 3.0 / 4.0 * _x23 * _y23 / Math.Pow(_l23, 2.0));
            _bCoeff.Add(5, 3.0 / 4.0 * _x31 * _y31 / Math.Pow(_l31, 2.0));
            _bCoeff.Add(6, 3.0 / 4.0 * _x12 * _y12 / Math.Pow(_l12, 2.0));

            _cCoeff.Add(4, (1.0 / 4.0 * Math.Pow(_x23, 2.0) - 1.0 / 2.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0));
            _cCoeff.Add(5, (1.0 / 4.0 * Math.Pow(_x31, 2.0) - 1.0 / 2.0 * Math.Pow(_y31, 2.0)) / Math.Pow(_l31, 2.0));
            _cCoeff.Add(6, (1.0 / 4.0 * Math.Pow(_x12, 2.0) - 1.0 / 2.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0));

            _dCoeff.Add(4, -_y23 / Math.Pow(_l23, 2.0));
            _dCoeff.Add(5, -_y31 / Math.Pow(_l31, 2.0));
            _dCoeff.Add(6, -_y12 / Math.Pow(_l12, 2.0));

            _eCoeff.Add(4, (-1.0 / 2.0 * Math.Pow(_x23, 2.0) + 1.0 / 4.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0));
            _eCoeff.Add(5, (-1.0 / 2.0 * Math.Pow(_x31, 2.0) + 1.0 / 4.0 * Math.Pow(_y31, 2.0)) / Math.Pow(_l31, 2.0));
            _eCoeff.Add(6, (-1.0 / 2.0 * Math.Pow(_x12, 2.0) + 1.0 / 4.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0));
            #endregion
        }

        internal Tri3DK(Node[] nodes, PlateProperty property) : this(nodes)
        {
            SetProperty(property);
        }

        public override void BuildMatrix()
        {
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

            //local node1 rx-rotation and ry in global coordinate
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

            //local node2 rx-rotation and ry in global coordinate
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

            //local node3 rx-rotation and ry in global coordinate
            dofGlobalToLocalTranspose[15, 7] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[15, 8] = localY.DotProduct(globalX);

            dofGlobalToLocalTranspose[16, 7] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[16, 8] = localY.DotProduct(globalY);

            dofGlobalToLocalTranspose[17, 7] = localX.DotProduct(globalZ);
            dofGlobalToLocalTranspose[17, 8] = localY.DotProduct(globalZ);
            #endregion
            _dofGlobalToLocal = dofGlobalToLocalTranspose.Transpose();

            /*Console.WriteLine("dofGlobalToLocalTranspose.");
            for (int r = 0; r < dofGlobalToLocalTranspose.RowCount; r++)
            {
                for (int c = 0; c < dofGlobalToLocalTranspose.ColumnCount; c++)
                {
                    Console.Write(dofGlobalToLocalTranspose[r,c] + " ");
                }
                Console.WriteLine();
            }*/
            
            #endregion

            #region matrixD
            double E = ((IsotropicFemMaterial)((PlateProperty)_property).Material).E;
            double ni = ((IsotropicFemMaterial)((PlateProperty)_property).Material).Ni;

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

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(9, 9);
            GaussIntegration.GaussPoint[] gaussPoints = GaussIntegration.GetPointsTriangular(3);
            for (int i = 0; i < gaussPoints.Length; i++) //trhough the 3 gauss points
            {
                double csi = gaussPoints[i].Point.X;
                double eta = gaussPoints[i].Point.Y;
                mnl.Matrix<double> b = GetB(csi, eta);
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
                _kElementLocalCoord = _kElementLocalCoord + gaussPoints[i].Weight * m;
            }
            double detJ = 2.0 * _areaElement;

            _kElementLocalCoord = (detJ) * _kElementLocalCoord;
            
            /*Console.WriteLine("kElementLocal:");
            for (int row = 0; row < _kElementLocalCoord.RowCount; row++)
            {
                for (int col = 0; col < _kElementLocalCoord.RowCount; col++)
                {
                    Console.Write(_kElementLocalCoord[row, col] +" ");
                }
                Console.WriteLine();
            }*/
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(3 * Nodes.Length); //3 = DOF in local : DZ, RX, RZ
            foreach (IPlateLoadCaseAttribute iAttribute in _attributesLoadCase)
            {
                if (iAttribute is PlatePressureAttribute)
                {
                    PlatePressureAttribute attribute = (PlatePressureAttribute)iAttribute;
                    //calcultation of pressures in local coordinate system of the element
                    Vector3d dirX = attribute.CoordinateSystem.V1;
                    dirX.Unitize();
                    Vector3d dirY = attribute.CoordinateSystem.V2;
                    dirY.Unitize();
                    Vector3d dirZ = attribute.CoordinateSystem.V3;
                    dirZ.Unitize();

                    Vector3d x = LocalCoordinateSystem.V1;
                    dirX.Unitize();
                    Vector3d y = LocalCoordinateSystem.V2;
                    dirY.Unitize();
                    Vector3d z = LocalCoordinateSystem.V3;
                    dirZ.Unitize();

                    //Set in local coordinates
                    double px = attribute.P1 * dirX.DotProduct(x) + attribute.P2 * dirY.DotProduct(x) + attribute.P3 * dirZ.DotProduct(x);
                    double py = attribute.P1 * dirX.DotProduct(y) + attribute.P2 * dirY.DotProduct(y) + attribute.P3 * dirZ.DotProduct(y);
                    double pz = attribute.P1 * dirX.DotProduct(z) + attribute.P2 * dirY.DotProduct(z) + attribute.P3 * dirZ.DotProduct(z);

                    //Pressure --> node force
                    Vector3d f = new Vector3d(px * _areaElement / 3.0, py * _areaElement / 3.0, pz * _areaElement / 3.0); //force applied in each node

                    for (int i = 0; i < _fLocalCoord.Count; i = i + 3)
                    {
                        _fLocalCoord[i] = f.Z;
                    }
                }
            }
            return _fLocalCoord;
        }

        public override mnl.Matrix<double> GetB(double csi, double eta)
        {
            mnl.Vector<double> hxdCsi = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hydCsi = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hxdEta = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hydEta = mnl.Vector<double>.Build.Dense(9);

            #region Derivatives
            /*
            #region manualDerivatives
            double a4 = -_x23 / Math.Pow(_l23, 2.0);
            double a5 = -_x31 / Math.Pow(_l31, 2.0);
            double a6 = -_x12 / Math.Pow(_l12, 2.0);
            //Console.WriteLine("a4 = " + a4);
            //Console.WriteLine("a5 = " + a5);
            //Console.WriteLine("a6 = " + a6);

            double b4 = 3.0 / 4.0 * _x23 * _y23 / Math.Pow(_l23, 2.0);
            double b5 = 3.0 / 4.0 * _x31 * _y31 / Math.Pow(_l31, 2.0);
            double b6 = 3.0 / 4.0 * _x12 * _y12 / Math.Pow(_l12, 2.0);
            //Console.WriteLine("b4 = " + b4);
            //Console.WriteLine("b5 = " + b5);
            //Console.WriteLine("b6 = " + b6);

            double c4 = (1.0 / 4.0 * Math.Pow(_x23, 2.0) - 1.0 / 2.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0);
            double c5 = (1.0 / 4.0 * Math.Pow(_x31, 2.0) - 1.0 / 2.0 * Math.Pow(_y31, 2.0)) / Math.Pow(_l31, 2.0);
            double c6 = (1.0 / 4.0 * Math.Pow(_x12, 2.0) - 1.0 / 2.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0);
            //Console.WriteLine("c4 = " + c4);
            //Console.WriteLine("c5 = " + c5);
            //Console.WriteLine("c6 = " + c6);

            double d4 = -_y23 / Math.Pow(_l23, 2.0);
            double d5 = -_y31 / Math.Pow(_l31, 2.0);
            double d6 = -_y12 / Math.Pow(_l12, 2.0);
            //Console.WriteLine("d4 = " + d4);
            //Console.WriteLine("d5 = " + d5);
            //Console.WriteLine("d6 = " + d6);

            double e4 = (1.0 / 4.0 * Math.Pow(_y23, 2.0) - 1.0 / 2.0 * Math.Pow(_x23, 2.0)) / Math.Pow(_l23, 2.0);
            double e5 = (1.0 / 4.0 * Math.Pow(_y31, 2.0) - 1.0 / 2.0 * Math.Pow(_x31, 2.0)) / Math.Pow(_l31, 2.0);
            double e6 = (1.0 / 4.0 * Math.Pow(_y12, 2.0) - 1.0 / 2.0 * Math.Pow(_x12, 2.0)) / Math.Pow(_l12, 2.0);
            //Console.WriteLine("e4 = " + e4);
            //Console.WriteLine("e5 = " + e5);
            //Console.WriteLine("e6 = " + e6);

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
            #endregion
            */

            #region AppendixFormulas
            //from appendix formulas
            hxdCsi[1 - 1] = GetShapeFunctionDerivative(1, "x", "csi")(csi, eta);
            hxdCsi[2 - 1] = GetShapeFunctionDerivative(2, "x", "csi")(csi, eta);
            hxdCsi[3 - 1] = GetShapeFunctionDerivative(3, "x", "csi")(csi, eta);

            hxdCsi[4 - 1] = GetShapeFunctionDerivative(4, "x", "csi")(csi, eta);
            hxdCsi[5 - 1] = GetShapeFunctionDerivative(5, "x", "csi")(csi, eta);
            hxdCsi[6 - 1] = GetShapeFunctionDerivative(6, "x", "csi")(csi, eta);

            hxdCsi[7 - 1] = GetShapeFunctionDerivative(7, "x", "csi")(csi, eta);
            hxdCsi[8 - 1] = GetShapeFunctionDerivative(8, "x", "csi")(csi, eta);
            hxdCsi[9 - 1] = GetShapeFunctionDerivative(9, "x", "csi")(csi, eta);

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            hxdEta[1 - 1] = GetShapeFunctionDerivative(1, "x", "eta")(csi, eta);
            hxdEta[2 - 1] = GetShapeFunctionDerivative(2, "x", "eta")(csi, eta);
            hxdEta[3 - 1] = GetShapeFunctionDerivative(3, "x", "eta")(csi, eta);

            hxdEta[4 - 1] = GetShapeFunctionDerivative(4, "x", "eta")(csi, eta);
            hxdEta[5 - 1] = GetShapeFunctionDerivative(5, "x", "eta")(csi, eta);
            hxdEta[6 - 1] = GetShapeFunctionDerivative(6, "x", "eta")(csi, eta);

            hxdEta[7 - 1] = GetShapeFunctionDerivative(7, "x", "eta")(csi, eta);
            hxdEta[8 - 1] = GetShapeFunctionDerivative(8, "x", "eta")(csi, eta);
            hxdEta[9 - 1] = GetShapeFunctionDerivative(9, "x", "eta")(csi, eta);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            hydCsi[1 - 1] = GetShapeFunctionDerivative(1, "y", "csi")(csi, eta);
            hydCsi[2 - 1] = GetShapeFunctionDerivative(2, "y", "csi")(csi, eta);
            hydCsi[3 - 1] = GetShapeFunctionDerivative(3, "y", "csi")(csi, eta);

            hydCsi[4 - 1] = GetShapeFunctionDerivative(4, "y", "csi")(csi, eta);
            hydCsi[5 - 1] = GetShapeFunctionDerivative(5, "y", "csi")(csi, eta);
            hydCsi[6 - 1] = GetShapeFunctionDerivative(6, "y", "csi")(csi, eta);

            hydCsi[7 - 1] = GetShapeFunctionDerivative(7, "y", "csi")(csi, eta);
            hydCsi[8 - 1] = GetShapeFunctionDerivative(8, "y", "csi")(csi, eta);
            hydCsi[9 - 1] = GetShapeFunctionDerivative(9, "y", "csi")(csi, eta);

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            hydEta[1 - 1] = GetShapeFunctionDerivative(1, "y", "eta")(csi, eta);
            hydEta[2 - 1] = GetShapeFunctionDerivative(2, "y", "eta")(csi, eta);
            hydEta[3 - 1] = GetShapeFunctionDerivative(3, "y", "eta")(csi, eta);

            hydEta[4 - 1] = GetShapeFunctionDerivative(4, "y", "eta")(csi, eta);
            hydEta[5 - 1] = GetShapeFunctionDerivative(5, "y", "eta")(csi, eta);
            hydEta[6 - 1] = GetShapeFunctionDerivative(6, "y", "eta")(csi, eta);

            hydEta[7 - 1] = GetShapeFunctionDerivative(7, "y", "eta")(csi, eta);
            hydEta[8 - 1] = GetShapeFunctionDerivative(8, "y", "eta")(csi, eta);
            hydEta[9 - 1] = GetShapeFunctionDerivative(9, "y", "eta")(csi, eta);
            #endregion            

            mnl.Vector<double> r0 = _y31 * hxdCsi + _y12 * hxdEta;
            mnl.Vector<double> r1 = -_x31 * hydCsi - _x12 * hydEta;
            mnl.Vector<double> r2 = -_x31 * hxdCsi - _x12 * hxdEta + _y31 * hydCsi + _y12 * hydEta;
            #endregion

            mnl.Matrix<double> b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0,r1,r2);
            b = 1.0 / (2.0 * _areaElement) * b;
            //Console.WriteLine("B(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + b);
            return b;
        }

        //TODO: Da ottimizzare/scrivere
        public void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            Console.WriteLine("Result element " + this.Name + " " + this.Id);

            //Tensor Rotation matrix
            mnl.Matrix<double> rotation = mnl.Matrix<double>.Build.Dense(3, 3);
            CoordinateSystem versorsLocalAxis = LocalCoordinateSystem;
            Vector3d xVersor = versorsLocalAxis.V1;
            Vector3d yVersor = versorsLocalAxis.V2;
            Vector3d zVersor = versorsLocalAxis.V3;

            rotation[0, 0] = xVersor.X;
            rotation[0, 1] = yVersor.X;
            rotation[0, 2] = zVersor.X;

            rotation[1, 0] = xVersor.Y;
            rotation[1, 1] = yVersor.Y;
            rotation[1, 2] = zVersor.Y;

            rotation[2, 0] = xVersor.Z;
            rotation[2, 1] = yVersor.Z;
            rotation[2, 2] = zVersor.Z;
            Console.WriteLine("Rotation matrix tensor:" + rotation.ToString());

            mnl.Vector<double> localDisplacementsVector = DofGlobalToLocal * mnl.Vector<double>.Build.Dense(globalDisplacementsNodes);
            localDisplacements = localDisplacementsVector.ToArray();

            //curvature = B * U
            //contains curvature xx, yy, xy
            mnl.Vector<double> curvatureLocal1 = GetB(0.0, 0.0) * localDisplacementsVector; //node 1
            mnl.Vector<double> curvatureLocal2 = GetB(1.0, 0.0) * localDisplacementsVector; //node 2
            mnl.Vector<double> curvatureLocal3 = GetB(0.0, 1.0) * localDisplacementsVector; //node 3

            mnl.Matrix<double> localPseudoDeformationNode1 = mnl.Matrix<double>.Build.Dense(3, 3);
            localPseudoDeformationNode1[0, 0] = curvatureLocal1[0];
            localPseudoDeformationNode1[1, 0] = curvatureLocal1[2];
            localPseudoDeformationNode1[0, 1] = curvatureLocal1[2];
            localPseudoDeformationNode1[1, 1] = curvatureLocal1[1];

            mnl.Matrix<double> localPseudoDeformationNode2 = mnl.Matrix<double>.Build.Dense(3, 3);
            localPseudoDeformationNode2[0, 0] = curvatureLocal2[0];
            localPseudoDeformationNode2[1, 0] = curvatureLocal2[2];
            localPseudoDeformationNode2[0, 1] = curvatureLocal2[2];
            localPseudoDeformationNode2[1, 1] = curvatureLocal2[1];

            mnl.Matrix<double> localPseudoDeformationNode3 = mnl.Matrix<double>.Build.Dense(3, 3);
            localPseudoDeformationNode3[0, 0] = curvatureLocal3[0];
            localPseudoDeformationNode3[1, 0] = curvatureLocal3[2];
            localPseudoDeformationNode3[0, 1] = curvatureLocal3[2];
            localPseudoDeformationNode3[1, 1] = curvatureLocal3[1];

            localPseudoDeformation = new mnl.Matrix<double>[3]
            {
                localPseudoDeformationNode1,
                localPseudoDeformationNode2,
                localPseudoDeformationNode3
            };
            Console.WriteLine("local curvature = " + localPseudoDeformation[0]);

            //convert in global coords
            globalPseudoDeformation = new mnl.Matrix<double>[3]
            {
                rotation * localPseudoDeformationNode1 * rotation.Transpose(),
                rotation * localPseudoDeformationNode2 * rotation.Transpose(),
                rotation * localPseudoDeformationNode3 * rotation.Transpose()
            };
            Console.WriteLine("global curvature = " + globalPseudoDeformation[0]);

            //get bending moment in the three nodes
            mnl.Vector<double> MLocalNode1 = D * curvatureLocal1; //node 1
            mnl.Vector<double> MLocalNode2 = D * curvatureLocal2; //node 2
            mnl.Vector<double> MLocalNode3 = D * curvatureLocal3; //node 3
            
            /*Console.WriteLine("Local coordinates:");
            Console.WriteLine("M node " + Nodes[0].Name +" =" + MNode1);
            Console.WriteLine("M node " + Nodes[1].Name + " =" + MNode2);
            Console.WriteLine("M node " + Nodes[2].Name + " =" + MNode3);*/

            //Node 1
            #region ConvertInGlobalCoordinates
            //Define Couchy Tensor
            mnl.Matrix<double> MLocalCouchyNode1 = mnl.Matrix<double>.Build.Dense(3, 3);

            MLocalCouchyNode1[0, 0] = MLocalNode1[0]; //M_xx
            MLocalCouchyNode1[1, 1] = MLocalNode1[1]; //M_yy
            MLocalCouchyNode1[0, 1] = MLocalNode1[2]; //M_xy
            MLocalCouchyNode1[1, 0] = MLocalNode1[2]; //M_yx
            Console.WriteLine("M local node 1 = " + MLocalCouchyNode1);

            //node2
            mnl.Matrix<double> MLocalCouchyNode2 = mnl.Matrix<double>.Build.Dense(3, 3);
            MLocalCouchyNode2[0, 0] = MLocalNode2[0]; //M_xx
            MLocalCouchyNode2[1, 1] = MLocalNode2[1]; //M_yy
            MLocalCouchyNode2[0, 1] = MLocalNode2[2]; //M_xy
            MLocalCouchyNode2[1, 0] = MLocalNode2[2]; //M_yx

            //node3
            mnl.Matrix<double> MLocalCouchyNode3 = mnl.Matrix<double>.Build.Dense(3, 3);
            MLocalCouchyNode3[0, 0] = MLocalNode3[0]; //M_xx
            MLocalCouchyNode3[1, 1] = MLocalNode3[1]; //M_yy
            MLocalCouchyNode3[0, 1] = MLocalNode3[2]; //M_xy
            MLocalCouchyNode3[1, 0] = MLocalNode3[2]; //M_yx

            localForces = new mnl.Matrix<double>[3] { MLocalCouchyNode1, MLocalCouchyNode2, MLocalCouchyNode3 };
            #endregion

            Console.WriteLine("Global coordinates:");
            //Second order tensor -> Trotated = Q * T * Q^T
            globalForces = new mnl.Matrix<double>[3] {
                rotation * MLocalCouchyNode1 * rotation.Transpose(),
                rotation * MLocalCouchyNode2 * rotation.Transpose(),
                rotation * MLocalCouchyNode3 * rotation.Transpose()
            };
            Console.WriteLine("M Global node " + Nodes[0].Name + " =" + globalForces[0]);

            //calculation of stress
            double tb = ((PlateProperty)Property).BendingThickness;
            double W = 1.0 / 6.0 * Math.Pow(tb, 2.0);

            //local top
            mnl.Matrix<double> stressLocalChouchyNode1Top = MLocalCouchyNode1 / W; //M > 0 -> sigma(z > t/2) > 0
            mnl.Matrix<double> stressLocalChouchyNode2Top = MLocalCouchyNode2 / W;
            mnl.Matrix<double> stressLocalChouchyNode3Top = MLocalCouchyNode3 / W;

            //local bottom
            mnl.Matrix<double> stressLocalChouchyNode1Bottom = -MLocalCouchyNode1 / W; //M > 0 -> sigma(z < t/2) < 0
            mnl.Matrix<double> stressLocalChouchyNode2Bottom = -MLocalCouchyNode2 / W;
            mnl.Matrix<double> stressLocalChouchyNode3Bottom = -MLocalCouchyNode2 / W;
            localStress = new mnl.Matrix<double>[6] {
                stressLocalChouchyNode1Top,
                stressLocalChouchyNode2Top,
                stressLocalChouchyNode3Top,
                stressLocalChouchyNode1Bottom,
                stressLocalChouchyNode2Bottom,
                stressLocalChouchyNode3Bottom
            };

            globalStress = new mnl.Matrix<double>[6] {
                rotation * stressLocalChouchyNode1Top * rotation.Transpose(),
                rotation * stressLocalChouchyNode2Top * rotation.Transpose(),
                rotation * stressLocalChouchyNode3Top * rotation.Transpose(),
                rotation * stressLocalChouchyNode1Bottom * rotation.Transpose(),
                rotation * stressLocalChouchyNode2Bottom * rotation.Transpose(),
                rotation * stressLocalChouchyNode3Bottom * rotation.Transpose()
            };
            Console.WriteLine("stress global node 1 top (localz=t/2): " + globalStress[0]);

            //matrix for plane stress

            mnl.Matrix<double> dPlaneStress = ((PlateProperty)_property).Material.GetPlaneStress();

            mnl.Matrix<double> dPlaneStressInv = dPlaneStress.Inverse();

            //calculation of epsilons
            localEpsilon = new mnl.Matrix<double>[6] {
                dPlaneStressInv * stressLocalChouchyNode1Top,
                dPlaneStressInv * stressLocalChouchyNode2Top,
                dPlaneStressInv * stressLocalChouchyNode3Top,
                dPlaneStressInv * stressLocalChouchyNode1Bottom,
                dPlaneStressInv * stressLocalChouchyNode2Bottom,
                dPlaneStressInv * stressLocalChouchyNode3Bottom
            };

            globalEpsilon = new mnl.Matrix<double>[6] {
                dPlaneStressInv * globalStress[0],
                dPlaneStressInv * globalStress[1],
                dPlaneStressInv * globalStress[2],
                dPlaneStressInv * globalStress[3],
                dPlaneStressInv * globalStress[4],
                dPlaneStressInv * globalStress[5]
            };

            //get Shear in local nodes
            //NOT APPLICABLE -> Kirchoff -> No shear
            /*double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();
            double tb = ((PlateProperty)Property).BendingThickness;
            double k = 5.0 / 6.0; //shear correction factor
            mnl.Matrix<double> Ds = E * tb * k / (2.0 * (1.0 + ni)) * mnl.Matrix<double>.Build.DenseIdentity(2);*/
        }


        /// <summary>
        /// Description between eq. 27b and 28 of the article
        /// </summary>
        /// <param name="indexes1"></param>
        /// <param name="indexes2"></param>
        /// <param name="indexes3"></param>
        private void GetIndexes(out int[] indexes1, out int[] indexes2, out int[] indexes3)
        {
            indexes1 = new int[3];
            indexes1[0] = 1;
            indexes1[1] = 2;
            indexes1[2] = 3;

            indexes2 = new int[3];
            indexes2[0] = 5;
            indexes2[1] = 6;
            indexes2[2] = 4;

            indexes3 = new int[3];
            indexes3[0] = 6;
            indexes3[1] = 4;
            indexes3[2] = 5;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexFunction"></param>
        /// <param name="dir">x or y</param>
        /// <param name="deriv">null for shaper function; csi or eta for dH(dir)dCsi or dH(dir)dy</param>
        /// <returns></returns>
        internal Func<double, double, double> GetFunction(int indexFunction, string dir, string deriv = "")
        {
            if (deriv != "")
            {
                throw new Exception();
            }
            GetIndexes(out int[] indexes1, out int[] indexes2, out int[] indexes3);

            Func<double, double, double> F(int i)
            {
                if (deriv == "")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionsTri6.NaturalShapeFunction(i, csi, eta);
                }
                else if (deriv == "csi")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionsTri6.DNdCsi(i, csi, eta);
                }
                else if (deriv == "eta")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionsTri6.DNdEta(i, csi, eta);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            #region Hx
            Func<double, double, double> H1x(int ind2, int ind3)
            {
                return (double csi, double eta) => 3.0 / 2.0 * (_aCoeff[ind3] * F(ind3)(csi, eta) - _aCoeff[ind2] * F(ind2)(csi, eta)); //H1x
            }
            Func<double, double, double> H2x(int ind2, int ind3)
            {
                return (double csi, double eta) => _bCoeff[ind3] * F(ind3)(csi, eta) + _bCoeff[ind2] * F(ind2)(csi, eta); //H2x
            }
            Func<double, double, double> H3x(int ind1, int ind2, int ind3)
            {
                return (double csi, double eta) => F(ind1)(csi, eta) - _cCoeff[ind3] * F(ind3)(csi, eta) - _cCoeff[ind2] * F(ind2)(csi, eta); //H3x
            }
            #endregion

            #region Hy
            Func<double, double, double> H1y(int ind2, int ind3)
            {
                return (double csi, double eta) => 3.0 / 2.0 * (_dCoeff[ind3] * F(ind3)(csi, eta) - _dCoeff[ind2] * F(ind2)(csi, eta)); //H1y
            }
            Func<double, double, double> H2y(int ind1, int ind2, int ind3)
            {
                return (double csi, double eta) => -F(ind1)(csi, eta) + _eCoeff[ind3] * F(ind3)(csi, eta) + _eCoeff[ind2] * F(ind2)(csi, eta); //H3x
            }
            Func<double, double, double> H3y(int ind2, int ind3)
            {
                return (double csi, double eta) => -_bCoeff[ind3] * F(ind3)(csi, eta) - _bCoeff[ind2] * F(ind2)(csi, eta); //H2x
            }
            #endregion

            int index1;
            int index2;
            int index3;
            switch (dir)
            {
                case "x":
                    switch (indexFunction)
                    {
                        case 1:
                            index2 = indexes2[0];
                            index3 = indexes3[0];
                            return H1x(index2, index3);
                        case 2:
                            index2 = indexes2[0];
                            index3 = indexes3[0];
                            return H2x(index2, index3);
                        case 3:
                            index1 = indexes1[0];
                            index2 = indexes2[0];
                            index3 = indexes3[0];
                            return H3x(index1, index2, index3);

                        case 4:
                            index2 = indexes2[1];
                            index3 = indexes3[1];
                            return H1x(index2, index3);
                        case 5:
                            index2 = indexes2[1];
                            index3 = indexes3[1];
                            return H2x(index2, index3);
                        case 6:
                            index1 = indexes1[1];
                            index2 = indexes2[1];
                            index3 = indexes3[1];
                            return H3x(index1, index2, index3);

                        case 7:
                            index2 = indexes2[2];
                            index3 = indexes3[2];
                            return H1x(index2, index3);
                        case 8:
                            index2 = indexes2[2];
                            index3 = indexes3[2];
                            return H2x(index2, index3);
                        case 9:
                            index1 = indexes1[2];
                            index2 = indexes2[2];
                            index3 = indexes3[2];
                            return H3x(index1, index2, index3);

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case "y":
                    switch (indexFunction)
                    {
                        case 1:
                            index2 = indexes2[0];
                            index3 = indexes3[0];
                            return H1y(index2, index3);
                        case 2:
                            index1 = indexes1[0];
                            index2 = indexes2[0];
                            index3 = indexes3[0];
                            return H2y(index1, index2, index3);
                        case 3:
                            index2 = indexes2[0];
                            index3 = indexes3[0];
                            return H3y(index2, index3);

                        case 4:
                            index2 = indexes2[1];
                            index3 = indexes3[1];
                            return H1y(index2, index3);
                        case 5:
                            index1 = indexes1[1];
                            index2 = indexes2[1];
                            index3 = indexes3[1];
                            return H2y(index1, index2, index3);
                        case 6:
                            index2 = indexes2[1];
                            index3 = indexes3[1];
                            return H3y(index2, index3);

                        case 7:
                            index2 = indexes2[2];
                            index3 = indexes3[2];
                            return H1y(index2, index3);
                        case 8:
                            index1 = indexes1[2];
                            index2 = indexes2[2];
                            index3 = indexes3[2];
                            return H2y(index1, index2, index3);
                        case 9:
                            index2 = indexes2[2];
                            index3 = indexes3[2];
                            return H3y(index2, index3);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal Func<double, double, double> GetShapeFunctionDerivative(int indexFunction, string dir, string deriv)
        {
            //derivative of "new shape function"
            #region formuleFornite
            
            double P4 = -6.0 * _x23 / Math.Pow(_l23, 2.0);
            double P5 = -6.0 * _x31 / Math.Pow(_l31, 2.0);
            double P6 = -6.0 * _x12 / Math.Pow(_l12, 2.0);

            double q4 = 3.0 * _x23 * _y23 / Math.Pow(_l23, 2.0);
            double q5 = 3.0 * _x31 * _y31 / Math.Pow(_l31, 2.0);
            double q6 = 3.0 * _x12 * _y12 / Math.Pow(_l12, 2.0);

            double r4 = 3.0 * Math.Pow(_y23, 2.0) / Math.Pow(_l23, 2.0);
            double r5 = 3.0 * Math.Pow(_y31, 2.0) / Math.Pow(_l31, 2.0);
            double r6 = 3.0 * Math.Pow(_y12, 2.0) / Math.Pow(_l12, 2.0);

            double t4 = -6.0 * _y23 / Math.Pow(_l23, 2.0);
            double t5 = -6.0 * _y31 / Math.Pow(_l31, 2.0);
            double t6 = -6.0 * _y12 / Math.Pow(_l12, 2.0);

            if (dir.ToLower() == "x") {
                if (deriv.ToLower() == "csi")
                {
                    //mnl.Vector<double> hxCsi = mnl.Vector<double>.Build.Dense(9);
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => P6 * (1.0 - 2.0 * csi) + (P5 - P6) * eta;
                        case 2:
                            return (double csi, double eta) => q6 * (1.0 - 2.0 * csi) - (q5 + q6) * eta;
                        case 3:
                            return (double csi, double eta) => -4.0 + 6.0 * (csi + eta) + r6 * (1.0 - 2.0 * csi) - eta * (r5 + r6);
                        case 4:
                            return (double csi, double eta) => -P6 * (1.0 - 2.0 * csi) + eta * (P4 + P6);
                        case 5:
                            return (double csi, double eta) => q6 * (1.0 - 2.0 * csi) - eta * (q6 - q4);
                        case 6:
                            return (double csi, double eta) => -2.0 + 6.0 * csi + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6);
                        case 7:
                            return (double csi, double eta) => -eta * (P5 + P4);
                        case 8:
                            return (double csi, double eta) => eta * (q4 - q5);
                        case 9:
                            return (double csi, double eta) => -eta * (r5 - r4);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    //Console.WriteLine("Hx,Csi(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hxCsi);
                } else if (deriv.ToLower() == "eta")
                {
                    //mnl.Vector<double> hxEta = mnl.Vector<double>.Build.Dense(9);
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => -P5 * (1.0 - 2.0 * eta) - csi * (P6 - P5);
                        case 2:
                            return (double csi, double eta) => q5 * (1.0 - 2.0 * eta) - csi * (q5 + q6);
                        case 3:
                            return (double csi, double eta) => -4.0 + 6.0 * (csi + eta) + r5 * (1.0 - 2.0 * eta) - csi * (r5 + r6);
                        case 4:
                            return (double csi, double eta) => csi * (P4 + P6);
                        case 5:
                            return (double csi, double eta) => csi * (q4 - q6);
                        case 6:
                            return (double csi, double eta) => -csi * (r6 - r4);
                        case 7:
                            return (double csi, double eta) => P5 * (1.0 - 2.0 * eta) - csi * (P4 + P5);
                        case 8:
                            return (double csi, double eta) => q5 * (1.0 - 2.0 * eta) + csi * (q4 - q5);
                        case 9:
                            return (double csi, double eta) => -2.0 + 6.0 * eta + r5 * (1.0 - 2.0 * eta) + csi * (r4 - r5);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }                  
                    //Console.WriteLine("Hx,Eta(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hxEta);
                } else
                {
                    throw new ArgumentOutOfRangeException();
                }
            } else if (dir.ToLower() == "y")
            {
                if (deriv == "csi")
                {
                    //mnl.Vector<double> hyCsi = mnl.Vector<double>.Build.Dense(9);
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => t6 * (1.0 - 2.0 * csi) + eta * (t5 - t6);
                        case 2:
                            return (double csi, double eta) => 1.0 + r6 * (1.0 - 2.0 * csi) - eta * (r5 + r6);
                        case 3:
                            return (double csi, double eta) => -q6 * (1.0 - 2.0 * csi) + eta * (q5 + q6); // <<<<<------------ -eta instead of + eta
                        case 4:
                            return (double csi, double eta) => -t6 * (1.0 - 2.0 * csi) + eta * (t4 + t6);
                        case 5:
                            return (double csi, double eta) => -1.0 + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6); //<-------------- -eta instead of + eta
                        case 6:
                            return (double csi, double eta) => -q6 * (1.0 - 2.0 * csi) - eta * (q4 - q6); //<---------- +eta instead of -eta
                        case 7:
                            return (double csi, double eta) => -eta * (t4 + t5);
                        case 8:
                            return (double csi, double eta) => eta * (r4 - r5);
                        case 9:
                            return (double csi, double eta) => -eta * (q4 - q5);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    //Console.WriteLine("Hy,Csi(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hyCsi);
                } else if (deriv == "eta")
                {
                    //mnl.Vector<double> hyEta = mnl.Vector<double>.Build.Dense(9);
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => -t5 * (1.0 - 2.0 * eta) - csi * (t6 - t5);
                        case 2:
                            return (double csi, double eta) => 1.0 + r5 * (1.0 - 2.0 * eta) - csi * (r5 + r6);
                        case 3:
                            return (double csi, double eta) => -q5 * (1.0 - 2.0 * eta) + csi * (q5 + q6);
                        case 4:
                            return (double csi, double eta) => csi * (t4 + t6);
                        case 5:
                            return (double csi, double eta) => csi * (r4 - r6);
                        case 6:
                            return (double csi, double eta) => -csi * (q4 - q6);
                        case 7:
                            return (double csi, double eta) => t5 * (1.0 - 2.0 * eta) - csi * (t4 + t5);
                        case 8:
                            return (double csi, double eta) => -1.0 + r5 * (1.0 - 2.0 * eta) + csi * (r4 - r5);
                        case 9:
                            return (double csi, double eta) => -q5 * (1.0 - 2.0 * eta) - csi * (q4 - q5);  //<----------- + csi instead of - csi
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    //Console.WriteLine("Hy,Eta(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hyEta);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
            #endregion
        }
    }
}
