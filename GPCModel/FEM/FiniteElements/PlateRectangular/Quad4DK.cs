using System;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Discrete Kirchoff Rectangular - Evaluation of new quadrilateral thin plate bending element - Jean-Louis Batoz
    /// International Jurnal for numerical methods in engineering, vol 18, 1655-1977 (1982)
    /// </summary>
    public class Quad4DK : Plate
    {
        #region variables
        //Differences of coordinates used for Matrix B and KLocal
        double _x12;
        double _y12;

        double _x21;
        double _y21;

        double _x23;
        double _y23;

        double _x34;
        double _y34;

        double _x32;
        double _y32;

        double _x41;
        double _y41;

        double _x31;
        double _y31;

        double _x42;
        double _y42;

        Node[] _localNodes;
        #endregion

        public Quad4DK(Node[] nodes) : base(nodes)
        {
            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            //displacement w il local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);

            #region DebugDerivativesOfShapeFunctions
            //Just for debug
            /*
            double c = -1.0 / Math.Pow(3, 0.5);
            double e = -1.0 / Math.Pow(3, 0.5);
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n"+i+ ",csi(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + " = " + dNdCsi(i,c,e).ToString("F2"));
            }
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n" + i + ",eta(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + ") = " + dNdEta(i, c, e).ToString("F2"));
            }
            Console.WriteLine();

            c = -1.0 / Math.Pow(3, 0.5);
            e = 1.0 / Math.Pow(3, 0.5);
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n" + i + ",csi(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + " = " + dNdCsi(i, c, e).ToString("F2"));
            }
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n" + i + ",eta(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + " = " + dNdEta(i, c, e).ToString("F2"));
            }
            Console.WriteLine();

            c = +1.0 / Math.Pow(3, 0.5);
            e = -1.0 / Math.Pow(3, 0.5);
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n" + i + ",csi(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + " = " + dNdCsi(i, c, e).ToString("F2"));
            }
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n" + i + ",eta(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + " = " + dNdEta(i, c, e).ToString("F2"));
            }
            Console.WriteLine();

            c = 1.0 / Math.Pow(3, 0.5);
            e = 1.0 / Math.Pow(3, 0.5);
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n" + i + ",csi(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + " = " + dNdCsi(i, c, e).ToString("F2"));
            }
            for (int i = 1; i <= 8; i++)
            {
                Console.WriteLine("n" + i + ",eta(csi = " + c.ToString("F2") + ",eta = " + e.ToString("F2") + " = " + dNdEta(i, c, e).ToString("F2"));
            }
            Console.WriteLine();
            */
            #endregion
        }

        public override void BuildMatrix()
        {
            //set local coordinate system
            _localNodes = Quad4Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);
            Node node1 = _localNodes[0];
            Node node2 = _localNodes[1];
            Node node3 = _localNodes[2];
            Node node4 = _localNodes[3];

            _x12 = node1.Position.X - node2.Position.X;
            _y12 = node1.Position.Y - node2.Position.Y;
            
            _x21 = node2.Position.X - node1.Position.X;
            _y21 = node2.Position.Y - node1.Position.Y;

            _x23 = node2.Position.X - node3.Position.X;
            _y23 = node2.Position.Y - node3.Position.Y;
            
            _x34 = node3.Position.X - node4.Position.X;
            _y34 = node3.Position.Y - node4.Position.Y;
            
            _x32 = node3.Position.X - node2.Position.X;
            _y32 = node3.Position.Y - node2.Position.Y;

            _x41 = node4.Position.X - node1.Position.X;
            _y41 = node4.Position.Y - node1.Position.Y;
           
            _x31 = node3.Position.X - node1.Position.X;
            _y31 = node3.Position.Y - node1.Position.Y;

            _x42 = node4.Position.X - node2.Position.X;
            _y42 = node4.Position.Y - node2.Position.Y;

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(24, 12);

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

            #region localToGlobalNode4
            //local node3 z-displacement in global coordinate
            dofGlobalToLocalTranspose[18, 9] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[19, 9] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[20, 9] = localZ.DotProduct(globalZ);

            //local node3 rx-rotation and ry in global coordinate
            dofGlobalToLocalTranspose[21, 10] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[21, 11] = localY.DotProduct(globalX);

            dofGlobalToLocalTranspose[22, 10] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[22, 11] = localY.DotProduct(globalY);

            dofGlobalToLocalTranspose[23, 10] = localX.DotProduct(globalZ);
            dofGlobalToLocalTranspose[23, 11] = localY.DotProduct(globalZ);
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
            double tb = ((PlateProperty)Property).BendingThickness;

            _d = mnl.Matrix<double>.Build.Dense(3, 3);
            _d[0, 0] = 1.0;
            _d[0, 1] = ni;
            _d[1, 0] = ni;
            _d[1, 1] = 1.0;
            _d[2, 2] = (1.0 - ni) / 2.0;
            _d = E * Math.Pow(tb, 3.0) / (12.0 * (1.0 - ni * ni)) * _d; //flexural rigidity
            //Console.WriteLine("Db = " + _d.ToString());
            #endregion

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(12, 12);
            GaussIntegration.GaussPoint[] gaussPoints = GaussIntegration.GetPointsRectangular(4);

            for (int i = 0; i < gaussPoints.Length; i++)
            {
                double csi = gaussPoints[i].Point.X;
                double eta = gaussPoints[i].Point.Y;
                mnl.Matrix<double> b = GetB(csi, eta);
                //Console.WriteLine("b(csi="+csi+",eta="+eta+")" + b);
                Console.WriteLine("detJ("+csi.ToString("F2")+","+eta.ToString("F2")+")="+ getDetJ(csi, eta));
                mnl.Matrix<double> m = gaussPoints[i].Weight * b.Transpose() * _d * b * getDetJ(csi, eta);
                _kElementLocalCoord = _kElementLocalCoord + m;
                
            }
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            // Occorre fare integrazione sulle funzioni di forma lineari di un quad4 (è possibile usare quella dell'elemento quad4 membranale)
            // l'integrazione delle funzione di forma Ni sul dominio dell'elemento è la quota parte della forza che va nel nodo i
            //esempio: F(nodo 1 = p * integrazione(N1 dcsi deta) = somma gauss N1(csi gauss, eta gauss) * detj(csi gauss, eta guass) * weightgauss
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(3 * Nodes.Length); //3 = DOF in local : DZ, RX, RZ
            foreach (IPlateLoadCaseAttribute iAttribute in _attributesLoadCase)
            {
                if (iAttribute is PlatePressureAttribute)
                {
                    /*
                     * Evaluation of a new quadrilateral thin plate bending element - jean louis batoz
                     *  pg. 1622:
                     *  Simple load vector or complete load vector with numerical integration?
                     */
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

                        for (int j = 0; j < _localNodes.Length; j++) {
                            J4nodeElement[0, 0] = J4nodeElement[0, 0] + LinearShapeFunctionQuad4.DNdCsi(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/dcsi
                            J4nodeElement[0, 1] = J4nodeElement[0, 1] + LinearShapeFunctionQuad4.DNdCsi(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/dcsi
                            J4nodeElement[1, 0] = J4nodeElement[1, 0] + LinearShapeFunctionQuad4.DNdEta(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/deta
                            J4nodeElement[1, 1] = J4nodeElement[1, 1] + LinearShapeFunctionQuad4.DNdEta(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/deta
                        }
                        double detJ = J4nodeElement.Determinant();
                        /*
                        Console.WriteLine("N1(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(1, csi, eta));
                        Console.WriteLine("N2(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(2, csi, eta));
                        Console.WriteLine("N3(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(3, csi, eta));
                        Console.WriteLine("N4(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(4, csi, eta));
                        Console.WriteLine("F: detJ("+csi+","+eta+") = " + detJ);
                        */
                        
                        _fLocalCoord[0] = _fLocalCoord[0] + LinearShapeFunctionQuad4.NaturalShapeFunction(1, csi, eta) * detJ * gaussWeight * pz; //node1
                        _fLocalCoord[3] = _fLocalCoord[3] + LinearShapeFunctionQuad4.NaturalShapeFunction(2, csi, eta) * detJ * gaussWeight * pz; //node2
                        _fLocalCoord[6] = _fLocalCoord[6] + LinearShapeFunctionQuad4.NaturalShapeFunction(3, csi, eta) * detJ * gaussWeight * pz; //node3
                        _fLocalCoord[9] = _fLocalCoord[9] + LinearShapeFunctionQuad4.NaturalShapeFunction(4, csi, eta) * detJ * gaussWeight * pz; //node4
                    }
                }
            }
            return _fLocalCoord;
        }

        public override mnl.Matrix<double> GetB(double csi, double eta, double zeta = 0)
        {
            double l12 = Math.Sqrt(_x12 * _x12 + _y12 * _y12);
            double l23 = Math.Sqrt(_x23 * _x23 + _y23 * _y23);
            double l34 = Math.Sqrt(_x34 * _x34 + _y34 * _y34);
            double l41 = Math.Sqrt(_x41 * _x41 + _y41 * _y41);

            double detJ = getDetJ(csi, eta);

            double j11 = 1.0 / detJ * 1.0 / 4.0 * (_y32 + _y41 + csi * (_y12 + _y34));
            double j12 = -1.0 / detJ * 1.0 / 4.0 * (_y21 + _y34 + eta * (_y12 + _y34)); //to be inverted?
            double j21 = -1.0 / detJ * 1.0 / 4.0 * (_x32 + _x41 + csi * (_x12 + _x34)); //to be inverted?
            double j22 = 1.0 / detJ * 1.0 / 4.0 * (_x21 + _x34 + eta * (_x12 + _x34));

            /*Console.WriteLine("j11 = " + j11);
            Console.WriteLine("j12 = " + j12);
            Console.WriteLine("j21 = " + j21);
            Console.WriteLine("j22 = " + j22);*/

            mnl.Vector<double> hxCsi = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hyCsi = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hxEta = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hyEta = mnl.Vector<double>.Build.Dense(12, 1);

            double a5 = - _x12 / Math.Pow(l12, 2.0);
            double a6 = - _x23 / Math.Pow(l23, 2.0);
            double a7 = - _x34 / Math.Pow(l34, 2.0);
            double a8 = - _x41 / Math.Pow(l41, 2.0);

            double b5 = 3.0 / 4.0 * _x12 * _y12 / Math.Pow(l12, 2.0);
            double b6 = 3.0 / 4.0 * _x23 * _y23 / Math.Pow(l23, 2.0);
            double b7 = 3.0 / 4.0 * _x34 * _y34 / Math.Pow(l34, 2.0);
            double b8 = 3.0 / 4.0 * _x41 * _y41 / Math.Pow(l41, 2.0);

            double c5 = (1.0 / 4.0 * Math.Pow(_x12, 2.0) - 1.0 / 2.0 * Math.Pow(_y12, 2.0)) / Math.Pow(l12, 2.0);
            double c6 = (1.0 / 4.0 * Math.Pow(_x23, 2.0) - 1.0 / 2.0 * Math.Pow(_y23, 2.0)) / Math.Pow(l23, 2.0);
            double c7 = (1.0 / 4.0 * Math.Pow(_x34, 2.0) - 1.0 / 2.0 * Math.Pow(_y34, 2.0)) / Math.Pow(l34, 2.0);
            double c8 = (1.0 / 4.0 * Math.Pow(_x41, 2.0) - 1.0 / 2.0 * Math.Pow(_y41, 2.0)) / Math.Pow(l41, 2.0);

            double d5 = -_y12 / Math.Pow(l12, 2.0);
            double d6 = -_y23 / Math.Pow(l23, 2.0);
            double d7 = -_y34 / Math.Pow(l34, 2.0);
            double d8 = -_y41 / Math.Pow(l41, 2.0);

            double e5 = (-1.0 / 2.0 * Math.Pow(_x12, 2.0) + 1.0 / 4.0 * Math.Pow(_y12, 2.0)) / Math.Pow(l12, 2.0);
            double e6 = (-1.0 / 2.0 * Math.Pow(_x23, 2.0) + 1.0 / 4.0 * Math.Pow(_y23, 2.0)) / Math.Pow(l23, 2.0);
            double e7 = (-1.0 / 2.0 * Math.Pow(_x34, 2.0) + 1.0 / 4.0 * Math.Pow(_y34, 2.0)) / Math.Pow(l34, 2.0);
            double e8 = (-1.0 / 2.0 * Math.Pow(_x41, 2.0) + 1.0 / 4.0 * Math.Pow(_y41, 2.0)) / Math.Pow(l41, 2.0);

            #region DebugCoefficient
            /*
            Console.WriteLine("a5 = " + a5);
            Console.WriteLine("b5 = " + b5);
            Console.WriteLine("c5 = " + c5);
            Console.WriteLine("d5 = " + d5);
            Console.WriteLine("e5 = " + e5);

            Console.WriteLine("a6 = " + a6);
            Console.WriteLine("b6 = " + b6);
            Console.WriteLine("c6 = " + c6);
            Console.WriteLine("d6 = " + d6);
            Console.WriteLine("e6 = " + e6);
            

            Console.WriteLine("a7 = " + a7);
            Console.WriteLine("b7 = " + b7);
            Console.WriteLine("c7 = " + c7);
            Console.WriteLine("d7 = " + d7);
            Console.WriteLine("e7 = " + e7);

            Console.WriteLine("a8 = " + a8);
            Console.WriteLine("b8 = " + b8);
            Console.WriteLine("c8 = " + c8);
            Console.WriteLine("d8 = " + d8);
            Console.WriteLine("e8 = " + e8);
            */
            #endregion

            hxCsi[1 - 1] = 3.0 / 2.0 * (a5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta) - a8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta));
            hxCsi[2 - 1] = b5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta) + b8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta);
            //Console.WriteLine("hx,csi[2-1=1] = " + b5.ToString("F2") + " * " + dNdCsi(5, csi, eta).ToString("F2") + " + " + b8.ToString("F2") + " * " + dNdCsi(8, csi, eta).ToString("F2") + " = " + hxCsi[2 - 1].ToString("F2"));
            hxCsi[3 - 1] = QuadraticShapeFunctionQuad8.DNdCsi(1, csi, eta) - c5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta) - c8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta);

            hxCsi[4 - 1] = 3.0 / 2.0 * (a6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta) - a5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta));
            hxCsi[5 - 1] = b6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta) + b5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta);
            hxCsi[6 - 1] = QuadraticShapeFunctionQuad8.DNdCsi(2, csi, eta) - c6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta) - c5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta);

            hxCsi[7 - 1] = 3.0 / 2.0 * (a7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta) - a6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta));
            hxCsi[8 - 1] = b7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta) + b6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta);
            hxCsi[9 - 1] = QuadraticShapeFunctionQuad8.DNdCsi(3, csi, eta) - c7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta) - c6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta);

            hxCsi[10 - 1] = 3.0 / 2.0 * (a8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta) - a7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta));
            hxCsi[11 - 1] = b8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta) + b7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta);
            hxCsi[12 - 1] = QuadraticShapeFunctionQuad8.DNdCsi(4, csi, eta) - c8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta) - c7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta);

            /////////////////////////////////////////////////////////////////////////////////////////////////
            
            hyCsi[1 - 1] = 3.0 / 2.0 * (d5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta) - d8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta));
            hyCsi[2 - 1] = -QuadraticShapeFunctionQuad8.DNdCsi(1, csi, eta) + e5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta) + e8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta);
            //Console.WriteLine("hy,csi[2-1=1]("+csi.ToString("F2")+","+eta.ToString("F2") + ") = -" + dNdCsi(1, csi, eta).ToString("F2") + "+" + e5.ToString("F2") + " * " + dNdCsi(5, csi, eta).ToString("F2") + " + " + e8.ToString("F2") + " * "+ dNdCsi(8, csi, eta).ToString("F2") + "=" + hyCsi[2 - 1]);
            hyCsi[3 - 1] = -b5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta) - b8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta);

            hyCsi[4 - 1] = 3.0 / 2.0 * (d6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta) - d5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta));
            hyCsi[5 - 1] = -QuadraticShapeFunctionQuad8.DNdCsi(2, csi, eta) + e6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta) + e5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta);
            hyCsi[6 - 1] = -b6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta) - b5 * QuadraticShapeFunctionQuad8.DNdCsi(5, csi, eta);

            hyCsi[7 - 1] = 3.0 / 2.0 * (d7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta) - d6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta));
            hyCsi[8 - 1] = -QuadraticShapeFunctionQuad8.DNdCsi(3, csi, eta) + e7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta) + e6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta);
            hyCsi[9 - 1] = -b7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta) - b6 * QuadraticShapeFunctionQuad8.DNdCsi(6, csi, eta);

            hyCsi[10 - 1] = 3.0 / 2.0 * (d8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta) - d7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta));
            hyCsi[11 - 1] = -QuadraticShapeFunctionQuad8.DNdCsi(4, csi, eta) + e8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta) + e7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta);
            hyCsi[12 - 1] = -b8 * QuadraticShapeFunctionQuad8.DNdCsi(8, csi, eta) - b7 * QuadraticShapeFunctionQuad8.DNdCsi(7, csi, eta);

            /////////////////////////////////////////////////////////////////////////////////////////////////

            hxEta[1 - 1] = 3.0 / 2.0 * (a5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta) - a8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta));
            hxEta[2 - 1] = b5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta) + b8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta);
            hxEta[3 - 1] = QuadraticShapeFunctionQuad8.DNdEta(1, csi, eta) - c5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta) - c8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta);

            hxEta[4 - 1] = 3.0 / 2.0 * (a6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta) - a5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta));
            hxEta[5 - 1] = b6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta) + b5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta);
            hxEta[6 - 1] = QuadraticShapeFunctionQuad8.DNdEta(2, csi, eta) - c6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta) - c5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta);

            hxEta[7 - 1] = 3.0 / 2.0 * (a7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta) - a6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta));
            hxEta[8 - 1] = b7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta) + b6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta);
            hxEta[9 - 1] = QuadraticShapeFunctionQuad8.DNdEta(3, csi, eta) - c7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta) - c6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta);

            hxEta[10 - 1] = 3.0 / 2.0 * (a8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta) - a7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta));
            hxEta[11 - 1] = b8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta) + b7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta);
            hxEta[12 - 1] = QuadraticShapeFunctionQuad8.DNdEta(4, csi, eta) - c8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta) - c7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta);

            /////////////////////////////////////////////////////////////////////////////////////////////////

            hyEta[1 - 1] = 3.0 / 2.0 * (d5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta) - d8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta));
            hyEta[2 - 1] = -QuadraticShapeFunctionQuad8.DNdEta(1, csi, eta) + e5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta) + e8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta);
            hyEta[3 - 1] = -b5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta) - b8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta);

            hyEta[4 - 1] = 3.0 / 2.0 * (d6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta) - d5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta));
            hyEta[5 - 1] = -QuadraticShapeFunctionQuad8.DNdEta(2, csi, eta) + e6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta) + e5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta);
            hyEta[6 - 1] = -b6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta) - b5 * QuadraticShapeFunctionQuad8.DNdEta(5, csi, eta);

            hyEta[7 - 1] = 3.0 / 2.0 * (d7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta) - d6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta));
            hyEta[8 - 1] = -QuadraticShapeFunctionQuad8.DNdEta(3, csi, eta) + e7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta) + e6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta);
            hyEta[9 - 1] = -b7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta) - b6 * QuadraticShapeFunctionQuad8.DNdEta(6, csi, eta);

            hyEta[10 - 1] = 3.0 / 2.0 * (d8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta) - d7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta));
            hyEta[11 - 1] = -QuadraticShapeFunctionQuad8.DNdEta(4, csi, eta) + e8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta) + e7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta);
            //Console.WriteLine("hy,eta[10]("+csi+","+eta+") = -" + dNdEta(4, csi, eta) + "+" + e8 + " * " + dNdEta(8, csi, eta) + " + " + e7 +" * "+ dNdEta(7, csi, eta));
            hyEta[12 - 1] = -b8 * QuadraticShapeFunctionQuad8.DNdEta(8, csi, eta) - b7 * QuadraticShapeFunctionQuad8.DNdEta(7, csi, eta);

            mnl.Vector<double> r0 = j11 * hxCsi + j12 * hxEta;
            mnl.Vector<double> r1 = j21 * hyCsi + j22 * hyEta;
            mnl.Vector<double> r2 = j11 * hyCsi + j12 * hyEta + j21 * hxCsi + j22 * hxEta;
                       
            mnl.Matrix<double> b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0, r1, r2);
            /*Console.WriteLine("csi = " + csi + " eta = " + eta);
            Console.WriteLine("B(csi,eta) matrix:" + b);

            Console.WriteLine("dH(csi="+csi+",eta="+eta+")");
            for (int i = 0; i < 12; i++)
            {
                Console.Write(hxCsi[i].ToString("F2") + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < 12; i++)
            {
                Console.Write(hxEta[i].ToString("F2") + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < 12; i++)
            {
                Console.Write(hyCsi[i].ToString("F2") + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < 12; i++)
            {
                Console.Write(hyEta[i].ToString("F2") + " ");
            }
            Console.WriteLine();
            Console.WriteLine();*/
            return b;
        }

        private double getDetJ(double csi, double eta)
        {
            return 1.0 / 8.0 * (_y42 * _x31 - _y31 * _x42) + csi / 8.0 * (_y34 * _x21 - _y21 * _x34) + eta / 8.0 * (_y41 * _x32 - _y32 * _x41);
        }
        

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            //TODO: "aggiornare";
            throw new NotImplementedException();
        }
    }
}
