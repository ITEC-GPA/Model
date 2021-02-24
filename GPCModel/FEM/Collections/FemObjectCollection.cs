using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// A collection of FemObject. This collection does not contains elements a duplicated ID
    /// </summary>
    public class FemObjectCollection<T> : IEnumerable<T> where T : FEMObject
    {
        private List<T> _collection = new List<T>();
        private HashSet<int> _index = new HashSet<int>();
        private int _maxIndex = 0;

        public int Count => _collection.Count();


        public T this[int index]
        {
            get => _collection.Where(i => i.Index == index).First();
        }


        /// <summary>
        /// Add a FEMObject to the collection. If the item.Index already exist in the collection it will be replaced with the collection maximum index + 1;
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The index of the item</returns>
        public int Add(T item)
        {
            if (_index.Contains(item.Index))
            {
                item.Index = _maxIndex++;
            }
            else
            {
                if (item.Index > _maxIndex)
                    _maxIndex = item.Index;
            }

            _collection.Add(item);
            _index.Add(item.Index);

            return item.Index;
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return _collection.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }

}
