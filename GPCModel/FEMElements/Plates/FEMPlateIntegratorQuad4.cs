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
    public class FEMPlateIntegratorQuad4 : FEMPlateIntegrator
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMPlateIntegratorQuad4(Guid guid)
            : base(guid)
        {
        }
        public FEMPlateIntegratorQuad4(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        public override void BuildK(FEMElement element)
        {
            Plate plate = element as Plate;
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
        public override void BuildTrfMatrix(FEMElement element)
        {
            Plate plate = element as Plate;
        }
        public override void BuildD(Plate plate)
        {
            _DPlane = Matrix<double>.Build.Dense(3, 3, 0);
            _DBending = Matrix<double>.Build.Dense(3, 3, 0);

            double c, cc;
            //c = pProp->m_E / (1. - pProp->m_Poisson * pProp->m_Poisson);
            //_DPlane[0, 0] = c;
            //_DPlane[1, 1] = c;
            //_DPlane[2, 2] = 0.5 * c * (1. - pProp->m_Poisson);
            //_DPlane[0, 1] = pProp->m_Poisson * c;
            //_DPlane[1, 0] = m_Dplane(0, 1);

            //cc = c * (pProp->m_Thickness * pProp->m_Thickness * pProp->m_Thickness) / 12.;
            //_DBending[0, 0] = cc;
            //_DBending[1, 1] = cc;
            //_DBending[2, 2] = 0.5 * cc * (1. - pProp->m_Poisson);
            //_DBending[0, 1] = pProp->m_Poisson * cc;
            //_DBending[1, 0] = _DBending(0, 1);
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
