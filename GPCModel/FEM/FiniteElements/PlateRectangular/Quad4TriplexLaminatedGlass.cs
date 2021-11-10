using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Materials;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// A plate finite element for modelling of tripled laminated glass and comparison with other computational method
    /// Ivanov, Velchev, Georgiev, Sadowki - 2015
    /// </summary>
    public class Quad4TriplexLaminatedGlass : Plate
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

        mnl.Matrix<double> _kLocalUnordered;

        Quad4DK _el;
        #endregion

        #region properties
        internal mnl.Matrix<double> Dg => _Dg;
        internal mnl.Matrix<double> Ds => _Ds;
        internal mnl.Matrix<double> KLayer => _kLayer;
        internal mnl.Matrix<double> KGlass => _kGlass;

        internal mnl.Matrix<double> KLocalUnordered => _kLocalUnordered;
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
        public Quad4TriplexLaminatedGlass(Node[] nodes, double G0, double h0, double h1, double h2, double EGlass, double niGlass) : base(nodes)
        {
            //eq. 51 -> lista dof locali
             /* deltaU = slippage between the glass layer in local x direction 
             * deltaV = slippage between the glass layer in local y direction 
             * w = deflection in local z direction
             * thetaX = rotation along x local direction
             * thetaY = rotation along y local direction
             */

            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            //displacement w il local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);
            //slippage
            DOF.Add(Solver.DOF.DDX);
            DOF.Add(Solver.DOF.DDY);
            DOF.Add(Solver.DOF.DDZ);

            _G0 = G0;
            _h0 = h0;
            _h1 = h1;
            _h2 = h2;
            _EGlass = EGlass;
            _niGlass = niGlass;
            _hc = GetHc(); //(2.0 * h0 + h1 + h2) / 2.0; //eq. (14)

            _PlaneStressGlassMatrix = IsotropicFemMaterial.GetMatrixPlaneStress(_EGlass, _niGlass);

            _el = new Quad4DK(_nodesGlobal);
        }

        public override void BuildMatrix()
        {
            //set local coordinate system
            _nodesLocal = Quad4Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);

            /*Console.WriteLine(_localCoordinateSystem.V1);
            Console.WriteLine(_localCoordinateSystem.V2);
            Console.WriteLine(_localCoordinateSystem.V3);*/

            Vector3d globalX = new Vector3d(1, 0, 0);
            Vector3d globalY = new Vector3d(0, 1, 0);
            Vector3d globalZ = new Vector3d(0, 0, 1);

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(9*4, 5*4); //(DDX, DDY, DDZ, DX, DY, DZ, RX, RY, RZ) * 4 nodes - (ddx, ddy, w, rx, ry) * 4 nodes

            Vector3d localX = LocalCoordinateSystem.V1;
            Vector3d localY = LocalCoordinateSystem.V2;
            Vector3d localZ = LocalCoordinateSystem.V3;

            int dimRow = 9;
            int dimCol = 5;

            double xX = localX.DotProduct(globalX);
            double xY = localX.DotProduct(globalY);
            double xZ = localX.DotProduct(globalZ);

            double yX = localY.DotProduct(globalX);
            double yY = localY.DotProduct(globalY);
            double yZ = localY.DotProduct(globalZ);

            double zX = localZ.DotProduct(globalX);
            double zY = localZ.DotProduct(globalY);
            double zZ = localZ.DotProduct(globalZ);

            for (int i = 0; i < _nodesLocal.Count(); i++)
            {
                #region localToGlobalNode
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

                //local node1 slip-x and slip-y in global coordinate
                dofGlobalToLocalTranspose[i * dimRow + 6, i * dimCol + 3] = xX;
                dofGlobalToLocalTranspose[i * dimRow + 6, i * dimCol + 4] = yX;

                dofGlobalToLocalTranspose[i * dimRow + 7, i * dimCol + 3] = xY;
                dofGlobalToLocalTranspose[i * dimRow + 7, i * dimCol + 4] = yY;

                dofGlobalToLocalTranspose[i * dimRow + 8, i * dimCol + 3] = xZ;
                dofGlobalToLocalTranspose[i * dimRow + 8, i * dimCol + 4] = yZ;
            }
            #endregion
            
            _dofGlobalToLocal = dofGlobalToLocalTranspose.Transpose();

            /*Console.WriteLine("dofGlobalToLocalTranspose:");
            FEMUtilities.WriteMatrix(dofGlobalToLocalTranspose);*/
            #endregion

            #region matricesD
            #region Ds - INTERLAYER
            _Ds = GetDs();
            #endregion

            #region Dg - GLASS
            _Dg = GetDg();
            //FEMUtilities.WriteMatrix("Dg", _Dg);
            #endregion
            #endregion

            //calculation of kelement using gauss quadrature
            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(5 * 4, 5 * 4);

            mnl.Matrix<double> fKLayer(double csi, double eta)
            {
                mnl.Matrix<double> Bs = GetBs(csi, eta);

                return Bs.Transpose() * _Ds * Bs; //equation 36
            }

            mnl.Matrix<double> fKGlass(double csi, double eta)
            {
                mnl.Matrix<double> Bg = GetBg(csi, eta);

                return Bg.Transpose() * _Dg * Bg; //equation 49
            }

            /*FEMUtilities.WriteMatrix("Bs(csi=-1, eta=-1)", GetBs(-1, -1));
            FEMUtilities.WriteMatrix("Bs(csi=1, eta=-1)", GetBs(1, -1));
            FEMUtilities.WriteMatrix("Bs(csi=1, eta=1)", GetBs(1, 1));
            FEMUtilities.WriteMatrix("Bs(csi=-1, eta=1)", GetBs(-1, 1));*/

            //FEMUtilities.WriteMatrix("Bg(csi=-0.57, eta=-0.57", GetBg(-0.577350269189626, -0.577350269189626), "F4");
            //FEMUtilities.WriteMatrix("Bs(csi=-0.57, eta=-0.57", GetBs(-0.577350269189626, -0.577350269189626));

            //FEMUtilities.WriteMatrix("Bg(csi=-0.57, eta=0.57", GetBg(-0.577350269189626, 0.577350269189626), "F4");
            //FEMUtilities.WriteMatrix("Bs(csi=0.57, eta=-0.57", GetBs(0.577350269189626, -0.577350269189626));

            //FEMUtilities.WriteMatrix("Bg(csi=-0.57, eta=-0.57", GetBg(0.577350269189626, -0.577350269189626), "F4");
            //FEMUtilities.WriteMatrix("Bs(csi=-0.57, eta=0.57", GetBs(-0.577350269189626, 0.577350269189626));

            //FEMUtilities.WriteMatrix("Bg(csi=-0.57, eta=-0.57", GetBg(0.577350269189626, 0.577350269189626), "F4");
            //FEMUtilities.WriteMatrix("Bs(csi=0.57, eta=0.57", GetBs(0.577350269189626, 0.577350269189626));

            Func<double, double, mnl.Matrix<double>> jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesLocal);

            _kLayer = OldGaussIntegration.IntegrationQuadrilateral(fKLayer, jacob, 4);
            _kGlass = OldGaussIntegration.IntegrationQuadrilateral(fKGlass, jacob, 4);

            _kLocalUnordered = _kLayer + _kGlass; //equation 28

            #region riordinoGDLPerAssemblaggio
            //riordino gradi di libertà per trasformazione in coordinate globali e assemblaggio
            //la matrice ottenuta ha i seguenti gradi di libertà: ddx, ddy, w, rx, ry
            //la si riordina in: w, rx, ry, ddx, ddy
            Solver.DOF[] dofNode = new Solver.DOF[5];
            dofNode[0] = Solver.DOF.DDX;
            dofNode[1] = Solver.DOF.DDY;
            dofNode[2] = Solver.DOF.DZ;
            dofNode[3] = Solver.DOF.RX;
            dofNode[4] = Solver.DOF.RY;

            Dictionary<Tuple<string, string>, Tuple<int, int>> legenda = new Dictionary<Tuple<string, string>, Tuple<int, int>>();
            for (int ni = 0; ni < _nodesLocal.Count(); ni++)
            {
                for (int nj = 0; nj < _nodesLocal.Count(); nj++)
                {
                    for (int i = 0; i < dofNode.Length; i++)
                    {
                        for (int j = 0; j < dofNode.Length; j++)
                        {
                            legenda.Add(new Tuple<string, string>(ni + " " + dofNode[i].ToString(), nj + " " + dofNode[j].ToString()), new Tuple<int, int>(ni * 5 + i, nj * 5 + j));
                        }
                    }
                }
            }

            Solver.DOF[] orderedDofNode = new Solver.DOF[5];
            orderedDofNode[0] = Solver.DOF.DZ;
            orderedDofNode[1] = Solver.DOF.RX;
            orderedDofNode[2] = Solver.DOF.RY;
            orderedDofNode[3] = Solver.DOF.DDX;
            orderedDofNode[4] = Solver.DOF.DDY;

            for (int ni = 0; ni < _nodesLocal.Count(); ni++)
            {
                for (int nj = 0; nj < _nodesLocal.Count(); nj++)
                {
                    for (int i = 0; i < orderedDofNode.Length; i++)
                    {
                        for (int j = 0; j < orderedDofNode.Length; j++)
                        {
                            var position = legenda[new Tuple<string, string>(ni + " " + orderedDofNode[i].ToString(), nj + " " + orderedDofNode[j].ToString())];
                            //Console.WriteLine(ni + " " + orderedDofNode[i].ToString() +" " + nj + " " + orderedDofNode[j].ToString() + " -> "  + position.Item1 + "," +position.Item2);
                            _kElementLocalCoord[ni * 5 + i, nj * 5 + j] = _kLocalUnordered[position.Item1, position.Item2];
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Eq. 63  f = integral(N^T * q)
        /// q = [0, 0 , p]^T
        /// </summary>
        /// <returns></returns>
        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> fLocalCoord = mnl.Vector<double>.Build.Dense(5 * Nodes.Length);
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

                    var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesLocal);

                    #region DKT
                    mnl.Matrix<double> Np(double csi, double eta)
                    {
                        var Ni = mnl.Matrix<double>.Build.Dense(4,1);
                        Ni[0,0] = Quad4Element.GetShapeFunction(1, csi, eta);
                        Ni[1,0] = Quad4Element.GetShapeFunction(2, csi, eta);
                        Ni[2,0] = Quad4Element.GetShapeFunction(3, csi, eta);
                        Ni[3,0] = Quad4Element.GetShapeFunction(4, csi, eta);

                        return Ni * pz;
                    }
                    mnl.Matrix<double> fDKT = OldGaussIntegration.IntegrationQuadrilateral(Np, jacob, 9);
                    #endregion

                    /*for (int i = 0; i < f.RowCount; i++)
                    {
                        fLocalCoord[i] = f[i, 0];
                    }*/

                    fLocalCoord[0] = fDKT[0, 0]; //node 1
                    fLocalCoord[5] = fDKT[1, 0]; //node 2
                    fLocalCoord[10] = fDKT[2, 0]; //node 3
                    fLocalCoord[15] = fDKT[3, 0]; //node 4
                }
            }
            
            return fLocalCoord;
        }

        /// <summary>
        /// eq. 35 computed for all nodes
        /// </summary>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBs(double csi, double eta)
        {
            mnl.Matrix<double> Bs = mnl.Matrix<double>.Build.Dense(4, 0);
            for (int indexNode = 1; indexNode <= 4; indexNode++)
            {
                mnl.Matrix<double> bsNode = GetBsi(indexNode, csi, eta);

                Bs = Bs.Append(bsNode);
            }
            return Bs;
        }

        /// <summary>
        /// eq. 48 computed for all nodes
        /// </summary>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBg(double csi, double eta)
        {
            mnl.Matrix<double> Bg = mnl.Matrix<double>.Build.Dense(6, 0);
            for (int indexNode = 1; indexNode <= 4; indexNode++)
            {
                #if DEBUG
                    //Console.WriteLine("GetBg: x = " + x + " y = " + y);
                #endif
                mnl.Matrix<double> bgNode = GetBgi(indexNode, csi, eta);
                Bg = Bg.Append(bgNode);
            }
            return Bg;
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
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>kxx, kyy, kxy in vector of double</returns>
        public mnl.Vector<double> GetGlassLocalCurvatures(double csi, double eta, double[] globalDisplacementNodes)
        {
            mnl.Vector<double> curvatures = mnl.Vector<double>.Build.Dense(3);

            var pseudoStrain = GetGlassLocalPseudoStrains(csi, eta, globalDisplacementNodes);

            curvatures[0] = pseudoStrain[3]; //kxx
            curvatures[1] = pseudoStrain[4]; //kyy
            curvatures[2] = pseudoStrain[5]; //kxy

            return curvatures;
        }

        /// <summary>
        /// Bending moment mxx myy mxy taken by the glasses, Total bending: mxx + fx * hc
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns></returns>
        public mnl.Vector<double> GetGlassLocalBending(double csi, double eta, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetGlassLocalPseudoStrains(csi, eta, globalDisplacementNodes);
            var pseudoStress = GetGlassLocalPseudoStress(pseudoStrain);

            mnl.Vector<double> bending = mnl.Vector<double>.Build.Dense(3);

            bending[0] = pseudoStress[3]; //mxx
            bending[1] = pseudoStress[4]; //myy
            bending[2] = pseudoStress[5]; //mxy

            return bending;
        }

        public mnl.Vector<double> GetGlassLocalForces(double csi, double eta, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetGlassLocalPseudoStrains(csi, eta, globalDisplacementNodes);
            var pseudoStress = GetGlassLocalPseudoStress(pseudoStrain);

            mnl.Vector<double> forces = mnl.Vector<double>.Build.Dense(3);

            forces[0] = pseudoStress[0]; //fxx
            forces[1] = pseudoStress[1]; //fyy
            forces[2] = pseudoStress[2]; //fxy

            return forces;
        }

        /// <summary>
        /// eq 44
        /// </summary>
        /// <returns>pseudoStrainGlass = [ddeltaUdx, ddeltaVdy, ddeltaUdx + ddeltaUdy, -d2wdx2 , -d2wdy2, -2.0 * d2wdxdy] as vector of double</returns>
        public mnl.Vector<double> GetGlassLocalPseudoStrains(double csi, double eta, double[] globalDisplacementsNodes)
        {
            //eq. 44: pseudoStrainGlass = [ddeltaUdx, ddeltaVdy, ddeltaUdx + ddeltaUdy, -d2wdx2 , -d2wdy2, -2.0 * d2wdxdy]
            var Bg = GetBg(csi, eta);
            var localDisplacement = _dofGlobalToLocal * mnl.Vector<double>.Build.DenseOfArray(globalDisplacementsNodes);

            var localDisplacementReordered = mnl.Vector<double>.Build.Dense(20);

            for (int i = 0; i < _nodesLocal.Count(); i++)
            {
                localDisplacementReordered[i * 5 + 0] = localDisplacement[i * 5 + 3];
                localDisplacementReordered[i * 5 + 1] = localDisplacement[i * 5 + 4];

                localDisplacementReordered[i * 5 + 2] = localDisplacement[i * 5 + 0];
                localDisplacementReordered[i * 5 + 3] = localDisplacement[i * 5 + 1];
                localDisplacementReordered[i * 5 + 4] = localDisplacement[i * 5 + 2];
            }

            var eg = Bg * localDisplacementReordered;
            return eg;
        }

        /// <summary>
        /// Dg * pseudostrain
        /// </summary>
        /// <param name="pseudoStrain"></param>
        /// <returns></returns>
        public mnl.Vector<double> GetGlassLocalPseudoStress(mnl.Vector<double> pseudoStrain)
        {
            return _Dg * pseudoStrain;
        }

        /// <summary>
        /// eq 24
        /// </summary>
        /// <param name="g">Top or bottom</param>
        /// <param name="face">Top, moddle or bottom</param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacementNodes">displacement of the element</param>
        /// <returns>epsilon_x, epslilon_y, tau_xy, kxx, kyy, kxy as vector of double</returns>
        public mnl.Vector<double> GetGlassLocalStrains(Glass g, Face face, double csi, double eta, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetGlassLocalPseudoStrains(csi, eta, globalDisplacementNodes);
            var pseudoStress = GetGlassLocalPseudoStress(pseudoStrain);

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

            var curvatures = GetGlassLocalCurvatures(csi, eta, globalDisplacementNodes);

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
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>sigma_xx, sigma_yy, tau_xy as vector of double</returns>
        public mnl.Vector<double> GetGlassLocalStress(Glass g, Face face, double csi, double eta, double[] globalDisplacementNodes)
        {
            var strains = GetGlassLocalStrains(g, face, csi, eta, globalDisplacementNodes);
            var stress = _PlaneStressGlassMatrix * strains;

            return stress;
        }

        public mnl.Matrix<double> GetGlassBending(double csi, double eta, double[] globalDisplacementNodes, CoordinateSystem newSys = null)
        {
            var mLocal = GetGlassLocalBending(csi, eta, globalDisplacementNodes); //mxx, myy, mxy

            mnl.Matrix<double> localTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            localTensor[0, 0] = mLocal[0];
            localTensor[0, 1] = mLocal[2];

            localTensor[1, 0] = mLocal[2];
            localTensor[1, 1] = mLocal[1];

            return FEMUtilities.RotateTensor(localTensor, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetGlassCurvatures(double csi, double eta, double[] globalDisplacementNodes, CoordinateSystem newSys = null)
        {
            var local = GetGlassLocalCurvatures(csi, eta, globalDisplacementNodes); //kxx, kyy, kxy

            mnl.Matrix<double> localTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            localTensor[0, 0] = local[0]; //kxx
            localTensor[0, 1] = local[2]; //kxy

            localTensor[1, 0] = local[2]; //kxy
            localTensor[1, 1] = local[1]; //kyy

            return FEMUtilities.RotateTensor(localTensor, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetGlassForces(double csi, double eta, double[] globalDisplacementNodes, CoordinateSystem newSys = null)
        {
            var local = GetGlassLocalForces(csi, eta, globalDisplacementNodes);

            mnl.Matrix<double> localTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            localTensor[0, 0] = local[0]; //fxx
            localTensor[0, 1] = local[2]; //fyx

            localTensor[1, 0] = local[2];
            localTensor[1, 1] = local[1]; //fyy

            return FEMUtilities.RotateTensor(localTensor, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetGlassStrains(Glass g, Face face, double csi, double eta, double[] globalDisplacementNodes, CoordinateSystem newSys = null)
        {
            var local = GetGlassLocalStrains(g, face, csi, eta, globalDisplacementNodes);

            mnl.Matrix<double> localTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            localTensor[0, 0] = local[0]; //exx
            localTensor[0, 1] = local[2]; //eyx

            localTensor[1, 0] = local[2];
            localTensor[1, 1] = local[1]; //eyy

            return FEMUtilities.RotateTensor(localTensor, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetGlassStress(Glass g, Face face, double csi, double eta, double[] globalDisplacementNodes, CoordinateSystem newSys = null)
        {
            var local = GetGlassLocalStress(g, face, csi, eta, globalDisplacementNodes);

            mnl.Matrix<double> localTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            localTensor[0, 0] = local[0]; //sxx
            localTensor[0, 1] = local[2]; //tauyx

            localTensor[1, 0] = local[2];
            localTensor[1, 1] = local[1]; //syy

            return FEMUtilities.RotateTensor(localTensor, _localCoordinateSystem, newSys);
        }
        #endregion

        #region Interlayer
        /// <summary>
        /// eq 33
        /// </summary>
        /// <param name="globalDisplacementsNodes"></param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns> pseudoStrainInterlayer = [deltaU, deltaV, dwdx , dwdy] as vector of double</returns>
        public mnl.Vector<double> GetInterlayerLocalPseudoStrains(double csi, double eta, double[] globalDisplacementsNodes)
        {
            //eq. 33: pseudoStrainInterlayer = [deltaU, deltaV, dwdx , dwdy]
            var Bs = GetBs(csi, eta);

            var localDisplacement = _dofGlobalToLocal * mnl.Vector<double>.Build.DenseOfArray(globalDisplacementsNodes);

            var localDisplacementReordered = mnl.Vector<double>.Build.Dense(5*4);

            for (int i = 0; i < _nodesLocal.Count(); i++)
            {
                localDisplacementReordered[i * 5 + 0] = localDisplacement[i * 5 + 3];
                localDisplacementReordered[i * 5 + 1] = localDisplacement[i * 5 + 4];

                localDisplacementReordered[i * 5 + 2] = localDisplacement[i * 5 + 0];
                localDisplacementReordered[i * 5 + 3] = localDisplacement[i * 5 + 1];
                localDisplacementReordered[i * 5 + 4] = localDisplacement[i * 5 + 2];
            }

            var es = Bs * localDisplacementReordered;

            return es;
        }

        /// <summary>
        /// Ds * pseudostrain
        /// </summary>
        /// <param name="pseudoStrain"></param>
        /// <returns>pseudoStressInterlayer = tau_zx, tau_zy, valore non identificato, valore non identificato</returns>
        public mnl.Vector<double> GetInterlayerLocalPseudoStress(mnl.Vector<double> pseudoStrain)
        {
            return _Ds * pseudoStrain;
        }

        /// <summary>
        /// strain interlayer using equations 12 and 15
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>gamma_xz, gamma_yz</returns>
        public mnl.Vector<double> GetInterlayerLocalStrains(double csi, double eta, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetInterlayerLocalPseudoStrains(csi, eta, globalDisplacementNodes);

            mnl.Vector<double> strain = mnl.Vector<double>.Build.Dense(2);
            strain[0] = 1.0 / _h0 * (pseudoStrain[0] + _hc * pseudoStrain[2]);
            strain[1] = 1.0 / _h0 * (pseudoStrain[1] + _hc * pseudoStrain[3]);

            return strain;
        }

        /// <summary>
        /// Stress in interlayer
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacementNodes"></param>
        /// <returns>tau_zx, tau_zy</returns>
        public mnl.Vector<double> GetInterlayerLocalStress(double csi, double eta, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetInterlayerLocalPseudoStrains(csi, eta, globalDisplacementNodes);
            var pseudoStress = GetInterlayerLocalPseudoStress(pseudoStrain);

            mnl.Vector<double> stress = mnl.Vector<double>.Build.Dense(2);
            stress[0] = pseudoStress[0];
            stress[1] = pseudoStress[1];

            return stress;
        }

        public mnl.Vector<double> GetInterlayerLocalBending(double csi, double eta, double[] globalDisplacementNodes)
        {
            var pseudoStrain = GetInterlayerLocalPseudoStrains(csi, eta, globalDisplacementNodes);
            var pseudoStress = GetInterlayerLocalPseudoStress(pseudoStrain);

            mnl.Vector<double> bending = mnl.Vector<double>.Build.Dense(2);
            bending[0] = pseudoStress[2];
            bending[1] = pseudoStress[3];

            return bending;
        }

        public mnl.Matrix<double> GetInterlayerStress(double csi, double eta, double[] globalDisplacementNodes, CoordinateSystem newSys = null)
        {
            var local = GetInterlayerLocalStress(csi, eta, globalDisplacementNodes);

            mnl.Matrix<double> localTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            localTensor[0, 2] = local[0]; //tau_zx
            localTensor[1, 2] = local[1]; //tau_zy

            localTensor[2, 0] = local[0]; //tau_zx
            localTensor[2, 1] = local[1]; //tau_zy

            return FEMUtilities.RotateTensor(localTensor, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetInterlayerStrains(double csi, double eta, double[] globalDisplacementNodes, CoordinateSystem newSys = null)
        {
            var local = GetInterlayerLocalStrains(csi, eta, globalDisplacementNodes);

            mnl.Matrix<double> localTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            localTensor[0, 2] = local[0]; //gamma_zx
            localTensor[1, 2] = local[1]; //gamma_zy

            localTensor[2, 0] = local[0]; //gamma_zx
            localTensor[2, 1] = local[1]; //gamma_zy

            return FEMUtilities.RotateTensor(localTensor, _localCoordinateSystem, newSys);
        }
        #endregion
        #endregion

        #region PrivateInternalFunctions

         #region ShapeFunction
        /// <summary>
        /// Shape functions for this element
        /// </summary>
        /// <param name="indexNode">1 to 4</param>
        /// <param name="indexDisplacement">1 to 2</param>
        /// <returns></returns>
        internal Func<double, double, double> GetN(int indexNode, int indexDisplacement)
        {
            Dictionary<int, (int, int)> indexes = GetIndices();

            int a = indexes[indexNode].Item1;
            int b = indexes[indexNode].Item2;

            //equations (56 + equations (57)
            if ((indexDisplacement == 1 && indexNode == 1) || (indexDisplacement == 2 && indexNode == 1))
            {
                return (double csi, double eta) => Quad4Element.GetShapeFunction(1, csi, eta);
            }
            else if ((indexDisplacement == 1 && indexNode == 2) || (indexDisplacement == 2 && indexNode == 2))
            {
                return (double csi, double eta) => Quad4Element.GetShapeFunction(2, csi, eta);
            }
            else if ((indexDisplacement == 1 && indexNode == 3) || (indexDisplacement == 2 && indexNode == 3))
            {
                return (double csi, double eta) => Quad4Element.GetShapeFunction(3, csi, eta);
            }
            else if ((indexDisplacement == 1 && indexNode == 4) || (indexDisplacement == 2 && indexNode == 4))
            {
                return (double csi, double eta) => Quad4Element.GetShapeFunction(4, csi, eta);
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

        /// <summary>
        /// equation 35 for a single node Bs = Ls * N with Ls derivative operator
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBsi(int indexNode, double csi, double eta)
        {
            mnl.Matrix<double> bs = mnl.Matrix<double>.Build.Dense(4, 5);
            bs[0, 0] = GetN(indexNode, 1)(csi, eta);

            bs[1, 1] = GetN(indexNode, 2)(csi, eta);

            int index1 = 0, index2 = 0, index3 = 0;
            switch (indexNode)
            {
                case 1:
                    index1 = 1;
                    index2 = 2;
                    index3 = 3;
                    break;
                case 2:
                    index1 = 4;
                    index2 = 5;
                    index3 = 6;
                    break;
                case 3:
                    index1 = 7;
                    index2 = 8;
                    index3 = 9;
                    break;
                case 4:
                    index1 = 10;
                    index2 = 11;
                    index3 = 12;
                    break;
            }

            bs[2, 2] = -_el.GetFunction(index1, "x")(csi, eta);
            bs[2, 3] = -_el.GetFunction(index2, "x")(csi, eta);
            bs[2, 4] = -_el.GetFunction(index3, "x")(csi, eta);

            bs[3, 2] = -_el.GetFunction(index1, "y")(csi, eta);
            bs[3, 3] = -_el.GetFunction(index2, "y")(csi, eta);
            bs[3, 4] = -_el.GetFunction(index3, "y")(csi, eta);

            return bs;
        }

        /// <summary>
        /// equation 48 for a single node Bg = Lg * N with Lg derivative operator
        /// </summary>
        /// <param name="indexNode"></param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        internal mnl.Matrix<double> GetBgi(int indexNode, double csi, double eta)
        {
            mnl.Matrix<double> bg = mnl.Matrix<double>.Build.Dense(6, 5);

            var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesLocal);
            var dNdCsi = FEMUtilities.FFirstFix<int, double, double, double>(indexNode, Quad4Element.GetdNdCsi);
            var dNdEta = FEMUtilities.FFirstFix<int, double, double, double>(indexNode, Quad4Element.GetdNdEta);

            var dNdLocal = FEMUtilities.GetdNdLocalFromdNdNatural2D(csi, eta, dNdCsi, dNdEta, jacob);

            bg[0, 0] = dNdLocal[0];

            bg[1, 1] = dNdLocal[1];

            bg[2, 0] = dNdLocal[1];
            bg[2, 1] = dNdLocal[0];

            var invJacob = _el.GetInvJacobian(csi, eta);

            Dictionary<string, double> getValues(int index)
            {
                var dH1xDCsi = _el.GetFunction(index, "x", "csi");
                var dH1xDEta = _el.GetFunction(index, "x", "eta");

                var dH1yDCsi = _el.GetFunction(index, "y", "csi");
                var dH1yDEta = _el.GetFunction(index, "y", "eta");

                var dH1xDx = invJacob[0, 0] * dH1xDCsi(csi, eta) + invJacob[0, 1] * dH1xDEta(csi, eta);
                var dH1yDy = invJacob[1, 0] * dH1yDCsi(csi, eta) + invJacob[1, 1] * dH1yDEta(csi, eta);
                var dH1xDyPlusdH1yDx = invJacob[0, 0] * dH1yDCsi(csi, eta) + invJacob[0, 1] * dH1yDEta(csi, eta) + invJacob[1,0] * dH1xDCsi(csi, eta) + invJacob[1, 1] * dH1xDEta(csi, eta);

                Dictionary<string, double> output = new Dictionary<string, double>();
                output.Add("dx", dH1xDx);
                output.Add("dy", dH1yDy);
                output.Add("dxdy", dH1xDyPlusdH1yDx);

                return output;
            }

            int index1 = 0, index2 = 0, index3 = 0;
            switch (indexNode)
            {
                case 1:
                    index1 = 1;
                    index2 = 2;
                    index3 = 3;
                    break;
                case 2:
                    index1 = 4;
                    index2 = 5;
                    index3 = 6;
                    break;
                case 3:
                    index1 = 7;
                    index2 = 8;
                    index3 = 9;
                    break;
                case 4:
                    index1 = 10;
                    index2 = 11;
                    index3 = 12;
                    break;
            }

            bg[3, 2] = getValues(index1)["dx"];
            bg[3, 3] = getValues(index2)["dx"];
            bg[3, 4] = getValues(index3)["dx"];

            bg[4, 2] = getValues(index1)["dy"];
            bg[4, 3] = getValues(index2)["dy"];
            bg[4, 4] = getValues(index3)["dy"];

            bg[5, 2] = getValues(index1)["dxdy"];
            bg[5, 3] = getValues(index2)["dxdy"];
            bg[5, 4] = getValues(index3)["dxdy"];

            return bg;
        }

        /// <summary>
        /// Equation 32
        /// </summary>
        /// <returns></returns>
        internal mnl.Matrix<double> GetDs()
        {
            var Ds = mnl.Matrix<double>.Build.Dense(4, 4); //equation (32)
            Ds[0, 0] = 1.0;
            Ds[0, 2] = _hc;

            Ds[1, 1] = 1.0;
            Ds[1, 3] = _hc;

            Ds[2, 0] = _hc;
            Ds[2, 2] = _hc * _hc;

            Ds[3, 1] = _hc;
            Ds[3, 3] = _hc * _hc;

            Ds = _G0 / _h0 * Ds;

            return Ds;
        }

        /// <summary>
        /// Equation 45
        /// </summary>
        /// <returns></returns>
        internal mnl.Matrix<double> GetDg()
        {
            var C = _PlaneStressGlassMatrix;

            var Dg = mnl.Matrix<double>.Build.Dense(6, 6); //equation (45)

            double factor1 = (_h1 * _h2) / (_h1 + _h2);
            double factor2 = (Math.Pow(_h1, 3.0) + Math.Pow(_h2, 3.0)) / 12.0;

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
        /// <returns></returns>
        private double GetHc()
        {
            return (2.0 * _h0 + _h1 + _h2) / 2.0;
        }
#endregion
    }
}
