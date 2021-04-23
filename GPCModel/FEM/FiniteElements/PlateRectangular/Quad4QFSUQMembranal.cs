using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// A high‑performance four‑node flat shell element with drilling degrees of freedom - Hosein Sangtarash1 · Hamed G. Arab1 · Mohammad R. Sohrabi1 · Mohammad R. Ghasemi1 - 2020
    /// </summary>
    public class Quad4QFSUQMembranal : Plate
    {
        #region variables
        Node[] _localNodes;
        #endregion

        public Quad4QFSUQMembranal(Node[] nodes) : base(nodes)
        {
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!
            _DOF.Add(LinearSolver.DOF.RX);
            _DOF.Add(LinearSolver.DOF.RY);
            _DOF.Add(LinearSolver.DOF.RZ);
            //a rotation in Local coordinate plane (Rx, Ry) can be a RX, RY, RZ in Global space!

            //Local matrix: 4 nodes x 3(dX, dY, rZ) gdl = 12x12 matrix
            //Global matrix: 4 nodes x 6(DX, DY, DZ, RX, RY, RZ) gdl = 24x24 matrix

            //DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //   [24x12]           [12x12]     [12x24]
        }

        internal Quad4QFSUQMembranal(Node[] nodes, PlateProperty property) : this(nodes)
        {
            SetProperty(property);
        }

        public override void BuildMatrix()
        {
            
            //Node 1 = Origin = Node i
            //Axis x assigned as Node 1 to Node 2, Node j = Node 2
            //Axis y ortogonal to axis x, Node k = node 3

            //calculation of matrix for transformation from Local to Global coordinates
            _localNodes = Quad4Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);

            //move origin to centroid
            double xG = _localNodes.ToList().Sum(x => x.Position.X) / 4.0;
            double yG = _localNodes.ToList().Sum(x => x.Position.Y) / 4.0;

            for (int i = 0; i < _localNodes.Length; i++)
            {
                Node moved = new Node(_localNodes[i].Position.X - xG, _localNodes[i].Position.Y - yG, _localNodes[i].Position.Z, _localNodes[i].Name);
                _localNodes[i] = moved;
            }

            _localNodes.ToList().ForEach(x => Console.WriteLine(x));

            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(24, 12);

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

            //local node1 z-rotation in global coordinate
            dofGlobalToLocalTranspose[3, 2] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[4, 2] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[5, 2] = localZ.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode2
            //local node2 x-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 3] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 3] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 3] = localX.DotProduct(globalZ);

            //local node2 y-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 4] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 4] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 4] = localY.DotProduct(globalZ);

            //local node2 z-rotation in global coordinate
            dofGlobalToLocalTranspose[9, 5] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[10, 5] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[11, 5] = localZ.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode3
            //local node3 x-displacement in global coordinate
            dofGlobalToLocalTranspose[12, 6] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[13, 6] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[14, 6] = localX.DotProduct(globalZ);

            //local node3 y-displacement in global coordinate
            dofGlobalToLocalTranspose[12, 7] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[13, 7] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[14, 7] = localY.DotProduct(globalZ);

            //local node3 z-rotation in global coordinate
            dofGlobalToLocalTranspose[15, 8] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[16, 8] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[17, 8] = localZ.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode4
            //local node1 x-displacement in global coordinate
            dofGlobalToLocalTranspose[18, 9] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[19, 9] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[20, 9] = localX.DotProduct(globalZ);

            //local node1 y-displacement in global coordinate
            dofGlobalToLocalTranspose[18, 10] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[19, 10] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[20, 10] = localY.DotProduct(globalZ);

            //local node1 z-rotation in global coordinate
            dofGlobalToLocalTranspose[21, 11] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[22, 11] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[23, 11] = localZ.DotProduct(globalZ);
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

            _d = Plate.DPlaneStress(E, ni);
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            double thk = ((PlateProperty)_property).MembraneThickness;

            Func<double, double, mnl.Matrix<double>> funJacobiano = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, _localNodes);

            mnl.Matrix<double> M = MMatrix(_d, _localNodes, thk);
            Console.WriteLine("M = ");
            FEMUtilities.WriteMatrix(M,"F1");

            mnl.Matrix<double> H = HMatrix(_localNodes, thk);
            Console.WriteLine("H = ");
            FEMUtilities.WriteMatrix(H, "F2");

            Func<double, double, mnl.Matrix<double>> funBTraspLMInvH = (double csi, double eta) =>
            {
                return BMatrix(csi, eta, _localNodes).Transpose() * LMatrix(csi, eta, _localNodes) * M.Inverse() * H;
            };
            mnl.Matrix<double> k = thk * GaussIntegration.IntegrationQuadrilateral(funBTraspLMInvH, funJacobiano, 9);

            _kElementLocalCoord = k;
            Console.WriteLine("KElementLocalCoord = ");
            FEMUtilities.WriteMatrix(_kElementLocalCoord, "F2");
            #endregion
        }

        public override mnl.Matrix<double> GetB(double csi, double eta)
        {
            return BMatrix(csi, eta, _localNodes);
        }

        internal static mnl.Matrix<double> BMatrix(double csi, double eta, Node[] nodes)
        {
            mnl.Matrix<double> jacob = FEMUtilities.Jacob2D(csi, eta, LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nodes);
            mnl.Matrix<double> invJacob = jacob.Inverse();

            Point2d p = GetXY(csi, eta, nodes);
            double x = p.X;
            double y = p.Y;
            
            mnl.Matrix<double> B = mnl.Matrix<double>.Build.Dense(3, 0);
            for (int i = 1; i <= nodes.Length; i++)
            {
                //trasnformation of dN/ dLocal to dN/ dNatural : dNdLocal = J ^ -1 * dNdNatural
                mnl.Vector<double> dNidLocal = mnl.Vector<double>.Build.Dense(2);
                mnl.Vector<double> dNidNatural = mnl.Vector<double>.Build.Dense(2);

                dNidNatural[0] = LinearShapeFunctionQuad4.DNdCsi(i, csi, eta);
                dNidNatural[1] = LinearShapeFunctionQuad4.DNdEta(i, csi, eta);
                dNidLocal = invJacob * dNidNatural;

                double dNidX = dNidLocal[0];
                double dNidY = dNidLocal[1];

                double xi = nodes[i - 1].Position.X;
                double yi = nodes[i - 1].Position.Y;

                mnl.Matrix<double> Bi = mnl.Matrix<double>.Build.Dense(3, 3);
                Bi[0, 0] = dNidX;
                Bi[0, 2] = -2.0 / 3.0 * dNidX * (y - yi);

                Bi[1, 1] = dNidY;
                Bi[1, 2] = 2.0 / 3.0 * dNidY * (x - xi);

                Bi[2, 0] = dNidY;
                Bi[2, 1] = dNidX;
                Bi[2, 2] = 2.0 / 3.0 * (dNidX * (x - xi) - dNidY * (y - yi));

                B = B.Append(Bi);
            }

            return B;
        }

        internal static Point2d GetXY(double csi, double eta, Node[] nodes)
        {
            double x = 0;
            double y = 0;
            for (int i = 1; i <= nodes.Length; i++)
            {
                x = x + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodes[i - 1].Position.X;
                y = y + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodes[i - 1].Position.Y;
            }

            return new Point2d(x, y);
        }

        internal static mnl.Matrix<double> LMatrix(double csi, double eta, Node[] nodes)
        {
            Point2d p = GetXY(csi, eta, nodes);
            double x = p.X;
            double y = p.Y;

            mnl.Matrix<double> L = mnl.Matrix<double>.Build.Dense(3, 11);
            L[0, 2] = 2.0;
            L[0, 5] = 2.0 * x;
            L[0, 6] = 6.0 * y;
            L[0, 8] = 6.0 * x * y;
            L[0, 9] = -12.0 * y * y;
            L[0, 10] = 12.0 * (x * x - y * y);

            L[1, 0] = 2.0;
            L[1, 3] = 6.0 * x;
            L[1, 4] = 2.0 * y;
            L[1, 7] = 6.0 * x * y;
            L[1, 9] = 12.0 * x * x;
            L[1, 10] = -12.0 * (x * x - y * y);

            L[2, 1] = -1.0;
            L[2, 4] = -2.0 * x;
            L[2, 5] = -2.0 * y;
            L[2, 7] = -3.0 * x * x;
            L[2, 9] = -3.0 * y * y;
            L[2, 10] = -24.0 * x * y;

            return L;
        }

        internal static mnl.Matrix<double> MMatrix(mnl.Matrix<double> D, Node[] nodes, double thickness)
        {
            Func<double, double, mnl.Matrix<double>> funJacobiano = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nodes);

            Func<double, double, mnl.Matrix<double>> funcM = (double csi, double eta) =>
            {
                return LMatrix(csi, eta, nodes).Transpose() * D.Inverse() * LMatrix(csi, eta, nodes);
            };

            mnl.Matrix<double> M = thickness * GaussIntegration.IntegrationQuadrilateral(funcM, funJacobiano, 9);
            
            return M;
        }

        internal static mnl.Matrix<double> HMatrix(Node[] nodes, double thickness)
        {
            Func<double, double, mnl.Matrix<double>> funJacobiano = FEMUtilities.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nodes);

            Func<double, double, mnl.Matrix<double>> funcH = (double csi, double eta) =>
            {
                return LMatrix(csi, eta, nodes).Transpose() * BMatrix(csi, eta, nodes);
            };

            mnl.Matrix<double> H = thickness * GaussIntegration.IntegrationQuadrilateral(funcH, funJacobiano, 9);

            return H;
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            // Occorre fare integrazione sulle funzioni di forma lineari di un quad4 (è possibile usare quella dell'elemento quad4 membranale)
            // l'integrazione delle funzione di forma Ni sul dominio dell'elemento è la quota parte della forza che va nel nodo i
            //esempio: F(nodo 1 = p * integrazione(N1 dcsi deta) = somma gauss N1(csi gauss, eta gauss) * detj(csi gauss, eta guass) * weightgauss
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(3 * Nodes.Length); //3 = DOF in local : DX, DY, RZ

            /*double thk = ((PlateProperty)Property).MembraneThickness;
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
                            J4nodeElement[0, 0] = J4nodeElement[0, 0] + LinearShapeFunctionQuad4.DNdCsi4nodes(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/dcsi
                            J4nodeElement[0, 1] = J4nodeElement[0, 1] + LinearShapeFunctionQuad4.DNdCsi4nodes(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/dcsi
                            J4nodeElement[1, 0] = J4nodeElement[1, 0] + LinearShapeFunctionQuad4.DNdEta4nodes(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/deta
                            J4nodeElement[1, 1] = J4nodeElement[1, 1] + LinearShapeFunctionQuad4.DNdEta4nodes(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/deta
                        }
                        double detJ = J4nodeElement.Determinant();
                        /*Console.WriteLine("N1(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(1, csi, eta));
                        Console.WriteLine("N2(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(2, csi, eta));
                        Console.WriteLine("N3(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(3, csi, eta));
                        Console.WriteLine("N4(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(4, csi, eta));
                        Console.WriteLine("F: detJ(" + csi + "," + eta + ") = " + detJ);*/

                        /*_fLocalCoord[0] = _fLocalCoord[0] + Quad4Element.N4nodes(1, csi, eta) * detJ * gaussWeight * px; //node1
                        _fLocalCoord[1] = _fLocalCoord[1] + Quad4Element.N4nodes(1, csi, eta) * detJ * gaussWeight * py; //node1

                        _fLocalCoord[2] = _fLocalCoord[2] + Quad4Element.N4nodes(2, csi, eta) * detJ * gaussWeight * px; //node2
                        _fLocalCoord[3] = _fLocalCoord[3] + Quad4Element.N4nodes(2, csi, eta) * detJ * gaussWeight * py; //node2

                        _fLocalCoord[4] = _fLocalCoord[4] + Quad4Element.N4nodes(3, csi, eta) * detJ * gaussWeight * px; //node3
                        _fLocalCoord[5] = _fLocalCoord[5] + Quad4Element.N4nodes(3, csi, eta) * detJ * gaussWeight * py; //node3
                    
                        _fLocalCoord[6] = _fLocalCoord[6] + Quad4Element.N4nodes(4, csi, eta) * detJ * gaussWeight * px; //node4
                        _fLocalCoord[7] = _fLocalCoord[7] + Quad4Element.N4nodes(4, csi, eta) * detJ * gaussWeight * py; //node4
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

            mnl.Vector<double>[] epsilonLocal = new mnl.Vector<double>[4]; //4 = nr of points
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