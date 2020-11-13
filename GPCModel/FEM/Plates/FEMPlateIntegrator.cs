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
    public abstract class FEMPlateIntegrator : FEMIntegrator
    {
        #region Variables 
        /// <summary>
        /// <param name="_Dm"> The constitutive matrix which relates stress and strain vectors - Membranal Components </param>
        /// <param name="_Db">  The constitutive matrix which relates stress and strain vectors - Bending Components  </param>
        /// <param name="_BmMatrix"> The matrix which relates joint displacements to strain field - Membranal Components </param>
        /// <param name="_BfMatrix"> The matrix which relates joint displacements to strain field - Bending Components  </param>
        /// <param name="_KmMatrix"> Stiffness matrix - Membranal Components </param>
        /// <param name="_KbMatrix"> Stiffness matrix - Bending Components  </param>
        /// </summary>

        protected double[] _detJacobian;
        protected Matrix<double>[] _NMatrix;
        protected Matrix<double>[] _dNMatrix;
        protected Matrix<double>[] _JMatrix;
        protected Matrix<double>[] _JInvMatrix;
        protected Matrix<double> _Dm;
        protected Matrix<double> _Db;
        protected Matrix<double>[] _BmMatrix;
        protected Matrix<double>[] _BbMatrix;
        protected Matrix<double>[] _dHMatrix;
        protected Matrix<double> _KmMatrix;
        protected Matrix<double> _KbMatrix;
        protected FEMShape _shapeBending;
        protected FEMShape _shapeDrilling;
        #endregion

        #region Properties
        public Matrix<double>[] NMatrix => _NMatrix;
        public Matrix<double>[] dNMatrix => _dNMatrix;
        public Matrix<double>[] JMatrix => _JMatrix;
        public Matrix<double>[] JInvMatrix => _JInvMatrix;
        public Matrix<double> Dm => _Dm;
        public Matrix<double> Db => _Db;
        public Matrix<double>[] BmMatrix => _BmMatrix;
        public Matrix<double>[] BbMatrix => _BbMatrix;
        public Matrix<double>[] dHMatrix => _dHMatrix;
        public Matrix<double> KmMatrix => _KmMatrix;
        public Matrix<double> KbMatrix => _KbMatrix;
        public FEMShape ShapeBending => _shapeBending;
        public FEMShape ShapeDrilling => _shapeDrilling;
        #endregion

        #region Public Constructors
        public FEMPlateIntegrator(Guid guid, int dim, int order, int numDefComp, FEMElement element)
            : base(guid, dim, order, numDefComp)
        {         
        }
        public FEMPlateIntegrator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        #region Public Methods Override
        public override void BuildN(FEMElement element, ref Matrix<double> NMatrix)
        {
            int numNode = element.NodesGlobal.Length;

            NMatrix = Matrix<double>.Build.Dense(_dim, _dim * numNode, 0.0);

            for (int j = 0; j < numNode; j++)
            {
                for (int i = 0; i < _dim; i++)
                {
                    NMatrix[i, i + j * _dim] = _shape.NShape[j];
                }
            }
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
        public override double BuildJ(FEMElement element, ref Matrix<double> J, ref Matrix<double> Jinv, ref Matrix<double> dNMatrix)
        {
            double detJ = 0;
            int numPt = element.NodesLocal.Length;

            Matrix<double> matCoord = Matrix<double>.Build.Dense(numPt, _dim, 0.0);
            Matrix<double> dNloc = Matrix<double>.Build.Dense(_dim, numPt, 0.0);

            for (int i = 0; i < numPt; i++)
            {
                matCoord[i, 0] = element.NodesLocal[i].Position.X;
                matCoord[i, 1] = element.NodesLocal[i].Position.Y;
            }

            J = Matrix<double>.Build.Dense(2, 2, 0.0);
            Jinv = Matrix<double>.Build.Dense(2, 2, 0.0);

            // J2 = dNloc * matCoord;
            J = _shape.dNShape * matCoord;
            detJ = J.Determinant();
            Jinv = J.Inverse();

            //dNGlob = JInv * dNloc
            dNMatrix = Jinv * _shape.dNShape;
   
            return detJ;
        }
        public override void BuildB()
        {


            //int j, k, m, p, q;
            //double dNx, dNy;
            //int col = 5;

            //int numNode = _shape.NumIntgrPts;

            //for (int i = 0; i < numNode; i++)
            //{
            //    j = col * i;
            //    k = j + 1;

            //    dNx = dN[0, i];
            //    _BmMatrix[0, j] = dNx;
            //    _BmMatrix[2, k] = dNx;

            //    dNy = dN[1, i];
            //    _BmMatrix[1, k] = dNy;
            //    _BmMatrix[2, j] = dNy;


            //    m = j + 2;
            //    p = j + 3;
            //    q = j + 4;

            //    // Versione Bathe
            //    /// Componente flessionale
            //    _BmMatrix[3, q] = -dNx;
            //    _BmMatrix[4, p] = +dNy;
            //    _BmMatrix[5, p] = +dNx;
            //    _BmMatrix[5, q] = -dNy;
            //    /// Componente tagliante
            //    _BmMatrix[6, m] = dNx;
            //    _BmMatrix[6, q] = +_shape.NShape[i];
            //    _BmMatrix[7, m] = dNy;
            //    _BmMatrix[7, p] = -_shape.NShape[i];

            //    //// Versione Cook
            //    ///// Componente flessionale
            //    //_BMatrix[3,p] = +dNx;
            //    //_BMatrix[4,q] = +dNy;
            //    //_BMatrix[5,q] = +dNx;
            //    //_BMatrix[5,p] = +dNy;

            //    ///// Componente tagliante
            //    //_BMatrix[6,m] = -dNy;
            //    //_BMatrix[6,q] = +_shape.NShape[i];
            //    //_BMatrix[7,m] = -dNx;
            //    //_BMatrix[7,p] = +_shape.NShape[i];
            //}
    }
        public override void RegisterDoF(Node node)
        {
        }
        public override void BuildTrfMatrix(FEMElement element)
        {
            Plate plate = element as Plate;

            if ((element.NodesGlobal.Length >= 3) == false ) { throw new ArgumentException($"{nameof(element.NodesGlobal.Length)} Number of Nodes not compatible with Element Definition"); }

            if ((element.NodesGlobal.Length >= 3) == true)
            {
                int totalDof = element.NodesGlobal.Length * 6;
                _trfMatrix = Matrix<double>.Build.Dense(totalDof, totalDof, 0);

                int tot = element.NodesGlobal.Length * 2;
                for (int i = 0; i < tot; i++)
                {
                    for (int r = 0; r < 3; r++)
                        for (int c = 0; c < 3; c++)
                            _trfMatrix[i * 3 + r, i * 3 + c] = element.CoordSys.TrfMatrix[c, r];
                }
            }
        }
        #endregion

        #region Public Methods Specific
        public abstract void BuildMembranal(ref Matrix<double> BmMatrix, Matrix<double> dNMatrix, Matrix<double> JInvMatrix);
        public abstract void BuildBending(ref Matrix<double> BpMatrix, ref Matrix<double> dHMatrix, Matrix<double> JInvMatrix);

        public override void BuildD(FEMElement element)
        {
            Plate plate = element as Plate;

            _Dm = Matrix<double>.Build.Dense(3, 3, 0);
            _Db = Matrix<double>.Build.Dense(3, 3, 0);

            double E = plate.Property.Material.E;
            double ni = plate.Property.Material.Ni;
            double tb = plate.Property.Tb;
            double tm = plate.Property.Tm;

            double c, cc;
            c = E / (1 - Math.Pow(ni, 2.0));
            _Dm[0, 0] = c;
            _Dm[1, 1] = c;
            _Dm[2, 2] = 0.5 * c * (1.0 - ni);
            _Dm[0, 1] = ni * c;
            _Dm[1, 0] = _Dm[0, 1];

            cc = c * Math.Pow(tb, 3.0) / 12.0;
            _Db[0, 0] = cc;
            _Db[1, 1] = cc;
            _Db[2, 2] = 0.5 * cc * (1.0 - ni);
            _Db[0, 1] = ni * cc;
            _Db[1, 0] = _Db[0, 1];
        }
        public override void StartIntegration(FEMElement element)
        {
            //_detJacobian = new double[_shape.NumIntgrPts];
            //_NMatrix = Matrix<double>.Build.Dense(_dim, _dim * element.Nodes.Length, 0);
            //_dNMatrix = Matrix<double>.Build.Dense(3, 3, 0);
            //_BmMatrix = Matrix<double>.Build.Dense(3, 3 * element.Nodes.Length, 0);

            //BuildN(element);

            //if (_shape.NumIntgrPts !=  _gaussIntegrationPoints.Coords.Count()) { throw new ArgumentException($"{nameof(_shape.NumIntgrPts)} Gauss Point Number in the Shape Function are different from the one defined in the Integrator"); }
            //if (_shape.NumIntgrPts == _gaussIntegrationPoints.Coords.Count())
            //{
            //    for (int i = 0; i < _shape.NumIntgrPts; i++)
            //    {
            //        /// Calculation of local N and dN in the shape fuction in the coordinates X,Y
            //        _shape.SetValue(_gaussIntegrationPoints.Coords[i]);

            //        _detJacobian[i] = BuildJ(element);
            //    }
            //}

            //BuildD(element);
        }
        protected void InitTri3(FEMElement element)
        {
            SetIntegrationPoint(new FEMGaussIntegrationTri(1), new FEMGaussIntegrationQuad(1));
        }
        protected void InitTri6(FEMElement element)
        {
            SetIntegrationPoint(new FEMGaussIntegrationTri(3), new FEMGaussIntegrationQuad(3));
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
