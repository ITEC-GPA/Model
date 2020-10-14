using GPC.Model.Elements;
using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public abstract class FEMElement : Element
    {
        #region Variables
        protected FEMIntegrator _integrator;
        #endregion

        #region Properties
        public FEMIntegrator Integrator => _integrator;
        #endregion

        #region Public Constructors
        protected FEMElement(Guid guid, FEMIntegrator integrator)
            : base(guid)
        {
            _integrator = integrator;
        }

        protected FEMElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        /*public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }*/
        #endregion

        #region Public Methods Specific
        public virtual void ElementIncidence()
        {
        }
        public virtual void StiffnessMatrixInGlobal()
        {
        }
        public virtual void TangentMatrixInGlobal()
        {
        }
        public virtual void MassMatrixInGlobal()
        {
        }
        public virtual void KnownValueVectorInGlobal()
        {
        }
        public virtual void ChooseIntegrator()
        {
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
