using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Fem.Materials;
using GPC.Model.Fem.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Fem.FiniteElements
{
    /// <summary>
    /// Based of "A study of optimal membrane triangles with drilling freedoms" - Felippa - 2003
    /// </summary>
    public class Tri3OPTMembrane : Plate
    {
        #region variables
        Node[] _localNodes;
        double _areaElement;
        double _thickness;

        mnl.Matrix<double> _L;
        mnl.Matrix<double> _Te;
        mnl.Matrix<double> _Q1;
        mnl.Matrix<double> _Q2;
        mnl.Matrix<double> _Q3;
        mnl.Matrix<double> _Tthetau;
        #endregion

        public Tri3OPTMembrane(Node[] nodes) : base(nodes)
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

        /// <summary>
        /// This constructor to be used ONLY for debugging purpose. Use <see cref="FiniteElement.SetProperty(ElementProperty)"/> or <see cref="FemObject.SetId(int)"/> instead
        /// </summary>
        internal Tri3OPTMembrane(Node[] nodes, PlateProperty property) : this(nodes)
        {
            SetProperty(property);
        }

        public override void BuildMatrix()
        {
            _thickness = ((PlateProperty)_property).MembraneThickness;

            #region matrixD
            double ni = ((IsotropicFemMaterial)((PlateProperty)_property).Material).Ni;

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

            #region ShapeFuction
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            double x12 = _localNodes[1 - 1].Position.X - _localNodes[2 - 1].Position.X;
            double x23 = _localNodes[2 - 1].Position.X - _localNodes[3 - 1].Position.X;
            double x31 = _localNodes[3 - 1].Position.X - _localNodes[1 - 1].Position.X;
            double x21 = -x12;
            double x32 = -x23;
            double x13 = -x31;

            double y12 = _localNodes[1 - 1].Position.Y - _localNodes[2 - 1].Position.Y;
            double y23 = _localNodes[2 - 1].Position.Y - _localNodes[3 - 1].Position.Y;
            double y31 = _localNodes[3 - 1].Position.Y - _localNodes[1 - 1].Position.Y;
            double y21 = -y12;
            double y32 = -y23;
            double y13 = -y31;

            _areaElement = (y21 * x13 - x21 * y13) / 2.0;
            Console.WriteLine("element area " + _areaElement);

            #region SHOULDBEUSEDFORNONISOTROPMATERIAL

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

            //Coefficient for OPTimal element
            double alphab = 3.0 / 2.0;
            double beta0 = Math.Max(1.0 / 2.0 * (1.0 - 4.0 * ni * ni), 0.01);//Non isotrop material: Math.Max(2.0 / e11C11avg - 3.0 / 2.0, 0.01);
            double beta1 = 1.0;
            double beta2 = 2.0;
            double beta3 = 1.0;
            double beta4 = 0.0;
            double beta5 = 1.0;
            double beta6 = -1.0;
            double beta7 = -1.0;
            double beta8 = -1.0;
            double beta9 = -2.0;

            _L = mnl.Matrix<double>.Build.Dense(0, 3);
            _L = _L.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { y23, 0.0, x32 }));
            _L = _L.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { 0.0, x32, y23 }));
            _L = _L.InsertRow(2, alphab / 6.0 * mnl.Vector<double>.Build.Dense(new double[] { y23 * (y13 - y21), x32 * (x31 - x12), (x31 * y13 - x12 * y21) * 2.0 }));
            _L = _L.InsertRow(3, mnl.Vector<double>.Build.Dense(new double[] { y31, 0.0, x13 }));
            _L = _L.InsertRow(4, mnl.Vector<double>.Build.Dense(new double[] { 0.0, x13, y31 }));
            _L = _L.InsertRow(5, alphab / 6.0 * mnl.Vector<double>.Build.Dense(new double[] { y31 * (y21 - y32), x13 * (x12 - x23), (x12 * y21 - x23 * y32) * 2.0 }));
            _L = _L.InsertRow(6, mnl.Vector<double>.Build.Dense(new double[] { y12, 0.0, x21 }));
            _L = _L.InsertRow(7, mnl.Vector<double>.Build.Dense(new double[] { 0.0, x21, y12 }));
            _L = _L.InsertRow(8, alphab / 6.0 * mnl.Vector<double>.Build.Dense(new double[] { y12 * (y32 - y13), x21 * (x23 - x31), (x23 * y32 - x31 * y13) * 2.0 }));

            //Console.WriteLine("2 / h * L = " + _L);

            _L = _L * _thickness / 2.0;

            mnl.Matrix<double> kb = _L * D * _L.Transpose() / (_thickness * _areaElement);

            /*Console.WriteLine("kb = ");
            for (int r = 0; r < kb.RowCount; r++)
            {
                for (int c = 0; c < kb.ColumnCount; c++)
                {
                    Console.Write(kb[r, c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }*/

            _Tthetau = mnl.Matrix<double>.Build.Dense(0, 9);
            _Tthetau = _Tthetau.InsertRow(0, 1.0 / (4.0 * _areaElement) * mnl.Vector<double>.Build.Dense(new double[] { x32, y32, 4.0 * _areaElement, x13, y13, 0.0, x21, y21, 0.0 }));
            _Tthetau = _Tthetau.InsertRow(1, 1.0 / (4.0 * _areaElement) * mnl.Vector<double>.Build.Dense(new double[] { x32, y32, 0.0, x13, y13, 4.0 * _areaElement, x21, y21, 0.0 }));
            _Tthetau = _Tthetau.InsertRow(2, 1.0 / (4.0 * _areaElement) * mnl.Vector<double>.Build.Dense(new double[] { x32, y32, 0.0, x13, y13, 0.0, x21, y21, 4.0 * _areaElement }));

            double ll21 = Math.Pow(x21, 2.0) + Math.Pow(y21, 2.0);
            double ll32 = Math.Pow(x32, 2.0) + Math.Pow(y32, 2.0);
            double ll13 = Math.Pow(x13, 2.0) + Math.Pow(y13, 2.0);

            _Te = mnl.Matrix<double>.Build.Dense(0, 3);
            _Te = _Te.InsertRow(0, 1.0 / (4.0 * _areaElement * _areaElement) * mnl.Vector<double>.Build.Dense(new double[] { y23 * y13 * ll21, y31 * y21 * ll32, y12 * y32 * ll13 }));
            _Te = _Te.InsertRow(1, 1.0 / (4.0 * _areaElement * _areaElement) * mnl.Vector<double>.Build.Dense(new double[] { x23 * x13 * ll21, x31 * x21 * ll32, x12 * x32 * ll13 }));
            _Te = _Te.InsertRow(2, 1.0 / (4.0 * _areaElement * _areaElement) * mnl.Vector<double>.Build.Dense(new double[] { (y23 * x31 + x32 * y13) * ll21, (y31 * x12 + x13 * y21) * ll32, (y12 * x23 + x21 * y32) * ll13 }));

            _Q1 = mnl.Matrix<double>.Build.Dense(0, 3);
            _Q1 = _Q1.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { beta1, beta2, beta3 }) / ll21 * 2.0 * _areaElement / 3.0);
            _Q1 = _Q1.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { beta4, beta5, beta6 }) / ll32 * 2.0 * _areaElement / 3.0);
            _Q1 = _Q1.InsertRow(2, mnl.Vector<double>.Build.Dense(new double[] { beta7, beta8, beta9 }) / ll13 * 2.0 * _areaElement / 3.0);

            _Q2 = mnl.Matrix<double>.Build.Dense(0, 3);
            _Q2 = _Q2.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { beta9, beta7, beta8 }) / ll21 * 2.0 * _areaElement / 3.0);
            _Q2 = _Q2.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { beta3, beta1, beta2 }) / ll32 * 2.0 * _areaElement / 3.0);
            _Q2 = _Q2.InsertRow(2, mnl.Vector<double>.Build.Dense(new double[] { beta6, beta4, beta5 }) / ll13 * 2.0 * _areaElement / 3.0);

            _Q3 = mnl.Matrix<double>.Build.Dense(0, 3);
            _Q3 = _Q3.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { beta5, beta6, beta4 }) / ll21 * 2.0 * _areaElement / 3.0);
            _Q3 = _Q3.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { beta8, beta9, beta7 }) / ll32 * 2.0 * _areaElement / 3.0);
            _Q3 = _Q3.InsertRow(2, mnl.Vector<double>.Build.Dense(new double[] { beta2, beta3, beta1 }) / ll13 * 2.0 * _areaElement / 3.0); //attention Section 6 different with section 4.6!!! beta2, beta3, beta1 

            mnl.Matrix<double> Q4 = (_Q1 + _Q2) / 2.0;
            mnl.Matrix<double> Q5 = (_Q2 + _Q3) / 2.0;
            mnl.Matrix<double> Q6 = (_Q3 + _Q1) / 2.0;

            mnl.Matrix<double> Enat = _Te.Transpose() * D * _Te;
            mnl.Matrix<double> kTheta = 3.0 / 4.0 * beta0 * _thickness * _areaElement * (Q4.Transpose() * Enat * Q4 + Q5.Transpose() * Enat * Q5 + Q6.Transpose() * Enat * Q6);
            mnl.Matrix<double> kh = _Tthetau.Transpose() * kTheta * _Tthetau;

            /*Console.WriteLine("kh = ");
            for (int r = 0; r < kh.RowCount; r++)
            {
                for (int c = 0; c < kh.ColumnCount; c++)
                {
                    Console.Write(kh[r, c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }*/

            _kElementLocalCoord = kb + kh; //k basic stiffness + k higher order stiffness (drilling)
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

        public mnl.Matrix<double> GetB(double x, double y)
        {
            /*
             * csi1, csi2 and csi3 are defined in 
             * Membrane Trinagles with corner drilling freedoms - I The EFF element - Felippa - 1992
             */

            Func<int, double, double, double> csi = delegate (int nr, double xLocal, double yLocal)
            {
                double x0 = _localNodes.ToList().Select(el => el.Position.X).Sum() / 3.0;//centroid
                double y0 = _localNodes.ToList().Select(el => el.Position.Y).Sum() / 3.0;//centroid

                int i = nr - 1;
                int j = i + 1;
                int k = i + 2;

                if (j > 2)
                {
                    j = j - 3;
                }
                if (k > 2)
                {
                    k = k - 3;
                }

                double xi = _localNodes[i].Position.X;
                double yj = _localNodes[j].Position.Y;
                double yk = _localNodes[k].Position.Y;
                double yjk = _localNodes[j].Position.Y - _localNodes[k].Position.Y;
                double xkj = _localNodes[k].Position.X - _localNodes[k].Position.X;

                return 1.0 / (2.0 * _areaElement) * (xi * yk - xkj * yj + (xLocal - x0) * yjk + (yLocal - y0) * xkj);
            };

            double beta0e = 3.0 / 2.0; //recoomend for isotropic material
            return _L.Transpose() / (_areaElement * _thickness) + _Te * beta0e * (_Q1 * csi(1, x, y) + _Q2 * csi(2, x, y) + _Q3 * csi(3, x, y)) * _Tthetau;
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
            globalForces = stressGlobalCouchy.Select(stressGlobalCouchyElement => thickness * stressGlobalCouchyElement).ToArray();
            localForces = stressLocalCouchy.Select(stressLocalCouchyElement => thickness * stressLocalCouchyElement).ToArray();

            globalEpsilon = epsilonGlobalCouchy;
            localEpsilon = stressGlobalCouchy;
        }
    }
}
