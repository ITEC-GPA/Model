using GPC.Model.Combinations;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Collections;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Properties;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.FEM
{
    [Serializable]
    public sealed class Stage : ModelObject, ISerializable, IEquatable<Stage>, ICloneable
    {
        private static int _maxId = 0;
        private int _id;

        private UniqueNameCollection<Combination> _combinations;

        private FemModel.AnalysisTypes _analysisType;

        /// <summary>
        /// List of elements active in this stage. Each element is mapped to a property override.
        /// </summary>
        /// <remarks>
        /// <para>Each of this elements must be contained in the reference model: <see cref="_femModel"/></para>
        /// <para>If an element is not in this list, it will be not active in this stage</para>
        /// <para>The id of the elements in this collection will be the same of the ones in the reference femModel</para>
        /// </remarks>
        private FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty> _elements;

        /// <summary>
        /// List of node property override. 
        /// </summary>
        /// <remarks>
        /// <para>This map contains only the nodes with a property override. Not all the nodes of the <see cref="Stage._elements"/></para> 
        /// <para>Each node in this list must be contained in the reference model: <see cref="_femModel"/></para>
        /// </remarks>
        private NodeStageCollection<Node, StageProperty> _nodes;

        private FemModel _femModel;
        
        private bool _morph;


        #region Properties

        public FemModel.AnalysisTypes AnalysisType => _analysisType;

        public bool Morph => _morph;

        public int Id { get => _id; internal set => _id = value; }

        public IEnumerable<Combination> Combinations => _combinations;

        #endregion 

        internal Stage(string name, FemModel referenceFemModel, FemModel.AnalysisTypes analysisType, bool morph, UniqueNameCollection<Combination> combinations)
            : base(name)
        {
            // Il costruttore è internal in modo che sia solamente la classe fem model a poter creare l'istanza di stage.
            // 
            this._analysisType = analysisType;
            this._combinations = combinations ?? new UniqueNameCollection<Combination>();
            this._morph = morph;

            this._elements = new FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty>();
            this._nodes = new NodeStageCollection<Node, StageProperty>();

            this._femModel = referenceFemModel ?? throw new ArgumentNullException("Fem Model can't be null");
            this._id = _maxId++;
        }

        internal Stage(string name, FemModel femModel, FemModel.AnalysisTypes analysisType)
            : this(name, femModel, analysisType, false, null)
        {

        }

        internal Stage(Stage stage)
        {
            this._analysisType = stage._analysisType;
            this._combinations = stage._combinations;
            this._morph = stage._morph;
            this._elements = stage._elements;
            this._nodes = stage._nodes;
            this._femModel = stage._femModel;
            this._id = _maxId++;
        }

        internal Stage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #region Adder / Setter

        #region Finite elements

        /// <summary>
        /// Add a <see cref="FiniteElement"/> to the stage element collection. All its attributes will be copied
        /// </summary>
        /// <returns>The <see cref="StageFiniteElementProperty"/> assigned to <paramref name="element"/> </returns>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.Add(T, D)"/>
        /// <exception cref="ArgumentException">If <paramref name="element"/> is not contained in the reference FemModel <see cref="_femModel"/></exception>
        /// <exception cref="ArgumentException">If <see cref="FiniteElement.Property"/> Name is not contained in the reference femModel properties list</exception>
        public StageFiniteElementProperty AddFiniteElement(FiniteElement element)
        {
            if (CanBeAdded(element, out Exception exception))
            {
                StageFiniteElementProperty sp = new StageFiniteElementProperty(element.Property.Name);

                sp.AddLoadCaseAttributes(element.AttributesLoadCase);
                sp.AddFreedomCaseAttributes(element.AttributesFreedomCase);

                _elements.Add(element, sp);

                return sp;
            }
            else
            {
                throw exception;
            }
        }

        /// <summary>
        /// Add a <see cref="FiniteElement"/> to the stage element collection. All its attributes will be copied
        /// </summary>
        /// <param name="element"></param>
        /// <param name="propertyName">Name of the overriding property</param>
        /// <returns>The <see cref="StageFiniteElementProperty"/> assigned to <paramref name="element"/> </returns>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.Add(T, D)"/>
        /// <exception cref="ArgumentException">If the <paramref name="element"/> does not exist the in the reference fem model</exception>
        /// <exception cref="ArgumentException">If the <paramref name="propertyName"/> does not exist in the reference fem model</exception>
        public StageFiniteElementProperty AddFiniteElement(FiniteElement element, string propertyName)
        {
            if (CanBeAdded(element, out Exception exception, propertyName))
            {
                StageFiniteElementProperty sp = new StageFiniteElementProperty(propertyName);

                sp.AddLoadCaseAttributes(element.AttributesLoadCase);
                sp.AddFreedomCaseAttributes(element.AttributesFreedomCase);

                _elements.Add(element, sp);

                return sp;
            }
            else
            {
                throw exception;
            }
        }


        /// <summary>
        /// Add the <see cref="FiniteElement"/> to the stage by means of their IDs from the reference FemModel: <see cref="Stage._femModel"/>
        /// </summary>
        /// <exception cref="KeyNotFoundException">If the <paramref name="elementIDs"/> are not in the reference model <see cref="FemModel._nodes"/> collection </exception>
        /// <remarks>This is a O(n) operations</remarks>
        public void AddFiniteElements(int[] elementIDs)
        {
            foreach (var id in elementIDs.Distinct())
            {
                _elements.Add(_femModel.GetFiniteElement(id));
            }
        }

        /// <summary>
        /// Add the <see cref="FiniteElement"/> to the stage by means of their IDs from the reference FemModel: <see cref="Stage._femModel"/>
        /// </summary>
        /// <param name="elementIDs"></param>
        /// <param name="propertyName">Name of the overriding property</param>
        /// <exception cref="KeyNotFoundException">If the <paramref name="elementIDs"/> are not in the reference model <see cref="FemModel._nodes"/> collection </exception>
        /// <remarks>This is a O(n^2) operations</remarks>
        public void AddFiniteElements(int[] elementIDs, string propertyName)
        {
            foreach (var id in elementIDs.Distinct())
            {
                var element = _femModel.GetFiniteElement(id);

                StageFiniteElementProperty sp = new StageFiniteElementProperty(propertyName);

                sp.AddLoadCaseAttributes(element.AttributesLoadCase);
                sp.AddFreedomCaseAttributes(element.AttributesFreedomCase);

                _elements.Add(element);
            }
        }



        /// <summary>
        /// Add the <see cref="FiniteElement"/> to the stage
        /// </summary>
        /// <exception cref="ArgumentException">If the <paramref name="elements"/> are not in the reference model <see cref="FemModel._elements"/> collection </exception>
        /// <remarks>This is a O(3n) operations</remarks>
        public void AddFiniteElements(FiniteElement[] elements)
        {
            foreach (var element in elements)
            {
                if (CanBeAdded(element, out Exception exception))
                {
                    _elements.Add(element, new StageFiniteElementProperty(element));
                }
                else
                {
                    throw exception;
                }
            }
        }

        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        /// <exception cref="ArgumentException">If <see cref="FiniteElement"/> in <paramref name="elements"/> is not contained in the reference femModel</exception>
        public void SetFiniteElements(FemObjectCollection<FiniteElement> elements)
        {
            foreach (var element in elements)
            {
                if (CanBeAdded(element, out Exception exception))
                {
                    _elements.Add(element);
                }
                else
                    throw exception;
            }
        }


        /// <summary>Set the <paramref name="stageFiniteElementProperty"/> of the element with id: <paramref name="elementId"/>
        /// <para>The element must be contained the <see cref="Stage._elements"/> collection</para></summary>
        /// <exception cref="KeyNotFoundException">If the <paramref name="elementId"/> is not contained in the reference femModel <see cref="FemModel._elements"/> collection </exception>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetStageProperty(T, D)"/>
        public bool SetFiniteElementProperty(int elementId, StageFiniteElementProperty stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty != null)
                return _elements.SetStageProperty(_elements[elementId], stageFiniteElementProperty);
            else
                throw new ArgumentNullException();
        }


        /// <summary>Set the <paramref name="stageFiniteElementProperty"/> of the elements with id: <paramref name="elementsId"/>
        /// <para>The element must be contained the <see cref="Stage._elements"/> collection</para></summary>
        /// <inheritdoc cref="FiniteElementStageCollection{T, D}.SetStageProperty(FiniteElement, StageFiniteElementProperty)"/>
        public bool SetFiniteElementsProperty(int[] elementsId, StageFiniteElementProperty stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty != null && elementsId != null)
            {
                foreach (int id in elementsId.Distinct())
                {
                    if (!_elements.SetStageProperty(_elements[id], stageFiniteElementProperty))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
                throw new ArgumentNullException();
        }

        #endregion Public method - Add / Set

        #region Nodes

        public void SetNodesProperty(int nodeId, StageProperty stageProperty)
        {
            var node = _femModel.GetNode(nodeId);

            _nodes.Add(node, stageProperty);

        }

        ///// <summary>
        ///// Add each node of the collection to the stage
        ///// </summary>
        ///// <returns></returns>
        ///// <inheritdoc cref="FemObjectStageCollection{T, D}.SetItem(T, D)"/>
        //internal void SetNodes(FemObjectCollection<Node> nodes)
        //{
        //    foreach (var node in nodes)
        //    {
        //        // TODO: controllare che nodo sia nel ref model

        //        StageProperty sp = new StageProperty();
        //        sp.AddLoadCaseAttributes(node.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
        //        sp.AddFreedomCaseAttributes(node.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

        //        _nodes.SetItem(node, sp);
        //    }
        //}

        ///// <summary>
        ///// Set the <paramref name="nodes"/> collection as the <see cref="Stage._nodes"/>
        ///// </summary>
        ///// <param name="nodes"></param>
        //internal void SetNodes(NodeStageCollection<Node, StageProperty> nodes)
        //{
        //    if (nodes != null)
        //        this._nodes = nodes;
        //}


        #endregion

        #region Combination

        public void AddCombination(Combination combination)
        {
            if (combination != null)
                _combinations.Add(combination);
        }

        public void AddCombinations(List<Combination> combinations)
        {
            _combinations.AddRange(combinations);
        }

        internal IEnumerable<Combination> GetCombinations()
        {
            return _combinations.ToList();
        }
        
        #endregion

        #region StageProperties

        public void SetAnalysisType(FemModel.AnalysisTypes analysisType)
        {
            _analysisType = analysisType;
        }

        public void SetMorph(bool active)
        {
            _morph = active;
        }

        #endregion

        #endregion

        #region Getter

        /// <summary>
        /// Return a model only with the elements active on this stage and the ovverided property and attributes
        /// </summary>
        /// <returns></returns>
        public FemModel ToModel()
        {
            FemModel femModel = new FemModel(_name);

            // TODO: al momento non scrive l'override delle proprietà dei nodi
            var enumerator = _elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var finiteElement = enumerator.Current.Key;
                var elementStageProperty = enumerator.Current.Value;

                ElementProperty property;
                if (finiteElement is Plate)
                    property = _femModel.GetPlateProperty(elementStageProperty.PropertyName);
                else if (finiteElement is Brick)
                    property = _femModel.GetBrickProperty(elementStageProperty.PropertyName);
                else
                    throw new NotImplementedException();


                FiniteElement duplicated = finiteElement.Duplicate(property,
                                                                   elementStageProperty.LoadCaseAttributes,
                                                                   elementStageProperty.FreedomCaseAttribute);

                femModel.AddProperty(duplicated.Property);
                femModel.AddFiniteElement(duplicated, duplicated.Property.Name); // Aggiunge l'elemento finito e i suoi nodi al modello.
            }

            return femModel;
        }

        #endregion

        #region Edit

        public void ClearCombinations()
        {
            _combinations.Clear();
        }

        public void ClearElements()
        {
            _elements.Clear();
        }

        public void ClearNodes()
        {
            _nodes.Clear();
        }


        #endregion

        #region Interrogate

        /// <summary>
        /// This method checks if a <paramref name="element"/> can be added to the <see cref="_elements"/> collection
        /// </summary>
        /// <param name="element"></param>
        /// <param name="exception"></param>
        /// <param name="propertyNameOverride"></param>
        /// <returns><see langword="true"/> if the element can be added</returns>
        /// <remarks>This is a O(n) operation</remarks>
        private bool CanBeAdded(FiniteElement element, out Exception exception, string propertyNameOverride = "")
        {
            if (_femModel.ContainsFiniteElement(element))
            {
                string property = String.IsNullOrEmpty(propertyNameOverride) || String.IsNullOrWhiteSpace(propertyNameOverride) ? element.Property.Name : propertyNameOverride;
                if (_femModel.ContainsProperty(property))
                {
                    exception = null;
                    return true;
                }
                else
                {
                    exception = new ArgumentException($"Property with name {property} not available in the reference femModel");
                    return false;
                }
            }
            exception = new ArgumentException($"Element not contained in the reference femModel");
            return false;
        }

        #endregion

        #region Interface, operators, hashcode

        public object Clone()
        {
            return new Stage(this);
        }

        /// <summary>
        /// <see cref="Stage._femModel"/> is not used as comparative factor
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public bool Equals(Stage sc)
        {
            if (sc is null)
                return false;

            if (ReferenceEquals(this, sc))
                return true;

            return !(sc is null) && _combinations.ScrambledEquals(sc._combinations)
                                 && _elements.Equals(sc._elements)
                                 && _nodes.Equals(sc._nodes)
                                 && _analysisType.Equals(sc._analysisType)
                                 && _morph.Equals(sc._morph)
                                 && base.Equals(sc);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Stage);
        }

        /// <summary>
        /// <see cref="Stage._femModel"/> is not used to calculate the hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();

            foreach (var combo in _combinations)
            {
                hashCode = hashCode + EqualityComparer<Combination>.Default.GetHashCode(combo);
            }
            hashCode = hashCode + _elements.GetHashCode();
            hashCode = hashCode + _nodes.GetHashCode();
            hashCode = hashCode + _morph.GetHashCode();
            hashCode = hashCode + _analysisType.GetHashCode();

            return hashCode;
        }

        public static bool operator ==(Stage obj1, Stage obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Stage obj1, Stage obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Interface, operators, hashcode

        #region Nested class

        public class StageProperty : ICloneable
        {
            private List<LoadCaseAttribute> _loadCaseAttributes;
            private List<FreedomCaseAttribute> _freedomCaseAttributes;

            public List<LoadCaseAttribute> LoadCaseAttributes => _loadCaseAttributes;
            public List<FreedomCaseAttribute> FreedomCaseAttribute => _freedomCaseAttributes;

            public StageProperty()
            {
                _loadCaseAttributes = new List<LoadCaseAttribute>();
                _freedomCaseAttributes = new List<FreedomCaseAttribute>();
            }

            public StageProperty(StageProperty stageProperty)
            {
                _loadCaseAttributes = new List<LoadCaseAttribute>();
                _freedomCaseAttributes = new List<FreedomCaseAttribute>();

                foreach (var lc in stageProperty._loadCaseAttributes)
                {
                    _loadCaseAttributes.Add((LoadCaseAttribute)lc.Clone());
                }

                foreach (var fc in stageProperty._freedomCaseAttributes)
                {
                    _freedomCaseAttributes.Add((FreedomCaseAttribute)fc.Clone());
                }
            }

            public virtual StageProperty Merge(StageProperty stagePropertyToMerge)
            {
                StageProperty merged = new StageProperty(this);

                for (int i = 0; i < stagePropertyToMerge.LoadCaseAttributes.Count; i++)
                {
                    if (!merged._loadCaseAttributes.Contains(stagePropertyToMerge.LoadCaseAttributes[i]))
                    {
                        merged.AddLoadCaseAttribute(stagePropertyToMerge.LoadCaseAttributes[i]);
                    }
                    else
                    {
                        // TODO: fare merge del singolo attributo
                    }
                }

                for (int i = 0; i < stagePropertyToMerge.FreedomCaseAttribute.Count; i++)
                {
                    if (!merged._freedomCaseAttributes.Contains(stagePropertyToMerge.FreedomCaseAttribute[i]))
                    {
                        merged.AddFreedomCaseAttributes(stagePropertyToMerge.FreedomCaseAttribute[i]);
                    }
                    else
                    {
                        // TODO: fare merge del singolo attributo
                    }
                }

                return merged;
            }

            public void AddLoadCaseAttribute(LoadCaseAttribute attribute)
            {
                if (attribute != null)
                    _loadCaseAttributes.Add(attribute);
            }

            public void AddLoadCaseAttributes(List<LoadCaseAttribute> attributes)
            {
                if (attributes != null)
                    _loadCaseAttributes.AddRange(attributes);
            }

            public void AddFreedomCaseAttributes(FreedomCaseAttribute attribute)
            {
                if (attribute != null)
                    _freedomCaseAttributes.Add(attribute);
            }

            public void AddFreedomCaseAttributes(List<FreedomCaseAttribute> attributes)
            {
                if (attributes != null)
                    _freedomCaseAttributes.AddRange(attributes);
            }

            public virtual object Clone()
            {
                return new StageProperty(this);
            }
        }

        public class StageFiniteElementProperty : StageProperty
        {
            private string _propertyName;

            public string PropertyName => _propertyName;

            public StageFiniteElementProperty(string propertyName)
            {
                _propertyName = propertyName;
            }

            public StageFiniteElementProperty(StageFiniteElementProperty stageFiniteElementProperty)
                : base(stageFiniteElementProperty)
            {
                _propertyName = stageFiniteElementProperty._propertyName;
            }

            public StageFiniteElementProperty(FiniteElement element)
            {
                this._propertyName = element.Property.Name;

                this.AddLoadCaseAttributes(element.AttributesLoadCase);
                this.AddFreedomCaseAttributes(element.AttributesFreedomCase);
            }


            public override object Clone()
            {
                return new StageFiniteElementProperty(this);
            }

            public override StageProperty Merge(StageProperty stagePropertyToMerge)
            {
                StageProperty merged = new StageFiniteElementProperty(this);

                for (int i = 0; i < stagePropertyToMerge.LoadCaseAttributes.Count; i++)
                {
                    if (!merged.LoadCaseAttributes.Contains(stagePropertyToMerge.LoadCaseAttributes[i]))
                    {
                        merged.AddLoadCaseAttribute(stagePropertyToMerge.LoadCaseAttributes[i]);
                    }
                    else
                    {
                        // TODO: fare merge del singolo attributo
                    }
                }

                for (int i = 0; i < stagePropertyToMerge.FreedomCaseAttribute.Count; i++)
                {
                    if (!merged.FreedomCaseAttribute.Contains(stagePropertyToMerge.FreedomCaseAttribute[i]))
                    {
                        merged.AddFreedomCaseAttributes(stagePropertyToMerge.FreedomCaseAttribute[i]);
                    }
                    else
                    {
                        // TODO: fare merge del singolo attributo
                    }
                }

                return merged;
            }
        } 
        
        #endregion
    }
}
