using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// 1993 - GENERALIZED CONFORMING QUADRILATERAL - YUQIU YIN
    /// </summary>
    public class Quad4GQ12Membranal : Plate
    {
        #region variables
        Node[] _localNodes;
        #endregion

        public Quad4GQ12Membranal(Node[] nodes) : base(nodes)
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

        internal Quad4GQ12Membranal(Node[] nodes, PlateProperty property) : this(nodes)
        {
            SetProperty(property);
        }

        public override void BuildMatrix()
        {
            
            //Node 1 = Origin = Node i
            //Axis x assigned as Node 1 to Node 2, Node j = Node 2
            //Axis y ortogonal to axis x, Node k = node 3

            //calculation of matrix for transformation from Local to Global coordinates
            _localNodes = Quad4Element.GetLocalNodesFromCentroid(_nodesGlobal, out _localCoordinateSystem);

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

            Func<double, double, mnl.Matrix<double>> funJacobiano = Util.J2D(LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, _localNodes);

            Func<double, double, mnl.Matrix<double>> BTraspDB = (double csi, double eta) =>
            {
                mnl.Matrix<double> B = BMatrix(csi, eta, _localNodes);
                return B.Transpose() * _d * B;
            };
            mnl.Matrix<double> k = thk * GaussIntegration.IntegrationQuadrilateral(BTraspDB, funJacobiano, 9);

            _kElementLocalCoord = k;
            /*Console.WriteLine("KElementLocalCoord = ");
            Util.WriteMatrix(_kElementLocalCoord, "F2");*/
            #endregion
        }

        public override mnl.Matrix<double> GetB(double csi, double eta, double zeta = 0)
        {
            return BMatrix(csi, eta, _localNodes);
        }

        internal static mnl.Matrix<double> BMatrix(double csi, double eta, Node[] nodes)
        {
            Func<double, double, mnl.Matrix<double>> fJacob = (double varCsi, double varEta) =>
            {
                return Util.Jacob2D(varCsi, varEta, LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, nodes);
            };
            
            mnl.Matrix<double> B = mnl.Matrix<double>.Build.Dense(3, 0);
            for (int i = 1; i <= nodes.Length; i++)
            {
                var dNdCsi = Util.FFirstFix<int, double, double, double>(i, LinearShapeFunctionQuad4.DNdCsi);
                var dNdEta = Util.FFirstFix<int, double, double, double>(i, LinearShapeFunctionQuad4.DNdEta);


                Func<double, double, double> fdNuThetadCsi = (double varCsi, double varEta) =>
                {
                    return DNuThetadCsi(i, varCsi, varEta, nodes); //fix "i" and "nodes"
                };

                Func<double, double, double> fdNvThetadCsi = (double varCsi, double varEta) =>
                {
                    return DNvThetadCsi(i, varCsi, varEta, nodes); //fix "i" and "nodes"
                };

                Func<double, double, double> fdNuThetadEta = (double varCsi, double varEta) =>
                {
                    return DNuThetadEta(i, varCsi, varEta, nodes); //fix "i" and "nodes"
                };

                Func<double, double, double> fdNvThetadEta = (double varCsi, double varEta) =>
                {
                    return DNvThetadEta(i, varCsi, varEta, nodes); //fix "i" and "nodes"
                };

                mnl.Vector<double> dNidLocal = Util.GetdNdLocalFromdNdNatural2D(csi, eta, dNdCsi, dNdEta, fJacob);
                double dNidX = dNidLocal[0];
                double dNidY = dNidLocal[1];

                mnl.Vector<double> dNuThetadLocal = Util.GetdNdLocalFromdNdNatural2D(csi, eta, fdNuThetadCsi, fdNuThetadEta, fJacob);
                double dNuThetadX = dNuThetadLocal[0];
                double dNuThetadY = dNuThetadLocal[1];

                mnl.Vector<double> dNvThetadLocal = Util.GetdNdLocalFromdNdNatural2D(csi, eta, fdNvThetadCsi, fdNvThetadEta, fJacob);
                double dNvThetadX = dNvThetadLocal[0];
                double dNvThetadY = dNvThetadLocal[1];

                mnl.Matrix<double> Bi = mnl.Matrix<double>.Build.Dense(3, 3);
                Bi[0, 0] = dNidX;
                Bi[0, 2] = dNuThetadX;

                Bi[1, 1] = dNidY;
                Bi[1, 2] = dNvThetadY;

                Bi[2, 0] = dNidY;
                Bi[2, 1] = dNidX;
                Bi[2, 2] = dNuThetadY + dNvThetadX;

                B = B.Append(Bi);
            }

            return B;
        }

        public static double DNuThetadCsi(int index, double csi, double eta, Node[] nodes)
        {
            double b1 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.Y + (+1.0) * nodes[2 - 1].Position.Y + (+1.0) * nodes[3 - 1].Position.Y + (-1) * nodes[4 - 1].Position.Y);
            double b2 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.Y + (-1.0) * nodes[2 - 1].Position.Y + (+1.0) * nodes[3 - 1].Position.Y + (+1) * nodes[4 - 1].Position.Y);
            double b3 = 1.0 / 4.0 * ((-1.0)*(-1.0) * nodes[1 - 1].Position.Y + (+1.0)*(-1.0) * nodes[2 - 1].Position.Y + (+1.0)*(+1.0) * nodes[3 - 1].Position.Y + (-1)*(+1.0) * nodes[4 - 1].Position.Y);

            double a = 0.0;
            double b = 0.0;
            switch (index)
            {
                case 1:
                    a = (b1 + b3 * (-1.0)); //eta_i
                    b = (b2 + b3 * (-1.0)); //csi_i
                    return -1.0 / 8.0 * (eta - 1.0) * (2.0 * a * csi + b * eta + b);
                case 2:
                    a = (b1 + b3 * (-1.0)); //eta_i
                    b = (b2 + b3 * (+1.0)); //csi_i
                    return 1.0 / 8.0 * (eta - 1.0) * (2.0 * a * csi + b * eta + b);
                case 3:
                    a = (b1 + b3 * (+1.0)); //eta_i
                    b = (b2 + b3 * (+1.0)); //csi_i
                    return -1.0 / 8.0 * (eta + 1.0) * (2.0 * a *csi + b * (eta - 1.0));
                case 4:
                    a = (b1 + b3 * (+1.0)); //eta_i
                    b = (b2 + b3 * (-1.0)); //csi_i
                    return 1.0 / 8.0 * (eta + 1.0) * (2.0 * a * csi + b * (eta - 1.0));
                default:
                    throw new ArgumentException("indice compreso tra 1 e 4");
            }
        }

        public static double DNuThetadEta(int index, double csi, double eta, Node[] nodes)
        {
            double b1 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.Y + (+1.0) * nodes[2 - 1].Position.Y + (+1.0) * nodes[3 - 1].Position.Y + (-1) * nodes[4 - 1].Position.Y);
            double b2 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.Y + (-1.0) * nodes[2 - 1].Position.Y + (+1.0) * nodes[3 - 1].Position.Y + (+1) * nodes[4 - 1].Position.Y);
            double b3 = 1.0 / 4.0 * ((-1.0) * (-1.0) * nodes[1 - 1].Position.Y + (+1.0) * (-1.0) * nodes[2 - 1].Position.Y + (+1.0) * (+1.0) * nodes[3 - 1].Position.Y + (-1) * (+1.0) * nodes[4 - 1].Position.Y);

            double a;
            double b;
            switch (index)
            {
                case 1:
                    a = (b1 + b3 * (-1.0)); //eta_i
                    b = (b2 + b3 * (-1.0)); //csi_i
                    return -1.0 / 8.0 * (csi - 1.0) * (a * csi + a + 2.0 * b * eta);
                case 2:
                    a = (b1 + b3 * (-1.0)); //eta_i
                    b = (b2 + b3 * (+1.0)); //csi_i
                    return 1.0 / 8.0 * (csi + 1.0) * (a * (csi - 1.0) + 2.0 * b * eta);
                case 3:
                    a = (b1 + b3 * (+1.0)); //eta_i
                    b = (b2 + b3 * (+1.0)); //csi_i
                    return -1.0 / 8.0 * (csi + 1.0) * (a * (csi - 1.0) + 2.0 * b * eta);
                case 4:
                    a = (b1 + b3 * (+1.0)); //eta_i
                    b = (b2 + b3 * (-1.0)); //csi_i
                    return 1.0 / 8.0 * (csi - 1.0) * (a * csi + a + 2.0 * b * eta);
                default:
                    throw new ArgumentException("indice compreso tra 1 e 4");
            }
        }

        public static double DNvThetadCsi(int index, double csi, double eta, Node[] nodes)
        {
            double a1 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.X + (+1.0) * nodes[2 - 1].Position.X + (+1.0) * nodes[3 - 1].Position.X + (-1) * nodes[4 - 1].Position.X);
            double a2 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.X + (-1.0) * nodes[2 - 1].Position.X + (+1.0) * nodes[3 - 1].Position.X + (+1) * nodes[4 - 1].Position.X);
            double a3 = 1.0 / 4.0 * ((-1.0) * (-1.0) * nodes[1 - 1].Position.X + (+1.0) * (-1.0) * nodes[2 - 1].Position.X + (+1.0) * (+1.0) * nodes[3 - 1].Position.X + (-1) * (+1.0) * nodes[4 - 1].Position.X);

            double a = 0.0;
            double b = 0.0;
            switch (index)
            {
                case 1:
                    a = (a1 + a3 * (-1.0)); //eta_i
                    b = (a2 + a3 * (-1.0)); //csi_i
                    return 1.0 / 8.0 * (eta - 1.0) * (2.0 * a * csi + b * eta + b);
                case 2:
                    a = (a1 + a3 * (-1.0)); //eta_i
                    b = (a2 + a3 * (+1.0)); //csi_i
                    return -1.0 / 8.0 * (eta - 1.0) * (2.0 * a * csi + b * eta + b);
                case 3:
                    a = (a1 + a3 * (+1.0)); //eta_i
                    b = (a2 + a3 * (+1.0)); //csi_i
                    return 1.0 / 8.0 * (eta + 1.0) * (2.0 * a * csi + b * (eta - 1.0));
                case 4:
                    a = (a1 + a3 * (+1.0)); //eta_i
                    b = (a2 + a3 * (-1.0)); //csi_i
                    return -1.0 / 8.0 * (eta + 1.0) * (2.0 * a * csi + b * (eta - 1.0));
                default:
                    throw new ArgumentException("indice compreso tra 1 e 4");
            }
        }

        public static double DNvThetadEta(int index, double csi, double eta, Node[] nodes)
        {
            double a1 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.X + (+1.0) * nodes[2 - 1].Position.X + (+1.0) * nodes[3 - 1].Position.X + (-1) * nodes[4 - 1].Position.X);
            double a2 = 1.0 / 4.0 * ((-1.0) * nodes[1 - 1].Position.X + (-1.0) * nodes[2 - 1].Position.X + (+1.0) * nodes[3 - 1].Position.X + (+1) * nodes[4 - 1].Position.X);
            double a3 = 1.0 / 4.0 * ((-1.0) * (-1.0) * nodes[1 - 1].Position.X + (+1.0) * (-1.0) * nodes[2 - 1].Position.X + (+1.0) * (+1.0) * nodes[3 - 1].Position.X + (-1) * (+1.0) * nodes[4 - 1].Position.X);

            double a = 0.0;
            double b = 0.0;
            switch (index)
            {
                case 1:
                    a = (a1 + a3 * (-1.0)); //eta_i
                    b = (a2 + a3 * (-1.0)); //csi_i
                    return 1.0 / 8.0 * (csi - 1.0) * (a * csi + a + 2.0 * b * eta);
                case 2:
                    a = (a1 + a3 * (-1.0)); //eta_i
                    b = (a2 + a3 * (+1.0)); //csi_i
                    return -1.0 / 8.0 * (csi + 1.0) * (a * (csi - 1.0) + 2.0 * b * eta);
                case 3:
                    a = (a1 + a3 * (+1.0)); //eta_i
                    b = (a2 + a3 * (+1.0)); //csi_i
                    return 1.0 / 8.0 * (csi + 1.0) * (a * (csi - 1.0) + 2.0 * b * eta);
                case 4:
                    a = (a1 + a3 * (+1.0)); //eta_i
                    b = (a2 + a3 * (-1.0)); //csi_i
                    return -1.0 / 8.0 * (csi - 1.0) * (a * csi + a + 2.0 * b * eta);
                default:
                    throw new ArgumentException("indice compreso tra 1 e 4");
            }
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