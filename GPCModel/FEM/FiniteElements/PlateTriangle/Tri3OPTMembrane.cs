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
    /// Based of "A study of optimal membrane triangles with drilling freedoms" - Felippa - 2003
    /// </summary>
    public class Tri3OPTMembrane : Plate
    {
        #region variables
        #endregion

        public Tri3OPTMembrane(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
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

        public override void BuildMatrix()
        {
            double thickness = ((PlateProperty)_property).MembraneThickness;

            #region matrixD
            double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();

            _d = mnl.Matrix<double>.Build.Dense(3, 3);
            _d[0, 0] = 1.0;
            _d[0, 1] = ni;
            _d[1, 0] = ni;
            _d[1, 1] = 1.0;
            _d[2, 2] = (1.0 - ni) / 2.0;
            _d = E / (1.0 - ni * ni) * _d;
            //Console.WriteLine("D = " + _d.ToString());
            #endregion

            //Node 1 = Origin = Node i
            //Axis x assigned as Node 1 to Node 2, Node j = Node 2
            //Axis y ortogonal to axis x, Node k = node 3

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates

            Node[] localNodes = Tri3Element.LocalNodes(_nodesGlobal, out _localCoordinateSystem); //take global node and transform in local nodes
            Node node1 = localNodes[0];
            Node node2 = localNodes[1];
            Node node3 = localNodes[2];
            
            _dofGlobalToLocal = mnl.Matrix<double>.Build.Dense(9, 18);
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(18, 9);

            Vector3d globalX = new Vector3d(1.0, 0.0, 0.0);
            Vector3d globalY = new Vector3d(0.0, 1.0, 0.0);
            Vector3d globalZ = new Vector3d(0.0, 0.0, 1.0);

            Vector3d localX = LocalCoordinateSystem.V1;
            Vector3d localY = LocalCoordinateSystem.V2;
            Vector3d localZ = LocalCoordinateSystem.V3;

            mnl.Matrix<double>[] k = GetK(localNodes, D, thickness);

            /*Console.WriteLine("kb:" + k[0]);
            Console.WriteLine("kh:" + k[1]);*/

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
            Console.WriteLine("Local to Global Matrix = ");
            for (int r = 0; r < dofGlobalToLocalTranspose.RowCount; r++)
            {
                for (int c = 0; c < dofGlobalToLocalTranspose.ColumnCount; c++)
                {
                    Console.Write(dofGlobalToLocalTranspose[r,c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }

            #endregion

            #region ShapeFuction
            #endregion

            #region BMatrixDerivateOfShapeFunctionInLocalCoordinates
            /*_b = mnl.Matrix<double>.Build.Dense(3, 6);
            _b[0, 0] = dy23;
            _*/
            //Console.WriteLine("Matrix B = " + _b.ToString());

            //_b = 1.0 / (2.0 * _areaElement) * _b;
            //Console.WriteLine("Matrix B = " + _b.ToString());
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            /*double thk = ((PlateProperty)_property).MembraneThickness;
            double V = _areaElement * thk;
            _kElementLocalCoord = V * _b.Transpose() * _d * _b;*/
            
            _kElementLocalCoord = k[0] + k[1]; //k basic stiffness + k higher order stiffness (drilling)
            Console.WriteLine("KElementLocalCoord = ");
            for (int r = 0; r < _kElementLocalCoord.RowCount; r++)
            {
                for (int c = 0; c < _kElementLocalCoord.ColumnCount; c++)
                {
                    Console.Write(_kElementLocalCoord[r, c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }
            #endregion
        }

        public override mnl.Matrix<double> GetB(double csi = 0, double eta = 0, double zeta = 0)
        {
            return mnl.Matrix<double>.Build.Dense(0, 0);
            //return _b; //constant in the element
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

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            localDisplacements = GetLocalDisplacement(globalDisplacementsNodes);
            mnl.Vector<double> vecLocalDispl = mnl.Vector<double>.Build.Dense(localDisplacements);
        
            #region CalculationOfStressAndDeformationsInLocalCoordinates
            mnl.Vector<double> epsilonLocal = GetB() * vecLocalDispl; //epsilon_xx; epsilon_yy; epsilon_xy
            mnl.Vector<double> stressLocal = D * epsilonLocal; //sigma_xx; sigma_yy; tau_xy
            /*Console.WriteLine("Strains in Local coordinates:" + epsilon.ToString());
            Console.WriteLine("Stress in Local coordinates:" + stress.ToString());*/
            #endregion

            #region ConvertInGlobalCoordinates
            //Define Couchy Tensor
            mnl.Matrix<double> epsilonLocalCouchy = mnl.Matrix<double>.Build.Dense(3, 3);
            epsilonLocalCouchy[0, 0] = epsilonLocal[0]; //epsilon_xx
            epsilonLocalCouchy[1, 1] = epsilonLocal[1]; //epsilon_yy

            epsilonLocalCouchy[0, 1] = epsilonLocal[2]; //epsilon_xy
            epsilonLocalCouchy[1, 0] = epsilonLocal[2]; //epsilon_yx
            //epsilonCouchy[2, 2] = -ni / E * (sigma_xx + sigma_yy) + alpha * Temperature ; //epsilon_zz
            //Console.WriteLine("Epsilon local coordinate:" + epsilonCouchy.ToString());

            mnl.Matrix<double> stressLocalCouchy = mnl.Matrix<double>.Build.Dense(3, 3);
            stressLocalCouchy[0, 0] = stressLocal[0]; //sigma_xx
            stressLocalCouchy[1, 1] = stressLocal[1]; //sigma_yy

            stressLocalCouchy[0, 1] = stressLocal[2]; //sigma_xy
            stressLocalCouchy[1, 0] = stressLocal[2]; //sigma_yx
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
            mnl.Matrix<double> epsilonGlobalCouchy = rotation * epsilonLocalCouchy * rotation.Transpose();
            Console.WriteLine("Epsilon in global coordinates = " + epsilonGlobalCouchy);

            mnl.Matrix<double> stressGlobalCouchy = rotation * stressLocalCouchy * rotation.Transpose();
            Console.WriteLine("Stress in global coordinates = " + stressGlobalCouchy);
            #endregion

            localPseudoDeformation = new mnl.Matrix<double>[1] { epsilonLocalCouchy };
            globalPseudoDeformation = new mnl.Matrix<double>[1] { epsilonGlobalCouchy };

            localStress = new mnl.Matrix<double>[1] { stressLocalCouchy };
            globalStress = new mnl.Matrix<double>[1] { stressGlobalCouchy };

            double thickness = ((PlateProperty)_property).MembraneThickness;
            globalForces = new mnl.Matrix<double>[1] { thickness * stressGlobalCouchy };
            localForces = new mnl.Matrix<double>[1] { thickness * stressLocalCouchy };
                        
            globalEpsilon = new mnl.Matrix<double>[1] { epsilonGlobalCouchy };
            localEpsilon = new mnl.Matrix<double>[1] { stressGlobalCouchy };
        }

        private mnl.Matrix<double>[] GetK(Node[] localNodes, mnl.Matrix<double> Emat, double thickness)
        {
            //first GetK[0] = Kb
            //first GetK[1] = Kh

            double x12 = localNodes[1 - 1].Position.X - localNodes[2 - 1].Position.X;
            double x23 = localNodes[2 - 1].Position.X - localNodes[3 - 1].Position.X;
            double x31 = localNodes[3 - 1].Position.X - localNodes[1 - 1].Position.X;
            double x21 = -x12;
            double x32 = -x23;
            double x13 = -x31;

            double y12 = localNodes[1 - 1].Position.Y - localNodes[2 - 1].Position.Y;
            double y23 = localNodes[2 - 1].Position.Y - localNodes[3 - 1].Position.Y;
            double y31 = localNodes[3 - 1].Position.Y - localNodes[1 - 1].Position.Y;
            double y21 = -y12;
            double y32 = -y23;
            double y13 = -y31;

            double area = (y21 * x13 - x21 * y13) / 2.0;
            double area2 = 2.0 * area;
            double area4 = 4.0 * area;

            #region UNUSED

            /*
             * This part came directly from routine in Mathematica of "A study of optimal membrane triangles with drilling
             * degrees of freedom" pg.21
             * and are used to get poisson coefficient "ni"
             */

            //double E11 = Emat[1 - 1, 1 - 1];
            //double E12 = Emat[1 - 1, 2 - 1];
            //double E13 = Emat[1 - 1, 3 - 1];

            //double E21 = Emat[2 - 1, 1 - 1];
            //double E22 = Emat[2 - 1, 2 - 1];
            //double E23 = Emat[2 - 1, 3 - 1];

            //double E31 = Emat[3 - 1, 1 - 1];
            //double E32 = Emat[3 - 1, 2 - 1];
            //double E33 = Emat[3 - 1, 3 - 1];

            //     Edet = E11 * E22 * E33 + 2 *   E12 * E13 * E23 - E11 *          E23 ^ 2   - E22 *          E13 ^ 2   - E33 *          E12 ^ 2;
            //double Edet = E11 * E22 * E33 + 2.0 * E12 * E13 * E23 - E11 * Math.Pow(E23, 2.0) - E22 * Math.Pow(E13, 2.0) - E33 * Math.Pow(E12, 2.0);

            //     W = -6 * E12^3 + 5 * E11^2 * E22 - 5 * E12^2 * E22 
            //          - E22 * (75 * E13^2 + 14 * E13 * E23 + 3 * E23 ^ 2)
            //          + 2 * E12 * (7 * E13 ^ 2 + 46 * E13 * E23 + 7 * E23 ^ 2) 
            //         -E11 * (5 * E12 ^ 2 + 3 * E13 ^ 2 - 6 * E12 * E22 - 5 * E22 ^ 2 + 14 * E13 * E23 + 75 * E23 ^ 2) +
            //         (3 * E11 ^ 2 + 82 * E11 * E22 + 3 * E22 ^ 2 - 4 * (6 * E12 ^ 2 + 5 * E13 ^ 2 - 6 * E13 * E23 + 5 * E23 ^ 2)) * E33
            //          + 4 * (5 * E11 - 6 * E12 + 5 * E22) * E33 ^ 2;

            /*double W = -6.0 * Math.Pow(E12, 3.0) + 5.0 * Math.Pow(E11, 2.0) * E22 - 5.0 * Math.Pow(E12, 2.0) * E22
                       - E22 * (75.0 * Math.Pow(E13, 2.0) + 14.0 * E13 * E23 + 3.0 * Math.Pow(E23, 2.0))
                       + 2.0 * E12 * (7.0 * Math.Pow(E13, 2.0) + 46.0 * E13 * E23 + 7.0 * Math.Pow(E23, 2))
                       - E11 * (5.0 * Math.Pow(E12, 2.0) + 3.0 * Math.Pow(E13, 2.0) - 6.0 * E12 * E22 - 5.0 * Math.Pow(E22, 2.0) + 14.0 * E13 * E23 + 75.0 * Math.Pow(E23, 2.0))
                       + (3.0 * Math.Pow(E11, 2.0) + 82.0 * E11 * E22 + 3.0 * Math.Pow(E22, 2.0) - 4.0 * (6.0 * Math.Pow(E12, 2.0) + 5.0 * Math.Pow(E13, 2.0) - 6.0 * E13 * E23 + 5.0 * Math.Pow(E23, 2.0))) * E33
                       + 4.0 * (5.0 * E11 - 6.0 * E12 + 5.0 * E22) * Math.Pow(E33, 2.0);*/

            //double e11C11avg = W / (128.0 * Edet);
            #endregion

            double ni = ((PlateProperty)_property).GetNi();

            //Coefficient for OPTimal element
            double alphab = 3.0 / 2.0;
            double beta0 = Math.Max(1.0 / 2.0 * (1.0 - 4.0 * ni * ni), 0.01);//Math.Max(2.0 / e11C11avg - 3.0 / 2.0, 0.01);
            double beta1 = 1.0;
            double beta2 = 2.0;
            double beta3 = 1.0;
            double beta4 = 0.0;
            double beta5 = 1.0;
            double beta6 = -1.0;
            double beta7 = -1.0;
            double beta8 = -1.0;
            double beta9 = -2.0;

            mnl.Matrix<double> L = mnl.Matrix<double>.Build.Dense(0, 3);
            L = L.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { y23, 0.0, x32 }));
            L = L.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { 0.0, x32, y23 }));
            L = L.InsertRow(2, alphab / 6.0 * mnl.Vector<double>.Build.Dense(new double[] { y23 * (y13 - y21), x32 * (x31 - x12), (x31 * y13 - x12 * y21) * 2.0 }));
            L = L.InsertRow(3, mnl.Vector<double>.Build.Dense(new double[] { y31, 0.0, x13 }));
            L = L.InsertRow(4, mnl.Vector<double>.Build.Dense(new double[] { 0.0, x13, y31 }));
            L = L.InsertRow(5, alphab / 6.0 * mnl.Vector<double>.Build.Dense(new double[] { y31 * (y21 - y32), x13 * (x12 - x23), (x12 * y21 - x23 * y32) * 2.0 }));
            L = L.InsertRow(6, mnl.Vector<double>.Build.Dense(new double[] { y12, 0.0, x21 }));
            L = L.InsertRow(7, mnl.Vector<double>.Build.Dense(new double[] { 0.0, x21, y12 }));
            L = L.InsertRow(8, alphab / 6.0 * mnl.Vector<double>.Build.Dense(new double[] { y12 * (y32 - y13), x21 * (x23 - x31), (x23 * y32 - x31 * y13) * 2.0 }));

            L = L * thickness / 2.0;

            mnl.Matrix<double> kb = L * Emat * L.Transpose() / (thickness * area);

            mnl.Matrix<double> Tthetau = mnl.Matrix<double>.Build.Dense(0, 9);
            Tthetau = Tthetau.InsertRow(0, 1.0 / area4 * mnl.Vector<double>.Build.Dense(new double[] { x32, y32, area4, x13, y13, 0.0, x21, y21, 0.0 }));
            Tthetau = Tthetau.InsertRow(1, 1.0 / area4 * mnl.Vector<double>.Build.Dense(new double[] { x32, y32, 0.0, x13, y13, area4, x21, y21, 0.0 }));
            Tthetau = Tthetau.InsertRow(2, 1.0 / area4 * mnl.Vector<double>.Build.Dense(new double[] { x32, y32, 0.0, x13, y13, 0.0, x21, y21, area4 }));

            double ll21 = Math.Pow(x21, 2.0) + Math.Pow(y21, 2.0);
            double ll32 = Math.Pow(x32, 2.0) + Math.Pow(y32, 2.0);
            double ll13 = Math.Pow(x13, 2.0) + Math.Pow(y13, 2.0);

            mnl.Matrix<double> Te = mnl.Matrix<double>.Build.Dense(0, 3);
            Te = Te.InsertRow(0, 1.0 / (area4 * area) * mnl.Vector<double>.Build.Dense(new double[] { y23 * y13 * ll21, y31 * y21 * ll32, y12 * y32 * ll13 }));
            Te = Te.InsertRow(1, 1.0 / (area4 * area) * mnl.Vector<double>.Build.Dense(new double[] { x23 * x13 * ll21, x31 * x21 * ll32, x12 * x32 * ll13 }));
            Te = Te.InsertRow(2, 1.0 / (area4 * area) * mnl.Vector<double>.Build.Dense(new double[] { (y23 * x31 + x32 * y13) * ll21, (y31 * x12 + x13 * y21) * ll32, (y12 * x23 + x21 * y32) * ll13 }));

            mnl.Matrix<double> Q1 = mnl.Matrix<double>.Build.Dense(0, 3);
            Q1 = Q1.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { beta1, beta2, beta3 }) / ll21 * area2 / 3.0);
            Q1 = Q1.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { beta4, beta5, beta6 }) / ll32 * area2 / 3.0);
            Q1 = Q1.InsertRow(2, mnl.Vector<double>.Build.Dense(new double[] { beta7, beta8, beta9 }) / ll13 * area2 / 3.0);

            mnl.Matrix<double> Q2 = mnl.Matrix<double>.Build.Dense(0, 3);
            Q2 = Q2.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { beta9, beta7, beta8 }) / ll21 * area2 / 3.0);
            Q2 = Q2.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { beta3, beta1, beta2 }) / ll32 * area2 / 3.0);
            Q2 = Q2.InsertRow(2, mnl.Vector<double>.Build.Dense(new double[] { beta6, beta4, beta5 }) / ll13 * area2 / 3.0);

            mnl.Matrix<double> Q3 = mnl.Matrix<double>.Build.Dense(0, 3);
            Q3 = Q3.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { beta5, beta6, beta4 }) / ll21 * area2 / 3.0);
            Q3 = Q3.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { beta8, beta9, beta7 }) / ll32 * area2 / 3.0);
            Q3 = Q3.InsertRow(2, mnl.Vector<double>.Build.Dense(new double[] { beta2, beta3, beta1 }) / ll13 * area2 / 3.0); //attention Section 6 different with section 4.6!!!

            mnl.Matrix<double> Q4 = (Q1 + Q2) / 2.0;
            mnl.Matrix<double> Q5 = (Q2 + Q3) / 2.0;
            mnl.Matrix<double> Q6 = (Q3 + Q1) / 2.0;

            mnl.Matrix<double> Enat = Te.Transpose() * Emat * Te;
            mnl.Matrix<double> kTheta = 3.0 / 4.0 * beta0 * thickness * area * (Q4.Transpose() * Enat * Q4 + Q5.Transpose() * Enat * Q5 + Q6.Transpose() * Enat * Q6);
            mnl.Matrix<double> kh = Tthetau.Transpose() * kTheta * Tthetau;

            return new mnl.Matrix<double>[2] { kb, kh };
        }
    }
}
