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
                int sx = GetNodesSignature(x.Nodes);
                int dx = GetNodesSignature(y.Nodes);
                if (sx < dx)
                    return -1;
                else if(sx > dx)
                    return 1;
                else
                    return 0;
            }

            public int GetNodesSignature(Node[] nodes)
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

        /// <summary>
        /// Add an finite element to the collection calling the <see cref="SortedCollection{T}.Add(T)"/> method.
        /// Provided only for compability with the previous class <see cref="FemObjectCollection{T}"/>
        /// </summary>
        /// <param name="element">The finite element to add</param>
        /// <returns>The Id of the added element</returns>
        public int AddUnique(FiniteElement element)
        {
            return Add(element);
        }

        /// <summary>
        /// Get a finite element by his id calling the <see cref="SortedCollection{T}.GetById(int)"/> method.
        /// Provided only for compability with the previous class <see cref="FemObjectCollection{T}"/>
        /// </summary>
        /// <param name="id">The id of the finite element to retrieve</param>
        /// <returns>The finite element</returns>
        public FiniteElement GetElementById(int id)
        {
            return GetById(id);
        }

    }
}
