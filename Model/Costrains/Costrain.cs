using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Costrains
{
    public abstract class Costrain : ModelObjectId
    {

        protected NodeElement _startNode;
        protected NodeElement[] _endNodes;

        protected MultiPointsCostrain[] _links;



        public NodeElement StartNode => _startNode;
        public NodeElement[] EndNodes => _endNodes;

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

        public MultiPointsCostrain[] Links => _links;


        public Costrain(NodeElement nodo1, NodeElement[] nodes, string name = "", int id = IDUNASSIGNED)
            : base(id, name)
        {
            _startNode = nodo1;
            _endNodes = nodes;
        }

        public override bool Equals(object obj)
        {
            return obj is Costrain costrain &&
                   base.Equals(obj) &&
                   EqualityComparer<NodeElement>.Default.Equals(_startNode, costrain._startNode) &&
                   _endNodes.SequenceEqual(costrain._endNodes) &&
                   _links.SequenceEqual(costrain._links);
        }


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