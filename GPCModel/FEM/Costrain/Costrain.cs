using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Fem.Costrains
{
    public abstract class Costrain : FemObject
    {

        protected Node _startNode;
        protected Node[] _endNodes;

        protected MultiPointsCostrain[] _links;



        public Node StartNode => _startNode;
        public Node[] EndNodes => _endNodes;

        public Node EndNode
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

        public MultiPointsCostrain[] Links => _links;


        public Costrain(Node nodo1, Node[] nodes, string name = "") 
            : base(name)
        {
            _startNode = nodo1;
            _endNodes = nodes;
        }

        public override bool Equals(object obj)
        {
            return obj is Costrain costrain &&
                   base.Equals(obj) &&
                   EqualityComparer<Node>.Default.Equals(_startNode, costrain._startNode) &&
                   _endNodes.SequenceEqual(costrain._endNodes) &&
                   _links.SequenceEqual(costrain._links);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Node>.Default.GetHashCode(_startNode);

                foreach (var element in _endNodes)
                {
                    hashCode = hashCode * -17 + EqualityComparer<Node>.Default.GetHashCode(element);
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