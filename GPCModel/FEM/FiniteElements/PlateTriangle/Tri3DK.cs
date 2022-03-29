using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Materials;
using GPC.Model.Fem.Properties;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Fem.FiniteElements
{
    /// <summary>
    /// Discrete Kirchoff Triangle - A study of three-node triangular plate bending elements - Jean-Louis Batoz
    /// International Jurnal for numerical methods in engineering, vol 15, 1771-1812 (1980)
    /// </summary>
    public class Tri3DK : DK
    {
        #region variables
        double _x31;
        double _y31;
        double _l31;

        double _x12;
        double _y12;
        double _l12;

        double _x23;
        double _y23;
        double _l23;

        double _areaElement;

        Dictionary<int, double> _aCoeff;
        Dictionary<int, double> _bCoeff;
        Dictionary<int, double> _cCoeff;
        Dictionary<int, double> _dCoeff;
        Dictionary<int, double> _eCoeff;
        #endregion

        public Tri3DK(Node[] nodes) : base(nodes)
        {
            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            //displacement out of local plane "w" in local coordinate system can be in X,Y,Z in global local coordinate system
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);

            //Global : 3 nodes x 6 (DX, DY, DZ, RX, RY, RZ) DOF each = matrix 18x18
            //Local  : 3 nodes x 3 (dZ+rX+rZ) DOF each = matrix 9x9

            //DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //   [18x9]             [9x9]     [9x18]

            #region calculationLocalAxisAndLocalCoordinates
            //Local axes calculater anticlockwise
            _nodesLocal = Tri3Element.GetLocalNodes(_nodesGlobal, out _localCoordinateSystem);
            Node node1 = _nodesLocal[0];
            Node node2 = _nodesLocal[1];
            Node node3 = _nodesLocal[2];
            /*Console.WriteLine("node 1: " + node1.Name + "==" + node1.Id + " " + node1.ToString());
            Console.WriteLine("node 2: " + node2.Name + "==" + node2.Id + " " + node2.ToString());
            Console.WriteLine("node 3: " + node3.Name + "==" + node3.Id + " " + node3.ToString());*/
            #endregion

            #region calculationVariablesForStiffnessEtcetera
            _x31 = node3.Position.X - node1.Position.X;
            _y31 = node3.Position.Y - node1.Position.Y;
            _l31 = Math.Sqrt(_x31 * _x31 + _y31 * _y31);

            _x12 = node1.Position.X - node2.Position.X;
            _y12 = node1.Position.Y - node2.Position.Y;
            _l12 = Math.Sqrt(_x12 * _x12 + _y12 * _y12);

            _x23 = node2.Position.X - node3.Position.X;
            _y23 = node2.Position.Y - node3.Position.Y;
            _l23 = Math.Sqrt(_x23 * _x23 + _y23 * _y23);

            _areaElement = (_x31 * _y12 - _x12 * _y31) / 2.0;

            _aCoeff = new Dictionary<int, double>();
            _bCoeff = new Dictionary<int, double>();
            _cCoeff = new Dictionary<int, double>();
            _dCoeff = new Dictionary<int, double>();
            _eCoeff = new Dictionary<int, double>();

            _aCoeff.Add(4, -_x23 / Math.Pow(_l23, 2.0));
            _aCoeff.Add(5, -_x31 / Math.Pow(_l31, 2.0));
            _aCoeff.Add(6, -_x12 / Math.Pow(_l12, 2.0));

            _bCoeff.Add(4, 3.0 / 4.0 * _x23 * _y23 / Math.Pow(_l23, 2.0));
            _bCoeff.Add(5, 3.0 / 4.0 * _x31 * _y31 / Math.Pow(_l31, 2.0));
            _bCoeff.Add(6, 3.0 / 4.0 * _x12 * _y12 / Math.Pow(_l12, 2.0));

            _cCoeff.Add(4, (1.0 / 4.0 * Math.Pow(_x23, 2.0) - 1.0 / 2.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0));
            _cCoeff.Add(5, (1.0 / 4.0 * Math.Pow(_x31, 2.0) - 1.0 / 2.0 * Math.Pow(_y31, 2.0)) / Math.Pow(_l31, 2.0));
            _cCoeff.Add(6, (1.0 / 4.0 * Math.Pow(_x12, 2.0) - 1.0 / 2.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0));

            _dCoeff.Add(4, -_y23 / Math.Pow(_l23, 2.0));
            _dCoeff.Add(5, -_y31 / Math.Pow(_l31, 2.0));
            _dCoeff.Add(6, -_y12 / Math.Pow(_l12, 2.0));

            _eCoeff.Add(4, (-1.0 / 2.0 * Math.Pow(_x23, 2.0) + 1.0 / 4.0 * Math.Pow(_y23, 2.0)) / Math.Pow(_l23, 2.0));
            _eCoeff.Add(5, (-1.0 / 2.0 * Math.Pow(_x31, 2.0) + 1.0 / 4.0 * Math.Pow(_y31, 2.0)) / Math.Pow(_l31, 2.0));
            _eCoeff.Add(6, (-1.0 / 2.0 * Math.Pow(_x12, 2.0) + 1.0 / 4.0 * Math.Pow(_y12, 2.0)) / Math.Pow(_l12, 2.0));
            #endregion

            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(6 * 3, 3 * 3);

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

            //FEMUtilities.WriteMatrix("dofGlobalToLocalTranspose.", _dofGlobalToLocal);
            #endregion
        }

        internal Tri3DK(Node[] nodes, PlateProperty property) : this(nodes)
        {
            SetProperty(property);
        }

        public override void BuildMatrix()
        {
            #region matrixD
            var planeStressMatrix = ((IsotropicFemMaterial)((PlateProperty)_property).Material).GetPlaneStress();
            double tb = ((PlateProperty)Property).BendingThickness;

            _d = Math.Pow(tb, 3.0) / 12.0 * planeStressMatrix; //flexural rigidity
            #endregion

            #region matrixK
            mnl.Matrix<double> bTdb(double csi, double eta)
            {
                mnl.Matrix<double> b = GetB(csi, eta);
                return b.Transpose() * _d * b;
            }

            var jacob = FemUtilities.J2D(Tri3Element.GetdNdCsi, Tri3Element.GetdNdEta, _nodesLocal);

            _kElementLocalCoord = OldGaussIntegration.IntegrationTriangular(bTdb, jacob, 3);
            #endregion
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(3 * Nodes.Length); //3 = DOF in local : DZ, RX, RZ
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

                    //Pressure --> node force
                    Vector3d f = new Vector3d(px * _areaElement / 3.0, py * _areaElement / 3.0, pz * _areaElement / 3.0); //force applied in each node

                    for (int i = 0; i < _fLocalCoord.Count; i = i + 3)
                    {
                        _fLocalCoord[i] = f.Z;
                    }
                }
            }
            return _fLocalCoord;
        }

        protected override mnl.Matrix<double> GetB(double csi, double eta)
        {
            mnl.Vector<double> hxdCsi = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hydCsi = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hxdEta = mnl.Vector<double>.Build.Dense(9);
            mnl.Vector<double> hydEta = mnl.Vector<double>.Build.Dense(9);

            #region Derivatives        

            #region AppendixFormulas
            //from appendix formulas
            hxdCsi[1 - 1] = GetShapeFunctionDerivative(1, "x", "csi")(csi, eta);
            hxdCsi[2 - 1] = GetShapeFunctionDerivative(2, "x", "csi")(csi, eta);
            hxdCsi[3 - 1] = GetShapeFunctionDerivative(3, "x", "csi")(csi, eta);

            hxdCsi[4 - 1] = GetShapeFunctionDerivative(4, "x", "csi")(csi, eta);
            hxdCsi[5 - 1] = GetShapeFunctionDerivative(5, "x", "csi")(csi, eta);
            hxdCsi[6 - 1] = GetShapeFunctionDerivative(6, "x", "csi")(csi, eta);

            hxdCsi[7 - 1] = GetShapeFunctionDerivative(7, "x", "csi")(csi, eta);
            hxdCsi[8 - 1] = GetShapeFunctionDerivative(8, "x", "csi")(csi, eta);
            hxdCsi[9 - 1] = GetShapeFunctionDerivative(9, "x", "csi")(csi, eta);

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            hxdEta[1 - 1] = GetShapeFunctionDerivative(1, "x", "eta")(csi, eta);
            hxdEta[2 - 1] = GetShapeFunctionDerivative(2, "x", "eta")(csi, eta);
            hxdEta[3 - 1] = GetShapeFunctionDerivative(3, "x", "eta")(csi, eta);

            hxdEta[4 - 1] = GetShapeFunctionDerivative(4, "x", "eta")(csi, eta);
            hxdEta[5 - 1] = GetShapeFunctionDerivative(5, "x", "eta")(csi, eta);
            hxdEta[6 - 1] = GetShapeFunctionDerivative(6, "x", "eta")(csi, eta);

            hxdEta[7 - 1] = GetShapeFunctionDerivative(7, "x", "eta")(csi, eta);
            hxdEta[8 - 1] = GetShapeFunctionDerivative(8, "x", "eta")(csi, eta);
            hxdEta[9 - 1] = GetShapeFunctionDerivative(9, "x", "eta")(csi, eta);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            hydCsi[1 - 1] = GetShapeFunctionDerivative(1, "y", "csi")(csi, eta);
            hydCsi[2 - 1] = GetShapeFunctionDerivative(2, "y", "csi")(csi, eta);
            hydCsi[3 - 1] = GetShapeFunctionDerivative(3, "y", "csi")(csi, eta);

            hydCsi[4 - 1] = GetShapeFunctionDerivative(4, "y", "csi")(csi, eta);
            hydCsi[5 - 1] = GetShapeFunctionDerivative(5, "y", "csi")(csi, eta);
            hydCsi[6 - 1] = GetShapeFunctionDerivative(6, "y", "csi")(csi, eta);

            hydCsi[7 - 1] = GetShapeFunctionDerivative(7, "y", "csi")(csi, eta);
            hydCsi[8 - 1] = GetShapeFunctionDerivative(8, "y", "csi")(csi, eta);
            hydCsi[9 - 1] = GetShapeFunctionDerivative(9, "y", "csi")(csi, eta);

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            hydEta[1 - 1] = GetShapeFunctionDerivative(1, "y", "eta")(csi, eta);
            hydEta[2 - 1] = GetShapeFunctionDerivative(2, "y", "eta")(csi, eta);
            hydEta[3 - 1] = GetShapeFunctionDerivative(3, "y", "eta")(csi, eta);

            hydEta[4 - 1] = GetShapeFunctionDerivative(4, "y", "eta")(csi, eta);
            hydEta[5 - 1] = GetShapeFunctionDerivative(5, "y", "eta")(csi, eta);
            hydEta[6 - 1] = GetShapeFunctionDerivative(6, "y", "eta")(csi, eta);

            hydEta[7 - 1] = GetShapeFunctionDerivative(7, "y", "eta")(csi, eta);
            hydEta[8 - 1] = GetShapeFunctionDerivative(8, "y", "eta")(csi, eta);
            hydEta[9 - 1] = GetShapeFunctionDerivative(9, "y", "eta")(csi, eta);
            #endregion

            //equation 30
            mnl.Vector<double> r0 = _y31 * hxdCsi + _y12 * hxdEta;
            mnl.Vector<double> r1 = -_x31 * hydCsi - _x12 * hydEta;
            mnl.Vector<double> r2 = -_x31 * hxdCsi - _x12 * hxdEta + _y31 * hydCsi + _y12 * hydEta;
            #endregion

            mnl.Matrix<double> b = mnl.Matrix<double>.Build.DenseOfRowVectors(r0, r1, r2);
            b = 1.0 / (2.0 * _areaElement) * b;
            //Console.WriteLine("B(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + b);
            return b;
        }

        /// <summary>
        /// Description between eq. 27b and 28 of the article
        /// </summary>
        /// <param name="indexes1"></param>
        /// <param name="indexes2"></param>
        /// <param name="indexes3"></param>
        private void GetIndexes(out int[] indexes1, out int[] indexes2, out int[] indexes3)
        {
            indexes1 = new int[3];
            indexes1[0] = 1;
            indexes1[1] = 2;
            indexes1[2] = 3;

            indexes2 = new int[3];
            indexes2[0] = 5;
            indexes2[1] = 6;
            indexes2[2] = 4;

            indexes3 = new int[3];
            indexes3[0] = 6;
            indexes3[1] = 4;
            indexes3[2] = 5;
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
            if (deriv != "")
            {
                throw new Exception();
            }
            GetIndexes(out int[] indexes1, out int[] indexes2, out int[] indexes3);

            Func<double, double, double> F(int i)
            {
                if (deriv == "")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionsTri6.NaturalShapeFunction(i, csi, eta);
                }
                else if (deriv == "csi")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionsTri6.DNdCsi(i, csi, eta);
                }
                else if (deriv == "eta")
                {
                    return (double csi, double eta) => QuadraticShapeFunctionsTri6.DNdEta(i, csi, eta);
                }
                else
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
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal Func<double, double, double> GetShapeFunctionDerivative(int indexFunction, string dir, string deriv)
        {
            //derivative of "new shape function"
            #region formuleFornite

            double P4 = -6.0 * _x23 / Math.Pow(_l23, 2.0);
            double P5 = -6.0 * _x31 / Math.Pow(_l31, 2.0);
            double P6 = -6.0 * _x12 / Math.Pow(_l12, 2.0);

            double q4 = 3.0 * _x23 * _y23 / Math.Pow(_l23, 2.0);
            double q5 = 3.0 * _x31 * _y31 / Math.Pow(_l31, 2.0);
            double q6 = 3.0 * _x12 * _y12 / Math.Pow(_l12, 2.0);

            double r4 = 3.0 * Math.Pow(_y23, 2.0) / Math.Pow(_l23, 2.0);
            double r5 = 3.0 * Math.Pow(_y31, 2.0) / Math.Pow(_l31, 2.0);
            double r6 = 3.0 * Math.Pow(_y12, 2.0) / Math.Pow(_l12, 2.0);

            double t4 = -6.0 * _y23 / Math.Pow(_l23, 2.0);
            double t5 = -6.0 * _y31 / Math.Pow(_l31, 2.0);
            double t6 = -6.0 * _y12 / Math.Pow(_l12, 2.0);

            if (dir.ToLower() == "x")
            {
                if (deriv.ToLower() == "csi")
                {
                    //hx,Csi
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => P6 * (1.0 - 2.0 * csi) + (P5 - P6) * eta;
                        case 2:
                            return (double csi, double eta) => q6 * (1.0 - 2.0 * csi) - (q5 + q6) * eta;
                        case 3:
                            return (double csi, double eta) => -4.0 + 6.0 * (csi + eta) + r6 * (1.0 - 2.0 * csi) - eta * (r5 + r6);
                        case 4:
                            return (double csi, double eta) => -P6 * (1.0 - 2.0 * csi) + eta * (P4 + P6);
                        case 5:
                            return (double csi, double eta) => q6 * (1.0 - 2.0 * csi) - eta * (q6 - q4);
                        case 6:
                            return (double csi, double eta) => -2.0 + 6.0 * csi + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6);
                        case 7:
                            return (double csi, double eta) => -eta * (P5 + P4);
                        case 8:
                            return (double csi, double eta) => eta * (q4 - q5);
                        case 9:
                            return (double csi, double eta) => -eta * (r5 - r4);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    //Console.WriteLine("Hx,Csi(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hxCsi);
                }
                else if (deriv.ToLower() == "eta")
                {
                    //hx,Eta
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => -P5 * (1.0 - 2.0 * eta) - csi * (P6 - P5);
                        case 2:
                            return (double csi, double eta) => q5 * (1.0 - 2.0 * eta) - csi * (q5 + q6);
                        case 3:
                            return (double csi, double eta) => -4.0 + 6.0 * (csi + eta) + r5 * (1.0 - 2.0 * eta) - csi * (r5 + r6);
                        case 4:
                            return (double csi, double eta) => csi * (P4 + P6);
                        case 5:
                            return (double csi, double eta) => csi * (q4 - q6);
                        case 6:
                            return (double csi, double eta) => -csi * (r6 - r4);
                        case 7:
                            return (double csi, double eta) => P5 * (1.0 - 2.0 * eta) - csi * (P4 + P5);
                        case 8:
                            return (double csi, double eta) => q5 * (1.0 - 2.0 * eta) + csi * (q4 - q5);
                        case 9:
                            return (double csi, double eta) => -2.0 + 6.0 * eta + r5 * (1.0 - 2.0 * eta) + csi * (r4 - r5);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    //Console.WriteLine("Hx,Eta(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hxEta);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else if (dir.ToLower() == "y")
            {
                if (deriv == "csi")
                {
                    //hy,Csi
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => t6 * (1.0 - 2.0 * csi) + eta * (t5 - t6);
                        case 2:
                            return (double csi, double eta) => 1.0 + r6 * (1.0 - 2.0 * csi) - eta * (r5 + r6);
                        case 3:
                            return (double csi, double eta) => -q6 * (1.0 - 2.0 * csi) + eta * (q5 + q6); // <<<<<------------ -eta instead of + eta ?
                        case 4:
                            return (double csi, double eta) => -t6 * (1.0 - 2.0 * csi) + eta * (t4 + t6);
                        case 5:
                            return (double csi, double eta) => -1.0 + r6 * (1.0 - 2.0 * csi) + eta * (r4 - r6); //<-------------- -eta instead of + eta ?
                        case 6:
                            return (double csi, double eta) => -q6 * (1.0 - 2.0 * csi) - eta * (q4 - q6); //<---------- +eta instead of -eta ?
                        case 7:
                            return (double csi, double eta) => -eta * (t4 + t5);
                        case 8:
                            return (double csi, double eta) => eta * (r4 - r5);
                        case 9:
                            return (double csi, double eta) => -eta * (q4 - q5);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    //Console.WriteLine("Hy,Csi(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hyCsi);
                }
                else if (deriv == "eta")
                {
                    //hy,Eta
                    switch (indexFunction)
                    {
                        case 1:
                            return (double csi, double eta) => -t5 * (1.0 - 2.0 * eta) - csi * (t6 - t5);
                        case 2:
                            return (double csi, double eta) => 1.0 + r5 * (1.0 - 2.0 * eta) - csi * (r5 + r6);
                        case 3:
                            return (double csi, double eta) => -q5 * (1.0 - 2.0 * eta) + csi * (q5 + q6);
                        case 4:
                            return (double csi, double eta) => csi * (t4 + t6);
                        case 5:
                            return (double csi, double eta) => csi * (r4 - r6);
                        case 6:
                            return (double csi, double eta) => -csi * (q4 - q6);
                        case 7:
                            return (double csi, double eta) => t5 * (1.0 - 2.0 * eta) - csi * (t4 + t5);
                        case 8:
                            return (double csi, double eta) => -1.0 + r5 * (1.0 - 2.0 * eta) + csi * (r4 - r5);
                        case 9:
                            return (double csi, double eta) => -q5 * (1.0 - 2.0 * eta) - csi * (q4 - q5);  //<----------- + csi instead of - csi ?
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    //Console.WriteLine("Hy,Eta(csi=" + csi.ToString("F2") + " ,eta=" + eta.ToString("F2") + ") = " + hyEta);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
            #endregion
        }
    }
}
