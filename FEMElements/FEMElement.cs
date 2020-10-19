using GPC.Model.Elements;
using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    public abstract class FEMElement : Element
    {
        #region Variables
        protected FEMIntegrator _integrator;
        protected Node[] _nodes;
        protected int[,] _elIncidence;
        #endregion

        #region Properties
        public FEMIntegrator Integrator => _integrator;
        public int[,] ElIncidence => _elIncidence;
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
        public abstract void ElementIncidence();
        public virtual void KInGlobal(ref Matrix<double> Kg)
        {
        }
        public virtual void TInGlobal()
        {
        }
        public virtual void MInGlobal()
        {
        }
        public virtual void FInGlobal()
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
