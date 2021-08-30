using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace GPC.Model
{
    /// <summary>
    /// Collection of <see cref="ModelObject"/> with unique name. This class use an <see cref="HashSet{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>The collection is thread-safe</remarks>
    [Serializable]
    public class UniqueNameCollection<T> : ModelObjectEnumerable<T>, ICollection<T> where T : ModelObject, ISerializable
    {
        private readonly HashSet<string> _names;

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;


        public UniqueNameCollection()
        {
            _collection = new HashSet<T>(new ModelObject.ModelObjectNameEqualityComparer());
            _names = new HashSet<string>();
        }


        public UniqueNameCollection(EqualityComparer<T> comparer)
        {
            _collection = new HashSet<T>(comparer);
            _names = new HashSet<string>();
        }


        public UniqueNameCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _names = (HashSet<string>)info.GetValue("Names", typeof(HashSet<string>));
        }

        /// <inheritdoc cref="ModelObjectEnumerable{T}.Add(T)" />
        /// <returns>True if the element has been added
        /// <para>False if the element has not been added since there was already an element in the collection with the same name</para>
        /// </returns>
        /// <remarks>This is a O(1) operation</remarks>
        public override bool Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (string.IsNullOrEmpty(item.Name) || string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentNullException(nameof(item));

            lock (_locker)
            {
                if (Contains(item)) // stesso nome
                    return false;

                _collection.Add(item);
                _names.Add(item.Name);
                return true; 
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


        /// <summary><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></summary>
        /// <returns><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></returns>
        /// <exception cref="InvalidOperationException" ></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <remarks>This is a O(n) operation</remarks>
        public virtual T GetElementByName(string name)
        {
            // l'add non fa aggiungere oggetti con nome duplicato.
            // se le istanze variano dopo che sono stati aggiunti e trova un duplicato va in eccezione

            lock (_locker)
            {
                if (ContainsName(name))
                    return _collection.SingleOrDefault(i => i.Name == name); 
                else
                    throw new KeyNotFoundException($"Collection does not contain a element with name: {name}");
            }
        }

        /// <returns>A list of all element names of this collection</returns>
        public virtual List<string> GetNames()
        {
            lock (_locker)
            {
                return _names.ToList();
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _collection.Clear();
                _names.Clear(); 
            }
        }

        /// <inheritdoc cref="ContainsName(string)"/>
        public bool Contains(T item)
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
                foreach (var item in items)
                {
                    if (Contains(item))
                        return false;
                }
                return true;
            }
        }

        /// <returns><see langword="True" /> if this collection contains an element with <see cref="ModelObject.Name"/> equals to <paramref name="name"/> </returns>
        public bool ContainsName(string name)
        {
            lock (_locker)
            {
                return _names.Contains(name);
            }
        }

        /// <returns><see langword="True" /> if all the <paramref name="names"/> are contained into the collection </returns>
        public bool ContainsNameRange(IEnumerable<string> names)
        {
            lock (_locker)
            {
                foreach (var item in names)
                {
                    if (_names.Contains(item))
                        return false;
                }
                return true;
            }
        }

        /// <remarks>This is a O(1) operation</remarks>
        public bool Remove(T item)
        {
            lock (_locker)
            {
                return _collection.Remove(item) && _names.Remove(item.Name);
            }
        }

        public bool Remove(string name)
        {
            lock (_locker)
            {
                return _collection.Remove(GetElementByName(name)) && _names.Remove(name); ;
            }
        }


        void ICollection<T>.Add(T item)
        {
            // metodo privato
            this.Add(item);
        }


        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (_locker)
            {
                return ((HashSet<T>)_collection).IsSubsetOf(other);
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (_locker)
            {
                return ((HashSet<T>)_collection).IsSupersetOf(other);
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (_locker)
            {
                return ((HashSet<T>)_collection).IsProperSupersetOf(other);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (_locker)
            {
                return ((HashSet<T>)_collection).IsProperSubsetOf(other);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            lock (_locker)
            {
                return ((HashSet<T>)_collection).Overlaps(other);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            lock (_locker)
            {
                return ((HashSet<T>)_collection).SetEquals(other);
            }
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                _collection.CopyTo(array, arrayIndex);
            }
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Names", _names, typeof(HashSet<string>));
        }


        #region Equals - hashcode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is UniqueNameCollection<T> collection && _collection.ScrambledEquals(collection._collection)
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


        public static bool operator ==(UniqueNameCollection<T> obj1, UniqueNameCollection<T> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(UniqueNameCollection<T> obj1, UniqueNameCollection<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion 
    }
}
