using GPC.Model.FEM.FiniteElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// A collection of FemObject. This collection does not contains elements with a duplicated ID
    /// </summary>
    public class FemObjectCollection<T> : IEnumerable<T> where T : FEMObject
    {
        protected ICollection<T> _collection;
        protected HashSet<int> _ids = new HashSet<int>();
        protected int _maxId = 0;

        public FemObjectCollection()
        {
            // Non ha equality comparer quindi gli oggetti vengono aggiunti senza controllare se esistono già
            _collection = new List<T>();
        }

        public int Count => _collection.Count();

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">If collection does not contain a element with Id:<param name="id"></param> </exception>
        public virtual T this[int id]
        {
            get
            {
                if (!_ids.Contains(id))
                    throw new KeyNotFoundException($"Collection does not contain a element with Id:{id}");

                return _collection.Where(i => i.Id.Equals(id)).First();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">If collection does not contain a element with Id:<param name="id"></param> </exception>
        public virtual T GetElementById(int id)
        {
            return this[id];
        }

        /// <summary>
        /// Add a FEMObject to the collection. If the item.Index already exist in the collection it will be replaced with the collection maximum index + 1;
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The index of the item</returns>
        public virtual int Add(T item)
        {
            if (_ids.Contains(item.Id))
            {
                item.SetId(++_maxId);
            }
            else
            {
                if (item.Id > _maxId)
                    _maxId = item.Id;
            }

            _collection.Add(item);
            _ids.Add(item.Id);

            return item.Id;
        }

        public virtual void Clear()
        {
            _collection.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public virtual bool Remove(T item)
        {
            return _collection.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }

    
}