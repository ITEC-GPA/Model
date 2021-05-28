using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UniqueIdCollection<T> : ModelObjectIdSet<T>, ICollection<T> where T : ModelObjectId
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

        public override void Clear()
        {
            lock (_locker)
            {
                _collection.Clear();
                _ids.Clear();
            };
        }

        public override bool Contains(T item)
        {
            lock (_locker)
            {
                return _collection.Contains(item);
            }
        }


        /// <returns><see langword="True" /> if all the <paramref name="items"/> are contained into the collection </returns>
        public bool ContainsRange(IEnumerable<T> items)
        {
            lock (_locker)
            {
                foreach(var item in items)
                {
                    if (_collection.Contains(item))
                        return false;
                }
                return true;
            }
        }

        /// <returns><see langword="True" /> if this collection contains an element with <see cref="ModelObjectId.Id"/> equals to <paramref name="id"/> </returns>
        public bool Contains(int id)
        {
            lock (_locker)
            {
                return _ids.Contains(id);
            }
        }

        /// <returns><see langword="True" /> if all the <paramref name="ids"/> are contained into the collection </returns>
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
            // l'add non fa aggiungere oggetti con nome duplicato.
            // se le istanze variano dopo che sono stati aggiunti e trova un duplicato va in eccezione

            lock (_locker)
            {
                if (this.Contains(id))
                    return _collection.SingleOrDefault(i => i.Id == id);
                else
                    throw new KeyNotFoundException($"Collection does not contain a element with id: {id}");
            }
        }

        public override bool Remove(T item)
        {
            lock (_locker)
            {
                return _collection.Remove(item) && _ids.Remove(item.Id);
            }
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
                    int hashCode = -391 + base.GetHashCode();

                    foreach (var element in _collection)
                    {
                        hashCode += element.GetHashCode();
                    }

                    return hashCode;
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
