using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// A plate finite element for modelling of tripled laminated glass and comparison with other computational method
    /// Ivanov, Velchev, Georgiev, Sadowki - 2015
    /// </summary>
    public class Quad4TripleLaminatedGlass : Plate
    {
        #region variables
        double _hc;
        double _G0;
        double _h0;
        double _h1;
        double _h2;
        double _EGlass;
        double _niGlass;

        Node[] _localNodes;
        double _lx;
        double _ly;
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="hc">distance between middle plane of glasses</param>
        /// <param name="G0">shear module of interlayer</param>
        /// <param name="h0">thickness interlayer</param>
        /// <param name="h1">Thickness of top glass</param>
        /// <param name="h2">Thickness of bottom glass</param>
        public Quad4TripleLaminatedGlass(Node[] nodes, double G0, double h0, double h1, double h2, double EGlass, double niGlass) : base(nodes)
        {
            //eq. 51 -> lista dof locali
            /*
             * deltaU = slippage between the glass layer in local x direction 
             * deltaV = slippage between the glass layer in local y direction 
             * w = deflection in local z direction
             * thetaX = rotation along x local direction
             * thetaY = rotation along y local direction
             * psi = rotation along z local direction
             */

            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            //displacement w il local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);

            _hc = (2.0 * h0 + h1 + h2) / 2.0; //eq. (14)
            _G0 = G0;
            _h0 = h0;
            _h1 = h1;
            _h2 = h2;
            _EGlass = EGlass;
            _niGlass = niGlass;
        }

        public override void BuildMatrix()
        {
            //set local coordinate system
            _localNodes = Quad4Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);
            _lx = _localNodes[1].Position.X - _localNodes[0].Position.X;
            _ly = _localNodes[3].Position.Y - _localNodes[0].Position.Y;

            #region ControlliGeometrici
            if (_lx <= 0 || _ly <= 0)
            {
                throw new Exception("lx or ly <= 0!");
            }
                    
            double lx2 = _localNodes[2].Position.X - _localNodes[3].Position.X;
            double ly2 = _localNodes[2].Position.Y - _localNodes[1].Position.Y;

            if (lx2 != _lx)
            {
                throw new Exception("Elemento finito funziona per elementi non rettangolari?");
            }

            if (ly2 != _ly)
            {
                throw new Exception("Elemento finito funziona per elementi non rettangolari?");
            }
            #endregion

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            //TODO: to be checked
            /*mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(24, 12);

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
            _dofGlobalToLocal = dofGlobalToLocalTranspose.Transpose();*/

            _dofGlobalToLocal = mnl.Matrix<double>.Build.DenseIdentity(24);

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

            #region matricesD
            /*double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();*/

            #region Ds - INTERLAYER
            mnl.Matrix<double> Ds = mnl.Matrix<double>.Build.Dense(4, 4);
            Ds[0, 0] = 1.0;
            Ds[0, 2] = _hc;

            Ds[1, 1] = 1.0;
            Ds[1, 3] = _hc;

            Ds[2, 0] = _hc;
            Ds[2, 2] = _hc * _hc;

            Ds[3, 1] = _hc;
            Ds[3, 3] = _hc * _hc;

            Ds = _G0 / _h0 * Ds; //equation (32)
#if DEBUG
            /*Console.WriteLine("Ds");
            FEMUtilities.WriteMatrix(Ds);*/
#endif
            #endregion

            #region Dg - GLASS
            mnl.Matrix<double> C = Plate.DPlaneStress(_EGlass,_niGlass);
            mnl.Matrix<double> Dg = mnl.Matrix<double>.Build.Dense(6, 6); //equation (45)

            double factor1 = _h1 * _h2 / (_h1 + _h2);
            double factor2 = Math.Pow(_h1,3.0) * Math.Pow(_h2,3.0) / 12.0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Dg[row, col] = factor1 * C[row, col];
                }
            }

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Dg[row + 3, col + 3] = factor2 * C[row, col];
                }
            }
            #if DEBUG
            /*Console.WriteLine("Dg");
            FEMUtilities.WriteMatrix(Dg);*/
            #endif
            #endregion
            #endregion

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(24, 24);
            
            mnl.Matrix<double> fKLayer(double csi, double eta)
            {
                double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _localNodes);
                double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _localNodes);

                mnl.Matrix<double> Bs = GetBs(x, y);

                #if DEBUG
                /*Console.WriteLine("csi = " + csi + " eta=" + eta);
                Console.WriteLine("x = " + x + " y=" + y);
                Console.WriteLine("Bs(x=" + x + ",y=" + y + ")");
                Console.WriteLine(Bs);*/
                #endif

                return Bs.Transpose() * Ds * Bs;
            }

            mnl.Matrix<double> fKGlass(double csi, double eta)
            {
                double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _localNodes);
                double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _localNodes);

                mnl.Matrix<double> Bg = GetBg(x, y);

                #if DEBUG
                /*Console.WriteLine("csi = " + csi + " eta=" + eta);
                Console.WriteLine("x = " + x + " y=" + y);
                Console.WriteLine("Bg(x=" + x + ",y=" + y + ")");
                Console.WriteLine(Bg);*/
                #endif

                return Bg.Transpose() * Dg * Bg;
            }

            var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _localNodes);

            mnl.Matrix<double> kLayer = GaussIntegration.IntegrationQuadrilateral(fKLayer, jacob, 9);
            mnl.Matrix<double> kGlass = GaussIntegration.IntegrationQuadrilateral(fKGlass, jacob, 9);

            _kElementLocalCoord = kLayer + kGlass;
        }

        /// <summary>
        /// Eq. 63  f = integral(N^T * q)
        /// q = [0, 0 , p]^T
        /// </summary>
        /// <returns></returns>
        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(6 * Nodes.Length);
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

                    Vector3d vecX = LocalCoordinateSystem.V1;
                    dirX.Unitize();
                    Vector3d vecY = LocalCoordinateSystem.V2;
                    dirY.Unitize();
                    Vector3d vecZ = LocalCoordinateSystem.V3;
                    dirZ.Unitize();

                    //Set in local coordinates                    
                    double px = attribute.P1 * dirX.DotProduct(vecX) + attribute.P2 * dirY.DotProduct(vecX) + attribute.P3 * dirZ.DotProduct(vecX);
                    double py = attribute.P1 * dirX.DotProduct(vecY) + attribute.P2 * dirY.DotProduct(vecY) + attribute.P3 * dirZ.DotProduct(vecY);
                    double pz = attribute.P1 * dirX.DotProduct(vecZ) + attribute.P2 * dirY.DotProduct(vecZ) + attribute.P3 * dirZ.DotProduct(vecZ);
                    mnl.Matrix<double> q = mnl.Matrix<double>.Build.Dense(3,1);
                    q[0,0] = px;
                    q[1,0] = py;
                    q[2,0] = pz;

                    mnl.Matrix<double> NtTraspQ(double csi, double eta)
                    {
                        double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _localNodes);
                        double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _localNodes);

                        return GetNMatrix(x, y).Transpose() * q;
                    }

                    var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _localNodes);
                    mnl.Matrix<double> f = GaussIntegration.IntegrationQuadrilateral(NtTraspQ, jacob, 9);
                    for (int i = 0; i < f.RowCount; i++)
                    {
                        _fLocalCoord[i] = f[i, 0];
                    }
                }
            }
            
            return _fLocalCoord;
        }

        public override mnl.Matrix<double> GetB(double csi, double eta, double zeta = 0)
        {
            return mnl.Matrix<double>.Build.Dense(24, 24);
        }

        /// <summary>
        /// eq. 53 and 54 computed for all nodes
        /// </summary>
        /// <returns></returns>
        internal mnl.Matrix<double> GetNMatrix(double x, double y)
        {
            mnl.Matrix<double> N = mnl.Matrix<double>.Build.Dense(3, 0);
            for (int indexNode = 1; indexNode <= 4; indexNode++)
            {
                /*double x = _localNodes[indexNode - 1].Position.X;
                double y = _localNodes[indexNode - 1].Position.Y;*/
                mnl.Matrix<double> nNode = GetNiMatrix(indexNode, x, y, _lx, _ly);
                #if DEBUG
                /*Console.WriteLine("lx  " + _lx + " ly = " + _ly);
                Console.WriteLine("N nodo  " + indexNode);
                FEMUtilities.WriteMatrix(nNode);*/
                #endif
                N = N.Append(nNode);
            }
            return N;
        }

        /// <summary>
        /// eq. 35 computed for all nodes
        /// </summary>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBs(double x, double y)
        {
            mnl.Matrix<double> Bs = mnl.Matrix<double>.Build.Dense(4, 0);
            for (int indexNode = 1; indexNode <= 4; indexNode++)
            {
                /*double x = _localNodes[indexNode - 1].Position.X;
                double y = _localNodes[indexNode - 1].Position.Y;*/
                mnl.Matrix<double> bsNode = GetBsi(indexNode, x, y, _lx, _ly);
                #if DEBUG
                /*Console.WriteLine("lx  " + _lx + " ly = " + _ly);
                Console.WriteLine("Bs nodo  "+ indexNode);
                FEMUtilities.WriteMatrix(bsNode);*/
                #endif
                Bs = Bs.Append(bsNode);
            }
            return Bs;
        }

        /// <summary>
        /// eq. 48 computed for all nodes
        /// </summary>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBg(double x, double y)
        {
            mnl.Matrix<double> Bg = mnl.Matrix<double>.Build.Dense(6, 0);
            for (int indexNode = 1; indexNode <= 4; indexNode++)
            {
                /*double x = _localNodes[indexNode - 1].Position.X;
                double y = _localNodes[indexNode - 1].Position.Y;*/
                #if DEBUG
                //Console.WriteLine("GetBg: x = " + x + " y = " + y);
                #endif
                mnl.Matrix<double> bgNode = GetBgi(indexNode, x, y, _lx, _ly);
                Bg = Bg.Append(bgNode);
            }
            return Bg;
        }

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            //TODO: "aggiornare";
            throw new NotImplementedException();
        }

        #region PrivateInternalFunctions

        #region ShapeFunction
        /// <summary>
        /// Shape functions for this element
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">1 to 6</param>
        /// <param name="lx">lenght of element along x local axis</param>
        /// <param name="ly">lenght of element along y local axis</param>
        /// <returns></returns>
        internal static Func<double, double, double> GetN(int indexNode, int indexDisplacement, double lx, double ly)
        {
            Dictionary<int, (int, int)> indexes = new Dictionary<int, (int, int)>();
            //eq. 59
            indexes.Add(1, (1, 1));
            indexes.Add(2, (2, 1));
            indexes.Add(3, (2, 2));
            indexes.Add(4, (1, 2));

            //equations (56 + equations (57)
            if ((indexDisplacement == 1 && indexNode == 1) || (indexDisplacement == 2 && indexNode == 1))
            {
                return (double x, double y) => (lx - x) * (ly - y) / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 2) || (indexDisplacement == 2 && indexNode == 2))
            {
                return (double x, double y) => x * (ly - y) / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 3) || (indexDisplacement == 2 && indexNode == 3))
            {
                return (double x, double y) => x * y / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 4) || (indexDisplacement == 2 && indexNode == 4))
            {
                return (double x, double y) => x * y / (lx * ly);
            } else if (indexDisplacement == 3)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => -GetHermite(1, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(1, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// First derivative of shape function along x local axis
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">1 to 6</param>
        /// <param name="lx">length of element along its x local axis</param>
        /// <param name="ly">length of element along its y local axis</param>
        /// <returns></returns>
        internal static Func<double, double, double> GetdNdx(int indexNode, int indexDisplacement, double lx, double ly)
        {
            Dictionary<int, (int, int)> indexes = new Dictionary<int, (int, int)>();
            //eq. 59
            indexes.Add(1, (1, 1));
            indexes.Add(2, (2, 1));
            indexes.Add(3, (2, 2));
            indexes.Add(4, (1, 2));

            if ((indexDisplacement == 1 && indexNode == 1) || (indexDisplacement == 2 && indexNode == 1))
            {
                return (double x, double y) => (y - ly) / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 2) || (indexDisplacement == 2 && indexNode == 2))
            {
                return (double x, double y) => (ly - y) / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 3) || (indexDisplacement == 2 && indexNode == 3))
            {
                return (double x, double y) => y / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 4) || (indexDisplacement == 2 && indexNode == 4))
            {
                return (double x, double y) => -y / (lx * ly);
            }
            else if (indexDisplacement == 3)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => -GetFirstDerivHermite(1, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetFirstDerivHermite(1, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// First derivative of shape function along y local axis
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">1 to 6</param>
        /// <param name="lx">length of element along its x local axis</param>
        /// <param name="ly">length of element along its y local axis</param>
        /// <returns></returns>
        internal static Func<double, double, double> GetdNdy(int indexNode, int indexDisplacement, double lx, double ly)
        {
            Dictionary<int, (int, int)> indexes = new Dictionary<int, (int, int)>();
            //eq. 59
            indexes.Add(1, (1, 1));
            indexes.Add(2, (2, 1));
            indexes.Add(3, (2, 2));
            indexes.Add(4, (1, 2));

            if ((indexDisplacement == 1 && indexNode == 1) || (indexDisplacement == 2 && indexNode == 1))
            {
                return (double x, double y) => (x - lx) / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 2) || (indexDisplacement == 2 && indexNode == 2))
            {
                return (double x, double y) => -x / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 3) || (indexDisplacement == 2 && indexNode == 3))
            {
                return (double x, double y) => x / (lx * ly);
            }
            else if ((indexDisplacement == 1 && indexNode == 4) || (indexDisplacement == 2 && indexNode == 4))
            {
                return (double x, double y) => (lx - x) / (lx * ly);
            }
            else if (indexDisplacement == 3)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetFirstDerivHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => -GetHermite(1, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(1, a, lx)(x) * GetFirstDerivHermite(1, b, ly)(y);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Mixed derivative d^2/(dx dy) of shape function
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">1 to 6</param>
        /// <param name="lx">length of element along its x local axis</param>
        /// <param name="ly">length of element along its y local axis</param>
        /// <returns></returns>
        internal static Func<double, double, double> GetdNdxdy(int indexNode, int indexDisplacement, double lx, double ly)
        {
            Dictionary<int, (int, int)> indexes = new Dictionary<int, (int, int)>();
            //eq. 59
            indexes.Add(1, (1, 1));
            indexes.Add(2, (2, 1));
            indexes.Add(3, (2, 2));
            indexes.Add(4, (1, 2));

            if (indexDisplacement == 3)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetFirstDerivHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => -GetFirstDerivHermite(1, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetFirstDerivHermite(1, a, lx)(x) * GetFirstDerivHermite(1, b, ly)(y);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Second derivative of shape function along x local axis
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">3 to 6</param>
        /// <param name="lx">length of element along its x local axis</param>
        /// <param name="ly">length of element along its y local axis</param>
        /// <returns></returns>
        internal static Func<double, double, double> GetdNdx2(int indexNode, int indexDisplacement, double lx, double ly)
        {
            Dictionary<int, (int, int)> indexes = new Dictionary<int, (int, int)>();
            //eq. 59
            indexes.Add(1, (1, 1));
            indexes.Add(2, (2, 1));
            indexes.Add(3, (2, 2));
            indexes.Add(4, (1, 2));

            if (indexDisplacement == 3)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetSecondDerivHermite(0, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetSecondDerivHermite(0, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => -GetSecondDerivHermite(1, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetSecondDerivHermite(1, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Second derivative of shape function along y local axis
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">1 to 6</param>
        /// <param name="lx">length of element along its x local axis</param>
        /// <param name="ly">length of element along its y local axis</param>
        /// <returns></returns>
        internal static Func<double, double, double> GetdNdy2(int indexNode, int indexDisplacement, double lx, double ly)
        {
            Dictionary<int, (int, int)> indexes = new Dictionary<int, (int, int)>();
            //eq. 59
            indexes.Add(1, (1, 1));
            indexes.Add(2, (2, 1));
            indexes.Add(3, (2, 2));
            indexes.Add(4, (1, 2));

            if (indexDisplacement == 3)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetSecondDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetSecondDerivHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => -GetHermite(1, a, lx)(x) * GetSecondDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
                int a = indexes[indexNode].Item1;
                int b = indexes[indexNode].Item2;
                return (double x, double y) => GetHermite(1, a, lx)(x) * GetSecondDerivHermite(1, b, ly)(y);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #region Hermite
        /// <summary>
        /// Hermite polynomial for used in this element H_(i,j) defined in eqts. 57
        /// </summary>
        /// <param name="i">first index</param>
        /// <param name="j">second index</param>
        /// <param name="L">Length</param>
        /// <returns></returns>
        internal static Func<double, double> GetHermite(int i, int j, double L)
        {
            //equations (57)
            if (i == 0 && j == 1)
            {
                return (double eta) => 1.0 - 3.0 * eta * eta / (L * L) + 2.0 * Math.Pow(eta, 3.0) / Math.Pow(L, 3.0);
            } else if (i == 0 && j == 2)
            {
                return (double eta) => 3.0 * eta * eta / (L * L) - 2.0 * Math.Pow(eta, 3.0) / Math.Pow(L, 3.0);
            } else if (i == 1 && j == 1) {
                return (double eta) => eta - 2.0 * eta * eta / L + Math.Pow(eta, 3.0) / (L * L);
            } else if (i == 1 && j == 2)
            {
                return (double eta) => -eta * eta / L + Math.Pow(eta, 3.0) / (L * L);
            } else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// First order derivative of Hermite functions defined in GetHermite function
        /// </summary>
        /// <param name="i">first index</param>
        /// <param name="j">second index</param>
        /// <param name="L">Length</param>
        /// <returns></returns>
        internal static Func<double, double> GetFirstDerivHermite(int i, int j, double L)
        {
            if (i == 0 && j == 1)
            {
                return (double eta) => 6.0 * eta * (eta - L) / Math.Pow(L,3.0);
            }
            else if (i == 0 && j == 2)
            {
                return (double eta) => 6.0 * eta * (L - eta) / Math.Pow(L,3.0);
            }
            else if (i == 1 && j == 1)
            {
                return (double eta) => (L*L - 4.0 * L * eta + 3.0 * eta * eta) / (L*L);
            }
            else if (i == 1 && j == 2)
            {
                return (double eta) => eta * (3.0 * eta - 2.0 * L) / (L*L);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Second order derivative of Hermite functions defined in GetHermite function
        /// </summary>
        /// <param name="i">first index</param>
        /// <param name="j">second index</param>
        /// <param name="L">Length</param>
        /// <returns></returns>
        internal static Func<double, double> GetSecondDerivHermite(int i, int j, double L)
        {
            if (i == 0 && j == 1)
            {
                return (double eta) => -6.0 * (L - 2.0 * eta) / Math.Pow(L, 3.0);
            }
            else if (i == 0 && j == 2)
            {
                return (double eta) => 6.0 * (L - 2.0 * eta) / Math.Pow(L, 3.0);
            }
            else if (i == 1 && j == 1)
            {
                return (double eta) => (6.0 * eta - 4.0 * L) / (L*L);
            }
            else if (i == 1 && j == 2)
            {
                return (double eta) => -2.0 * (L - 3.0 * eta) / (L*L);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        /// <summary>
        /// equation 35 for a single node Bs = Ls * N with Ls derivative operator
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <returns></returns>
        internal static mnl.Matrix<double> GetNiMatrix(int indexNode, double x, double y, double lx, double ly)
        {
            mnl.Matrix<double> NiMatrix = mnl.Matrix<double>.Build.Dense(3, 6);
            NiMatrix[0, 0] = GetN(indexNode, 1, lx, ly)(x, y);

            NiMatrix[1, 1] = GetN(indexNode, 2, lx, ly)(x, y);

            NiMatrix[2, 2] = GetN(indexNode, 3, lx, ly)(x, y);
            NiMatrix[2, 3] = GetN(indexNode, 4, lx, ly)(x, y);
            NiMatrix[2, 4] = GetN(indexNode, 5, lx, ly)(x, y);
            NiMatrix[2, 5] = GetN(indexNode, 6, lx, ly)(x, y);

            return NiMatrix;
        }

        /// <summary>
        /// equation 35 for a single node Bs = Ls * N with Ls derivative operator
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <returns></returns>
        internal static mnl.Matrix<double> GetBsi(int indexNode, double x, double y, double lx, double ly)
        {
            mnl.Matrix<double> bs = mnl.Matrix<double>.Build.Dense(4, 6);
            bs[0, 0] = GetN(indexNode, 1, lx, ly)(x, y);

            bs[1, 1] = GetN(indexNode, 2, lx, ly)(x, y);

            bs[2, 2] = GetdNdx(indexNode, 3, lx, ly)(x, y);
            bs[2, 3] = GetdNdx(indexNode, 4, lx, ly)(x, y);
            bs[2, 4] = GetdNdx(indexNode, 5, lx, ly)(x, y);
            bs[2, 5] = GetdNdx(indexNode, 6, lx, ly)(x, y);

            bs[3, 2] = GetdNdy(indexNode, 3, lx, ly)(x, y);
            bs[3, 3] = GetdNdy(indexNode, 4, lx, ly)(x, y);
            bs[3, 4] = GetdNdy(indexNode, 5, lx, ly)(x, y);
            bs[3, 5] = GetdNdy(indexNode, 6, lx, ly)(x, y);

            return bs;
        }

        /// <summary>
        /// equation 48 for a single node Bg = Lg * N with Lg derivative operator
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <returns></returns>
        internal static mnl.Matrix<double> GetBgi(int indexNode, double x, double y, double lx, double ly)
        {
            mnl.Matrix<double> bg = mnl.Matrix<double>.Build.Dense(6, 6);
            bg[0, 0] = GetdNdx(indexNode, 1, lx, ly)(x, y);

            bg[1, 1] = GetdNdy(indexNode, 2, lx, ly)(x, y);

            bg[2, 0] = GetdNdy(indexNode, 1, lx, ly)(x, y);
            bg[2, 1] = GetdNdx(indexNode, 2, lx, ly)(x, y);

            bg[3, 2] = -GetdNdx2(indexNode, 3, lx, ly)(x, y);
            bg[3, 3] = -GetdNdx2(indexNode, 4, lx, ly)(x, y);
            bg[3, 4] = -GetdNdx2(indexNode, 5, lx, ly)(x, y);
            bg[3, 5] = -GetdNdx2(indexNode, 6, lx, ly)(x, y);

            bg[4, 2] = -GetdNdy2(indexNode, 3, lx, ly)(x, y);
            bg[4, 3] = -GetdNdy2(indexNode, 4, lx, ly)(x, y);
            bg[4, 4] = -GetdNdy2(indexNode, 5, lx, ly)(x, y);
            bg[4, 5] = -GetdNdy2(indexNode, 6, lx, ly)(x, y);

            bg[5, 2] = -2.0 * GetdNdxdy(indexNode, 3, lx, ly)(x, y);
            bg[5, 3] = -2.0 * GetdNdxdy(indexNode, 4, lx, ly)(x, y);
            bg[5, 4] = -2.0 * GetdNdxdy(indexNode, 5, lx, ly)(x, y);
            bg[5, 5] = -2.0 * GetdNdxdy(indexNode, 6, lx, ly)(x, y);

            return bg;
        }
        #endregion
    }
}
