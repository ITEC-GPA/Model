using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Materials;
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

        double _l12;
        double _l23;
        double _l34;
        double _l41;

        Dictionary<int, double> _aCoeff;
        Dictionary<int, double> _bCoeff;
        Dictionary<int, double> _cCoeff;
        Dictionary<int, double> _dCoeff;
        Dictionary<int, double> _eCoeff;
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

            //set local coordinate system
            _nodesLocal = Quad4Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);
            Node node1 = _nodesLocal[0];
            Node node2 = _nodesLocal[1];
            Node node3 = _nodesLocal[2];
            Node node4 = _nodesLocal[3];

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

            _l12 = Math.Sqrt(_x12 * _x12 + _y12 * _y12);
            _l23 = Math.Sqrt(_x23 * _x23 + _y23 * _y23);
            _l34 = Math.Sqrt(_x34 * _x34 + _y34 * _y34);
            _l41 = Math.Sqrt(_x41 * _x41 + _y41 * _y41);

            _aCoeff = new Dictionary<int, double>();
            _bCoeff = new Dictionary<int, double>();
            _cCoeff = new Dictionary<int, double>();
            _dCoeff = new Dictionary<int, double>();
            _eCoeff = new Dictionary<int, double>();

            _aCoeff.Add(5, -_x12 / Math.Pow(_l12, 2.0));
            _aCoeff.Add(6, -_x23 / Math.Pow(_l23, 2.0));
            _aCoeff.Add(7, -_x34 / Math.Pow(_l34, 2.0));
            _aCoeff.Add(8, -_x41 / Math.Pow(_l41, 2.0));

            _bCoeff.Add(5, 3.0 / 4.0 * _x12 * _y12 / Math.Pow(_l12, 2.0));
            _bCoeff.Add(6, 3.0 / 4.0 * _x23 * _y23 / Math.Pow(_l23, 2.0));
            _bCoeff.Add(7, 3.0 / 4.0 * _x34 * _y34 / Math.Pow(_l34, 2.0));
            _bCoeff.Add(8, 3.0 / 4.0 * _x41 * _y41 / Math.Pow(_l41, 2.0));

            _cCoeff.Add(5, (1.0 / 4.0 * Math.Pow(_x12, 2.0) - 1.0 / 2.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0));
            _cCoeff.Add(6, (1.0 / 4.0 * Math.Pow(_x23, 2.0) - 1.0 / 2.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0));
            _cCoeff.Add(7, (1.0 / 4.0 * Math.Pow(_x34, 2.0) - 1.0 / 2.0 * Math.Pow(_y34, 2.0)) / Math.Pow(_l34, 2.0));
            _cCoeff.Add(8, (1.0 / 4.0 * Math.Pow(_x41, 2.0) - 1.0 / 2.0 * Math.Pow(_y41, 2.0)) / Math.Pow(_l41, 2.0));

            _dCoeff.Add(5, -_y12 / Math.Pow(_l12, 2.0));
            _dCoeff.Add(6, -_y23 / Math.Pow(_l23, 2.0));
            _dCoeff.Add(7, -_y34 / Math.Pow(_l34, 2.0));
            _dCoeff.Add(8, -_y41 / Math.Pow(_l41, 2.0));

            _eCoeff.Add(5, (-1.0 / 2.0 * Math.Pow(_x12, 2.0) + 1.0 / 4.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0));
            _eCoeff.Add(6, (-1.0 / 2.0 * Math.Pow(_x23, 2.0) + 1.0 / 4.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0));
            _eCoeff.Add(7, (-1.0 / 2.0 * Math.Pow(_x34, 2.0) + 1.0 / 4.0 * Math.Pow(_y34, 2.0)) / Math.Pow(_l34, 2.0));
            _eCoeff.Add(8, (-1.0 / 2.0 * Math.Pow(_x41, 2.0) + 1.0 / 4.0 * Math.Pow(_y41, 2.0)) / Math.Pow(_l41, 2.0));

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(6 * 4, 4 * 3);

            Vector3d globalX = new Vector3d(1.0, 0.0, 0.0);
            Vector3d globalY = new Vector3d(0.0, 1.0, 0.0);
            Vector3d globalZ = new Vector3d(0.0, 0.0, 1.0);

            Vector3d localX = LocalCoordinateSystem.V1;
            Vector3d localY = LocalCoordinateSystem.V2;
            Vector3d localZ = LocalCoordinateSystem.V3;

            double xX = localX.DotProduct(globalX);
            double yX = localY.DotProduct(globalX);
            double zX = localZ.DotProduct(globalX);

            double xY = localX.DotProduct(globalY);
            double yY = localY.DotProduct(globalY);
            double zY = localZ.DotProduct(globalY);

            double xZ = localX.DotProduct(globalZ);
            double yZ = localY.DotProduct(globalZ);
            double zZ = localZ.DotProduct(globalZ);

            int dimRow = 6; //DX,DY,DZ,RX,RY,RZ
            int dimCol = 3; //dz, rx, ry
            for (int i = 0; i < _nodesLocal.Length; i++)
            {
                //local node1 z-displacement in global coordinate
                dofGlobalToLocalTranspose[i * dimRow + 0, i * dimCol + 0] = zX;
                dofGlobalToLocalTranspose[i * dimRow + 1, i * dimCol + 0] = zY;
                dofGlobalToLocalTranspose[i * dimRow + 2, i * dimCol + 0] = zZ;

                //local node1 rx-rotation and ry in global coordinate
                dofGlobalToLocalTranspose[i * dimRow + 3, i * dimCol + 1] = xX;
                dofGlobalToLocalTranspose[i * dimRow + 3, i * dimCol + 2] = yX;

                dofGlobalToLocalTranspose[i * dimRow + 4, i * dimCol + 1] = xY;
                dofGlobalToLocalTranspose[i * dimRow + 4, i * dimCol + 2] = yY;

                dofGlobalToLocalTranspose[i * dimRow + 5, i * dimCol + 1] = xZ;
                dofGlobalToLocalTranspose[i * dimRow + 5, i * dimCol + 2] = yZ;
            }

            _dofGlobalToLocal = dofGlobalToLocalTranspose.Transpose();
            #endregion
        }

        public override void BuildMatrix()
        {
            #region matrixD
            double tb = ((PlateProperty)_property).BendingThickness;
            
            _d = (_property as PlateProperty).Material.GetPlaneStress();

            _d = Math.Pow(tb, 3.0) / (12.0) * _d; //flexural rigidity
            #endregion

            #region matrixK
            mnl.Matrix<double> bTdb(double csi, double eta)
            {
                mnl.Matrix<double> b = GetB(csi, eta);
                return b.Transpose() * _d * b;
            }

            var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesLocal);

            _kElementLocalCoord = GaussIntegration.IntegrationQuadrilateral(bTdb, jacob, 4);
            #endregion
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            // Occorre fare integrazione sulle funzioni di forma lineari di un quad4 (è possibile usare quella dell'elemento quad4 membranale)
            // l'integrazione delle funzione di forma Ni sul dominio dell'elemento è la quota parte della forza che va nel nodo i
            //esempio: F(nodo 1 = p * integrazione(N1 dcsi deta) = somma gauss N1(csi gauss, eta gauss) * detj(csi gauss, eta guass) * weightgauss
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(3 * Nodes.Length); //3 = DOF in local : dz, rx, ry
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
                    //double px = attribute.P1 * dirX.DotProduct(x) + attribute.P2 * dirY.DotProduct(x) + attribute.P3 * dirZ.DotProduct(x);
                    //double py = attribute.P1 * dirX.DotProduct(y) + attribute.P2 * dirY.DotProduct(y) + attribute.P3 * dirZ.DotProduct(y);
                    double pz = attribute.P1 * dirX.DotProduct(z) + attribute.P2 * dirY.DotProduct(z) + attribute.P3 * dirZ.DotProduct(z);

                    //equation 18
                    mnl.Matrix<double> wDotp (double csi, double eta)
                    {
                        mnl.Matrix<double> wp = mnl.Matrix<double>.Build.Dense(4, 1);
                        wp[0, 0] = Quad4Element.GetShapeFunction(1, csi, eta) * pz;
                        wp[1, 0] = Quad4Element.GetShapeFunction(2, csi, eta) * pz;
                        wp[2, 0] = Quad4Element.GetShapeFunction(3, csi, eta) * pz;
                        wp[3, 0] = Quad4Element.GetShapeFunction(4, csi, eta) * pz;

                        return wp;
                    }

                    var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesLocal);
                    var f = GaussIntegration.IntegrationQuadrilateral(wDotp, jacob, 4);

                    _fLocalCoord[0] = f[0,0]; //node1
                    _fLocalCoord[3] = f[1,0]; //node2
                    _fLocalCoord[6] = f[2,0]; //node3
                    _fLocalCoord[9] = f[3,0]; //node4
                }
            }
            return _fLocalCoord;
        }

        internal mnl.Matrix<double> GetInvJacobian(double csi, double eta)
        {
            double detJ = getDetJ(csi, eta);

            double j11 = 1.0 / detJ * 1.0 / 4.0 * (_y32 + _y41 + csi * (_y12 + _y34));
            double j12 = -1.0 / detJ * 1.0 / 4.0 * (_y21 + _y34 + eta * (_y12 + _y34)); //to be inverted?
            double j21 = -1.0 / detJ * 1.0 / 4.0 * (_x32 + _x41 + csi * (_x12 + _x34)); //to be inverted?
            double j22 = 1.0 / detJ * 1.0 / 4.0 * (_x21 + _x34 + eta * (_x12 + _x34));

            mnl.Matrix<double> invJacob = mnl.Matrix<double>.Build.Dense(2, 2);
            invJacob[0, 0] = j11;
            invJacob[0, 1] = j12;
            invJacob[1, 0] = j21;
            invJacob[1, 1] = j22;

            return invJacob;
        }

        public override mnl.Matrix<double> GetB(double csi, double eta)
        {
            var invJacob = GetInvJacobian(csi, eta);

            double j11 = invJacob[0, 0];
            double j12 = invJacob[0, 1];
            double j21 = invJacob[1, 0];
            double j22 = invJacob[1, 1];

            mnl.Vector<double> hxCsi = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hyCsi = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hxEta = mnl.Vector<double>.Build.Dense(12, 1);
            mnl.Vector<double> hyEta = mnl.Vector<double>.Build.Dense(12, 1);

            for (int i = 1; i <= 12; i++)
            {
                hxCsi[i - 1] = GetFunction(i, "x", "csi")(csi, eta);
                hyCsi[i - 1] = GetFunction(i, "y", "csi")(csi, eta);

                hxEta[i - 1] = GetFunction(i, "x", "eta")(csi, eta);
                hyEta[i - 1] = GetFunction(i, "y", "eta")(csi, eta);
            }

            mnl.Vector<double> r0 = j11 * hxCsi + j12 * hxEta;
            mnl.Vector<double> r1 = j21 * hyCsi + j22 * hyEta;
            mnl.Vector<double> r2 = j11 * hyCsi + j12 * hyEta + j21 * hxCsi + j22 * hxEta;
                       
            mnl.Matrix<double> b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0, r1, r2);
            return b;
        }

        private double getDetJ(double csi, double eta)
        {
            return 1.0 / 8.0 * (_y42 * _x31 - _y31 * _x42) + csi / 8.0 * (_y34 * _x21 - _y21 * _x34) + eta / 8.0 * (_y41 * _x32 - _y32 * _x41);
        }

        /// <summary>
        /// Descrition between eq. 11 and 12 of the article
        /// </summary>
        /// <param name="indexes1"></param>
        /// <param name="indexes2"></param>
        /// <param name="indexes3"></param>
        private void GetIndexes(out int[] indexes1, out int[] indexes2, out int[] indexes3)
        {
            indexes1 = new int[4];
            indexes1[0] = 1;
            indexes1[1] = 2;
            indexes1[2] = 3;
            indexes1[3] = 4;

            indexes2 = new int[4];
            indexes2[0] = 8;
            indexes2[1] = 5;
            indexes2[2] = 6;
            indexes2[3] = 7;

            indexes3 = new int[4];
            indexes3[0] = 5;
            indexes3[1] = 6;
            indexes3[2] = 7;
            indexes3[3] = 8;
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
            GetIndexes(out int[] indexes1, out int[] indexes2, out int[] indexes3);

            Func<double, double, double> F(int i)
            {
                if (deriv == "")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionQuad8.NaturalShapeFunction(i, csi, eta);
                } else if (deriv == "csi")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionQuad8.DNdCsi(i, csi, eta);
                } else if (deriv == "eta")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionQuad8.DNdEta(i, csi, eta);
                } else
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

                        case 10:
                            index2 = indexes2[3];
                            index3 = indexes3[3];
                            return H1x(index2, index3);
                        case 11:
                            index2 = indexes2[3];
                            index3 = indexes3[3];
                            return H2x(index2, index3);
                        case 12:
                            index1 = indexes1[3];
                            index2 = indexes2[3];
                            index3 = indexes3[3];
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

                        case 10:
                            index2 = indexes2[3];
                            index3 = indexes3[3];
                            return H1y(index2, index3);
                        case 11:
                            index1 = indexes1[3];
                            index2 = indexes2[3];
                            index3 = indexes3[3];
                            return H2y(index1, index2, index3);
                        case 12:                            
                            index2 = indexes2[3];
                            index3 = indexes3[3];
                            return H3y(index2, index3);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Results
        /// <summary>
        /// 
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacements"></param>
        /// <returns>tensor curvature xx, yy, xy</returns>
        private mnl.Matrix<double> GetCurvaturesLocalCoordinates(double csi, double eta, double[] globalDisplacements)
        {
            var localDisplacements = GetLocalDisplacementVector(globalDisplacements);

            //curvature = B * U
            //contains curvature xx, yy, xy
            mnl.Vector<double> curvatureLocal = GetB(csi, eta) * localDisplacements;

            mnl.Matrix<double> curvatureTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            curvatureTensor[0, 0] = curvatureLocal[0]; //xx

            curvatureTensor[0, 1] = curvatureLocal[2]; //xy
            curvatureTensor[1, 0] = curvatureLocal[2]; //yx

            curvatureTensor[1, 1] = curvatureLocal[1]; //yy

            return curvatureTensor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacements"></param>
        /// <param name="newSys">if null, local axis system are used</param>
        /// <returns></returns>
        public mnl.Matrix<double> GetCurvatures(double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            var localCurvatures = GetCurvaturesLocalCoordinates(csi, eta, globalDisplacements);

            return FEMUtilities.RotateTensor(localCurvatures, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetBending(double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            var localCurvatures = GetCurvaturesLocalCoordinates(csi, eta, globalDisplacements);

            var vecLocalCurvatures = mnl.Vector<double>.Build.Dense(new double[] { localCurvatures[0, 0], localCurvatures[1, 1], localCurvatures[1, 0] });

            var localBending = _d * vecLocalCurvatures;

            var tensorLocalBending = mnl.Matrix<double>.Build.Dense(3, 3);
            tensorLocalBending[0, 0] = localBending[0]; //mxx

            tensorLocalBending[0, 1] = localBending[2]; //mxy
            tensorLocalBending[1, 0] = localBending[2]; //myx

            tensorLocalBending[1, 1] = localBending[1]; //myy

            return FEMUtilities.RotateTensor(tensorLocalBending, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetStrains(Face face, double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            double h = ((PlateProperty)Property).BendingThickness;

            var localCurvatures = GetCurvaturesLocalCoordinates(csi, eta, globalDisplacements);

            var newCurvatures = FEMUtilities.RotateTensor(localCurvatures, _localCoordinateSystem, newSys);

            var strain = mnl.Matrix<double>.Build.Dense(3, 3);
            if (face == Face.Top)
            {
                return h / 2.0 * newCurvatures;
            }
            else if (face == Face.Bottom)
            {
                return -h / 2.0 * newCurvatures;
            }
            else
            {
                return strain;
            }
        }

        public mnl.Matrix<double> GetStress(Plate.Face face, double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            var strains = GetStrains(face, csi, eta, globalDisplacements, newSys);

            var strainVec = mnl.Vector<double>.Build.Dense(3);
            strainVec[0] = strains[0, 0]; //exx
            strainVec[1] = strains[1, 1]; //eyy
            strainVec[2] = strains[0, 1]; //exy

            var planeStressMatrix = ((IsotropicFemMaterial)((PlateProperty)_property).Material).GetPlaneStress();
            var stressVector = planeStressMatrix * strainVec;

            var tensorStress = mnl.Matrix<double>.Build.Dense(3, 3);
            tensorStress[0, 0] = stressVector[0];

            tensorStress[0, 1] = stressVector[2];
            tensorStress[1, 0] = stressVector[2];

            tensorStress[1, 1] = stressVector[1];

            return tensorStress;
        }
        #endregion
    }
}
