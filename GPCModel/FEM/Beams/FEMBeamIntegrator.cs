using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using GPC.Model.Sections;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    public class FEMBeamIntegrator : FEMIntegrator
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMBeamIntegrator(Guid guid, FEMElement element, int dim = 1, int order = 2)
            : base(guid)
        {
            InitBeam2(element);
            _dim = dim;
            _shape = new FEMShapeBeamLinear2(_gaussIntegrationPoints.NumPoints, order);
        }

        public FEMBeamIntegrator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods Override
        public override void BuildMass(ref Matrix<double> NmassMatrix)
        {
            int j, k, m;

            Vector<double> NLoc = _shape.NShape;

            /// Mass DoF local
            for (int i = 0; i < 2; i++)
            {
                j = 3 * i;
                k = j + 1;
                m = k + 1;

                NmassMatrix[0, j] = NLoc[i];
                NmassMatrix[1, k] = NLoc[i];
                NmassMatrix[2, m] = NLoc[i];
            }
        }
        public override void StartIntegration(FEMElement element)
        {
            Beam beam = element as Beam;

            // Matrices Initialization
            _detJacobian = new double[_shape.NumIntgrPts];
            _NMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _dNMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _JMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _JInvMatrix = new Matrix<double>[_shape.NumIntgrPts];
            _NmassMatrix = new Matrix<double>[_shape.NumIntgrPts];


            if (_shape.NumIntgrPts == _gaussIntegrationPoints.Coords.Count())
            {
                for (int i = 0; i < _shape.NumIntgrPts; i++)
                {
                    _NMatrix[i] = Matrix<double>.Build.Dense(_dim, _dim * beam.NodesGlobal.Length, 0.0);
                    _dNMatrix[i] = Matrix<double>.Build.Dense(_dim, _dim * beam.NodesGlobal.Length, 0.0);
                    _JMatrix[i] = Matrix<double>.Build.Dense(2, 2, 0.0);
                    _JInvMatrix[i] = Matrix<double>.Build.Dense(2, 2, 0.0);
                    _NmassMatrix[i] = Matrix<double>.Build.Dense(3, 6, 0.0);
                }
            }

            /// Build Integration Matrices
            if (_shape.NumIntgrPts == _gaussIntegrationPoints.Coords.Count())
            {
                for (int i = 0; i < _shape.NumIntgrPts; i++)
                {
                    /// Calculation of local N and dN matrices ad the Gauss Points 
                    _shape.SetValue(_gaussIntegrationPoints.Coords[i]);

                    /// Compute the columns of the N matrix
                    BuildN(element, ref _NMatrix[i]);

                    /// Compute the columns of the J, JInv, dN matrices
                    _detJacobian[i] = BuildJ(element, ref _JMatrix[i], ref _JInvMatrix[i], ref _dNMatrix[i]);

                    /// Compute Mass Components
                    BuildMass(ref _NmassMatrix[i]);
                }
            }

            /// Build Transformation Matrix
            BuildTrfMatrix(element);

            /// Built Stiffness Matrix
            BuildK(element);

            /// Built Mass Matrix
            BuildM(element);
        }
        public override void BuildK(FEMElement element)
        {
            Beam beam = element as Beam;
            Matrix<double> k = Matrix<double>.Build.Dense(12, 12, 0.0);

            double L = beam.Length;
            double A = beam.Section.Area;
            double I1 = beam.Section.I11;
            double I2 = beam.Section.I22;
            double Jt = beam.Section.J;

            double E = beam.Section.Material.E;
            double ni = beam.Section.Material.Ni;
            double G = E / (2 * (1 + ni));
            double L2 = L * L;
            double L3 = L2 * L;

            // Column 1
            k[0, 0] = E * A / L;
            k[6, 0] = -k[0, 0];

            // Column 2
            k[1, 1] = 12.0 * E * I1 / L3;
            k[5, 1] = 6.0 * E * I1 / L2;
            k[7, 1] = -k[1, 1];
            k[11, 1] = k[5, 1];

            // Column 3
            k[2, 2] = 12.0 * E * I2 / L3;
            k[4, 2] = 6.0 * E * I2 / L2;
            k[8, 2] = -k[2, 2];
            k[10, 2] = k[4, 2];

            // Column 4
            k[3, 3] = G * Jt / L;
            k[9, 3] = -k[3, 3];

            // Column 5
            k[4, 4] = 4.0 * E * I2 / L;
            k[8, 4] = -k[4, 2];
            k[10, 4] = 2.0 * I2 * E / L;

            // Column 6
            k[5, 5] = 4.0 * I1 * E / L;
            k[7, 5] = -k[5, 1];
            k[11, 5] = 2.0 * I1 * E / L;

            // Column 7
            k[6, 6] = k[0, 0];

            // Column 8
            k[7, 7] = k[1, 1];
            k[11, 7] = -k[5, 1];

            // Column 9
            k[8, 8] = k[2, 2];
            k[10, 8] = -k[4, 2];

            // Column 10
            k[9, 9] = k[3, 3];

            // Column 11
            k[10, 10] = k[4, 4];

            // Column 12
            k[11, 11] = k[5, 5];


            // Mirroring of the Stiffness Matrix
            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < r; c++)
                    //k[r, c] = k[c, r];
                    k[c, r] = k[r, c];
            }

            _kMatrix = _trfMatrix.Transpose() * k;
            _kMatrix = _kMatrix * _trfMatrix;
            //_stiffnessMatrix = k;
            //string path = "C:\\Users\\r.vochescu\\Desktop\\" + "STIFF-MATRIX" + beam.Guid.ToString() + ".txt";
            //// This text is added only once to the file.
            //if (File.Exists(path) == true)
            //{
            //    File.Delete(path);
            //}
            //if (!File.Exists(path))
            //{
            //    string matrix = "";
            //    for (int r = 0; r < 12; r++)
            //    {
            //        for (int c = 0; c < 12; c++)
            //        {
            //            matrix = matrix + "\t" + _stiffnessMatrix[r, c].ToString();
            //        }
            //        matrix  = matrix + Environment.NewLine;
            //    }
            //    File.WriteAllText(path, matrix);
            //}
        }
        public override void BuildD(FEMElement element)
        {
        }
        public override void BuildN(FEMElement element, ref Matrix<double> NMatrix)
        {
            Beam beam = element as Beam;

            int numNode = beam.NodesGlobal.Length;

            NMatrix = Matrix<double>.Build.Dense(_dim, _dim * numNode, 2.0);

            for (int j = 0; j < numNode; j++)
            {
                for (int i = 0; i < _dim; i++)
                {
                    NMatrix[i, i + j * _dim] = _shape.NShape[j];
                }
            }
        }
        public override void BuildT()
        {
        }
        public override void BuildF()
        {
        }
        public override double BuildJ(FEMElement element, ref Matrix<double> J, ref Matrix<double> Jinv, ref Matrix<double> dNMatrix)
        {
            double detJ = 0;
            int numPt = element.NodesLocal.Length;

            Matrix<double> matCoord = Matrix<double>.Build.Dense(numPt, _dim, 0.0);
            Matrix<double> dNloc = Matrix<double>.Build.Dense(_dim, numPt, 0.0);

            for (int i = 0; i < numPt; i++)
            {
                matCoord[i, 0] = element.NodesLocal[i].Position.X;
                //matCoord[i, 1] = element.NodesLocal[i].Position.Y;
            }

            J = Matrix<double>.Build.Dense(2, 2, 0.0);
            Jinv = Matrix<double>.Build.Dense(2, 2, 0.0);

            // J2 = dNloc * matCoord;
            J = _shape.dNShape * matCoord;
            detJ = J.Determinant();
            Jinv = J.Inverse();

            //dNGlob = JInv * dNloc
            dNMatrix = Jinv * _shape.dNShape;

            return detJ;
        }
        public override void BuildB()
        {
        }
        public override void RegisterDoF(Node node)
        {
        }
        public override void BuildTrfMatrix(FEMElement element)
        {
            Beam beam = element as Beam;
            Vector3d ZAxis = new Vector3d(0, 0, 1);
            double checkVert = ZAxis.CrossProduct(beam.CoordSys.V33).Length;

            /// Create Local Transformation Matrix
            Matrix<double> tfrMatrix1 = Matrix<double>.Build.Dense(3, 3, 0);

            if (checkVert < 1.0E-12)
            {
                double CX = beam.CoordSys.V33.X;
                double CY = beam.CoordSys.V33.Y;
                double CZ = beam.CoordSys.V33.Z;
                double sen = Math.Sin(beam.CoordSys.RotAngle * 3.14159 / 180);
                double cos = Math.Cos(beam.CoordSys.RotAngle * 3.14159 / 180);

                tfrMatrix1[0, 0] = 0;
                tfrMatrix1[0, 1] = 0;
                tfrMatrix1[0, 2] = CZ;

                tfrMatrix1[1, 0] = -Math.Pow(CZ, 2.0);
                tfrMatrix1[1, 1] = 0;
                tfrMatrix1[1, 2] = 0;

                tfrMatrix1[2, 0] = 0;
                tfrMatrix1[2, 1] = CZ;
                tfrMatrix1[2, 2] = 0;
            }
            else
            {
                double CX = beam.CoordSys.V33.X;
                double CY = beam.CoordSys.V33.Y;
                double CZ = beam.CoordSys.V33.Z;
                double d = Math.Sqrt(Math.Pow(CX, 2.0) + Math.Pow(CY, 2.0));
                double sen = Math.Sin(beam.CoordSys.RotAngle * 3.14159 / 180);
                double cos = Math.Cos(beam.CoordSys.RotAngle * 3.14159 / 180);

                tfrMatrix1[0, 0] = CX;
                tfrMatrix1[0, 1] = CY;
                tfrMatrix1[0, 2] = CZ;

                tfrMatrix1[1, 0] = -CY / d;
                tfrMatrix1[1, 1] = CX / d;
                tfrMatrix1[1, 2] = 0;

                tfrMatrix1[2, 0] = -CX * CZ / d;
                tfrMatrix1[2, 1] = -CY * CZ / d;
                tfrMatrix1[2, 2] = (Math.Pow(CX, 2.0) + Math.Pow(CY, 2.0)) / d;
            }

            /// Set Transformation Matrix for beam Element
            _trfMatrix = Matrix<double>.Build.Dense(12, 12, 0);

            for (int i = 0; i < 4; i++)
            {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        _trfMatrix[i * 3 + r, i * 3 + c] = tfrMatrix1[r, c];
                    }
                }
            }
        }

        public override void BuildM(FEMElement element)
        {
            Beam beam = element as Beam;

            _massMatrix = Matrix<double>.Build.Dense(12, 12, 0.0);
            Matrix<double> M = Matrix<double>.Build.Dense(12, 12, 0.0);
            Matrix<double> mMatrix = Matrix<double>.Build.Dense(6, 6, 0.0);

            if (_numTotDoF > 0 && _gaussIntegrationPoints.Coords.Length > 0)
            {
                Matrix<double> scalM;
                Matrix<double> gaussM;

                for (int i = 0; i < _gaussIntegrationPoints.Coords.Length; i++)
                {
                    double c = beam.Section.Material.Density * _gaussIntegrationPoints.Weights[i] * _detJacobian[i] * beam.Section.Area;
                    gaussM = _NmassMatrix[i].Transpose() * c * _NmassMatrix[i];
                    mMatrix = mMatrix + gaussM;
                }
            }

            /// Assembling Stiffness Matrix
            int[] nmass = new int[] { 0, 1, 2, 6, 7, 9};
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    M[nmass[i], nmass[j]] = mMatrix[i, j];
                }
            }
            _massMatrix = _trfMatrix.Transpose() * M * _trfMatrix;
        }
        #endregion

        #region Public Methods Specific
        protected void InitBeam2(FEMElement element)
        {
            SetIntegrationPoint(new FEMGaussIntegrationBeam(2), new FEMGaussIntegrationBeam(2));
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}

