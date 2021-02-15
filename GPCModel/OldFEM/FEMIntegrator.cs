using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    public abstract class FEMIntegrator : FEMObject
    {
        #region Variables 
        /// <summary>
        /// <param name="_dim"> Problem dimension (one dimension domain, two-dimension domain, three-dimensions domain </param>
        /// <param name="_order"> Order of shape functions </param>
        /// </summary>
        protected Matrix<double> _kMatrix;
        protected Matrix<double> _tangentMatrix;
        protected Matrix<double> _massMatrix;
        protected Matrix<double> _trfMatrix;
        protected int _numTotDoF;
        protected int _numTotActiveDoF;
        protected FEMGaussIntegration _gaussIntegrationPoints;
        protected FEMGaussIntegration _gaussStrainPoints;
        protected FEMShape _shape;
        protected int _dim;
        protected int _numDefComp;
        protected int _order;

        protected double[] _detJacobian;
        protected Matrix<double>[] _NMatrix;
        protected Matrix<double>[] _dNMatrix;
        protected Matrix<double>[] _JMatrix;
        protected Matrix<double>[] _JInvMatrix;
        protected Matrix<double>[] _NmassMatrix;
        #endregion

        #region Properties
        public Matrix<double> KMatrix => _kMatrix;
        public Matrix<double> TangentMatrix => _tangentMatrix;
        public Matrix<double> MassMatrix => _massMatrix;
        public Matrix<double> TrfMatrix { get => _trfMatrix; set { _trfMatrix = value; } }
        public int NumTotDoF { get => _numTotDoF; set { _numTotDoF = value; } }
        public int NumTotActiveDoF { get => _numTotActiveDoF; set { _numTotActiveDoF = value; } }
        public FEMGaussIntegration GaussIntegrationPoints => _gaussIntegrationPoints;
        public FEMGaussIntegration GaussStrainPoints => _gaussStrainPoints;
        public FEMShape Shape => _shape;
        public int Dim => _dim;
        public int Order => _order;
        public int NumDefComp => _numDefComp;
        public double[] DetJacobian => _detJacobian;
        public Matrix<double>[] NMatrix => _NMatrix;
        public Matrix<double>[] dNMatrix => _dNMatrix;
        public Matrix<double>[] JMatrix => _JMatrix;
        public Matrix<double>[] JInvMatrix => _JInvMatrix;
        public Matrix<double>[] NmassMatrix => _NmassMatrix;
        #endregion

        #region Public Constructors
        protected FEMIntegrator(Guid guid)
        {
            _dim = 1;
            _order = 1;
        }

        protected FEMIntegrator(Guid guid, int dim, int order, int numDefComp)
        {
            _guid = guid;
            _dim = dim;
            _order = order;
            _numDefComp = numDefComp;
        }

        protected FEMIntegrator(SerializationInfo info, StreamingContext context)
        {
        }

        #endregion

        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        public abstract void BuildMass(ref Matrix<double> NmassMatrix);
        public abstract void BuildN(FEMElement element, ref Matrix<double> NMatrix);
        public abstract double BuildJ(FEMElement element, ref Matrix<double> J, ref Matrix<double> Jinv, ref Matrix<double> dNGlob);
        public abstract void BuildB();
        public abstract void BuildD(FEMElement element);
        public abstract void BuildK(FEMElement element);
        public abstract void BuildM(FEMElement element);
        public abstract void BuildT();
        public abstract void BuildF();
        public abstract void BuildTrfMatrix(FEMElement element);
        public abstract void RegisterDoF(Node node);
        public abstract void StartIntegration(FEMElement element);
        public virtual void SetIntegrationPoint(FEMGaussIntegration gaussIntegrationPoints, FEMGaussIntegration gaussStrainPoints)
        {
            _gaussIntegrationPoints = gaussIntegrationPoints;
            _gaussStrainPoints = gaussStrainPoints;
        }

        public virtual void ComputeStrain(FEMElement element)
        {
        }
        public virtual void ComputeStress(Node node)
        {
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
