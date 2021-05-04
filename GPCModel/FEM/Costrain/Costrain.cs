using System;
using System.Linq;

namespace GPC.Model.FEM.Costrains
{
    public abstract class Costrain : FEMObject
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

        }


    }
}