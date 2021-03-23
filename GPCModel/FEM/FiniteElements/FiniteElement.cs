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
        protected mnl.Matrix<double> _d;


        protected List<LoadCaseAttribute> _attributesLoadCase;

        protected ElementProperty _property;

        protected Node[] _nodesGlobal;

        #endregion

        #region Properties

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
        /// B : derivative of ShapeFunctions, need for epsilon = [B] * q with q = node displacements vector
        /// </summary>
        public abstract mnl.Matrix<double> GetB(double csi = 0, double eta = 0, double zeta = 0);

        /// <summary>
        /// F,M = [D] * (epsilon, curvature...)
        /// </summary>
        public mnl.Matrix<double> D => _d;

        /// <summary>
        /// ke = int [B]^T [D] [B] dV (stiffness matrix in local coordinates)
        /// </summary>
        public mnl.Matrix<double> KElementLocalCoord => _kElementLocalCoord;
        
        public List<LoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;

        #endregion

        #region Constructor

        /// <summary>
        ///  
        /// </summary>
        /// <param name="nodes">Nodes of the element</param>
        /// <param name="id">id of element</param>
        public FiniteElement(Node[] nodes, ElementProperty property, int id) : base(id)
        {
            _nodesGlobal = nodes;
            _property = property;
            _DOF = new SortedSet<LinearSolver.DOF>();
            _attributesLoadCase = new List<LoadCaseAttribute>();
        }

        #endregion

        #region PublicFunction

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

        /// <summary>
        /// Retrieve sigma, epsilon, N, M, etc in the element from displacement
        /// Top then bottom , then nr node. Example: stress[5] in element with 3 nodes with top and bottom: in equal to: 3 top, 2 bottom -> node 2 bottom
        /// </summary>
        /// <param name="displacementsNodes"></param>
        public abstract void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

        public abstract void GetResultPositionNaturalCoordinates(double csi, double eta, double zeta, double[] globalDisplacementsNodes, out double x, out double y, out double z, out double[] localDisplacements, out mnl.Matrix<double> gloabalPseudoDeformation, out mnl.Matrix<double> localPseudoDeformation, out mnl.Matrix<double> globalForces, out mnl.Matrix<double> localForces, out mnl.Matrix<double> globalStress, out mnl.Matrix<double> localStress, out mnl.Matrix<double> globalEpsilon, out mnl.Matrix<double> localEpsilon);

        /// <summary>
        /// Get displacements in local coordinates of the element
        /// </summary>
        /// <param name="displacementsNodes"></param>
        /// <returns></returns>
        public double[] GetLocalDisplacement(double[] globalDisplacementsNodes)
        {
            return (DofGlobalToLocal * mnl.Vector<double>.Build.Dense(globalDisplacementsNodes)).ToArray();
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
