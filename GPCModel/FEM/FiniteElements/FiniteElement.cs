using System.Collections.Generic;
using System.Linq;
using System;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.Results;
using mnl = MathNet.Numerics.LinearAlgebra;
using System.ComponentModel;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Each finite element should derive from this
    /// </summary>
    public abstract class FiniteElement : FEMObject, INotifyPropertyChanged
    {
        #region Variables

        //define the local axis of the element
        protected CoordinateSystem _localCoordinateSystem;
        //contains the degree of fredom active foreach node in global coordinates
        protected SortedSet<Solver.DOF> _DOF;
        
        //transformation matrix from local coordinates to global coordinates
        protected mnl.Matrix<double> _dofGlobalToLocal;
        //local stiffness matrix of the element in local coordinates
        protected mnl.Matrix<double> _kElementLocalCoord;
        
        protected UniqueNameCollection<LoadCaseAttribute> _attributesLoadCase;
        protected UniqueNameCollection<FreedomCaseAttribute> _attributesFreedomCase;

        //contains informations about section, thickness, material etc of the element
        protected ElementProperty _property;

        //contains the nodes in global coordinates
        protected Node[] _nodesGlobal;
        //contains the nodes in local coordinates
        protected Node[] _nodesLocal;

        protected readonly ModelObjectSet<FiniteElementResult> _results;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties
        /// <summary>
        /// Return the local axis of the element in global exis
        /// </summary>
        public CoordinateSystem LocalCoordinateSystem => _localCoordinateSystem;

        /// <summary>
        /// Contains the DOF active in the element
        /// </summary>
        public SortedSet<Solver.DOF> DOF => _DOF;

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
        /// Nodes of the element in local axis
        /// </summary>
        public Node[] LocalNodes => _nodesLocal;

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
        
        public UniqueNameCollection<LoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;
        public UniqueNameCollection<FreedomCaseAttribute> AttributesFreedomCase => _attributesFreedomCase;
        public IEnumerable<FiniteElementResult> Results => _results;

        #endregion

        #region Constructor

        /// <param name="nodes">Nodes of the element</param>
        internal FiniteElement(Node[] nodes) : base()
        {
            _nodesGlobal = nodes;
            _DOF = new SortedSet<Solver.DOF>();
            _attributesLoadCase = new UniqueNameCollection<LoadCaseAttribute>(new LoadCaseAttributeEqualityComparer());
            _attributesFreedomCase = new UniqueNameCollection<FreedomCaseAttribute>(new FreedomCaseAttributeEqualityComparer());

            _results = new ModelObjectSet<FiniteElementResult>(EqualityComparer<ElementResult>.Default); // comparer di ElementResult, usa solo il case come comparatore
        }

        #endregion

        #region PublicFunction

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal virtual void SetProperty(ElementProperty property)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            _property = property;
        }

        /// <summary>
        /// If an attribute, with the same loadcase name, already exist in the <see cref="_attributesLoadCase"/> it will be replaced with <paramref name="attribute"/>.
        /// </summary>
        protected void AddLoadCaseAttribute(LoadCaseAttribute attribute)
        {
            if (!_attributesLoadCase.Add(attribute))
            {
                _attributesLoadCase.Remove(attribute.LoadCaseName);
                _attributesLoadCase.Add(attribute);
            }
            else
            {
                _attributesLoadCase.Add(attribute);
            }            
        }

        /// <summary>
        /// If an attribute, with the same freedom name, already exist in the <see cref="_attributesFreedomCase"/> it will be replaced with <paramref name="attribute"/>.
        /// </summary>
        protected void AddFreedomCaseAttribute(FreedomCaseAttribute attribute)
        {
            if (!_attributesFreedomCase.Add(attribute))
            {
                _attributesFreedomCase.Remove(attribute.FreedomCaseName);
                _attributesFreedomCase.Add(attribute);
            }
            else
            {
                _attributesFreedomCase.Add(attribute);
            }
        }

        public LoadCaseAttribute GetLoadCaseAttribute(string loadCaseName)
        {
            return _attributesLoadCase.GetElementByName(loadCaseName);
        }

        public FreedomCaseAttribute GetFreedomCaseAttribute(string freedomCaseAttribute)
        {
            return _attributesFreedomCase.GetElementByName(freedomCaseAttribute);
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

        public virtual void AddResult(FiniteElementResult result)
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
