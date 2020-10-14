using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public abstract class FEMIntegrator : FEMObject
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        protected FEMIntegrator(Guid guid)
        {
        }

        protected FEMIntegrator(SerializationInfo info, StreamingContext context)
        {
        }

        #endregion

        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        public abstract void BuildStiffnessMatrix();
        public abstract void BuildTangentMatrix();
        public abstract void BuildJacobianMatrix();
        public abstract void BuildBMatrix();
        public abstract void BuildKnownValue();
        public abstract void RegisterDoF(Node node);

        #endregion

        #region Private Methods Specific
        #endregion
    }
}
