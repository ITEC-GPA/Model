using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    public abstract class FEMIntegrator : FEMObject
    {
        #region Variables 
        protected Matrix<double> _stiffnessMatrix;
        protected Matrix<double> _tangentMatrix;
        protected Matrix<double> _massMatrix;
        protected Matrix<double> _transformationMatrix;
        protected int _numTotDoF;
        protected int _numTotActiveDoF;
        #endregion

        #region Properties
        public Matrix<double> StiffnessMatrix => _stiffnessMatrix;
        public Matrix<double> TangentMatrix => _tangentMatrix;
        public Matrix<double> MassMatrix => _massMatrix;
        public Matrix<double> TransformationMatrix { get => _transformationMatrix; set { _transformationMatrix = value; } }
        public int NumTotDoF { get =>  _numTotDoF; set { _numTotDoF = value; }  }
        public int NumTotActiveDoF { get => _numTotActiveDoF; set { _numTotActiveDoF = value; } }
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
        public abstract void BuildK(FEMElement element);
        public abstract void BuildT();
        public abstract void BuildJ();
        public abstract void BuildB();
        public abstract void BuildF();
        public abstract void BuildTrfMatrix(FEMElement element);
        public abstract void RegisterDoF(Node node);

        #endregion

        #region Private Methods Specific
        #endregion
    }
}
