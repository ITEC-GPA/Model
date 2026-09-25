using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Costrains
{
    /// <summary>
    /// A constraint between a node and other nodes, expressed by linear equations between their degrees of freedom (see <see cref="MultiPointsCostrain"/>)
    /// </summary>
    public abstract class Costrain : ModelObjectId
    {

        /// <summary>
        /// The master node
        /// </summary>
        protected NodeElement _startNode;
        /// <summary>
        /// The constrained nodes
        /// </summary>
        protected NodeElement[] _endNodes;

        /// <summary>
        /// The equations of the constraint (null until a derived class sets them)
        /// </summary>
        protected MultiPointsCostrain[] _links;



        /// <summary>
        /// The master node
        /// </summary>
        public NodeElement StartNode => _startNode;
        /// <summary>
        /// The constrained nodes
        /// </summary>
        public NodeElement[] EndNodes => _endNodes;

        /// <summary>
        /// The constrained node, when there is only one
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">If there is not exactly one constrained node</exception>
        public NodeElement EndNode
        {
            get
            {
                if (_endNodes.Count() == 1)
                {
                    return _endNodes[0];
                }
                else
                {
                    throw new IndexOutOfRangeException("This link connect more than 1 node");
                }
            }
        }

        /// <summary>
        /// The equations of the constraint
        /// </summary>
        public MultiPointsCostrain[] Links => _links;


        /// <summary>
        /// Creates a constraint (the equations are set by the derived classes)
        /// </summary>
        /// <param name="nodo1">The master node</param>
        /// <param name="nodes">The constrained nodes</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public Costrain(NodeElement nodo1, NodeElement[] nodes, string name = "", int id = IDUNASSIGNED)
            : base(id, name)
        {
            _startNode = nodo1;
            _endNodes = nodes;
        }

        /// <summary>
        /// Equality of name, nodes and equations (the equations must not be null)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal constraint</returns>
        public override bool Equals(object obj)
        {
            return obj is Costrain costrain &&
                   base.Equals(obj) &&
                   EqualityComparer<NodeElement>.Default.Equals(_startNode, costrain._startNode) &&
                   _endNodes.SequenceEqual(costrain._endNodes) &&
                   _links.SequenceEqual(costrain._links);
        }


        /// <summary>
        /// The hash code of name, nodes and equations
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<NodeElement>.Default.GetHashCode(_startNode);

                foreach (var element in _endNodes)
                {
                    hashCode = hashCode * -17 + EqualityComparer<NodeElement>.Default.GetHashCode(element);
                }

                foreach (var element in _links)
                {
                    hashCode = hashCode * -17 + EqualityComparer<MultiPointsCostrain>.Default.GetHashCode(element);
                }

                return hashCode;
            }
        }
    }
}