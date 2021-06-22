using GPC.Model.FEM.FiniteElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Collections
{
    public class FiniteElementCollection : SortedCollection<FiniteElement>
    {
        protected class NodesComparer : IComparer<FiniteElement>
        {
            public int Compare(FiniteElement x, FiniteElement y)
            {
                int sx = GetNodesSigneture(x.Nodes);
                int dx = GetNodesSigneture(y.Nodes);
                if (sx < dx)
                    return -1;
                else if(sx > dx)
                    return 1;
                else
                    return 0;
            }

            public int GetNodesSigneture(Node[] nodes)
            {
                int s = 3;
                for (int i = 0; i < nodes.Length; i++)
                {
                    s = s * 7 + nodes[i].Id.GetHashCode();
                }
                return s;
            }
        }

        protected static NodesComparer _positionComparer = new NodesComparer();

        public override IComparer<FiniteElement> Comparer => _positionComparer;
    }
}
