using GPC.Model.FEM.FiniteElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// A collection of FemObject. 
    /// <para>This collection does not contains elements with a duplicated ID</para>
    /// <para>This collection can contain duplicate element (with different ID)</para>
    /// </summary>
    /// <typeparam name="T">A <see cref="FEMObject"/></typeparam>
    public class FemObjectCollection<T> : IEnumerable<T> where T : FEMObject
    {
        protected ICollection<T> _collection;

        /// <summary>
        /// Set di ID unici, l'indice d'ingresso non è garantito essere quello di uscita
        /// </summary>
        protected HashSet<int> _ids = new HashSet<int>();
        protected int _maxId = 0;

        public FemObjectCollection()
        {
            // Usiamo l'equality comparer che confronta solamente gli ID, quindi due oggetti uguali vengono aggiunti se hanno id diverso
            _collection = new HashSet<T>(new FEMObject.FemObjectOnlyIdComparer());
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

                return _collection.SingleOrDefault(i => i.Id.Equals(id));
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
        /// Add a FEMObject to the collection. 
        /// <para>If the item index already exist in the collection, its ID will be replaced with the collection maximum index + 1</para> 
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

        /// <inheritdoc cref="ICollection.CopyTo(System.Array, int)"/>
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