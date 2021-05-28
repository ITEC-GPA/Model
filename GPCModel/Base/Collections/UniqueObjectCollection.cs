using GPC.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model
{

    /// <summary>
    /// Collection of unique <see cref="ModelObject"/>.
    /// This class is a wrapper of <see cref="List{T}"/>. Then <typeparamref name="T"/> can be a mutable object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>The collection is thread-safe</remarks>
    public class UniqueObjectCollection<T> : ModelObjectEnumerable<T>, ICollection<T> where T : ModelObject
    {
        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        /// <summary>
        /// Build the collection with <see cref="List{T}"/>. Then <typeparamref name="T"/> can be a mutable object
        /// </summary>
        public UniqueObjectCollection()
        {
            _collection = new List<T>();
        }

        /// <inheritdoc cref="ModelObjectEnumerable{T}.Add(T)" />
        /// <returns>True if the element has been added
        /// <para>False if the element has not been added there is already an equal element in the collection.</para>
        /// </returns>
        /// <remarks>This is a O(n) operation
        /// <para> To get the element in the collection use <see cref="GetItem(T, out T)"/> </para></remarks>
        public override bool Add(T item)
        {
            lock (_locker)
            {
                if (!_collection.Contains(item))
                {
                    _collection.Add(item);
                    return true;
                }

                return true;
            }
        }

        /// <summary>
        /// Get the item inside the collection that is equal to <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemFound"></param>
        /// <returns><see langword="True" /> if there is an element equal to <paramref name="item"/> in this collection </returns>
        public bool GetItem(T item, out T itemFound)
        {
            lock (_locker)
            {
                itemFound = _collection.Where(i => i.Equals(item)).FirstOrDefault();
                return itemFound != null;
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _collection.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_locker)
            {
                return _collection.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                _collection.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (_locker)
            {
                return _collection.Remove(item);
            }
        }

        void ICollection<T>.Add(T item)
        {
            lock (_locker)
            {
                this.Add(item);
            }
        }

        #region Equals - hashcode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is UniqueObjectCollection<T> collection && _collection.ScrambledEquals(collection._collection)
                                                                   && base.Equals(collection);
            }
        }

        public override int GetHashCode()
        {
            lock (_locker)
            {
                unchecked
                {
                    int hashCode = -391 + base.GetHashCode();

                    foreach (var element in _collection)
                    {
                        hashCode += element.GetHashCode();
                    }

                    return hashCode;
                }
            }
        }


        public static bool operator ==(UniqueObjectCollection<T> obj1, UniqueObjectCollection<T> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(UniqueObjectCollection<T> obj1, UniqueObjectCollection<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion 
    }
}