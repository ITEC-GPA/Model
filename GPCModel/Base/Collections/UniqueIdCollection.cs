using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{

    /// <summary>
    /// Collection of <see cref="ModelObjectId"/> with unique id. This class use an <see cref="HashSet{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>The collection is thread-safe
    /// <para>Id of <typeparamref name="T"/> must be unmutable</para></remarks>
    [Serializable]
    public class UniqueIdCollection<T> : ModelObjectIdSet<T>, ICollection<T> where T : ModelObjectId, ISerializable
    {

        /// <summary>
        /// Set di ID unici, l'indice d'ingresso non è garantito essere quello di uscita
        /// </summary>
        protected HashSet<int> _ids = new HashSet<int>();

        public UniqueIdCollection() 
            : base(new ModelObjectId.ModelObjectIdEqualityComparer())
        {
            _ids = new HashSet<int>();
        }

        
        public UniqueIdCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _ids = (HashSet<int>)info.GetValue("Ids", typeof(HashSet<int>));
        }


        #region Add

        /// <inheritdoc cref="ModelObjectSet{T}.Add(T)" />
        public override bool Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            lock (_locker)
            {
                if (Contains(item)) // stesso ID
                    return false;

                _collection.Add(item);
                _ids.Add(item.Id);
                return true;
            }
        }
        #endregion

        #region Get

        public override void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        public override bool GetItem(T item, out T itemFound)
        {
            return base.GetItem(item, out itemFound);
        }


        /// <summary><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></summary>
        /// <returns><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></returns>
        /// <exception cref="InvalidOperationException" ></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <remarks>This is a O(n) operation</remarks>
        public virtual T GetElementById(int id)
        {
            // l'Add non fa aggiungere oggetti con id duplicato.
            // se le istanze variano dopo che sono stati aggiunti e trova un duplicato va in eccezione

            lock (_locker)
            {
                if (this.Contains(id))
                    return _collection.SingleOrDefault(i => i.Id == id);
                else
                    throw new KeyNotFoundException($"Collection does not contain a element with id: {id}");
            }
        }
        #endregion

        #region Check

        /// <returns><see langword="True" /> if <paramref name="item"/> id already contained in the collection </returns>
        public override bool Contains(T item)
        {
            lock (_locker)
            {
                return _collection.Contains(item);
            }
        }


        /// <returns><see langword="True" /> if all the <paramref name="items"/> id already contained in the collection </returns>
        public bool ContainsRange(IEnumerable<T> items)
        {
            lock (_locker)
            {
                foreach (var item in items)
                {
                    if (!_collection.Contains(item))
                        return false;
                }
                return true;
            }
        }

        /// <returns><see langword="True" /> if this collection contains an element with <see cref="ModelObjectId.Id"/> equals to <paramref name="id"/> </returns>
        /// <remarks>This is an O(1) operation</remarks>
        public bool Contains(int id)
        {
            lock (_locker)
            {
                return _ids.Contains(id);
            }
        }

        /// <returns><see langword="True" /> if all the <paramref name="ids"/> are contained into the collection </returns>
        /// <remarks>This is an O(1*n) operation</remarks>
        public bool ContainsRange(IEnumerable<int> ids)
        {
            lock (_locker)
            {
                foreach (var item in ids)
                {
                    if (!_ids.Contains(item))
                        return false;
                }
                return true;
            }
        }
        #endregion


        #region Edit

        public override void Clear()
        {
            lock (_locker)
            {
                _collection.Clear();
                _ids.Clear();
            };
        }

        public bool Remove(int id)
        {
            lock (_locker)
            {
                return _collection.Remove(GetElementById(id)) && _ids.Remove(id);
            }
        }

        public override bool Remove(T item)
        {
            lock (_locker)
            {
                return _collection.Remove(item) && _ids.Remove(item.Id);
            }
        }

        public override bool RemoveRange(IEnumerable<T> items)
        {
            lock (_locker)
            {
                foreach (var item in items)
                {
                    if (!(_collection.Remove(item) && _ids.Remove(item.Id)))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool RemoveRange(IEnumerable<int> ids)
        {
            lock (_locker)
            {
                foreach (var item in ids)
                {
                    if (!(_collection.Remove(GetElementById(item)) && _ids.Remove(item)))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Ids", _ids, typeof(HashSet<int>));
        }

        #region Equals - hashcode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is UniqueIdCollection<T> collection && _collection.ScrambledEquals(collection._collection)
                                                               && base.Equals(collection);
            }
        }

        public override int GetHashCode()
        {
            lock (_locker)
            {
                unchecked
                {
                    return -391 + base.GetHashCode() + _collection.GetHashCodeScrambled();
                }
            }
        }


        public static bool operator ==(UniqueIdCollection<T> obj1, UniqueIdCollection<T> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(UniqueIdCollection<T> obj1, UniqueIdCollection<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion 

    }
}
