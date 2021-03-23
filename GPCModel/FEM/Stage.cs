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


        private bool _morph;

        private FemModel _femModel;


        public List<Combination> Combinations => _combinations;

        public FemModel.AnalysisType AnalysisType => _analysisType;

        public bool Morph => _morph;

        public NodeStageCollection<Node, StageProperty> Nodes => _nodes;

        public FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty> Elements => _elements;


        public Stage(string name, FemModel.AnalysisType analysisType, bool morph, List<Combination> combinations, FemModel femModel)
            : base(name)
        {
            this._analysisType = analysisType;
            this._combinations = combinations ?? new List<Combination>();
            this._morph = morph;

            this._elements = new FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty>();
            this._nodes = new NodeStageCollection<Node, StageProperty>();

            this._femModel = femModel;
        }

        //public Stage(string name, FemModel.AnalysisType analysisType) 
        //    : this(name, analysisType, false, null)
        //{

        //}

        public Stage(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public void AddFiniteElements(int[] idArray)
        {
            foreach(var id in idArray)
            {
                foreach (var node in _femModel.Elements[id].Nodes)
                {
                    _nodes.Add(node);
                }

                _elements.Add(_femModel.Elements[id]);
            }
        }

        public void AddFiniteElements(FiniteElement[] elements)
        {
            foreach (var element in elements)
            {
                foreach (var node in element.Nodes)
                {
                    _nodes.Add(node);
                }

                if (!_femModel.Elements.Contains(element))
                {
                    throw new ArgumentException();
                }
                _elements.Add(element);
            }
        }

        public void AddCombination(Combination combination)
        {
            _combinations.Add(combination);
        }

        public void SetAnalysisType(FemModel.AnalysisType analysisType)
        {
            _analysisType = analysisType;
        }

        public void SetMorph(bool active)
        {
            _morph = active;
        }

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

        public void AddNodes(NodeStageCollection<Node, StageProperty> nodes)
        {
            this._nodes = nodes;
        }


        public void SetFiniteElementProperty(int elementId, StageFiniteElementProperty sp)
        {
            var element = _elements[elementId];
            _elements.SetStageProperty(element, sp);
        }

        public StageFiniteElementProperty AddFiniteElement(FiniteElement element)
        {
            StageFiniteElementProperty sp = new StageFiniteElementProperty(element.Property);

            sp.AddLoadCaseAttributes(element.AttributesLoadCase);

            _elements.Add(element, sp);

            return sp;
        }

        public void AddFiniteElements(FemObjectCollection<FiniteElement> elements)
        {

            foreach (var element in elements)
            {
                StageFiniteElementProperty sp = new StageFiniteElementProperty(element.Property);
                
                sp.AddLoadCaseAttributes(element.AttributesLoadCase);

                _elements.Add(element, sp);
            }

        }

        public void AddFiniteElements(FiniteElementStageCollection<FiniteElement, StageFiniteElementProperty> finiteElements)
        {
            this._elements = finiteElements;
        }


        public FemModel ToModel()
        {
            FemModel femModel = new FemModel();

            for (int i = 0; i < _elements.Count; i++)
            {
                FiniteElement duplicated = _elements[i].Duplicate(_elements.GetStageProperty(i).Property, _elements.GetStageProperty(i).LoadCaseAttributes);

                femModel.Elements.Add(duplicated);

            }

            return femModel;
        }


        #region Interface, operators, hashcode
        public object Clone()
        {
            var s = new Stage(_name, _analysisType, _morph, _combinations);
            s._nodes = nodes;
            s._elements = elements;

            return s;
        }

        public bool Equals(Stage sc)
        {
            if (sc is null)
                return false;

            if (ReferenceEquals(this, sc))
                return true;

            return !(sc is null) && _combinations.ScrambledEquals(sc._combinations)
                                 && _analysisType.Equals(sc._analysisType)
                                 && _morph.Equals(sc._morph)
                                 && base.Equals(sc);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Stage);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();

            foreach (var combo in _combinations)
            {
                hashCode = hashCode + EqualityComparer<Combination>.Default.GetHashCode(combo);
            }
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


        public class StageProperty
        {
            private List<LoadCaseAttribute> _loadCaseAttributes;
            private List<FreedomCaseAttribute> _freedomCaseAttributes;

            public List<LoadCaseAttribute> LoadCaseAttributes => _loadCaseAttributes;

            public StageProperty()
            {
                _loadCaseAttributes = new List<LoadCaseAttribute>();
                _freedomCaseAttributes = new List<FreedomCaseAttribute>();
            }

            public void AddLoadCaseAttribute(LoadCaseAttribute attribute)
            {
                _loadCaseAttributes.Add(attribute);
            }

            public void AddLoadCaseAttributes(List<LoadCaseAttribute> attributes)
            {
                _loadCaseAttributes.AddRange(attributes);
            }

            public void AddFreedomCaseAttributes(FreedomCaseAttribute attribute)
            {
                _freedomCaseAttributes.Add(attribute);
            }

            public void AddFreedomCaseAttributes(List<FreedomCaseAttribute> attributes)
            {
                _freedomCaseAttributes.AddRange(attributes);
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
