using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Collections;
using GPC.Model.Results;
using GPC.Utilities.Extensions;

namespace GPC.Model.Fem
{
    /// <summary>
    /// Rapresent a Node of a <see cref="FiniteElements.FiniteElement"/>
    /// </summary>
    [Serializable]
    public class Node : FemObject, INotifyPropertyChanged
    {
        #region Variables

        private Point3d _position;
        private readonly AttributesCollection<LoadCaseAttribute> _attributesLoadCase;
        private readonly AttributesCollection<FreedomCaseAttribute> _attributesFreedomCase;
        private readonly ModelObjectSet<NodeResult> _results;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// A copy of the node position. The changes on the returned point will not affect the node because this is a copy
        /// </summary>
        public Point3d Position
        {
            get => _position.Clone() as Point3d;
            set
            {
                if (!_position.Equals(value))
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        /// <summary>
        /// Contains the degree of freedom active for the node
        /// </summary>
        public SortedSet<LinearSolver.DOF> DOF { get; set; }

        public int NrActiveDof
        {
            get
            {
                int ris = 0;
                for (int i = 0; i < LinearSolver.MAXDOFPERNODE; i++)
                {
                    if (DOF.Contains((LinearSolver.DOF)i) == true)
                    {
                        ris++;
                    }
                }
                return ris;
            }
        }

        public AttributesCollection<FreedomCaseAttribute> AttributesFreedomCase => _attributesFreedomCase;
        public AttributesCollection<LoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;
        public IEnumerable<NodeResult> Results => _results;

        #endregion

        public Node(Point3d point, string name = "")
            : base(name)
        {
            _position = point;

            DOF = new SortedSet<Solver.DOF>();

            _attributesLoadCase = new AttributesCollection<LoadCaseAttribute>();
            _attributesFreedomCase = new AttributesCollection<FreedomCaseAttribute>();

            _results = new ModelObjectSet<NodeResult>(EqualityComparer<ElementResult>.Default); // comparer di ElementResult, usa solo il case come comparatore
        }

        public Node(double X, double Y, double Z, string name = "")
            : this(new Point3d(X, Y, Z), name)
        {
        }

        /// <summary>
        /// Internal constructor, that allows to add a group directly during construction to speedup femmodel build
        /// </summary>
        // Do not set this constructor to public
        internal Node(Point3d point, Group group)
            : this(point, "")
        {
            _groups.Add(group);
        }

        /// <summary>
        /// only for test purpose
        /// </summary>
        internal Node(Point3d point, int id)
            : this(point)
        {
            Id = id;
        }

        /// <summary>
        /// only for test purpose
        /// </summary>
        internal Node(double X, double Y, double Z, string name, int id)
            : this(new Point3d(X, Y, Z), name)
        {
            Id = id;
        }

        protected Node(SerializationInfo info, StreamingContext context)
        {
            _position = (Point3d)info.GetValue("Position", typeof(Point3d));
            _results = (ModelObjectSet<NodeResult>)info.GetValue("Result", typeof(ModelObjectSet<NodeResult>));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"ID={Id}; Name={Name}; X={Position.X}; Y={Position.Y}; Z={Position.Z}";
        }


        public virtual bool AddAttribute(INodeFreedomCaseAttribute attribute, out bool replace)
        {
            return _attributesFreedomCase.Add((FreedomCaseAttribute)attribute, out replace);
        }

        public virtual bool AddAttribute(INodeFreedomCaseAttribute attribute)
        {
            return _attributesFreedomCase.Add((FreedomCaseAttribute)attribute);
        }

        public virtual bool AddAttribute(INodeLoadCaseAttribute attribute, out bool replace)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute, out replace);
        }

        public virtual bool AddAttribute(INodeLoadCaseAttribute attribute)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute);
        }

        public void AddResult(NodeResult result)
        {
            if (result != null)
                _results.Add(result);
        }

        public Node Duplicate()
        {
            Node duplicate = new Node(new Point3d(Position.X, Position.Y, Position.Z), Name)
            {
                DOF = DOF
            };

            duplicate.Id = Id;

            foreach (INodeFreedomCaseAttribute attribute in _attributesFreedomCase)
            {
                duplicate.AddAttribute(attribute);
            }
            foreach (INodeLoadCaseAttribute attribute in _attributesLoadCase)
            {
                duplicate.AddAttribute(attribute);
            }
            return duplicate;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Position", _position, typeof(Point3d));
            info.AddValue("Result", _results, typeof(ModelObjectSet<NodeResult>));
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is Node node) && _position.Equals(node._position)
                                      && _attributesFreedomCase.ScrambledEquals(node._attributesFreedomCase)
                                      && _attributesLoadCase.ScrambledEquals(node._attributesLoadCase)
                                      && base.Equals(node);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_position);

                foreach (var element in _attributesLoadCase)
                {
                    hashCode += -17 * EqualityComparer<LoadCaseAttribute>.Default.GetHashCode(element);
                }
                foreach (var element in _attributesFreedomCase)
                {
                    hashCode += -17 * EqualityComparer<FreedomCaseAttribute>.Default.GetHashCode(element);
                }

                return hashCode;
            }
        }

    }
}
