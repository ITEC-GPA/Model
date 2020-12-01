using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using GPC.Model.Sections;
using GPC.Model.Elements;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    public class FEMPlateIntegratorDKQ : FEMPlateIntegrator
    {
        #region Variables 
        // Coefficients
        double[] _a = new double[4];
        double[] _b = new double[4];
        double[] _c = new double[4];
        double[] _d = new double[4];
        double[] _e = new double[4];
        double[] _Cu = new double[4];
        double[] _Cv = new double[4];
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMPlateIntegratorDKQ(Guid guid, int dim, int order, int numDefComp, FEMElement element)
            : base(guid, dim, order, numDefComp, element)
        {
            /// Define Integration Points
            InitQuad4(element);
            _shape = new FEMShapeQuad4(_gaussIntegrationPoints.NumPoints);
            _shapeBending = new FEMShapeQuad8(_gaussIntegrationPoints.NumPoints);
            _shapeDrilling = new FEMShapeDilling4(_gaussIntegrationPoints.NumPoints);
        }
        public FEMPlateIntegratorDKQ(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        private void BuildCoefficient(FEMElement element)
        {
            for (int i = 0; i < 4; i++)
            {
                Node p0 = element.NodesLocal[i];
                Node p1 = element.NodesLocal[(i + 1) % 4];

                //Vector2d v = new Vector2d(p0.Position.X - p1.Position.X, p0.Position.Y - p1.Position.Y);
                Vector2d v = new Vector2d(p1.Position.X - p0.Position.X, p1.Position.Y - p0.Position.Y);
                double length2 = Math.Pow(v.Length, 2.0);
                double length = v.Length;

                _a[i] = -v.X / length2;
                _b[i] = 0.75 * (v.X * v.Y) / length2;
                _c[i] = (0.25 * v.X * v.X - 0.5 * v.Y * v.Y) / length2;
                _d[i] = -v.Y / length2;
                _e[i] = (-0.5 * v.X * v.X + 0.25 * v.Y * v.Y) / length2;

                double angle = Math.Atan2(v.X, -v.Y);
                _Cu[i] = 0.0625 * length * Math.Cos(angle);
                _Cv[i] = 0.0625 * length * Math.Sin(angle);
                //_Cu[i] = 0.0625 * length * (v.Y/length);
                //_Cv[i] = 0.0625 * length * (-v.X / length);
            }
        }
        public override void BuildMembranal(ref Matrix<double> BmMatrix, Matrix<double> dNMatrix, Matrix<double> JInvMatrix)
        {
            BmMatrix = Matrix<double>.Build.Dense(3, 12);

            int j, k, m;
            double dNx, dNy;

            /// Drilling rotation DoF RZ (around normal to the plate)
            Matrix<double> dNRot =  Matrix<double>.Build.Dense(2, 4, 0.0);
            Matrix<double> dNGlobRot = Matrix<double>.Build.Dense(2, 4, 0.0);

            dNGlobRot = JInvMatrix * _shapeDrilling.dNShape;

            Matrix<double> dNWu = Matrix<double>.Build.Dense(2, 4, 0.0);
            Matrix<double> dNWv = Matrix<double>.Build.Dense(2, 4, 0.0);

            for (int i = 0; i < 4; i++)
            {
                //dNWu[0, i] = - _Cu[(i + 3) % 4] * dNGlobRot[0, i] + _Cu[i] * dNGlobRot[0, (i + 1) % 4];
                //dNWu[1, i] = - _Cu[(i + 3) % 4] * dNGlobRot[1, i] + _Cu[i] * dNGlobRot[1, (i + 1) % 4];

                dNWu[0, i] = - _Cu[(i + 3) % 4] * dNGlobRot[0, i] + _Cu[i] * dNGlobRot[0, (i + 1) % 4];
                dNWu[1, i] = - _Cu[(i + 3) % 4] * dNGlobRot[1, i] + _Cu[i] * dNGlobRot[1, (i + 1) % 4];

                dNWv[0, i] = _Cv[(i + 3) % 4] * dNGlobRot[0, i] - _Cv[i] * dNGlobRot[0, (i + 1) % 4];
                dNWv[1, i] = _Cv[(i + 3) % 4] * dNGlobRot[1, i] - _Cv[i] * dNGlobRot[1, (i + 1) % 4];
            }

            /// Memebranal Components DoF local x,y
            for (int i = 0; i < 4; i++)
            {
                j = 3 * i;
                k = j + 1;
                m = k + 1;

                // Componenti SPT
                dNx = dNMatrix[0, i];
                BmMatrix[0, j] = dNx;
                BmMatrix[2, k] = dNx;

                dNy = dNMatrix[1, i];
                BmMatrix[1, k] = dNy;
                BmMatrix[2, j] = dNy;

                // Componenti w
                BmMatrix[0, m] = dNWu[0, i];
                BmMatrix[1, m] = dNWv[1, i];
                BmMatrix[2, m] = dNWu[1, i] + dNWv[0, i];
            }
        }
        public override void BuildBending(ref Matrix<double> BpMatrix, ref Matrix<double> dHMatrix, Matrix<double> JInvMatrix)
        {
            // [ dHx1/dcsi, dHx2/dcsi, ..., dHx12/dcsi ]
            // [ dHx1/deta, dHx2/deta, ..., dHx12/deta ]
            // [ dHy1/dcsi, dHy2/dcsi, ..., dHy12/dcsi ]
            // [ dHy1/deta, dHy2/deta, ..., dHy12/deta ]

            dHMatrix = Matrix<double>.Build.Dense(4, 12);
            int[,] index = new int[4, 3] {
                                { 0, 7, 4},
                                { 1, 4, 5},
                                { 2, 5, 6},
                                { 3, 6, 7}};

            Matrix<double> dNLoc = _shapeBending.dNShape;

            for (int i = 0; i < 4; i++)
            {
                int p = index[i, 0];
                int q = index[i, 1];
                int r = index[i, 2];
                int s = q - 4;
                int t = r - 4;

                int j = i * 3;
                int k = j + 1;
                int n = j + 2;

                for (int m = 0; m < 2; m++)
                {
                    dHMatrix[m, j] = 1.5 * (_a[t] * dNLoc[m, r] - _a[s] * dNLoc[m, q]);
                    dHMatrix[m, k] = _b[t] * dNLoc[m, r] + _b[s] * dNLoc[m, q];
                    dHMatrix[m, n] = dNLoc[m, p] - _c[t] * dNLoc[m, r] - _c[s] * dNLoc[m, q];

                    int row = 2 + m;
                    dHMatrix[row, j] = 1.5 * (_d[t] * dNLoc[m, r] - _d[s] * dNLoc[m, q]);
                    dHMatrix[row, k] = -dNLoc[m, p] + _e[t] * dNLoc[m, r] + _e[s] * dNLoc[m, q];
                    dHMatrix[row, n] = -_b[t] * dNLoc[m, r] - _b[s] * dNLoc[m, q];
                }
            }

            // B build
            /// dHMatrix[0, i] derivate of Hx(chi, eta) respect to csi
            /// dHMatrix[1, i] derivate of Hx(chi, eta) respect to eta
            /// dHMatrix[2, i] derivate of Hy(chi, eta) respect to csi
            /// dHMatrix[3, i] derivate of Hy(chi, eta) respect to eta
            BpMatrix = Matrix<double>.Build.Dense(3, 12);
            for (int i = 0; i < 12; i++)
            {
                BpMatrix[0, i] = JInvMatrix[0, 0] * dHMatrix[0, i] + JInvMatrix[0, 1] * dHMatrix[1, i];
                BpMatrix[1, i] = JInvMatrix[1, 0] * dHMatrix[2, i] + JInvMatrix[1, 1] * dHMatrix[3, i];
                BpMatrix[2, i] = JInvMatrix[0, 0] * dHMatrix[2, i] + JInvMatrix[0, 1] * dHMatrix[3, i] +
                                 JInvMatrix[1, 0] * dHMatrix[0, i] + JInvMatrix[1, 1] * dHMatrix[1, i];
            }
        }

        public override void BuildMass(ref Matrix<double> NmassMatrix)
        {
            int j, k, m;

            Vector<double> NLoc = _shape.NShape;

            /// Mass DoF local
            for (int i = 0; i < 4; i++)
            {
                j = 3 * i;
                k = j + 1;
                m = k + 1;

                NmassMatrix[0, j] = NLoc[i];
                NmassMatrix[1, k] = NLoc[i];
                NmassMatrix[2, m] = NLoc[i];
            }
        }

        public override void BuildK(FEMElement element)
        {
            Plate plate = element as Plate;
            PlateProperty elProp = element.Property as PlateProperty;

            Matrix<double> scalD;
            Matrix<double> gaussK;
            Matrix<double> K;

            _kMatrix = Matrix<double>.Build.Dense(24, 24, 0.0);
            K = Matrix<double>.Build.Dense(24, 24, 0.0);

            _KmMatrix = Matrix<double>.Build.Dense(12, 12, 0.0);
            _KbMatrix = Matrix<double>.Build.Dense(12, 12, 0.0);


            /// Bending Components
            for (int i = 0; i < _gaussIntegrationPoints.NumPoints; i++)
            {
                double ar = _gaussIntegrationPoints.Weights[i] * _detJacobian[i];
                scalD = ar * _Db;
                gaussK = _BbMatrix[i].Transpose() * scalD * _BbMatrix[i];
                _KbMatrix = _KbMatrix + gaussK;

                //Product_At_B_A(_Db[i], scalD, &gaussK);
                //Sum(gaussK, &m_Kbend);
            }

            /// Membranal Components
            for (int i = 0; i < _gaussIntegrationPoints.NumPoints ; i++)
            {
                double ar = elProp.Tm * _gaussIntegrationPoints.Weights[i] * _detJacobian[i];
                scalD = ar * _Dm;
                gaussK = _BmMatrix[i].Transpose() * scalD * _BmMatrix[i];
                _KmMatrix = _KmMatrix + gaussK;

                //Product(ar, m_Dplane, &scalD);
                //Product_At_B_A(m_Bplane[i], scalD, &gaussK);
                //Sum(gaussK, &m_Kplane);
            }

            /// Assembling Stiffness Matrix
            int[] nm = new int[] { 0, 1, 5, 6, 7, 11, 12, 13, 17, 18, 19, 23 };
            int[] nb = new int[] { 2, 3, 4, 8, 9, 10, 14, 15, 16, 20, 21, 22 };
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    K[nm[i], nm[j]] = _KmMatrix[i, j];
                    K[nb[i], nb[j]] = _KbMatrix[i, j];
                }
            }            
            _kMatrix = _trfMatrix.Transpose() * K * _trfMatrix;

            //string path = "C:\\Users\\r.vochescu\\Desktop\\" + "DKQ_SHELL_STIFF-MATRIX.txt";
            //// This text is added only once to the file.
            //if (File.Exists(path) == true)
            //{
            //    File.Delete(path);
            //}
            //if (!File.Exists(path))
            //{
            //    string matrix = "";
            //    for (int r = 0; r < _kMatrix.RowCount; r++)
            //    {
            //        for (int c = 0; c < _kMatrix.ColumnCount; c++)
            //        {
            //            matrix = matrix + "\t" + _kMatrix[r, c].ToString();
            //        }
            //        matrix = matrix + Environment.NewLine;
            //    }
            //    File.WriteAllText(path, matrix);
            //}
        }
        public override void BuildT()
        {
        }
        public override void BuildF()
        {
        }
        public override void BuildB()
        {
            /// Build Membranal Components

            /// Build Bending Components
        }
        public override void RegisterDoF(Node node)
        {
        }
        public override void StartIntegration(FEMElement element)
        {
            int numNodes = element.NodesGlobal.Length;

            if (_shape.NumIntgrPts != _gaussIntegrationPoints.Coords.Count()) { throw new ArgumentException($"{nameof(_shape.NumIntgrPts)} Gauss Point Number in the Shape Function are different from the one defined in the Integrator"); }

            // Matrices Initialization
            _detJacobian = new double[_shape.NumIntgrPts];
            _NMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _dNMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _JMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _JInvMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _dHMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _BmMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _BbMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _NmassMatrix = new Matrix<double>[_shape.NumIntgrPts];

            if (_shape.NumIntgrPts == _gaussIntegrationPoints.Coords.Count())
            {
                for (int i = 0; i < _shape.NumIntgrPts; i++)
                {
                    //_NMatrix[i] = Matrix<double>.Build.Dense(2, 8, 0.0);
                    //_dNMatrix[i] = Matrix<double>.Build.Dense(2, 4, 0.0);
                    //_JMatrix[i] = Matrix<double>.Build.Dense(2, 2, 0.0);
                    //_JInvMatrix[i] = Matrix<double>.Build.Dense(2, 2, 0.0);
                    //_dHMatrix[i] = Matrix<double>.Build.Dense(4, 12, 0.0);
                    //_BmMatrix[i] = Matrix<double>.Build.Dense(3, 12, 0.0);
                    //_BbMatrix[i] = Matrix<double>.Build.Dense(3, 12, 0.0);

                    _NMatrix[i] = Matrix<double>.Build.Dense(_dim, _dim * numNodes, 0.0);
                    _dNMatrix[i] = Matrix<double>.Build.Dense(_dim, numNodes, 0.0);
                    _JMatrix[i] = Matrix<double>.Build.Dense(_dim, _dim, 0.0);
                    _JInvMatrix[i] = Matrix<double>.Build.Dense(_dim, _dim, 0.0);

                    _dHMatrix[i] = Matrix<double>.Build.Dense(4, 12, 0.0);
                    _BmMatrix[i] = Matrix<double>.Build.Dense(3, 12, 0.0);
                    _BbMatrix[i] = Matrix<double>.Build.Dense(3, 12, 0.0);

                    _NmassMatrix[i] = Matrix<double>.Build.Dense(3, 12, 0.0);
                }
            }

            // Build Integration Matrices
            if (_shape.NumIntgrPts == _gaussIntegrationPoints.Coords.Count())
            {
                BuildCoefficient(element);

                for (int i = 0; i < _shape.NumIntgrPts; i++)
                {
                    /// Calculation of local N and dN matrices ad the Gauss Points 
                    _shape.SetValue(_gaussIntegrationPoints.Coords[i]);
                    _shapeBending.SetValue(_gaussIntegrationPoints.Coords[i]);
                    _shapeDrilling.SetValue(_gaussIntegrationPoints.Coords[i]);

                    /// Compute the columns of the N matrix
                    BuildN(element, ref _NMatrix[i]);

                    /// Compute the columns of the J, JInv, dN matrices
                    _detJacobian[i] = BuildJ(element, ref _JMatrix[i], ref _JInvMatrix[i], ref _dNMatrix[i]);

                    /// Compute Membranal Components of B matrix
                    BuildMembranal(ref _BmMatrix[i], _dNMatrix[i], _JInvMatrix[i]);

                    /// Compute Membranal Components of B matrix
                    BuildBending(ref _BbMatrix[i], ref _dHMatrix[i], _JInvMatrix[i]);

                    /// Compute Mass Components
                    BuildMass(ref _NmassMatrix[i]);
                }
            }
            /// Calculate Area
            CalcArea(element);

            /// Calculate costitutive Matrices
            BuildD(element);

            /// Build Transformation Matrix
            BuildTrfMatrix(element);

            /// Build  Stiffness Matrix
            BuildK(element);

            /// Built Mass Matrix
            BuildM(element);
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
