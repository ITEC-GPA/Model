using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using mnl = MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// A conforming triangular plate element with rotationa degrees of freedom - 2014 - Xiang-Rong Fu - Ming-Wu Yuan - Chen Pu
    /// NON FUNZIONA - NON MI FORNISCE BUONI RISULTATI
    /// </summary>
    public class Tri3TR3RDOFMembrane : Plate
    {
        #region variables
        Node[] _localNodes;
        double _thickness;
        double _areaElement;
        #endregion

        public Tri3TR3RDOFMembrane(Node[] nodes) : base(nodes)
        {
            //recalled base(nodes)
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            _DOF.Add(LinearSolver.DOF.RX);
            _DOF.Add(LinearSolver.DOF.RY);
            _DOF.Add(LinearSolver.DOF.RZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!
            //a rotation in Local coordinate plane (rz) can be a RX, RY, RZ in Global space!

            //Local matrix: 3 nodes x 3(dx, dy, rz) gdl = 9x9 matrix
            //Global matrix: 3 nodes x 6(DX, DY, DZ, RX, RY, RZ) gdl = 18x18 matrix

            //keGlobal = DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //[18x18]          [18x9]         [9x9]     [9x18]
        }

        internal Tri3TR3RDOFMembrane(Node[] nodes, PlateProperty property) :this(nodes)
        {
            SetProperty(property);
        }

        public override void BuildMatrix()
        {
            _thickness = ((PlateProperty)_property).MembraneThickness;

            #region matrixD
            /*double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();*/

            _d = ((PlateProperty)_property).Material.GetPlaneStress();
            //Console.WriteLine("D = " + _d.ToString());
            #endregion

            //Node 1 = Origin = Node i
            //Axis x assigned as Node 1 to Node 2, Node j = Node 2
            //Axis y ortogonal to axis x, Node k = node 3

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates

            _localNodes = Tri3Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem); //take global node and transform in local nodes
            Console.WriteLine("Element Local Nodes");
            _localNodes.ToList().ForEach(x => Console.WriteLine(x));
            
            _dofGlobalToLocal = mnl.Matrix<double>.Build.Dense(9, 18);
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(18, 9);

            Vector3d globalX = new Vector3d(1.0, 0.0, 0.0);
            Vector3d globalY = new Vector3d(0.0, 1.0, 0.0);
            Vector3d globalZ = new Vector3d(0.0, 0.0, 1.0);

            Vector3d localX = LocalCoordinateSystem.V1;
            Vector3d localY = LocalCoordinateSystem.V2;
            Vector3d localZ = LocalCoordinateSystem.V3;

            #region localToGlobalNode1
            //local node1 x-displacement in global coordinate
            dofGlobalToLocalTranspose[0, 0] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[1, 0] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[2, 0] = localX.DotProduct(globalZ);

            //local node1 y-displacement in global coord
            dofGlobalToLocalTranspose[0, 1] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[1, 1] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[2, 1] = localY.DotProduct(globalZ);

            //local node1 rz in global coord
            dofGlobalToLocalTranspose[3, 2] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[4, 2] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[5, 2] = localZ.DotProduct(globalZ);
            #endregion
            
            #region localToGlobalNode2
            //local node2 x-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 3] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 3] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 3] = localX.DotProduct(globalZ);

            //local node2 y-displacement in global coord
            dofGlobalToLocalTranspose[6, 4] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 4] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 4] = localY.DotProduct(globalZ);

            //local node2 rz in global coord
            dofGlobalToLocalTranspose[9, 5] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[10, 5] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[11, 5] = localZ.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode3
            //local node3 x-displacement in global coordinate
            dofGlobalToLocalTranspose[12, 6] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[13, 6] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[14, 6] = localX.DotProduct(globalZ);

            //local node3 y-displacement in global coord
            dofGlobalToLocalTranspose[12, 7] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[13, 7] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[14, 7] = localY.DotProduct(globalZ);

            //local node3 rz in global coord
            dofGlobalToLocalTranspose[15, 8] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[16, 8] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[17, 8] = localZ.DotProduct(globalZ);
            #endregion

            _dofGlobalToLocal = dofGlobalToLocalTranspose.Transpose();
            //Console.WriteLine("Local To Global Matrix = " + DofGlobalToLocal.ToString());
            /*Console.WriteLine("Local to Global Matrix = ");
            for (int r = 0; r < dofGlobalToLocalTranspose.RowCount; r++)
            {
                for (int c = 0; c < dofGlobalToLocalTranspose.ColumnCount; c++)
                {
                    Console.Write(dofGlobalToLocalTranspose[r,c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }*/

            #endregion

            #region stiffnessMatrixInLocalCoordinates
            /*_areaElement = 0.5 * ((-_localNodes[1 - 1].Position.X + _localNodes[2 - 1].Position.X) * (-_localNodes[1 - 1].Position.Y + _localNodes[3 - 1].Position.Y)
                                       -(-_localNodes[1 - 1].Position.X + _localNodes[3 - 1].Position.X) * (-_localNodes[1 - 1].Position.Y + _localNodes[2 - 1].Position.Y));*/
            
            

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(9, 9);
            OldGaussIntegration.GaussPoint[] gaussPoints = OldGaussIntegration.GetPointsTriangular(3);
            for (int i = 0; i < gaussPoints.Length; i++) //trhough the gauss points
            {
                double csi = gaussPoints[i].Point.X;
                double eta = gaussPoints[i].Point.Y;
                mnl.Matrix<double> b = GetB(csi, eta);
                mnl.Matrix<double> m = b.Transpose() * _d * b;
                /*Console.WriteLine("B(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2") + ")^T * D * B(csi=" + csi.ToString("F2") + ",eta=");
                for (int row = 0; row < m.RowCount; row++)
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
            _kElementLocalCoord = (_thickness * detJ) * _kElementLocalCoord;

            //_kElementLocalCoord = ; 
            /*Console.WriteLine("KElementLocalCoord = ");
            for (int r = 0; r < _kElementLocalCoord.RowCount; r++)
            {
                for (int c = 0; c < _kElementLocalCoord.ColumnCount; c++)
                {
                    Console.Write(_kElementLocalCoord[r, c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }*/
            #endregion
        }

        public mnl.Matrix<double> GetB(double csi, double eta)
        {
            double x = N(1, csi, eta) * _localNodes[1 - 1].Position.X + N(2, csi, eta) * _localNodes[2 - 1].Position.X + N(3, csi, eta) * _localNodes[3 - 1].Position.X;
            double y = N(1, csi, eta) * _localNodes[1 - 1].Position.Y + N(2, csi, eta) * _localNodes[2 - 1].Position.Y + N(3, csi, eta) * _localNodes[3 - 1].Position.Y;

            Console.WriteLine("x = " + x);
            Console.WriteLine("y = " + y);

            double x1 = _localNodes[1 - 1].Position.X;
            double y1 = _localNodes[1 - 1].Position.Y;
            Console.WriteLine("x1 = " + x1 + " y1 = " + y1);

            double x2 = _localNodes[2 - 1].Position.X;
            double y2 = _localNodes[2 - 1].Position.Y;
            Console.WriteLine("x2 = " + x2 + " y2 = " + y2);

            double x3 = _localNodes[3 - 1].Position.X;
            double y3 = _localNodes[3 - 1].Position.Y;
            Console.WriteLine("x3 = " + x3 + " y3 = " + y3);

            _areaElement = 0.5 * (x1 * y2 - x2 * y1 - x1 * y3 + x3 * y1 + x2 * y3 - x3 * y2);
            Console.WriteLine("area element = " + _areaElement);

            /*double a1 = (x2 * y3) - (x3 * y2);
            double a2 = (x3 * y1) - (x1 * y3);
            double a3 = (x1 * y2) - (x2 * y1);*/

            double b1 = y2 - y3;
            double b2 = y3 - y1;
            double b3 = y1 - y2;
            Console.WriteLine("b1 = " + b1);
            Console.WriteLine("b2 = " + b2);
            Console.WriteLine("b3 = " + b3);

            double c1 = x3 - x2;
            double c2 = x1 - x3;
            double c3 = x2 - x1;
            Console.WriteLine("c1 = " + c1);
            Console.WriteLine("c2 = " + c2);
            Console.WriteLine("c3 = " + c3);

            double bx = (x1 * y1 * b1 + x2 * y2 * b2 + x3 * y3 * b3) / (2.0 * _areaElement);
            Console.WriteLine("bx = " + bx);
            double by = (x1 * y1 * c1 + x2 * y2 * c2 + x3 * y3 * c3) / (2.0 * _areaElement);
            Console.WriteLine("by = " + by);

            mnl.Matrix<double> B1 = mnl.Matrix<double>.Build.Dense(3, 3);
            mnl.Matrix<double> B2 = mnl.Matrix<double>.Build.Dense(3, 3);
            mnl.Matrix<double> B3 = mnl.Matrix<double>.Build.Dense(3, 3);

            #region B1
            B1[1 - 1, 1 - 1] = b1 / (2.0 * _areaElement);
            B1[1 - 1, 3 - 1] = b1 * (2.0 * y - y1 - bx) / 4.0;

            B1[2 - 1, 2 - 1] = c1 / (2.0 * _areaElement);
            B1[2 - 1, 3 - 1] = -c1 * (2.0 * x - x1 - by) / 4.0;

            B1[3 - 1, 1 - 1] = c1 / (2.0 * _areaElement);
            B1[3 - 1, 2 - 1] = b1 / (2.0 * _areaElement);
            B1[3 - 1, 3 - 1] = (x1 * b1 + x2 * b3 + x3 * b2) / 4.0;
            Console.WriteLine("B1 = " + B1);
            #endregion

            #region B2
            B2[1 - 1, 1 - 1] = b2 / (2.0 * _areaElement);
            B2[1 - 1, 3 - 1] = b2 * (2.0 * y - y2 - bx) / 4.0;

            B2[2 - 1, 2 - 1] = c2 / (2.0 * _areaElement);
            B2[2 - 1, 3 - 1] = -c2 * (2.0 * x - x2 - by) / 4.0;

            B2[3 - 1, 1 - 1] = c2 / (2.0 * _areaElement);
            B2[3 - 1, 2 - 1] = b2 / (2.0 * _areaElement);
            B2[3 - 1, 3 - 1] = (x1 * b3 + x2 * b2 + x3 * b2) / 4.0;
            Console.WriteLine("B2 = " + B2);
            #endregion

            #region B3
            B3[1 - 1, 1 - 1] = b3 / (2.0 * _areaElement);
            B3[1 - 1, 3 - 1] = b3 * (2.0 * y - y3 - bx) / 4.0;

            B3[2 - 1, 2 - 1] = c3 / (2.0 * _areaElement);
            B3[2 - 1, 3 - 1] = -c3 * (2.0 * x - x3 - by) / 4.0;

            B3[3 - 1, 1 - 1] = c3 / (2.0 * _areaElement);
            B3[3 - 1, 2 - 1] = b3 / (2.0 * _areaElement);
            B3[3 - 1, 3 - 1] = (x1 * b2 + x2 * b1 + x3 * b3) / 4.0;
            Console.WriteLine("B3 = " + B3);
            #endregion

            mnl.Matrix<double> B = mnl.Matrix<double>.Build.Dense(3, 0);
            B = B.Append(B1);
            B = B.Append(B2);
            B = B.Append(B3);

            Console.WriteLine("B = " + B);
            return B;
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(3 * Nodes.Length); //3 = DOF in local : DX and DY, RZ
            /*foreach (IPlateLoadCaseAttribute iAttribute in _attributesLoadCase)
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

                    for (int i = 0; i < _fLocalCoord.Count; i=i+2)
                    {
                        _fLocalCoord[i] = f.X;
                        _fLocalCoord[i+1] = f.Y;
                    }
                }
            }*/
            return _fLocalCoord;
        }

        //TODO: Da ottimizzare/scrivere
        public void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            localDisplacements = GetLocalDisplacement(globalDisplacementsNodes);
            mnl.Vector<double> vecLocalDispl = mnl.Vector<double>.Build.Dense(localDisplacements);

            double xG = _localNodes.Select(x => x.Position.X).Sum() / 3.0;
            double yG = _localNodes.Select(x => x.Position.Y).Sum() / 3.0;

            #region CalculationOfStressAndDeformationsInLocalCoordinates
            mnl.Vector<double>[] epsilonLocal = new mnl.Vector<double>[] {
                //epsilon_xx; epsilon_yy; epsilon_xy
                GetB(_localNodes[0].Position.X, _localNodes[0].Position.Y) * vecLocalDispl, //Node1
                GetB(_localNodes[1].Position.X, _localNodes[1].Position.Y) * vecLocalDispl, //Node2
                GetB(_localNodes[2].Position.X, _localNodes[2].Position.Y) * vecLocalDispl, //Node3
                GetB(xG, yG) * vecLocalDispl, //Centroid
            };

            mnl.Vector<double>[] stressLocal = epsilonLocal.Select(epsilonLoc => D * epsilonLoc).ToArray(); //sigma_xx; sigma_yy; tau_xy

            /*Console.WriteLine("Strains in Local coordinates:" + epsilon.ToString());
            Console.WriteLine("Stress in Local coordinates:" + stress.ToString());*/
            #endregion

            #region ConvertInGlobalCoordinates
            //Define Couchy Tensor
            Func<mnl.Vector<double>, mnl.Matrix<double>> VectorToCouchy = delegate (mnl.Vector<double> input)
            {
                mnl.Matrix<double> couchy = mnl.Matrix<double>.Build.Dense(3, 3);
                couchy[0, 0] = input[0]; //epsilon_xx or sigma_xx
                couchy[1, 1] = input[1]; //epsilon_yy or sigma_xx

                couchy[0, 1] = input[2]; //epsilon_xy or sigma_xy
                couchy[1, 0] = input[2]; //epsilon_yx or sigma_yx
                //epsilonCouchy[2, 2] = -ni / E * (sigma_xx + sigma_yy) + alpha * Temperature ; //epsilon_zz
                return couchy;
            };

            mnl.Matrix<double>[] epsilonLocalCouchy = epsilonLocal.Select(VectorToCouchy).ToArray();
            //Console.WriteLine("Epsilon local coordinate:" + epsilonCouchy.ToString());            


            mnl.Matrix<double>[] stressLocalCouchy = stressLocal.Select(VectorToCouchy).ToArray();
            //Console.WriteLine("Stress local coordinate:" + stressCouchy.ToString());

            //Rotation matrix
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
            //Console.WriteLine("Rotation matrix tensor:" + rotation.ToString());

            //Second order tensor -> Trotated = Q * T * Q^T
            mnl.Matrix<double>[] epsilonGlobalCouchy = epsilonLocalCouchy.Select(epsilonLocalCouchyElement => rotation * epsilonLocalCouchyElement * rotation.Transpose()).ToArray();
            //Console.WriteLine("Epsilon in global coordinates = " + epsilonGlobalCouchy);

            mnl.Matrix<double>[] stressGlobalCouchy = stressLocalCouchy.Select(stressLocalCouchyElement => rotation * stressLocalCouchyElement * rotation.Transpose()).ToArray();
            //Console.WriteLine("Stress in global coordinates = " + stressGlobalCouchy);
            #endregion

            localPseudoDeformation = epsilonLocalCouchy;
            globalPseudoDeformation = epsilonGlobalCouchy;

            localStress = stressLocalCouchy;
            globalStress = stressGlobalCouchy;

            double thickness = ((PlateProperty)_property).MembraneThickness;
            globalForces = stressGlobalCouchy.Select(stressGlobalCouchyElement => thickness * stressGlobalCouchyElement).ToArray() ;
            localForces = stressLocalCouchy.Select(stressLocalCouchyElement => thickness * stressLocalCouchyElement).ToArray();
                        
            globalEpsilon = epsilonGlobalCouchy;
            localEpsilon = stressGlobalCouchy;
        }

        /// <summary>
        /// Shape functions
        /// </summary>
        private double N(int i, double csi, double eta)
        {
            switch (i)
            {
                case 1:
                    return 1.0 - csi - eta;
                case 2:
                    return csi;
                case 3:
                    return eta;
                default:
                    throw new Exception();
            }
        }
    }
}
