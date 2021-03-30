using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Combinations;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Collections;
using GPC.Utilities.Extensions;

namespace GPC.Model.FEM
{
    [Serializable]
    public sealed class Stage : ModelObject, ISerializable, IEquatable<Stage>, ICloneable
    {

        private List<Combination> _combinations;

        private FemModel.AnalysisType _analysisType;

        private FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty> _elements;

        private NodeStageCollection<Node, StageProperty> _nodes;

        private FemModel _femModel;

        private bool _morph;


        #region Properties

        public List<Combination> Combinations => _combinations;

        public FemModel.AnalysisType AnalysisType => _analysisType;

        public bool Morph => _morph;


        #endregion


        internal Stage(string name, FemModel referenceFemModel, FemModel.AnalysisType analysisType, bool morph, List<Combination> combinations)
            : base(name)
        {
            this._analysisType = analysisType;
            this._combinations = combinations ?? new List<Combination>();
            this._morph = morph;

            this._elements = new FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty>();
            this._nodes = new NodeStageCollection<Node, StageProperty>();

            this._femModel = referenceFemModel ?? throw new  ArgumentNullException("Fem Model can't be null");
        }

        internal Stage(string name, FemModel femModel, FemModel.AnalysisType analysisType)
            : this(name, femModel, analysisType, false, null)
        {

        }

        internal Stage(Stage stage)
        {
            this._analysisType = stage._analysisType;
            this._combinations = stage._combinations;
            this._morph        = stage._morph;
            this._elements     = stage._elements;
            this._nodes        = stage._nodes;
            this._femModel     = stage._femModel;
        }

        internal Stage(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add the <see cref="FiniteElement"/> to the stage by means of their IDs from the reference FemModel: <see cref="Stage._femModel"/>
        /// </summary>
        /// <exception cref="KeyNotFoundException">If the <paramref name="elementIDs"/> are not in the reference model <see cref="FemModel._nodes"/> collection </exception>
        public void AddFiniteElements(int[] elementIDs)
        {
            foreach(var id in elementIDs)
            {
                foreach (var node in _femModel.GetFiniteElement(id).Nodes)
                {
                    _nodes.Add(node);
                }

                _elements.Add(_femModel.GetFiniteElement(id));
            }
        }


        /// <summary>
        /// Add the <see cref="FiniteElement"/> to the stage
        /// </summary>
        /// <exception cref="ArgumentException">If the <paramref name="elements"/> are not in the reference model <see cref="FemModel._elements"/> collection </exception>
        public void AddFiniteElements(FiniteElement[] elements)
        {
            foreach (var element in elements)
            {
                foreach (var node in element.Nodes)
                    _nodes.Add(node);


                if (!_femModel.ContainsFiniteElement(element))
                    throw new ArgumentException();


                _elements.Add(element);
            }
        }

        public void AddCombination(Combination combination)
        {
            if (combination != null)
                _combinations.Add(combination);
        }

        public void AddCombinations(List<Combination> combinations)
        {
            _combinations.AddRange(combinations);
        }

        public void SetAnalysisType(FemModel.AnalysisType analysisType)
        {
            _analysisType = analysisType;
        }

        public void SetMorph(bool active)
        {
            _morph = active;
        }

        /// <summary>
        /// Add each node of the collection to the stage
        /// </summary>
        /// <param name="nodes"></param>
        public void AddNodes(FemObjectCollection<Node> nodes)
        {
            
            foreach (var node in nodes)
            {
                StageProperty sp = new StageProperty();
                sp.AddLoadCaseAttributes(node.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
                sp.AddFreedomCaseAttributes(node.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

                _nodes.Add(node, sp);
            }
        }

        /// <summary>
        /// Add each node of the collection to the stage
        /// </summary>
        /// <param name="nodes"></param>
        public void AddNodes(NodeStageCollection<Node, StageProperty> nodes)
        {
            foreach (var node in nodes)
            {
                _nodes.Add(node);
            }
        }

        /// <summary>
        /// Set the <paramref name="nodes"/> collection as the <see cref="Stage._nodes"/>
        /// </summary>
        /// <param name="nodes"></param>
        public void SetNodes(NodeStageCollection<Node, StageProperty> nodes)
        {
            if (nodes != null)
                this._nodes = nodes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="stageFiniteElementProperty"></param>
        /// <exception cref="KeyNotFoundException">IF the <paramref name="elementId"/> is not contained in the reference femModel <see cref="FemModel._elements"/> collection </exception>
        public void SetFiniteElementProperty(int elementId, StageFiniteElementProperty stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty != null)
                _elements.SetStageProperty(_elements[elementId], stageFiniteElementProperty);
        }

        /// <summary>
        /// Add a <see cref="FiniteElement"/> to the stage element collection. All its attributes will be copied
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public StageFiniteElementProperty AddFiniteElement(FiniteElement element)
        {
            if (_femModel.ContainsFiniteElement(element))
            {
                StageFiniteElementProperty sp = new StageFiniteElementProperty(element.Property);

                sp.AddLoadCaseAttributes(element.AttributesLoadCase);
                sp.AddFreedomCaseAttributes(element.AttributesFreedomCase);

                _elements.Add(element, sp);

                return sp;
            }
            throw new ArgumentException("Element not contained in the reference femModel");
        }

        /// <summary>
        /// Add a <see cref="FiniteElement"/> to the stage element collection. All its attributes will be copied
        /// </summary>
        /// <param name="element"></param>
        /// <param name="property">Overriding property</param>
        /// <returns></returns>
        public StageFiniteElementProperty AddFiniteElement(FiniteElement element, ElementProperty property)
        {
            if (_femModel.ContainsFiniteElement(element))
            {
                StageFiniteElementProperty sp = new StageFiniteElementProperty(property);

                sp.AddLoadCaseAttributes(element.AttributesLoadCase);
                sp.AddFreedomCaseAttributes(element.AttributesFreedomCase);

                _elements.Add(element, sp);

                return sp;
            }
            throw new ArgumentException("Element not contained in the reference femModel");
        }


        /// <inheritdoc cref="AddFiniteElement(FiniteElement)"/>
        public void AddFiniteElements(FemObjectCollection<FiniteElement> elements)
        {

            foreach (var element in elements)
            {
                this.AddFiniteElement(element);
            }

        }

        /// <summary>
        /// Set the <paramref name="finiteElements"/> collection as the <see cref="Stage._elements"/>
        /// </summary>
        /// <param name="nodes"></param>
        public void SetFiniteElements(FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty> finiteElements)
        {
            if (finiteElements != null)
                this._elements = finiteElements;
        }

        /// <summary>
        /// Return a model only with the elements active on this stage and the ovverided property and attributes
        /// </summary>
        /// <returns></returns>
        public FemModel ToModel()
        {
            FemModel femModel = new FemModel(_name);

            var enumerator = _elements.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var finiteElement = enumerator.Current;
                FiniteElement duplicated = finiteElement.Duplicate(_elements.GetStageProperty(finiteElement).Property, 
                                                                    _elements.GetStageProperty(finiteElement).LoadCaseAttributes,
                                                                    _elements.GetStageProperty(finiteElement).FreedomCaseAttribute);

                femModel.AddProperty(finiteElement.Property);
                femModel.AddFiniteElement(duplicated, finiteElement.Property.Name);
            }

            return femModel;
        }


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

        #endregion

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


            public StageProperty Merge(StageProperty stagePropertyToMerge)
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

            public object Clone()
            {
                return new StageProperty(this);
            }
        }


        public class StageFiniteElementProperty : StageProperty
        {
            private ElementProperty _property;

            public ElementProperty Property => _property;

            public StageFiniteElementProperty(ElementProperty property)
            {
                _property = property;
            }
        }

    }
}
