using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    class FEMBeamIntegrator : FEMIntegrator
    {
        #region Variables 
        protected Matrix<double> _StiffnessMatrix;
        protected Matrix<double> _TangentMatrix;
        protected Matrix<double> _MassMatrix;
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        protected FEMBeamIntegrator(Guid guid)
            : base(guid)
        {
        }

        protected FEMBeamIntegrator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        public override void BuildStiffnessMatrix()
        {
            Matrix<double> k = Matrix<double>.Build.Dense(12, 12, 0.0);

            //TBeamElasticProperty* pProp = (TBeamElasticProperty*)m_pElement->GetProperty();
            //TFemSectionData* pSectionData = &pProp->m_SectionData;
            //double E = pProp->m_E;
            //double G = E / (2 * (1 + pProp->m_Poisson));
            //double L = pBeam->m_Length, L2 = L * L, L3 = L2 * L;
            //double A = pSectionData->GetA();
            //double I1 = pSectionData->GetI11();
            //double I2 = pSectionData->GetI22();
            //double K = pSectionData->GetJt();

            double E = 200000;
            double ni = 0.3;
            double G = E / (2 * (1 + 0.3));
            double L = 1000;
            double L2 = L * L; 
            double L3 = L2 * L;
            double A = 10000;
            double I1 = 1e9;
            double I2 = 1e9;
            double Jt = 1e9;

            k[0, 0] =  12 * E * I2 / L3;
            k[6, 6] = k[0, 0];
            k[0, 6] = -k[0, 0];
            k[0, 4] = 6 * E * I2 / L2;
            k[0, 10] = k[0, 4];

            k[1, 1] = 12 * E * I1 / L3;
            k[7, 7] = k[1, 1];
            k[1, 7] = -k[1, 1];
            k[1, 3] = -6 * E * I1 / L2;
            k[1, 9] = k[1, 3];

            k[2, 2] = E * A / L;
            k[8, 8] = k[2, 2];
            k[2, 8] = -k[2, 2];

            k[3, 3] = 4 * E * I1 / L;
            k[9, 9] = k[3, 3];
            k[3, 7] = 6 * E * I1 / L2;
            k[3, 9] = 2 * E * I1 / L;

            k[4, 4] =  4 * E * I2 / L;
            k[10, 10] = k[4, 4];
            k[4, 6] = -6 * E * I2 / L2;
            k[4, 10] = 2 * E * I2 / L;

            k[5, 5] = G * Jt / L;
            k[11, 11] = k[5, 5];
            k[5, 11] = -k[5, 5];

            k[6, 10] = -k[0, 4];
            k[7, 9] = -k[1, 3];

            // Specchia la matrice per simmetria
            for (int r = 0; r < 11; r++)
            {
                for (int c = 0; c < r; c++)
                    k[r, c] = k[c, r];
            }
        }
        public override void BuildTangentMatrix()
        {
        }
        public override void BuildKnownValue()
        {
        }
        public override void BuildJacobianMatrix()
        {
        }
        public override void BuildBMatrix()
        {
        }
        public override void RegisterDoF(Node node)
        {
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}

