using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Each finite element should derive from this
    /// </summary>
    public abstract class FiniteElement : FEMObject
    {
        #region Variables
        protected CoordinateSystem _localCoordinateSystem;
        /*protected double[] _vecXLocal = new double[3]; //versor X local in Global Coordinate Sys
        protected double[] _vecYLocal = new double[3]; //versor Y local in Global Coordinate Sys
        protected double[] _vecZLocal = new double[3]; //versor Z local in Global Coordinate Sys*/

        protected SortedSet<LinearSolver.DOF> _DOF;
        
        protected mnl.Matrix<double> _dofGlobalToLocal;
        protected mnl.Matrix<double> _kElementLocalCoord;
        protected mnl.Matrix<double> _b;
        protected mnl.Matrix<double> _d;
        protected ElementProperty _property;
        protected List<IPlateLoadCaseAttribute> _attributes;

        protected Node[] _nodes;
        #endregion

        #region Properties
        /*public double[][] LocalAxisVersors
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
        }*/
        public CoordinateSystem LocalCoordinateSystem => _localCoordinateSystem;

        /// <summary>
        /// Contains the DOF active in the element
        /// </summary>
        public SortedSet<LinearSolver.DOF> DOF => _DOF;

        /// <summary>
        /// Contains Material for brick, thickness and material for plate, material + section for beam
        /// </summary>
        public ElementProperty Property => _property;

        /// <summary>
        /// Nr of degree of freedom active
        /// </summary>
        public int NrDOFActive
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < DOF.Count(); i++)
                {
                    if (DOF.Contains((LinearSolver.DOF)i) == true) {
                        counter++;
                    }
                }
                return counter;
            }
        }

        /// <summary>
        /// Nodes of the element
        /// </summary>
        public Node[] Nodes => _nodes;

        /// <summary>
        /// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal] or for UlocalCoord = DofGlobalToLocal UglobalCoord; NOTE: DofLocalToGlobal = DofGlobalToLocal^TRASPOSTE
        /// </summary>
        public mnl.Matrix<double> DofGlobalToLocal => _dofGlobalToLocal;

        /// <summary>
        /// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal]
        /// </summary>
        public mnl.Matrix<double> KElementGlobalCoord => DofGlobalToLocal.Transpose() * KElementLocalCoord * DofGlobalToLocal;

        /// <summary>
        /// B : derivative of ShapeFunctions, need for epsilon = [B] * q with q = node displacements vector
        /// </summary>
        public mnl.Matrix<double> B => _b;

        /// <summary>
        /// sigma = [D] * epsilon
        /// </summary>
        public mnl.Matrix<double> D => _d;

        /// <summary>
        /// ke = int [B]^T [D] [B] dV (stiffness matrix in local coordinates)
        /// </summary>
        public mnl.Matrix<double> KElementLocalCoord => _kElementLocalCoord;
        #endregion

        #region Constructor

        /// <summary>
        ///  
        /// </summary>
        /// <param name="nodes">Nodes of the element</param>
        /// <param name="id">id of element</param>
        public FiniteElement(Node[] nodes, ElementProperty property, int id) : base(id)
        {
            _nodes = nodes;
            _property = property;
            _attributes = new List<IPlateLoadCaseAttribute>();
            _DOF = new SortedSet<LinearSolver.DOF>();
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
        protected abstract mnl.Vector<double> BuildFLocalCoord();

        public mnl.Vector<double> GlobalCoordF()
        {
            mnl.Vector<double> _fLocalCoord = BuildFLocalCoord();
            mnl.Vector<double> F = DofGlobalToLocal.Transpose() * _fLocalCoord;
            
            return F;
        }

        public void AddAttribute(IPlateLoadCaseAttribute attribute)
        {
            _attributes.Add(attribute);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>An array of <see cref="FEMObject.Id"/>of the element Nodes</returns>
        public int[] GetNodesID()
        {
            return Nodes.Select(i => i.Id).ToArray();
        }

        public override bool Equals(object obj)
        {
            return obj is FiniteElement element &&
                   base.Equals(obj) &&
                   EqualityComparer<ElementProperty>.Default.Equals(_property, element._property) &&
                   EqualityComparer<Node[]>.Default.Equals(Nodes, element.Nodes);
        }

        public override int GetHashCode()
        {
            int hashCode = 1596002646;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementProperty>.Default.GetHashCode(_property);
            hashCode = hashCode * -1521134295 + EqualityComparer<Node[]>.Default.GetHashCode(Nodes);
            return hashCode;
        }
        #endregion
    }
}
