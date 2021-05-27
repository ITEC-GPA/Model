using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Utilities.Extensions;
using GPC.Model.Results;

namespace GPC.Model.FEM
{
    /// <summary>
    /// Rapresent a Node of a <see cref="FiniteElements.FiniteElement"/>
    /// </summary>
    public class Node : FEMObject
    {
        #region Variables

        private Point3d _position;

        private List<INodeLoadCaseAttribute> _attributesLoadCase;
        private List<INodeFreedomCaseAttribute> _attributesFreedomCase;

        private readonly ModelObjectSet<NodeResult> _results;

        #endregion

        #region Properties

        public Point3d Position => _position;

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

        public List<INodeFreedomCaseAttribute> AttributesFreedomCase => _attributesFreedomCase;
        public List<INodeLoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;
        public IEnumerable<NodeResult> Results => _results;

        #endregion

        public Node(Point3d point, string name = "") : base(name)
        {
            _position = point;
            
            DOF = new SortedSet<Solver.DOF>();
            
            _attributesLoadCase = new List<INodeLoadCaseAttribute>();
            _attributesFreedomCase = new List<INodeFreedomCaseAttribute>();

            _results = new ModelObjectSet<NodeResult>(EqualityComparer<ElementResult>.Default); // comparer di ElementResult, usa solo il case come comparatore
        }

        public Node(double X, double Y, double Z, string label="") : this(new Point3d(X, Y, Z), label)
        {

        }

        /// <summary>
        /// only for test purpose
        /// </summary>
        internal Node(Point3d point, int id) : this(point)
        {
            SetId(id);
        }

        /// <summary>
        /// only for test purpose
        /// </summary>
        internal Node(double X, double Y, double Z, string name, int id) : this(new Point3d(X, Y, Z), name)
        {
            SetId(id);
        }

        public override string ToString()
        {
            return "ID = " + Id + " Name = " + Name + "  X=" + Position.X + " Y=" + Position.Y + " Z=" + Position.Z;
        }

        public void AddAttribute(INodeFreedomCaseAttribute attribute)
        {
            _attributesFreedomCase.Add(attribute);
        }

        public void AddAttribute(INodeLoadCaseAttribute attribute)
        {
            _attributesLoadCase.Add(attribute);
        }

        public void AddResult(NodeResult result)
        {
            if (result != null)
                _results.Add(result);
        }

        public Node Duplicate()
        {
            Node duplicate = new Node(new Point3d(Position.X, Position.Y, Position.Z), this.Name)
            {
                DOF = DOF
            };

            duplicate.SetId(Id);
            
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
                    hashCode += -17 * EqualityComparer<INodeLoadCaseAttribute>.Default.GetHashCode(element);
                }
                foreach (var element in _attributesFreedomCase)
                {
                    hashCode += -17 * EqualityComparer<INodeFreedomCaseAttribute>.Default.GetHashCode(element);
                }

                return hashCode;
            }

        }

    }
}
