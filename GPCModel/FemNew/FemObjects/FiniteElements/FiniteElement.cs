using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Collections;
using GPC.Model.Fem.Properties;
using GPC.Model.Results;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Fem.FemObjects.FiniteElements
{
    /// <summary>
    /// Each finite element should derive from this
    /// </summary>
    [Serializable]
    public abstract class FiniteElement : FemObject, INotifyPropertyChanged, IFemObjectDuplicable<FiniteElement>
    {
        #region Variables

        // Local axis of the element
        protected CoordinateSystem _localCoordinateSystem;

        //contains the degree of fredom active foreach node in global coordinates
        //protected SortedSet<Solver.DOF> _DOF;

        ////transformation matrix from local coordinates to global coordinates
        //protected mnl.Matrix<double> _dofGlobalToLocal;
        
        ////local stiffness matrix of the element in local coordinates
        //protected mnl.Matrix<double> _kElementLocalCoord;

        protected AttributesCollection<LoadCaseAttribute> _attributesLoadCase;
        protected AttributesCollection<FreedomCaseAttribute> _attributesFreedomCase;

        protected ElementProperty _property;

        //contains the nodes in global coordinates
        protected Node[] _nodes;

        ////contains the nodes in local coordinates
        //protected Node[] _nodesLocal;

        //protected readonly ModelObjectSet<FiniteElementResult> _results;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Element local axis
        /// </summary>
        public CoordinateSystem LocalCoordinateSystem => _localCoordinateSystem;

        ///// <summary>
        ///// Contains the DOF active in the element
        ///// </summary>
        //public SortedSet<Solver.DOF> DOF => _DOF;

        public ElementProperty Property => _property;

        ///// <summary>
        ///// Nr of degree of freedom active for each node
        ///// </summary>
        //public int NrDOFActive
        //{
        //    get
        //    {
        //        int counter = 0;
        //        for (int i = 0; i < DOF.Count(); i++)
        //        {
        //            if (DOF.Contains((Solver.DOF)i) == true)
        //            {
        //                counter++;
        //            }
        //        }
        //        return counter;
        //    }
        //}

        /// <summary>
        /// Nodes of the element in global axis
        /// </summary>
        public Node[] Nodes => _nodes;

        ///// <summary>
        ///// Nodes of the element in local axis
        ///// </summary>
        //public Node[] LocalNodes => _nodesLocal;

        ///// <summary>
        ///// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal] or for UlocalCoord = DofGlobalToLocal UglobalCoord; NOTE: DofLocalToGlobal = DofGlobalToLocal^TRASPOSTE
        ///// </summary>
        //public mnl.Matrix<double> DofGlobalToLocal => _dofGlobalToLocal;

        ///// <summary>
        ///// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal]
        ///// </summary>
        //public virtual mnl.Matrix<double> KElementGlobalCoord => DofGlobalToLocal.Transpose() * KElementLocalCoord * DofGlobalToLocal;

        ///// <summary>
        ///// ke = int [B]^T [D] [B] dV (stiffness matrix in local coordinates)
        ///// </summary>
        //public mnl.Matrix<double> KElementLocalCoord => _kElementLocalCoord;

        public AttributesCollection<LoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;
        public AttributesCollection<FreedomCaseAttribute> AttributesFreedomCase => _attributesFreedomCase;
        //public IEnumerable<FiniteElementResult> Results => _results;

        #endregion

        #region Constructor

        /// <param name="nodes">Nodes of the element</param>
        internal FiniteElement(Node[] nodes) : base()
        {
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            
            if (nodes.Select(i => i == null).Count() > 0)
                throw new ArgumentNullException(nameof(nodes));


            //_DOF = new SortedSet<Solver.DOF>();
            _attributesLoadCase = new AttributesCollection<LoadCaseAttribute>();
            _attributesFreedomCase = new AttributesCollection<FreedomCaseAttribute>();

            //_results = new ModelObjectSet<FiniteElementResult>(EqualityComparer<ElementResult>.Default); // comparer di ElementResult, usa solo il case come comparatore
        }

        public FiniteElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //_results = (ModelObjectSet<FiniteElementResult>)info.GetValue("Result", typeof(ModelObjectSet<FiniteElementResult>));
            _nodes = (Node[])info.GetValue("NodesGlobal", typeof(Node[]));
            //_nodesLocal = (Node[])info.GetValue("NodesLocal", typeof(Node[]));
            _property = (ElementProperty)info.GetValue("Property", typeof(ElementProperty));
            _attributesLoadCase = (AttributesCollection<LoadCaseAttribute>)info.GetValue("AttributesLoadCase", typeof(AttributesCollection<LoadCaseAttribute>));
            _attributesFreedomCase = (AttributesCollection<FreedomCaseAttribute>)info.GetValue("AttributesFreedomCase", typeof(AttributesCollection<FreedomCaseAttribute>));
        }


        #endregion


        #region internal methods

        internal virtual void SetProperty(ElementProperty property)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            _property = property;
        }

        #endregion

        #region protected methods

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion



        #region Public methods


        public LoadCaseAttribute GetLoadCaseAttribute(string loadCaseName)
        {
            return (LoadCaseAttribute)_attributesLoadCase.GetElementByCaseName(loadCaseName);
        }


        public FreedomCaseAttribute GetFreedomCaseAttribute(string freedomCaseName)
        {
            return (FreedomCaseAttribute)_attributesFreedomCase.GetElementByCaseName(freedomCaseName);
        }

        #endregion

        #region Abstract methods

        public abstract FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes);

        public abstract FiniteElement Duplicate();

        /// <summary>
        /// Build Stiffness Matrix etc
        /// </summary>
        public abstract void BuildMatrix();

        #endregion


        ///// <summary>
        ///// Build vector of Forces in nodes due to internal action applied (shear stress, prestress etc) : integral N^T vectorPression dS, N = shape function matrix
        ///// </summary>
        //protected abstract mnl.Vector<double> BuildFLocalCoord();

        //public virtual mnl.Vector<double> GetGlobalCoordF()
        //{
        //    mnl.Vector<double> _fLocalCoord = BuildFLocalCoord();
        //    mnl.Vector<double> F = DofGlobalToLocal.Transpose() * _fLocalCoord;

        //    return F;
        //}

        //public virtual void AddResult(FiniteElementResult result)
        //{
        //    if (result != null)
        //        _results.Add(result);
        //}

        //#region GetInternalForces
        ///// <summary>
        ///// Return global internal force for the element
        ///// </summary>
        ///// <param name="globalDisplacementsNodes"></param>
        ///// <returns>KglobalElement * displGlobal</returns>
        //public mnl.Vector<double> GetInternalGlobalForces(double[] globalDisplacementsNodes)
        //{
        //    return KElementGlobalCoord * mnl.Vector<double>.Build.Dense(globalDisplacementsNodes);
        //}

        ///// <summary>
        ///// Return local internal force for the element
        ///// </summary>
        ///// <param name="localDisplacementsNodes"></param>
        ///// <returns>KglobalElement * displGlobal</returns>
        //public mnl.Vector<double> GetInternalNodalLocalForces(mnl.Vector<double> localDisplacementsNodes)
        //{
        //    return _kElementLocalCoord * localDisplacementsNodes;
        //}

        //public mnl.Vector<double> GetInternalLocalForces(double[] localDisplacementsNodes)
        //{
        //    return GetInternalNodalLocalForces(mnl.Vector<double>.Build.Dense(localDisplacementsNodes));
        //}
        //#endregion

        //#region GetLocalDisplacement
        ///// <summary>
        ///// Get displacements in local coordinates of the element
        ///// </summary>
        ///// <returns></returns>
        //public mnl.Vector<double> GetLocalDisplacementVector(double[] globalDisplacementsNodes)
        //{
        //    return (DofGlobalToLocal * mnl.Vector<double>.Build.Dense(globalDisplacementsNodes));
        //}

        //public double[] GetLocalDisplacement(double[] globalDisplacementsNodes)
        //{
        //    return (GetLocalDisplacementVector(globalDisplacementsNodes)).ToArray();
        //}
        //#endregion

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
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + EqualityComparer<ElementProperty>.Default.GetHashCode(_property);

                foreach (var node in _nodes)
                {
                    hashCode = hashCode * 17 + EqualityComparer<Node>.Default.GetHashCode(node);
                }
                return hashCode;
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            //info.AddValue("Result", _results, typeof(ModelObjectSet<FiniteElementResult>));
            info.AddValue("NodesGlobal", _nodes, typeof(Node[]));
            //info.AddValue("NodesLocal", _nodesLocal, typeof(Node[]));
            info.AddValue("Property", _property, typeof(ElementProperty));

            info.AddValue("AttributesLoadCase", _attributesLoadCase, typeof(AttributesCollection<LoadCaseAttribute>));
            info.AddValue("AttributesFreedomCase", _attributesFreedomCase, typeof(AttributesCollection<FreedomCaseAttribute>));

        }


        #endregion
        
        
    }
}
