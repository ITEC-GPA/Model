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
    class FEMPlateIntegrator : FEMIntegrator
    {
        #region Variables 
        #endregion

        #region Properties
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
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
