using GPC.Model.FEM.FiniteElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Collections
{
    public class FiniteElementCollection : HashCollection<FiniteElement>
    {

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

        /// <summary>
        /// Calculate the element hash code based on the Ids of its nodes
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override int GetItemHashCode(FiniteElement item)
        {
            unchecked
            {
                int hash = 23;

                for (int i = 0; i < item.Nodes.Length; i++)
                {
                    hash = hash * 17 + item.Nodes[i].Id;
                }

                return hash; 
            }
        }
    }
}
