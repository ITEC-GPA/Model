using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class Quad4Membranal : Plate
    {
        #region variables
        Node[] _localNodes;
        #endregion

        public Quad4Membranal(Node[] nodes) : base(nodes)
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

        /// <summary>
        /// This constructor to be used ONLY for debugging purpose. Use <see cref="FiniteElement.SetProperty(ElementProperty)"/> or <see cref="FEMObject.SetId(int)"/> instead
        /// </summary>
        internal Quad4Membranal(Node[] nodes, PlateProperty property, int id) : base(nodes)
        {
            SetProperty(property);
            SetId(id);
        }

        public override void BuildMatrix()
        {
            //Node 1 = Origin = Node i
            //Axis x assigned as Node 1 to Node 2, Node j = Node 2
            //Axis y ortogonal to axis y, Node k = node 3

            //calculation of matrix for transformation from Local to Global coordinates
            _localNodes = Quad4Element.LocalNodes(_nodesGlobal, out _localCoordinateSystem);
            
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
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

            GaussIntegration.GaussPoint[] gaussPoints = GaussIntegration.GetPointsRectangular(4);

            for (int i = 0; i < gaussPoints.Length; i++) //trhough the gauss points
            {
                double csi = gaussPoints[i].Point.X;
                double eta = gaussPoints[i].Point.Y;

                mnl.Matrix<double> b = GetB(csi, eta);
                mnl.Matrix<double> m = b.Transpose() * _d * b;
                mnl.Matrix<double> jacob = Quad4Element.J(csi, eta, _localNodes);
                    
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
                //Console.WriteLine("detJ("+csi.ToString("F2")+","+eta.ToString("F2")+") = " + jacob.Determinant());

                _kElementLocalCoord = _kElementLocalCoord + gaussPoints[i].Weight * m * jacob.Determinant();
            }
            _kElementLocalCoord = thk * _kElementLocalCoord;

            //Console.WriteLine("KElementLocalCoord = " + KElementLocalCoord);
            #endregion
        }

        public override mnl.Matrix<double> GetB(double csi, double eta, double zeta = 0)
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
            mnl.Matrix<double> j = Quad4Element.J(csi, eta, _localNodes);
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
            g[0, 0] = Quad4Element.dNdCsi4nodes(1, csi, eta);
            g[0, 2] = Quad4Element.dNdCsi4nodes(2, csi, eta);
            g[0, 4] = Quad4Element.dNdCsi4nodes(3, csi, eta);
            g[0, 6] = Quad4Element.dNdCsi4nodes(4, csi, eta);

            g[1, 0] = Quad4Element.dNdEta4nodes(1, csi, eta);
            g[1, 2] = Quad4Element.dNdEta4nodes(2, csi, eta);
            g[1, 4] = Quad4Element.dNdEta4nodes(3, csi, eta);
            g[1, 6] = Quad4Element.dNdEta4nodes(4, csi, eta);

            g[2, 1] = Quad4Element.dNdCsi4nodes(1, csi, eta);
            g[2, 3] = Quad4Element.dNdCsi4nodes(2, csi, eta);
            g[2, 5] = Quad4Element.dNdCsi4nodes(3, csi, eta);
            g[2, 7] = Quad4Element.dNdCsi4nodes(4, csi, eta);

            g[3, 1] = Quad4Element.dNdEta4nodes(1, csi, eta);
            g[3, 3] = Quad4Element.dNdEta4nodes(2, csi, eta);
            g[3, 5] = Quad4Element.dNdEta4nodes(3, csi, eta);
            g[3, 7] = Quad4Element.dNdEta4nodes(4, csi, eta);
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

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            // Occorre fare integrazione sulle funzioni di forma lineari di un quad4 (è possibile usare quella dell'elemento quad4 membranale)
            // l'integrazione delle funzione di forma Ni sul dominio dell'elemento è la quota parte della forza che va nel nodo i
            //esempio: F(nodo 1 = p * integrazione(N1 dcsi deta) = somma gauss N1(csi gauss, eta gauss) * detj(csi gauss, eta guass) * weightgauss
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(2 * Nodes.Length); //2 = DOF in local : DX, DY

            double thk = ((PlateProperty)Property).MembraneThickness;
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

                    GaussIntegration.GaussPoint[] gaussPoints = GaussIntegration.GetPointsRectangular(4);
                    mnl.Matrix<double> J4nodeElement;
                    for (int i = 0; i < gaussPoints.Length; i++)
                    {
                        double csi = gaussPoints[i].Point.X;
                        double eta = gaussPoints[i].Point.Y;
                        double gaussWeight = gaussPoints[i].Weight;

                        J4nodeElement = mnl.Matrix<double>.Build.Dense(2, 2);

                        for (int j = 0; j < _localNodes.Length; j++)
                        {
                            J4nodeElement[0, 0] = J4nodeElement[0, 0] + Quad4Element.dNdCsi4nodes(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/dcsi
                            J4nodeElement[0, 1] = J4nodeElement[0, 1] + Quad4Element.dNdCsi4nodes(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/dcsi
                            J4nodeElement[1, 0] = J4nodeElement[1, 0] + Quad4Element.dNdEta4nodes(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/deta
                            J4nodeElement[1, 1] = J4nodeElement[1, 1] + Quad4Element.dNdEta4nodes(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/deta
                        }
                        double detJ = J4nodeElement.Determinant();
                        /*Console.WriteLine("N1(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(1, csi, eta));
                        Console.WriteLine("N2(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(2, csi, eta));
                        Console.WriteLine("N3(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(3, csi, eta));
                        Console.WriteLine("N4(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(4, csi, eta));
                        Console.WriteLine("F: detJ(" + csi + "," + eta + ") = " + detJ);*/

                        _fLocalCoord[0] = _fLocalCoord[0] + Quad4Element.N4nodes(1, csi, eta) * detJ * gaussWeight * px; //node1
                        _fLocalCoord[1] = _fLocalCoord[1] + Quad4Element.N4nodes(1, csi, eta) * detJ * gaussWeight * py; //node1

                        _fLocalCoord[2] = _fLocalCoord[2] + Quad4Element.N4nodes(2, csi, eta) * detJ * gaussWeight * px; //node2
                        _fLocalCoord[3] = _fLocalCoord[3] + Quad4Element.N4nodes(2, csi, eta) * detJ * gaussWeight * py; //node2

                        _fLocalCoord[4] = _fLocalCoord[4] + Quad4Element.N4nodes(3, csi, eta) * detJ * gaussWeight * px; //node3
                        _fLocalCoord[5] = _fLocalCoord[5] + Quad4Element.N4nodes(3, csi, eta) * detJ * gaussWeight * py; //node3
                    
                        _fLocalCoord[6] = _fLocalCoord[6] + Quad4Element.N4nodes(4, csi, eta) * detJ * gaussWeight * px; //node4
                        _fLocalCoord[7] = _fLocalCoord[7] + Quad4Element.N4nodes(4, csi, eta) * detJ * gaussWeight * py; //node4
                    }
                }
            }
            return _fLocalCoord;
        }

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            localDisplacements = GetLocalDisplacement(globalDisplacementsNodes);
            mnl.Vector<double> vecLocalDispl = mnl.Vector<double>.Build.Dense(localDisplacements);

            //TODO: aggiornare
            
            Point2d[] naturalCoordNodes = new Point2d[4];
            naturalCoordNodes[0] = new Point2d(-1.0, -1.0);
            naturalCoordNodes[1] = new Point2d(+1.0, -1.0);
            naturalCoordNodes[2] = new Point2d(+1.0, +1.0);
            naturalCoordNodes[3] = new Point2d(-1.0, +1.0);

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

            mnl.Vector<double>[] epsilonLocal = new mnl.Vector<double>[4];
            mnl.Vector<double>[] stressLocal = new mnl.Vector<double>[4];

            mnl.Matrix<double>[] epsilonLocalCouchy = new mnl.Matrix<double>[4];
            mnl.Matrix<double>[] stressLocalCouchy = new mnl.Matrix<double>[4];

            mnl.Matrix<double>[] epsilonGlobalCouchy = new mnl.Matrix<double>[4];
            mnl.Matrix<double>[] stressGlobalCouchy = new mnl.Matrix<double>[4];

            globalForces = new mnl.Matrix<double>[4];
            localForces = new mnl.Matrix<double>[4];

            double thickness = ((PlateProperty)_property).MembraneThickness;

            for (int i = 0; i < naturalCoordNodes.Length; i++) {
                #region CalculationOfStressAndDeformationsInLocalCoordinates
                epsilonLocal[i] = GetB(naturalCoordNodes[i].X, naturalCoordNodes[i].Y) * vecLocalDispl; //epsilon_xx; epsilon_yy; epsilon_xy
                stressLocal[i] = D * epsilonLocal[i]; //sigma_xx; sigma_yy; tau_xy
                /*Console.WriteLine("Strains in Local coordinates:" + epsilon.ToString());
                Console.WriteLine("Stress in Local coordinates:" + stress.ToString());*/
                #endregion

                #region ConvertInGlobalCoordinates
                epsilonLocalCouchy[i] = mnl.Matrix<double>.Build.Dense(3, 3);
                epsilonLocalCouchy[i][0, 0] = epsilonLocal[i][0]; //epsilon_xx
                epsilonLocalCouchy[i][1, 1] = epsilonLocal[i][1]; //epsilon_yy

                epsilonLocalCouchy[i][0, 1] = epsilonLocal[i][2]; //epsilon_xy
                epsilonLocalCouchy[i][1, 0] = epsilonLocal[i][2]; //epsilon_yx
                                                            //epsilonCouchy[2, 2] = -ni / E * (sigma_xx + sigma_yy) + alpha * Temperature ; //epsilon_zz
                                                            //Console.WriteLine("Epsilon local coordinate:" + epsilonCouchy.ToString());

                stressLocalCouchy[i] = mnl.Matrix<double>.Build.Dense(3, 3);
                stressLocalCouchy[i][0, 0] = stressLocal[i][0]; //sigma_xx
                stressLocalCouchy[i][1, 1] = stressLocal[i][1]; //sigma_yy

                stressLocalCouchy[i][0, 1] = stressLocal[i][2]; //sigma_xy
                stressLocalCouchy[i][1, 0] = stressLocal[i][2]; //sigma_yx
                                                          //Console.WriteLine("Stress local coordinate:" + stressCouchy.ToString());

                //Second order tensor -> Trotated = Q * T * Q^T
                epsilonGlobalCouchy[i] = rotation * epsilonLocalCouchy[i] * rotation.Transpose();
                //Console.WriteLine("Epsilon in global coordinates = " + epsilonGlobalCouchy);

                stressGlobalCouchy[i] = rotation * stressLocalCouchy[i] * rotation.Transpose();
                //Console.WriteLine("Stress in global coordinates = " + stressGlobalCouchy);
                #endregion

                globalForces[i] = thickness * stressGlobalCouchy[i];
                localForces[i] = thickness * stressLocalCouchy[i];
            }

            localPseudoDeformation = epsilonLocalCouchy;
            globalPseudoDeformation = epsilonGlobalCouchy;

            localStress = stressLocalCouchy;
            globalStress = stressGlobalCouchy;

            globalEpsilon = epsilonGlobalCouchy;
            localEpsilon = stressGlobalCouchy;
        }
    }
}