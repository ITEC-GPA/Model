using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Materials;
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
        public enum Glass
        {
            Top,
            Bottom
        }

        #region variables
        double _hc;
        double _G0;
        double _h0;
        double _h1;
        double _h2;
        double _EGlass;
        double _niGlass;

        mnl.Matrix<double> _Dg;
        mnl.Matrix<double> _Ds;
        mnl.Matrix<double> _PlaneStressGlassMatrix;

        mnl.Matrix<double> _kLayer;
        mnl.Matrix<double> _kGlass;

        Node[] _nodesFirstTransformed; //come localnodes ma per prova trasformazione coordinate 
        Node[] _nodesSecondTransformed;
        Func<double, double, mnl.Matrix<double>> _jacobXGlobalToXLocal;

        double[] _length = new double[4];
        
        bool _quadrilateral;
        #endregion

        #region properties
        internal mnl.Matrix<double> Dg => _Dg;
        internal mnl.Matrix<double> Ds => _Ds;
        internal mnl.Matrix<double> KLayer => _kLayer;
        internal mnl.Matrix<double> KGlass => _kGlass;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="G0">shear module of interlayer</param>
        /// <param name="h0">thickness interlayer</param>
        /// <param name="h1">Thickness of top glass</param>
        /// <param name="h2">Thickness of bottom glass</param>
        /// <param name="EGlass">Glass elastic modulus</param>
        /// <param name="niGlass">poisson glass</param>
        /// <param name="quadrilateral">if false, used formulation of arrticle, if true, try to use transformation of coordinates</param>
        public Quad4TripleLaminatedGlass(Node[] nodes, double G0, double h0, double h1, double h2, double EGlass, double niGlass, bool quadrilateral = false) : base(nodes)
        {
            //eq. 51 -> lista dof locali
             /* deltaU = slippage between the glass layer in local x direction 
             * deltaV = slippage between the glass layer in local y direction 
             * w = deflection in local z direction
             * thetaX = rotation along x local direction
             * thetaY = rotation along y local direction
             * psi = ? rotation along z local direction
             */

            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            //displacement w il local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);

            _hc = GetHc(h0, h1, h2); //(2.0 * h0 + h1 + h2) / 2.0; //eq. (14)
            _G0 = G0;
            _h0 = h0;
            _h1 = h1;
            _h2 = h2;
            _EGlass = EGlass;
            _niGlass = niGlass;

            _quadrilateral = quadrilateral;
        }

        public override void BuildMatrix()
        {
            //set local coordinate system
            _nodesLocal = Quad4Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);

            if (_quadrilateral == true) {
                _nodesSecondTransformed = new Node[4];
                _nodesSecondTransformed[0] = new Node(0.0, 0.0, 0);
                _nodesSecondTransformed[1] = new Node(2.0, 0.0, 0);
                _nodesSecondTransformed[2] = new Node(2.0, 2.0, 0);
                _nodesSecondTransformed[3] = new Node(0.0, 2.0, 0);

                /*double x = 0;
                double y = 0;
                double X = FEMUtilities.GetLocalCoordinate2D("x", x, y, Quad4Element.GetShapeFunction, _nodesGlobal);
                double Y = FEMUtilities.GetLocalCoordinate2D("y", x, y, Quad4Element.GetShapeFunction, _nodesGlobal);

                Console.WriteLine("x=" + x + " y=" + y);
                Console.WriteLine("goes to X=" + X + " Y=" + Y);*/

            }
            
            Vector3d globalX = new Vector3d(1, 0, 0);
            Vector3d globalY = new Vector3d(0, 1, 0);
            Vector3d globalZ = new Vector3d(0, 0, 1);

            if (_quadrilateral == false)
            {
                if (_localCoordinateSystem.V1 != globalX || _localCoordinateSystem.V2 != globalY || _localCoordinateSystem.V3 != globalZ)
                {
                    Console.WriteLine("Nodi coordinate globali:");
                    Console.WriteLine(_nodesGlobal[0].Position);
                    Console.WriteLine(_nodesGlobal[1].Position);
                    Console.WriteLine(_nodesGlobal[2].Position);
                    Console.WriteLine(_nodesGlobal[3].Position);
                    throw new NotImplementedException("Elemento finito al momento funzionante solo con assi locali coincidenti con assi globali");
                }

                _length[0] = _nodesLocal[1].Position.X - _nodesLocal[0].Position.X; //_lx = _localNodes[1].Position.X - _localNodes[0].Position.X;
                _length[3] = _nodesLocal[3].Position.Y - _nodesLocal[0].Position.Y; //_ly = _localNodes[3].Position.Y - _localNodes[0].Position.Y;

                #region ControlliGeometrici
                if (_length[0] <= 0 || _length[3] <= 0 /*_lx <= 0 || _ly <= 0*/)
                {
                    throw new Exception("lx or ly <= 0!");
                }

                _length[2] = _nodesLocal[2].Position.X - _nodesLocal[3].Position.X; //lx2
                _length[1] = _nodesLocal[2].Position.Y - _nodesLocal[1].Position.Y; //ly2

                if (_length[0] != _length[2]) //lx2 != _lx
                {
                    throw new Exception("Elemento finito funziona per elementi non rettangolari?");
                }

                if (_length[1] != _length[3]) //ly2 != _ly
                {
                    throw new Exception("Elemento finito funziona per elementi non rettangolari?");
                }
                #endregion
            }
            else
            {
                double X0 = _nodesGlobal[0].Position.X;
                double Y0 = _nodesGlobal[0].Position.Y;
                double Z0 = _nodesGlobal[0].Position.Z;

                _nodesFirstTransformed = new Node[4];
                _nodesFirstTransformed[0] = new Node(_nodesGlobal[0].Position.X - X0, _nodesGlobal[0].Position.Y - Y0, _nodesGlobal[0].Position.Z - Z0);
                _nodesFirstTransformed[1] = new Node(_nodesGlobal[1].Position.X - X0, _nodesGlobal[1].Position.Y - Y0, _nodesGlobal[1].Position.Z - Z0);
                _nodesFirstTransformed[2] = new Node(_nodesGlobal[2].Position.X - X0, _nodesGlobal[2].Position.Y - Y0, _nodesGlobal[2].Position.Z - Z0);
                _nodesFirstTransformed[3] = new Node(_nodesGlobal[3].Position.X - X0, _nodesGlobal[3].Position.Y - Y0, _nodesGlobal[3].Position.Z - Z0);

                double XP1 = FEMUtilities.GetLocalCoordinate2D("x", -1, -1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                double XP2 = FEMUtilities.GetLocalCoordinate2D("x", 1, -1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                double XP3 = FEMUtilities.GetLocalCoordinate2D("x", 1, 1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                double XP4 = FEMUtilities.GetLocalCoordinate2D("x", -1, 1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);

                double YP1 = FEMUtilities.GetLocalCoordinate2D("y", -1, -1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                double YP2 = FEMUtilities.GetLocalCoordinate2D("y", 1, -1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                double YP3 = FEMUtilities.GetLocalCoordinate2D("y", 1, 1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                double YP4 = FEMUtilities.GetLocalCoordinate2D("y", -1, 1, Quad4Element.GetShapeFunction, _nodesFirstTransformed);

                _length[0] = Math.Sqrt(Math.Pow(XP2 - XP1, 2.0) + Math.Pow(YP2 - YP1, 2.0));
                _length[1] = Math.Sqrt(Math.Pow(XP3 - XP2, 2.0) + Math.Pow(YP3 - YP2, 2.0));
                _length[2] = Math.Sqrt(Math.Pow(XP4 - XP3, 2.0) + Math.Pow(YP4 - YP3, 2.0));
                _length[3] = Math.Sqrt(Math.Pow(XP1 - XP4, 2.0) + Math.Pow(YP1 - YP4, 2.0));
            }

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

            _dofGlobalToLocal = mnl.Matrix<double>.Build.DenseIdentity(24); //TODO: aggiornare

                /*Console.WriteLine("dofGlobalToLocalTranspose.");
                    * FemUtilites.WriteMatrix(_dofGlobalToLocal);
                }*/

#endregion

            #region matricesD
            /*double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();*/

            #region Ds - INTERLAYER
            _Ds = GetDs(_G0, _h0, _hc);
            #endregion

            #region Dg - GLASS
            _PlaneStressGlassMatrix = IsotropicFemMaterial.GetMatrixPlaneStress(_EGlass,_niGlass);
            _Dg = GetDg(_h1, _h2, _PlaneStressGlassMatrix);
            #endregion
            #endregion

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(24, 24);

            _jacobXGlobalToXLocal = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesFirstTransformed); //Transform X and Y in x and y
            mnl.Matrix<double> fKLayer(double csi, double eta)
            {
                if (_quadrilateral == true)
                {
                    double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _nodesSecondTransformed);
                    double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _nodesSecondTransformed);

                    double X = FEMUtilities.GetLocalCoordinate2D("x", x - 1.0, y - 1.0, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                    double Y = FEMUtilities.GetLocalCoordinate2D("y", x - 1.0, y - 1.0, Quad4Element.GetShapeFunction, _nodesFirstTransformed);

                    mnl.Matrix<double> Bs = GetBs(X, Y);

                    return Bs.Transpose() * _Ds * Bs * _jacobXGlobalToXLocal(x, y).Determinant();
                }
                else
                {
                    double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _nodesLocal);
                    double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _nodesLocal);

                    mnl.Matrix<double> Bs = GetBs(x, y);

                    return Bs.Transpose() * _Ds * Bs;
                }
            }

            mnl.Matrix<double> fKGlass(double csi, double eta)
            {
                //usata solo per prova è possibile cancellarla al termine
                double f(double input1, double input2)
                {
                    return 1.0 + 0.0 * Math.Pow(input1, 1.0) + 0.0 * Math.Pow(input2, 1.0);
                }

                if (_quadrilateral == true)
                {
                    double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _nodesSecondTransformed);
                    double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _nodesSecondTransformed);

                    double X = FEMUtilities.GetLocalCoordinate2D("x", x - 1.0, y - 1.0, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                    double Y = FEMUtilities.GetLocalCoordinate2D("y", x - 1.0, y - 1.0, Quad4Element.GetShapeFunction, _nodesFirstTransformed);

                    /*Console.WriteLine("csi = " + csi + " eta = " + eta);
                    Console.WriteLine("x = " + x + " y = " + y);
                    Console.WriteLine("X = " + X + " Y = " + Y);
                    Console.WriteLine("detJ X to x = " + _jacobXGlobalToXLocal(x, y).Determinant());*/

                    mnl.Matrix<double> Bg = GetBg(X, Y);

                    return Bg.Transpose() * _Dg * Bg * _jacobXGlobalToXLocal(x, y).Determinant();

                    /*mnl.Matrix<double> Bg = mnl.Matrix<double>.Build.Dense(1,1);
                    Bg[0,0] = f(X,Y);
                    Console.WriteLine("f(X,Y) = "+Bg[0, 0]);

                    return Bg * jacobXGlobalToXLocal(x, y).Determinant();*/
                } else
                {
                    double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _nodesLocal);
                    double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _nodesLocal);

                    mnl.Matrix<double> Bg = GetBg(x, y);

                    return Bg.Transpose() * _Dg * Bg;

                    /*mnl.Matrix<double> Bg = mnl.Matrix<double>.Build.Dense(1, 1);
                    Bg[0, 0] = f(x,y);*/
                    //Console.WriteLine("TLG classic f(x=" + x + ",y=" + y + ")=" + Bg[0, 0]);

                    //return Bg;
                }
            }

            Func<double, double, mnl.Matrix<double>> jacob;
            if (_quadrilateral == false)
            {
                jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesLocal);
            } else
            {
                jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesSecondTransformed);
            }

            _kLayer = GaussIntegration.IntegrationQuadrilateral(fKLayer, jacob, 16); // 16 è valore corretto
            _kGlass = GaussIntegration.IntegrationQuadrilateral(fKGlass, jacob, 16); // 16 è valore corretto

            _kElementLocalCoord = _kLayer + _kGlass;

            /*Console.WriteLine("length");
            Console.WriteLine(_length[0]);
            Console.WriteLine(_length[1]);
            Console.WriteLine(_length[2]);
            Console.WriteLine(_length[3]);*/
        }

        /// <summary>
        /// Eq. 63  f = integral(N^T * q)
        /// q = [0, 0 , p]^T
        /// </summary>
        /// <returns></returns>
        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> fLocalCoord = mnl.Vector<double>.Build.Dense(6 * Nodes.Length);
            foreach (IPlateLoadCaseAttribute iAttribute in _attributesLoadCase)
            {
                if (iAttribute is PlatePressureAttribute)
                {
                    PlatePressureAttribute attribute = (PlatePressureAttribute)iAttribute;
                    //calculation of pressures in local coordinate system of the element
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
                        if (_quadrilateral == false)
                        {
                            double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _nodesLocal);
                            double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _nodesLocal);

                            return GetNMatrix(x, y).Transpose() * q;
                        } else
                        {
                            double x = FEMUtilities.GetLocalCoordinate2D("x", csi, eta, Quad4Element.GetShapeFunction, _nodesSecondTransformed);
                            double y = FEMUtilities.GetLocalCoordinate2D("y", csi, eta, Quad4Element.GetShapeFunction, _nodesSecondTransformed);

                            double X = FEMUtilities.GetLocalCoordinate2D("x", x - 1.0, y - 1.0, Quad4Element.GetShapeFunction, _nodesFirstTransformed);
                            double Y = FEMUtilities.GetLocalCoordinate2D("y", x - 1.0, y - 1.0, Quad4Element.GetShapeFunction, _nodesFirstTransformed);

                            return GetNMatrix(x, y).Transpose() * q * _jacobXGlobalToXLocal(x, y).Determinant();
                        }                       
                    }

                    var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesLocal);
                    mnl.Matrix<double> f = GaussIntegration.IntegrationQuadrilateral(NtTraspQ, jacob, 9);
                    for (int i = 0; i < f.RowCount; i++)
                    {
                        fLocalCoord[i] = f[i, 0];
                    }
                }
            }
            
            return fLocalCoord;
        }

        public override mnl.Matrix<double> GetB(double csi, double eta)
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
                mnl.Matrix<double> nNode = GetNiMatrix(indexNode, x, y);

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

                mnl.Matrix<double> bsNode = GetBsi(indexNode, x, y);

                Bs = Bs.Append(bsNode);
            }
            return Bs;
            //return GetBsi(1, x, y, _lx, _ly);//TODO: cancellare
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
#if DEBUG
                //Console.WriteLine("GetBg: x = " + x + " y = " + y);
#endif
                mnl.Matrix<double> bgNode = GetBgi(indexNode, x, y);
                Bg = Bg.Append(bgNode);
            }
            return Bg;
            //return GetBgi(1, x, y, _lx, _ly);
        }

        //TODO: Da ottimizzare/scrivere
        public void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            
            //TODO: "aggiornare";
            throw new NotImplementedException();
        }

        #region Result

        #region Glass
        /// <summary>
        /// curvatures
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>kxx, kyy, kxy in vector of double</returns>
        public mnl.Vector<double> GetGlassCurvatures(double x, double y, double[] globalDisplacementNodes)
        {
            mnl.Vector<double> curvatures = mnl.Vector<double>.Build.Dense(3);

            var pseudoStrain = GetPseudoStrainGlass(x, y, globalDisplacementNodes);

            curvatures[0] = pseudoStrain[3]; //kxx
            curvatures[1] = pseudoStrain[4]; //kyy
            curvatures[2] = pseudoStrain[5]; //kxy

            return curvatures;
        }

        /// <summary>
        /// Bending moment mxx myy mxy taken by the glasses, Total bending: mxx + fx * hc
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns></returns>
        public mnl.Vector<double> GetGlassBending(double x, double y, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetPseudoStrainGlass(x, y, globalDisplacementNodes);
            var pseudoStress = GetPseudoStressGlass(pseudoStrain);

            mnl.Vector<double> bending = mnl.Vector<double>.Build.Dense(3);

            bending[0] = pseudoStress[3]; //mxx
            bending[1] = pseudoStress[4]; //myy
            bending[2] = pseudoStress[5]; //mxy

            return bending;
        }

        public mnl.Vector<double> GetGlassForces(double x, double y, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetPseudoStrainGlass(x, y, globalDisplacementNodes);
            var pseudoStress = GetPseudoStressGlass(pseudoStrain);

            mnl.Vector<double> forces = mnl.Vector<double>.Build.Dense(3);

            forces[0] = pseudoStress[0]; //fxx
            forces[1] = pseudoStress[1]; //fyy
            forces[2] = pseudoStress[2]; //fxy

            return forces;
        }

        /// <summary>
        /// eq 44
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="globalDisplacementsNodes"></param>
        /// <returns>pseudoStrainGlass = [ddeltaUdx, ddeltaVdy, ddeltaUdx + ddeltaUdy, -d2wdx2 , -d2wdy2, -2.0 * d2wdxdy] as vector of double</returns>
        public mnl.Vector<double> GetPseudoStrainGlass(double x, double y, double[] globalDisplacementsNodes)
        {
            //eq. 44: pseudoStrainGlass = [ddeltaUdx, ddeltaVdy, ddeltaUdx + ddeltaUdy, -d2wdx2 , -d2wdy2, -2.0 * d2wdxdy]
            var Bg = GetBg(x, y);
            var eg = Bg * mnl.Vector<double>.Build.DenseOfArray(globalDisplacementsNodes);
            return eg;
        }

        /// <summary>
        /// Dg * pseudostrain
        /// </summary>
        /// <param name="pseudoStrain"></param>
        /// <returns></returns>
        public mnl.Vector<double> GetPseudoStressGlass(mnl.Vector<double> pseudoStrain)
        {
            return _Dg * pseudoStrain;
        }

        /// <summary>
        /// eq 24
        /// </summary>
        /// <param name="g">Top or bottom</param>
        /// <param name="face">Top, moddle or bottom</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="globalDisplacementNodes">displacement of the element</param>
        /// <returns>epsilon_x, epslilon_y, tau_xy, kxx, kyy, kxy as vector of double</returns>
        public mnl.Vector<double> GetStrainGlass(Glass g, Face face, double x, double y, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetPseudoStrainGlass(x,y,globalDisplacementNodes);
            var pseudoStress = GetPseudoStressGlass(pseudoStrain);

            mnl.Vector<double> strains = mnl.Vector<double>.Build.Dense(3); //eq 24 epsilon_x, epslilon_y, gamma_xy

            double k;
            double h;
            if (g == Glass.Top)
            {
                k = 2; //glass in +z
                h = _h1;
            } else
            {
                k = 1; //glass in -z
                h = _h2;
            }
            double strainsXX = Math.Pow(-1.0, k) * pseudoStress[0] / (_EGlass * h); //epsilon x
            double strainsYY = Math.Pow(-1.0, k) * pseudoStress[1] / (_EGlass * h); //epsilon y
            double strainsXY = Math.Pow(-1.0, k) * 2.0 * (1.0 + _niGlass) / (_EGlass * h) * pseudoStress[2]; //gamma_xy

            var curvatures = GetGlassCurvatures(x,y, globalDisplacementNodes);

            if (face == Face.Top)
            {
                strainsXX += + h / 2.0 * curvatures[0];
                strainsYY += h / 2.0 * curvatures[1];
                strainsXY += h / 2.0 * curvatures[2];
            }
            else if (face == Face.Bottom)
            {
                strainsXX -= h / 2.0 * curvatures[0];
                strainsYY -= h / 2.0 * curvatures[1];
                strainsXY -= h / 2.0 * curvatures[2];
            }

            strains[0] = strainsXX;
            strains[1] = strainsYY;
            strains[2] = strainsXY;

            return strains;
        }

        /// <summary>
        /// get stress in the selected glass
        /// </summary>
        /// <param name="g">top or bottom</param>
        /// <param name="face">Top, middle, bottom</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>sigma_xx, sigma_yy, tau_xy as vector of double</returns>
        public mnl.Vector<double> GetStressGlass(Glass g, Face face, double x, double y, double[] globalDisplacementNodes)
        {
            var strains = GetStrainGlass(g, face, x,y, globalDisplacementNodes);
            var stress = _PlaneStressGlassMatrix * strains;

            return stress;
        }
        #endregion

        #region Interlayer
        /// <summary>
        /// eq 33
        /// </summary>
        /// <param name="globalDisplacementsNodes"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns> pseudoStrainInterlayer = [deltaU, deltaV, dwdx , dwdy] as vector of double</returns>
        public mnl.Vector<double> GetPseudoStrainInterlayer(double x, double y, double[] globalDisplacementsNodes)
        {
            //eq. 33: pseudoStrainInterlayer = [deltaU, deltaV, dwdx , dwdy]
            var Bs = GetBs(x, y);
            var es = Bs * mnl.Vector<double>.Build.DenseOfArray(globalDisplacementsNodes);

            return es;
        }

        /// <summary>
        /// Ds * pseudostrain
        /// </summary>
        /// <param name="pseudoStrain"></param>
        /// <returns>pseudoStressInterlayer = tau_zx, tau_zy, valore non identificato, valore non identificato</returns>
        public mnl.Vector<double> GetPseudoStressInterlayer(mnl.Vector<double> pseudoStrain)
        {
            return _Ds * pseudoStrain;
        }

        /// <summary>
        /// strain interlayer using equations 12 and 15
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>gamma_xz, gamma_yz</returns>
        public mnl.Vector<double> GetStrainInterlayer(double x, double y, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetPseudoStrainInterlayer(x, y, globalDisplacementNodes);

            mnl.Vector<double> strain = mnl.Vector<double>.Build.Dense(2);
            strain[0] = 1.0 / _h0 * (pseudoStrain[0] + _hc * pseudoStrain[2]);
            strain[1] = 1.0 / _h0 * (pseudoStrain[1] + _hc * pseudoStrain[3]);

            return strain;
        }

        /// <summary>
        /// Stress in interlayer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>tau_zx, tau_zy</returns>
        public mnl.Vector<double> GetStressInterlayer(double x, double y, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetPseudoStrainInterlayer(x, y, globalDisplacementNodes);
            var pseudoStress = GetPseudoStressInterlayer(pseudoStrain);

            mnl.Vector<double> stress = mnl.Vector<double>.Build.Dense(2);
            stress[0] = pseudoStress[0];
            stress[1] = pseudoStress[1];

            return stress;
        }

        public mnl.Vector<double> GetBendingInterlayer(double x, double y, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetPseudoStrainInterlayer(x, y, globalDisplacementNodes);
            var pseudoStress = GetPseudoStressInterlayer(pseudoStrain);

            mnl.Vector<double> bending = mnl.Vector<double>.Build.Dense(2);
            bending[0] = pseudoStress[2];
            bending[1] = pseudoStress[3];

            return bending;
        }
        #endregion
        #endregion

        #region PrivateInternalFunctions

        /// <summary>
        /// Select in globalDisplacementsNodes the displacement of the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="globalDisplacementsNodes"></param>
        /// <returns></returns>
        private mnl.Vector<double> GetDisplacementsNode(Node node, double[] globalDisplacementsNodes)
        {
            mnl.Vector<double> displPoint = mnl.Vector<double>.Build.Dense(6);
            int indexNode = _nodesGlobal.ToList().IndexOf(node);
            
            int start = indexNode * 6;
            int counter = 0;
            for (int i = start; i < start + 6; i++)
            {
                displPoint[counter] = globalDisplacementsNodes[i];
                counter++;
            }
            return displPoint;
        }

#region ShapeFunction
        /// <summary>
        /// Shape functions for this element
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">1 to 6</param>
        /// <returns></returns>
        internal Func<double, double, double> GetN(int indexNode, int indexDisplacement)
        {
            Dictionary<int, (int, int)> indexes = GetIndices();

            int a = indexes[indexNode].Item1;
            int b = indexes[indexNode].Item2;

            double lx, ly;
            GetLxLy(indexNode, out lx, out ly);

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
                return (double x, double y) => (lx - x) * y / (lx * ly);
            } else if (indexDisplacement == 3)
            {
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                return (double x, double y) => -GetHermite(1, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
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
        /// <returns></returns>
        internal Func<double, double, double> GetdNdx(int indexNode, int indexDisplacement)
        {
            Dictionary<int, (int, int)> indexes = GetIndices();

            int a = indexes[indexNode].Item1;
            int b = indexes[indexNode].Item2;

            double lx, ly;
            GetLxLy(indexNode, out lx, out ly);

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
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                return (double x, double y) => -GetFirstDerivHermite(1, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
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
        /// <returns></returns>
        internal Func<double, double, double> GetdNdy(int indexNode, int indexDisplacement)
        {
            Dictionary<int, (int, int)> indexes = GetIndices();

            int a = indexes[indexNode].Item1;
            int b = indexes[indexNode].Item2;

            double lx, ly;
            GetLxLy(indexNode, out lx, out ly);

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
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetFirstDerivHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                return (double x, double y) => -GetHermite(1, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
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
        /// <returns></returns>
        internal Func<double, double, double> GetdNdxdy(int indexNode, int indexDisplacement)
        {
            Dictionary<int, (int, int)> indexes = GetIndices();

            int a = indexes[indexNode].Item1;
            int b = indexes[indexNode].Item2;

            double lx, ly;
            GetLxLy(indexNode, out lx, out ly);

            if (indexDisplacement == 3)
            {                 
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                return (double x, double y) => GetFirstDerivHermite(0, a, lx)(x) * GetFirstDerivHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                return (double x, double y) => -GetFirstDerivHermite(1, a, lx)(x) * GetFirstDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
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
        /// <returns></returns>
        internal Func<double, double, double> GetdNdx2(int indexNode, int indexDisplacement)
        {
            Dictionary<int, (int, int)> indexes = GetIndices();

            int a = indexes[indexNode].Item1;
            int b = indexes[indexNode].Item2;

            double lx, ly;
            GetLxLy(indexNode, out lx, out ly);

            if (indexDisplacement == 3)
            {
                return (double x, double y) => GetSecondDerivHermite(0, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                return (double x, double y) => GetSecondDerivHermite(0, a, lx)(x) * GetHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                return (double x, double y) => -GetSecondDerivHermite(1, a, lx)(x) * GetHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
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
        /// <returns></returns>
        internal Func<double, double, double> GetdNdy2(int indexNode, int indexDisplacement)
        {
            Dictionary<int, (int, int)> indexes = GetIndices();

            int a = indexes[indexNode].Item1;
            int b = indexes[indexNode].Item2;

            double lx, ly;
            GetLxLy(indexNode, out lx, out ly);

            if (indexDisplacement == 3)
            {
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetSecondDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 4)
            {
                return (double x, double y) => GetHermite(0, a, lx)(x) * GetSecondDerivHermite(1, b, ly)(y);
            }
            else if (indexDisplacement == 5)
            {
                return (double x, double y) => -GetHermite(1, a, lx)(x) * GetSecondDerivHermite(0, b, ly)(y);
            }
            else if (indexDisplacement == 6)
            {
                return (double x, double y) => GetHermite(1, a, lx)(x) * GetSecondDerivHermite(1, b, ly)(y);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static Dictionary<int, (int, int)> GetIndices()
        {
            Dictionary<int, (int, int)> indexes = new Dictionary<int, (int, int)>();
            //eq. 59    i|  a  b
            indexes.Add(1, (1, 1));
            indexes.Add(2, (2, 1));
            indexes.Add(3, (2, 2));
            indexes.Add(4, (1, 2));
            return indexes;
        }
#endregion

#region Hermite
        /// <summary>
        /// Hermite polynomial used in this element H_(i,j) defined in eqts. 57
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
        /// equation 54
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal mnl.Matrix<double> GetNiMatrix(int indexNode, double x, double y)
        {
            mnl.Matrix<double> NiMatrix = mnl.Matrix<double>.Build.Dense(3, 6);
            NiMatrix[0, 0] = GetN(indexNode, 1)(x, y);

            NiMatrix[1, 1] = GetN(indexNode, 2)(x, y);

            NiMatrix[2, 2] = GetN(indexNode, 3)(x, y);
            NiMatrix[2, 3] = GetN(indexNode, 4)(x, y);
            NiMatrix[2, 4] = GetN(indexNode, 5)(x, y);
            NiMatrix[2, 5] = GetN(indexNode, 6)(x, y);

            return NiMatrix;
        }

        /// <summary>
        /// equation 35 for a single node Bs = Ls * N with Ls derivative operator
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBsi(int indexNode, double x, double y)
        {
            mnl.Matrix<double> bs = mnl.Matrix<double>.Build.Dense(4, 6);
            bs[0, 0] = GetN(indexNode, 1)(x, y);

            bs[1, 1] = GetN(indexNode, 2)(x, y);

            bs[2, 2] = GetdNdx(indexNode, 3)(x, y);
            bs[2, 3] = GetdNdx(indexNode, 4)(x, y);
            bs[2, 4] = GetdNdx(indexNode, 5)(x, y);
            bs[2, 5] = GetdNdx(indexNode, 6)(x, y);

            bs[3, 2] = GetdNdy(indexNode, 3)(x, y);
            bs[3, 3] = GetdNdy(indexNode, 4)(x, y);
            bs[3, 4] = GetdNdy(indexNode, 5)(x, y);
            bs[3, 5] = GetdNdy(indexNode, 6)(x, y);

            return bs;
        }

        /// <summary>
        /// equation 48 for a single node Bg = Lg * N with Lg derivative operator
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBgi(int indexNode, double x, double y)
        {
            double lx, ly;
            GetLxLy(indexNode, out lx, out ly);

            mnl.Matrix<double> bg = mnl.Matrix<double>.Build.Dense(6, 6);
            bg[0, 0] = GetdNdx(indexNode, 1)(x, y);

            bg[1, 1] = GetdNdy(indexNode, 2)(x, y);

            bg[2, 0] = GetdNdy(indexNode, 1)(x, y);
            bg[2, 1] = GetdNdx(indexNode, 2)(x, y);

            bg[3, 2] = -GetdNdx2(indexNode, 3)(x, y);
            bg[3, 3] = -GetdNdx2(indexNode, 4)(x, y);
            bg[3, 4] = -GetdNdx2(indexNode, 5)(x, y);
            bg[3, 5] = -GetdNdx2(indexNode, 6)(x, y);

            bg[4, 2] = -GetdNdy2(indexNode, 3)(x, y);
            bg[4, 3] = -GetdNdy2(indexNode, 4)(x, y);
            bg[4, 4] = -GetdNdy2(indexNode, 5)(x, y);
            bg[4, 5] = -GetdNdy2(indexNode, 6)(x, y);

            bg[5, 2] = -2.0 * GetdNdxdy(indexNode, 3)(x, y);
            bg[5, 3] = -2.0 * GetdNdxdy(indexNode, 4)(x, y);
            bg[5, 4] = -2.0 * GetdNdxdy(indexNode, 5)(x, y);
            bg[5, 5] = -2.0 * GetdNdxdy(indexNode, 6)(x, y);

            return bg;
        }

        /// <summary>
        /// Equation 32
        /// </summary>
        /// <param name="G0"></param>
        /// <param name="h0"></param>
        /// <param name="hc"></param>
        /// <returns></returns>
        internal static mnl.Matrix<double> GetDs(double G0, double h0, double hc)
        {
            var Ds = mnl.Matrix<double>.Build.Dense(4, 4); //equation (32)
            Ds[0, 0] = 1.0;
            Ds[0, 2] = hc;

            Ds[1, 1] = 1.0;
            Ds[1, 3] = hc;

            Ds[2, 0] = hc;
            Ds[2, 2] = hc * hc;

            Ds[3, 1] = hc;
            Ds[3, 3] = hc * hc;

            Ds = G0 / Math.Pow(h0, 2.0) * Ds; //Ds = G0 / h0 * Ds;

            return Ds;
        }

        /// <summary>
        /// Equation 45
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        internal static mnl.Matrix<double> GetDg(double h1, double h2, mnl.Matrix<double> C)
        {
            var Dg = mnl.Matrix<double>.Build.Dense(6, 6); //equation (45)

            double factor1 = (h1 * h2) / (h1 + h2);
            double factor2 = (Math.Pow(h1, 3.0) + Math.Pow(h2, 3.0)) / 12.0;

            /*#if DEBUG
            Console.WriteLine("Dg factor 1 = " + factor1);
            Console.WriteLine("Dg factor 2 = " + factor2);
#endif*/

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

            return Dg;
        }

        /// <summary>
        /// eq. 14
        /// </summary>
        /// <param name="h0"></param>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        internal static double GetHc(double h0, double h1, double h2)
        {
            return (2.0 * h0 + h1 + h2) / 2.0;
        }

        private void GetLxLy(int indexNode, out double lx, out double ly)
        {
            switch (indexNode)
            {
                case 1:
                    lx = _length[0];
                    ly = _length[3];
                    break;
                case 2:
                    lx = _length[0];
                    ly = _length[1];
                    break;
                case 3:
                    lx = _length[2];
                    ly = _length[1];
                    break;
                case 4:
                    lx = _length[2];
                    ly = _length[3];
                    break;
                default:
                    throw new ArgumentOutOfRangeException("node 1 to 4");
            }
        }
#endregion
    }
}
