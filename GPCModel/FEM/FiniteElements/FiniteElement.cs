using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Each finite element should derive from this
    /// </summary>
    public abstract class FiniteElement //: ModelObject //COMMENTATO PERCHE' EQUALS NON FUNZIONA CORRETTAMENTE SE DERIVATO DA ModelObject
    {
        #region Variables
        protected int _ID;

        protected double[] _vecXLocal = new double[3]; //versor X local in Global Coordinate Sys
        protected double[] _vecYLocal = new double[3]; //versor Y local in Global Coordinate Sys
        protected double[] _vecZLocal = new double[3]; //versor Z local in Global Coordinate Sys

        protected Dictionary<DOF, bool> _DOF = new Dictionary<DOF, bool>(Enum.GetNames(typeof(DOF)).Length);

        protected Matrix<double> _dofGlobalToLocal;
        protected Matrix<double> _kElementLocalCoord;
        protected Matrix<double> _b;
        protected Matrix<double> _d;
        #endregion


        #region Properties

        public double[][] LocalAxisVersors
        {
            get
            {
                double[][] axis = new double[3][];
                axis[0] = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    axis[0][i] = _vecXLocal[i];
                }
                axis[1] = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    axis[1][i] = _vecYLocal[i];
                }
                axis[2] = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    axis[2][i] = _vecZLocal[i];
                }
                return axis;
            }
        }

        /// <summary>
        /// DOF[degree of freedom] = true if active, false if unactive
        /// </summary>
        public Dictionary<DOF, bool> DOF => _DOF;

        /// <summary>
        /// Nr of degree of freedom active
        /// </summary>
        public int NrDOFActive
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < DOF.Count; i++)
                {
                    if (DOF[(DOF)i] == true) {
                        counter++;
                    }
                }
                return counter;
            }
        }

        /// <summary>
        /// Node with in gloabal coordinate system
        /// </summary>
        public Node[] GlobalNodesElement { get; }

        /// <summary>
        /// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal] or for UlocalCoord = DofGlobalToLocal UglobalCoord; NOTE: DofLocalToGlobal = DofGlobalToLocal^TRASPOSTE
        /// </summary>
        public Matrix<double> DofGlobalToLocal => _dofGlobalToLocal;

        /// <summary>
        /// B : derivative of ShapeFunctions, need for epsilon = [B] * q with q = node displacements vector
        /// </summary>
        public Matrix<double> B => _b;

        /// <summary>
        /// sigma = [D] * epsilon
        /// </summary>
        public Matrix<double> D => _d;

        /// <summary>
        /// ke = int [B]^T [D] [B] dV (stiffness matrix in local coordinates)
        /// </summary>
        public Matrix<double> KElementLocalCoord => _kElementLocalCoord;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="nodes">Set the nodes of element</param>
        /// <param name="id">id of element</param>
        public FiniteElement(IEnumerable<Node> nodes, int id)
        {
            _ID = id;
            GlobalNodesElement = nodes.ToArray();
            _DOF[FEM.DOF.DX] = false;
            _DOF[FEM.DOF.DY] = false;
            _DOF[FEM.DOF.DZ] = false;
            _DOF[FEM.DOF.RX] = false;
            _DOF[FEM.DOF.RY] = false;
            _DOF[FEM.DOF.RZ] = false;
        }
        #endregion

        #region PublicFunction
        /// <summary>
        /// Build Stiffness Matrix etc
        /// </summary>
        public abstract void BuildMatrix();

        /// <summary>
        /// Build vector of Forces in nodes due to internal action applied (shear stress, prestress etc)
        /// </summary>
        public abstract void BuildF();
        #endregion
    }
}
