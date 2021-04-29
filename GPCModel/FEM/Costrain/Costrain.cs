using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Costrains
{
    abstract public class Costrain : FEMObject
    {
        #region variables
        protected Node _startNode;
        protected Node[] _endNodes;

        protected MultiPointsCostrain[] _links;
        #endregion

        #region Properties
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
        #endregion


        public Costrain(Node nodo1, Node[] nodes, string name = "") : base(name) { }
    }
}
