using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{
    public class ModelObjectSet<T> : ModelObjectEnumerable<T>, ICollection<T> where T : ModelObject
    {

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;


        /// <summary>
        /// Build the collection with <see cref="HashSet{T}"/> with default comparer of <typeparamref name="T"/>
        /// </summary>
        /// <remarks> <typeparamref name="T"/> must be an unmutable object</remarks>
        public ModelObjectSet()
        {
            _collection = new HashSet<T>();
        }

        /// <summary>
        /// Build the collection with <see cref="HashSet{T}"/> with a custom <paramref name="comparer"/>
        /// </summary>
        /// <remarks> <typeparamref name="T"/> must be an unmutable object</remarks>
        public ModelObjectSet(IEqualityComparer<T> comparer)
        {
            _collection = new HashSet<T>(comparer);
        }


        /// <inheritdoc cref="ModelObjectEnumerable{T}.Add(T)" />
        /// <returns>True if the element has been added
        /// <para>False if the element has not been added there is already an equal element in the collection.</para>
        /// </returns>
        /// <remarks>This is a O(n) operation
        /// <para> To get the element in the collection use <see cref="GetItem(T, out T)"/> </para></remarks>
        public override bool Add(T item)
        {
            if (!_collection.Contains(item))
            {
                _collection.Add(item);
                return true;
            }

            return true;
        }


        /// <summary>
        /// Get the item inside the collection that is equal to <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemFound"></param>
        /// <returns><see langword="True" /> if there is an element equal to <paramref name="item"/> in this collection </returns>
        public bool GetItem(T item, out T itemFound)
        {
            bool status = ((HashSet<T>)_collection).TryGetValue(item, out T found);

            if (status)
            {
                itemFound = found;
                return true;
            }
            else
            {
                itemFound = null;
                return false;
            }
        }

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
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

        public bool Remove(T item)
        {
            return _collection.Remove(item);
        }

    }
}
