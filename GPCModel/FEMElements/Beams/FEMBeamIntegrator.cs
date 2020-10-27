using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using GPC.Model.Sections;

namespace GPC.Model.FEM
{
    public class FEMBeamIntegrator : FEMIntegrator
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMBeamIntegrator(Guid guid)
            : base(guid)
        {
        }

        public FEMBeamIntegrator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        public override void BuildK(Beam beam)
        {
            Matrix<double> k = Matrix<double>.Build.Dense(12, 12, 0.0);

            //TBeamElasticProperty* pProp = (TBeamElasticProperty*)m_pElement->GetProperty();
            //TFemSectionData* pSectionData = &pProp->m_SectionData;
            //double E = pProp->m_E;
            //double G = E / (2 * (1 + pProp->m_Poisson));
            //double L = pBeam->m_Length, L2 = L * L, L3 = L2 * L;

            double L = beam.Length;
            double A = beam.Section.Area;
            double I1 = beam.Section.I11;
            double I2 = beam.Section.I22;
            double Jt = beam.Section.J;
            
            double E = 30000;
            double ni = 0.3;
            double G = E / (2 * (1 + 0.3));
            double L2 = L * L; 
            double L3 = L2 * L;

            // Column 1
            k[0, 0] =  E * A / L;
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
            k[11, 5] = 2.0 * I1 * E/ L;

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

    
            // Specchia la matrice per simmetria
            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < r; c++)
                    //k[r, c] = k[c, r];
                    k[c, r] = k[r, c];
            }

            _stiffnessMatrix = _transformationMatrix.Transpose() * k;
            _stiffnessMatrix = _stiffnessMatrix * _transformationMatrix;
            //_stiffnessMatrix = k;

            string path = "C:\\Users\\r.vochescu\\Desktop\\" + "STIFF-MATRIX" + beam.Guid.ToString() + ".txt";
            // This text is added only once to the file.
            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
                string matrix = "";
                for (int r = 0; r < 12; r++)
                {
                    for (int c = 0; c < 12; c++)
                    {
                        matrix = matrix + "\t" + _stiffnessMatrix[r, c].ToString();
                    }
                    matrix  = matrix + Environment.NewLine;
                }
                File.WriteAllText(path, matrix);
            }
        }
        public override void BuildT()
        {
        }
        public override void BuildF()
        {
        }
        public override void BuildJ()
        {
        }
        public override void BuildB()
        {
        }
        public override void RegisterDoF(Node node)
        {
        }
        public override void BuildTrfMatrix(Beam beam)
        {

        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}

