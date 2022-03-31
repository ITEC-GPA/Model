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


        protected AttributesCollection<LoadCaseAttribute> _attributesLoadCase;
        protected AttributesCollection<FreedomCaseAttribute> _attributesFreedomCase;

        protected ElementProperty _property;

        //contains the nodes in global coordinates
        protected Node[] _nodes;

        protected NodalDegreeOfFreedom[] _nodalDegreeOfFreedoms;


        //protected readonly ModelObjectSet<FiniteElementResult> _results;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Element local axis
        /// </summary>
        public CoordinateSystem LocalCoordinateSystem => _localCoordinateSystem;


        public ElementProperty Property => _property;


        /// <summary>
        /// Nodes of the element in global axis
        /// </summary>
        public Node[] Nodes => _nodes;

        

        public AttributesCollection<LoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;
        public AttributesCollection<FreedomCaseAttribute> AttributesFreedomCase => _attributesFreedomCase;

        //public IEnumerable<FiniteElementResult> Results => _results;

        #endregion

        #region Constructor

        /// <param name="nodes">Nodes of the element</param>
        internal FiniteElement(Node[] nodes) 
        {
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));            
            if (nodes.Where(i => i == null).Count() > 0)
                throw new ArgumentNullException(nameof(nodes));


            _nodalDegreeOfFreedoms = GetNodalDegreeOfFreedom();

            if (_nodalDegreeOfFreedoms == null)
                throw new ArgumentNullException(nameof(_nodalDegreeOfFreedoms));

            if (_nodalDegreeOfFreedoms.Where(i => i == null).Count() > 0)
                throw new ArgumentNullException(nameof(_nodalDegreeOfFreedoms));
            

            _attributesLoadCase = new AttributesCollection<LoadCaseAttribute>();
            _attributesFreedomCase = new AttributesCollection<FreedomCaseAttribute>();

            //_results = new ModelObjectSet<FiniteElementResult>(EqualityComparer<ElementResult>.Default); // comparer di ElementResult, usa solo il case come comparatore
        }

        protected FiniteElement(SerializationInfo info, StreamingContext context)
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

        /// <remarks>Only the <see cref="FemModel"/> class can set the property of the elements. For this reason the visibility is internal</remarks>
        /// <param name="property"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal virtual void SetProperty<T>(T property) where T : ElementProperty
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

        internal abstract NodalDegreeOfFreedom[] GetNodalDegreeOfFreedom();

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
