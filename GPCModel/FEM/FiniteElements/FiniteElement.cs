using System.Collections.Generic;
using System.Linq;
using System;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.Results;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Each finite element should derive from this
    /// </summary>
    public abstract class FiniteElement : FEMObject
    {
        #region Variables

        //define the local axis of the element
        protected CoordinateSystem _localCoordinateSystem;
        //contains the degree of fredom active foreach node in global coordinates
        protected SortedSet<LinearSolver.DOF> _DOF;
        
        //transformation matrix from local coordinates to global coordinates
        protected mnl.Matrix<double> _dofGlobalToLocal;
        //local stiffness matrix of the element in local coordinates
        protected mnl.Matrix<double> _kElementLocalCoord;
        
        protected List<LoadCaseAttribute> _attributesLoadCase;
        protected List<FreedomCaseAttribute> _attributesFreedomCase;

        //contains informations about section, thickness, material etc of the element
        protected ElementProperty _property;

        //contains the nodes in global coordinates
        protected Node[] _nodesGlobal;


        protected readonly ModelObjectSet<FiniteElementResult> _results;

        #endregion

        #region Properties
        /// <summary>
        /// Return the local axis of the element in global exis
        /// </summary>
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
        /// Nr of degree of freedom active for each node
        /// </summary>
        public int NrDOFActive
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < DOF.Count(); i++)
                {
                    if (DOF.Contains((Solver.DOF)i) == true) {
                        counter++;
                    }
                }
                return counter;
            }
        }

        /// <summary>
        /// Nodes of the element in global axis
        /// </summary>
        public Node[] Nodes => _nodesGlobal;

        /// <summary>
        /// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal] or for UlocalCoord = DofGlobalToLocal UglobalCoord; NOTE: DofLocalToGlobal = DofGlobalToLocal^TRASPOSTE
        /// </summary>
        public mnl.Matrix<double> DofGlobalToLocal => _dofGlobalToLocal;

        /// <summary>
        /// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal]
        /// </summary>
        public virtual mnl.Matrix<double> KElementGlobalCoord => DofGlobalToLocal.Transpose() * KElementLocalCoord * DofGlobalToLocal;

        /// <summary>
        /// ke = int [B]^T [D] [B] dV (stiffness matrix in local coordinates)
        /// </summary>
        public mnl.Matrix<double> KElementLocalCoord => _kElementLocalCoord;
        
        public List<LoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;
        public List<FreedomCaseAttribute> AttributesFreedomCase => _attributesFreedomCase;
        public IEnumerable<FiniteElementResult> Results => _results;

        #endregion

        #region Constructor

        /// <param name="nodes">Nodes of the element</param>
        internal FiniteElement(Node[] nodes) : base()
        {
            _nodesGlobal = nodes;
            _DOF = new SortedSet<Solver.DOF>();
            _attributesLoadCase = new List<LoadCaseAttribute>();
            _attributesFreedomCase = new List<FreedomCaseAttribute>();

            _results = new ModelObjectSet<FiniteElementResult>(EqualityComparer<ElementResult>.Default); // comparer di ElementResult, usa solo il case come comparatore
        }

        #endregion

        #region PublicFunction

        internal virtual void SetProperty(ElementProperty property)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            _property = property;
        }

        public abstract FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes);

        public abstract FiniteElement Duplicate();

        /// <summary>
        /// Build Stiffness Matrix etc
        /// </summary>
        public abstract void BuildMatrix();

        /// <summary>
        /// Build vector of Forces in nodes due to internal action applied (shear stress, prestress etc) : integral N^T vectorPression dS, N = shape function matrix
        /// </summary>
        protected abstract mnl.Vector<double> BuildFLocalCoord();

        public virtual mnl.Vector<double> GetGlobalCoordF()
        {
            mnl.Vector<double> _fLocalCoord = BuildFLocalCoord();
            mnl.Vector<double> F = DofGlobalToLocal.Transpose() * _fLocalCoord;
            
            return F;
        }

        /*/// <summary>
        /// Retrieve sigma, epsilon, N, M, etc in the element from displacement
        /// Top then bottom , then nr node. Example: stress[5] in element with 3 nodes with top and bottom: in equal to: 3 top, 2 bottom -> node 2 bottom
        /// </summary>*/
        //TODO: Da ottimizzare/scrivere
        public void GetNodesResults(double[] globalDisplacementsNodes, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            throw new Exception("ottimizzare questa funzione");
        }

        public void AddResult(FiniteElementResult result)
        {
            if (result != null)
                _results.Add(result);
        }

        #region GetInternalForces
        /// <summary>
        /// Return global internal force for the element
        /// </summary>
        /// <param name="globalDisplacementsNodes"></param>
        /// <returns>KglobalElement * displGlobal</returns>
        public mnl.Vector<double> GetInternalGlobalForces(double[] globalDisplacementsNodes)
        {
            return KElementGlobalCoord * mnl.Vector<double>.Build.Dense(globalDisplacementsNodes);
        }

        /// <summary>
        /// Return local internal force for the element
        /// </summary>
        /// <param name="localDisplacementsNodes"></param>
        /// <returns>KglobalElement * displGlobal</returns>
        public mnl.Vector<double> GetInternalNodalLocalForces(mnl.Vector<double> localDisplacementsNodes)
        {
            return _kElementLocalCoord * localDisplacementsNodes;
        }

        public mnl.Vector<double> GetInternalLocalForces(double[] localDisplacementsNodes)
        {
            return GetInternalNodalLocalForces(mnl.Vector<double>.Build.Dense(localDisplacementsNodes));
        }       
        #endregion

        #region GetLocalDisplacement
        /// <summary>
        /// Get displacements in local coordinates of the element
        /// </summary>
        /// <returns></returns>
        public mnl.Vector<double> GetLocalDisplacementVector(double[] globalDisplacementsNodes)
        {
            return (DofGlobalToLocal * mnl.Vector<double>.Build.Dense(globalDisplacementsNodes));
        }

        public double[] GetLocalDisplacement(double[] globalDisplacementsNodes)
        {
            return (GetLocalDisplacementVector(globalDisplacementsNodes)).ToArray();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns>An array of <see cref="ModelObjectId.Id"/>of the element Nodes</returns>
        public int[] GetNodesID()
        {
            return Nodes.Select(i => i.Id).ToArray();
        }

        #region EqualsAndHashCode
        public override bool Equals(object obj)
        {
            return obj is FiniteElement element &&
                   base.Equals(obj) &&
                   EqualityComparer<ElementProperty>.Default.Equals(_property, element._property) &&
                   Nodes.SequenceEqual(element.Nodes);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 1596002646;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<ElementProperty>.Default.GetHashCode(_property);

                foreach (var node in _nodesGlobal)
                {
                    hashCode = hashCode * 17 + EqualityComparer<Node>.Default.GetHashCode(node);
                }
                return hashCode; 
            }
        }
        #endregion
        #endregion
    }
}
