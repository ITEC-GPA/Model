using System.Collections.Generic;
using System;
using GPC.Utilities.Extensions;
using System.Runtime.Serialization;

namespace GPC.Model
{
    /// <summary>
    /// Collection of unique <see cref="ModelObject"/>
    /// This class is a wrapper of <see cref="HashSet{T}"/>. Then <typeparamref name="T"/> must be an unmutable object
    /// </summary>
    /// <typeparam name="T">The type of collection derived from <see cref="ModelObject"/> </typeparam>
    /// <remarks>The collection is thread-safe</remarks>
    [Serializable]
    public class ModelObjectSet<T> : ModelObjectEnumerable<T>, ICollection<T> where T : ModelObject, ISerializable
    {
        #region Properties

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        #endregion

        #region Public Constructors

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

        protected ModelObjectSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

		#endregion

		#region Public Methods

		/// <inheritdoc cref="ModelObjectEnumerable{T}.Add(T)" />
		/// <returns><see langword="True"/> if the element has been added
		/// <para><see langword="False"/> if the element has not been added since there is already an equal element in the collection.</para>
		/// </returns>
		/// <remarks>This is a O(1) operation
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

                return false; 
            }
        }

        /// <inheritdoc cref="Add(T)"/>
        public virtual bool AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (!this.Add(item))
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the item inside the collection that is equal to <paramref name="item"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemFound"></param>
        /// <returns><see langword="True" /> if there is an element equal to <paramref name="item"/> in this collection </returns>
        public virtual bool GetItem(T item, out T itemFound)
        {
            lock (_locker)
            {
                if(!_collection.Contains(item))
                {
					itemFound = null;
					return false;
				}
                else
				{
					itemFound = item;
					return true;
				}
            }
        }

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        public virtual void Clear()
        {
            lock (_locker)
            {
                _collection.Clear(); 
            }
        }

        public virtual bool Contains(T item)
        {
            lock (_locker)
            {
                return _collection.Contains(item); 
            }
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                _collection.CopyTo(array, arrayIndex);
            }
        }

        public virtual bool Remove(T item)
        {
            lock (_locker)
            {
                return _collection.Remove(item);
            }
        }

        public virtual bool RemoveRange(IEnumerable<T> items)
        {
            lock (_locker)
            {
                foreach (var item in items)
                {
                    if (!(_collection.Remove(item)))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

		#endregion

		#region Equals - hashcode - Operators

		public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is ModelObjectSet<T> collection && _collection.ScrambledEquals(collection._collection)
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public static bool operator ==(ModelObjectSet<T> obj1, ModelObjectSet<T> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ModelObjectSet<T> obj1, ModelObjectSet<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion 
    }
}
