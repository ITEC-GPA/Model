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
    public class FEMPlateIntegrator : FEMIntegrator
    {
        #region Variables 
        protected FEMShape _shape;
        protected Matrix<double> _DPlane;
        protected Matrix<double> _DBending;
       #endregion

         #region Properties
        public FEMShape Shape => _shape;
        public Matrix<double> DPlane => _DPlane;
        public Matrix<double> DBending => _DBending;
        #endregion

        #region Public Constructors
        public FEMPlateIntegrator(Guid guid)
            : base(guid)
        {
        }
        public FEMPlateIntegrator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        #region Public Methods Override
        public override void BuildN()
        {
        }
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
        #endregion

        #region Public Methods Specific
        public override void BuildD(FEMElement element)
        {
        }
        public override void StartIntegration(FEMElement element)
        {
        }
        protected void InitTri3(FEMElement element)
        {
        }
        protected void InitQuad4(FEMElement element)
        {
            SetIntegrationPoint(new FEMGaussIntegrationQuad(2, 2), new FEMGaussIntegrationQuad(2, 2));
        }
        protected void InitQuad8(FEMElement element)
        {
            SetIntegrationPoint(new FEMGaussIntegrationQuad(3, 3), new FEMGaussIntegrationQuad(2, 2));
        }
        protected void InitQuad9(FEMElement element)
        {
            SetIntegrationPoint(new FEMGaussIntegrationQuad(3, 3), new FEMGaussIntegrationQuad(2, 2));
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
