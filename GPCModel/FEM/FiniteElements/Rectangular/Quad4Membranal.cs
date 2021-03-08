using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using mnl = MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class Quad4Membranal : Plate
    {
        public Quad4Membranal(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            //recalled base(nodes)
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!

            //Local matrix: 4 nodes x 2(dX, dY) gdl = 8x8 matrix
            //Global matrix: 4 nodes x 3(DX, DY, DZ) gdl = 12x12 matrix

            //DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //   [12x8]               [8x8]     [8x12]
        }

        public override void BuildMatrix()
        {
            //Node 1 = Origin = Node i
            //Axis x assigned as Node 1 to Node 2, Node j = Node 2
            //Axis y ortogonal to axis y, Node k = node 3

            //calculation of matrix for transformation from Local to Global coordinates
            Node[] localNodes = Quad4Element.LocalNodes(_nodesGlobal, out _localCoordinateSystem);
            Node node1 = localNodes[0];
            Node node2 = localNodes[1];
            Node node3 = localNodes[2];
            Node node4 = localNodes[3];

            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            _dofGlobalToLocal = mnl.Matrix<double>.Build.Dense(12, 8);

            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(12, 8);

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

            //local node1 y-displacement in global coordinate
            dofGlobalToLocalTranspose[0, 1] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[1, 1] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[2, 1] = localY.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode2
            //local node1 x-displacement in global coordinate
            dofGlobalToLocalTranspose[3, 2] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[4, 2] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[5, 2] = localX.DotProduct(globalZ);

            //local node1 y-displacement in global coordinate
            dofGlobalToLocalTranspose[3, 3] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[4, 3] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[5, 3] = localY.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode3
            //local node1 x-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 4] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 4] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 4] = localX.DotProduct(globalZ);

            //local node1 y-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 5] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 5] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 5] = localY.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode4
            //local node1 x-displacement in global coordinate
            dofGlobalToLocalTranspose[9, 6] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[10, 6] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[11, 6] = localX.DotProduct(globalZ);

            //local node1 y-displacement in global coordinate
            dofGlobalToLocalTranspose[9, 7] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[10, 7] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[11, 7] = localY.DotProduct(globalZ);
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

            _d = mnl.Matrix<double>.Build.Dense(3, 3);
            _d[0, 0] = 1.0;
            _d[0, 1] = ni;
            _d[1, 0] = ni;
            _d[1, 1] = 1.0;
            _d[2, 2] = (1.0 - ni) / 2.0;
            _d = E / (1.0 - ni * ni) * _d;
            /*Console.WriteLine();
            Console.WriteLine("D = " + _d.ToString());*/
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            double thk = ((PlateProperty)_property).MembraneThickness;
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(8, 8);

            GaussIntegration.GaussPoint[] gaussPoints = GaussIntegration.GetRectangularDomain(4);

            for (int i = 0; i < gaussPoints.Length; i++) //trhough the 2 gauss points
            {
                double csi = gaussPoints[i].Point.X;
                double eta = gaussPoints[i].Point.Y;

                mnl.Matrix<double> b = GetB(csi, eta);
                mnl.Matrix<double> m = b.Transpose() * _d * b;
                mnl.Matrix<double> jacob = J(csi, eta);
                    
                /*Console.WriteLine();
                Console.WriteLine("B(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2") + ")^T * D * B(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2")+"):");
                for (int row = 0; row < m.RowCount; row++)
                {
                    for (int col = 0; col < m.RowCount; col++)
                    {
                        Console.Write(m[row, col].ToString("F2") +" ");
                    }
                    Console.WriteLine();
                }*/
                Console.WriteLine("detJ("+csi.ToString("F2")+","+eta.ToString("F2")+") = " + jacob.Determinant());

                _kElementLocalCoord = _kElementLocalCoord + gaussPoints[i].Weight * m * jacob.Determinant();
            }
            _kElementLocalCoord = thk * _kElementLocalCoord;

            //Console.WriteLine("KElementLocalCoord = " + KElementLocalCoord);
            #endregion
        }

        public override Matrix<double> GetB(double csi, double eta, double zeta = 0)
        {
            /*
             * THESIS - DEVELOPMENT OF MEMBRANE, PLATE AND SHELL ELEMENTS IN JAVA
             * pg. 28
             * */


            /*
             * Matrix A
             *   [3x4]
             *      
             * epsilon_x        du/dcsi
             * epsllon_y  = a * du/deta
             * gamma_xy         dv/dcsi
             *                  dv/deta
             * 
             * a = Jacob^(-1) oppurtunamente disposto in matrice 3x4
            */
            mnl.Matrix<double> a = mnl.Matrix<double>.Build.Dense(3, 4);
            mnl.Matrix<double> j = J(csi, eta);
            a[0, 0] = j[1, 1];
            a[0, 1] = -j[0, 1];

            a[1, 2] = -j[1, 0];
            a[1, 3] = j[0, 0];

            a[2, 0] = -j[1, 0];
            a[2, 1] = j[0, 0];
            a[2, 2] = j[1, 1];
            a[2, 3] = -j[0, 1];

            a = a / j.Determinant();
            //Console.WriteLine("A=" + a);

            mnl.Matrix<double> g = mnl.Matrix<double>.Build.Dense(4, 8);
            /*
             * matrice g
             *  [4x8]
             * du/dcsi          u1
             * du/eta   = g *   v1
             * ...              ...
             * dv/deta          v4
             * 
             * g = dN1/dcsi  ... dN4/dcsi 0
             *     dNi1/deta ... dN4/deta 0
             *     ..        ...  ...     dN4/dcsi
             *     0         ...  ...     dN4/deta
             */
            g[0, 0] = dNdCsi(1, csi, eta);
            g[0, 2] = dNdCsi(2, csi, eta);
            g[0, 4] = dNdCsi(3, csi, eta);
            g[0, 6] = dNdCsi(4, csi, eta);

            g[1, 0] = dNdEta(1, csi, eta);
            g[1, 2] = dNdEta(2, csi, eta);
            g[1, 4] = dNdEta(3, csi, eta);
            g[1, 6] = dNdEta(4, csi, eta);

            g[2, 1] = dNdCsi(1, csi, eta);
            g[2, 3] = dNdCsi(2, csi, eta);
            g[2, 5] = dNdCsi(3, csi, eta);
            g[2, 7] = dNdCsi(4, csi, eta);

            g[3, 1] = dNdEta(1, csi, eta);
            g[3, 3] = dNdEta(2, csi, eta);
            g[3, 5] = dNdEta(3, csi, eta);
            g[3, 7] = dNdEta(4, csi, eta);
            /*Console.WriteLine("g=");
            for (int row = 0; row < g.RowCount; row++)
            {
                for (int col = 0; col < g.RowCount; col++)
                {
                    Console.Write(g[row, col].ToString("F4") + " ");
                }
                Console.WriteLine();
            }*/

            return a * g;
        }

        /// <summary>
        /// jacobiano:
        /// dx/dCsi, dy/dEta
        /// dy/dCsi, dy/dEta
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        private mnl.Matrix<double> J(double csi, double eta)
        {
            double j11 = 0.0;
            double j12 = 0.0;
            double j21 = 0.0;
            double j22 = 0.0;
            for (int node = 0; node < 4; node++)
            {
                int i = node + 1;
                double xi = _nodesGlobal[node].Position.X;
                double yi = _nodesGlobal[node].Position.Y;

                j11 = j11 + dNdCsi(i, csi, eta) * xi;
                j12 = j12 + dNdCsi(i, csi, eta) * yi;
                j21 = j21 + dNdEta(i, csi, eta) * xi;
                j22 = j22 + dNdEta(i, csi, eta) * yi;
            }

            mnl.Matrix<double> J = mnl.Matrix<double>.Build.Dense(2, 2);
            J[0, 0] = j11;

            J[0, 1] = j12;
            J[1, 0] = j21;

            J[1, 1] = j22;

            /*Console.WriteLine("J(csi="+csi.ToString("F2")+",eta="+eta.ToString("F2")+"="+J);
            Console.WriteLine("detJ(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2") + "=" + J.Determinant());*/

            return J;
        }

        #region ShapeFunction
        private double dNdCsi(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (eta - 1.0);
                case 2:
                    return 1.0 / 4.0 * (-eta + 1.0);
                case 3:
                    return 1.0 / 4.0 * (eta + 1.0);
                case 4:
                    return 1.0 / 4.0 * (-eta - 1.0);
                default:
                    throw new Exception();
            }
        }

        private double dNdEta(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (csi - 1.0);
                case 2:
                    return 1.0 / 4.0 * (-csi - 1.0);
                case 3:
                    return 1.0 / 4.0 * (csi + 1.0);
                case 4:
                    return 1.0 / 4.0 * (-csi + 1.0);
                default:
                    throw new Exception();
            }
        }
        #endregion

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            //TODO "aggiornare"
            
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(2 * Nodes.Length); //2 = DOF in local : DX and DY
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

        public override void GetResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            //TODO "aggiornare"
            localDisplacements = GetLocalDisplacement(globalDisplacementsNodes);
            mnl.Vector<double> vecLocalDispl = mnl.Vector<double>.Build.Dense(localDisplacements);
        
            #region CalculationOfStressAndDeformationsInLocalCoordinates
            mnl.Vector<double> epsilonLocal = GetB(0, 0) * vecLocalDispl; //epsilon_xx; epsilon_yy; epsilon_xy
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
    }
}