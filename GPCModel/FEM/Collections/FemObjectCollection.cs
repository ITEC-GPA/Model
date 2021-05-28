using GPC.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// A collection of FemObject.
    /// <para>This collection does not contains elements with a duplicated ID</para>
    /// </summary>
    /// <typeparam name="T">A <see cref="FEMObject"/></typeparam>
    /// <remarks>The collection is thread-safe</remarks>
    public class FemObjectCollection<T> : IEnumerable<T> where T : FEMObject
    {
        protected readonly object _locker = new object();

        protected ICollection<T> _collection;

        /// <summary>
        /// Set di ID unici, l'indice d'ingresso non è garantito essere quello di uscita
        /// </summary>
        protected HashSet<int> _ids = new HashSet<int>();

        protected int _maxId = 0;

        public int Count => _collection.Count();

        public FemObjectCollection() : base()
        {
            _collection = new List<T>();
        }

        #region Private method

        /// <summary>
        /// If the <paramref name="item"/>.Id already exist in the collection, its ID will be replaced with the collection maximum index + 1
        /// </summary>
        /// <returns>The Id of the item</returns>
        /// <remarks>The item will be added without checking if already exist in <see cref="FemObjectCollection{T}._collection"/></remarks>
        private int AddItem(T item)
        {
            lock (_locker)
            {
                // obj non presente
                if (item.Id == ModelObjectId.IDUNASSIGNED || _ids.Contains(item.Id))
                {
                    // id già presente
                    // cambio id e aggiungo obj

                    item.SetId(++_maxId); // Forzo id ad essere maggiore di zero

                    _collection.Add(item);
                    _ids.Add(item.Id);

                    return item.Id;
                }
                else
                {
                    // id non presente
                    // aggiungo obj

                    if (item.Id == 0) // Forzo id ad essere maggiore di zero
                        item.SetId(++_maxId);

                    _collection.Add(item);
                    _ids.Add(item.Id);

                    if (item.Id > _maxId)
                        _maxId = item.Id;

                    return item.Id;
                } 
            }
        }

        #endregion Private method

        #region Public method - Setter

        /// <summary>
        /// Add a FEMObject to the collection.
        /// <para>Object will be added only if not already present</para>
        /// <para>In any case, if the <paramref name="item"/> id already exist in the collection, its ID will be replaced with the collection maximum index + 1</para>
        /// </summary>
        /// <returns>The Id of the item</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null </exception>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual int Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());

            if (!_collection.Contains(item) || _collection.Count == 0)
            {
                return AddItem(item);
            }
            else
            {
                lock (_locker)
                {
                    // obj già presente
                    if (_ids.Contains(item.Id))
                    {
                        // id già presente
                        // non aggiungo, ritorno id dell'elemento già presente

                        return (_collection as List<T>).SingleOrDefault(i => i.Equals(item)).Id;
                    }
                    else
                    {
                        // id non presente
                        // ritorno id dell'elemento già presente
                        return (_collection as List<T>).SingleOrDefault(i => i.Equals(item)).Id;
                    } 
                }
            }
        }

        /// <summary>
        /// Add a FEMObject to the collection.
        /// <para>Object will be added in any case. Checks of duplicates not performed</para>
        /// <para>In any case, if the <paramref name="item"/> id already exist in the collection, its ID will be replaced with the collection maximum index + 1</para>
        /// </summary>
        /// <returns>The Id of the item</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null</exception>
        /// <remarks>This is an O(1) operation</remarks>
        public virtual int SetItem(T item)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());

            return AddItem(item);
        }

        #endregion Public method - Setter

        #region Public method - Getter

        /// <inheritdoc cref="FemObjectCollection{T}.GetElementById(int)"/>
        public virtual T this[int id]
        {
            get
            {
                return GetElementById(id);
            }
        }

        /// <param name="id">The <see cref="ModelObjectId.Id"/> of the FemObject</param>
        /// <returns><typeparamref name="T"/> with id equal to <paramref name="id"/></returns>
        /// <exception cref="KeyNotFoundException"> If collection does not contain a element with Id: <paramref name="id"/> </exception>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual T GetElementById(int id)
        {
            if (!_ids.Contains(id))
                throw new KeyNotFoundException($"Collection does not contain a element with Id:{id}");

            lock (_locker)
            {
                return _collection.SingleOrDefault(i => i.Id.Equals(id)); 
            }
        }

        public virtual HashSet<int> GetIds()
        {
            return _ids;
        }

        /// <returns>A map between <typeparamref name="T"/> HashCode and the index of <typeparamref name="T"/> in the <see cref="_collection"/> </returns>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual Dictionary<int, int> GetElementHashMap()
        {
            lock (_locker)
            {
                Dictionary<int, int> hashMap = new Dictionary<int, int>();
                var list = (_collection as List<T>);
                for (int i = 0; i < list.Count; i++)
                {
                    hashMap[list[i].GetHashCode()] = i;
                }

                return hashMap; 
            }
        }

        /// <returns>A map between <typeparamref name="T"/>.Id  and the index of <typeparamref name="T"/> in the <see cref="_collection"/> </returns>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual Dictionary<int, int> GetElementIdMap()
        {
            lock (_locker)
            {
                Dictionary<int, int> hashMap = new Dictionary<int, int>();

                var list = (_collection as List<T>);

                for (int i = 0; i < list.Count; i++)
                {
                    hashMap[list[i].Id] = i;
                }

                return hashMap; 
            }
        }

        /// <summary>
        /// Get the element by its position on the <see cref="_collection"/>
        /// </summary>
        /// <param name="index">The index of <typeparamref name="T"/> in the <see cref="_collection"/>
        /// <para>This is different from the ID of <typeparamref name="T"/></para>
        /// </param>
        /// <returns><typeparamref name="T"/></returns>
        /// <remarks>This method should be used along with <see cref="GetElementIdMap()"/>
        /// <para>This is an O(1) operation</para></remarks>
        public virtual T GetElementByIndex(int index)
        {
            return (_collection as List<T>)[index];
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        #endregion Public method - Getter

        #region Public method - Check

        /// <summary>
        /// Check if <paramref name="item"/> is contained in the collection
        /// </summary>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual bool Contains(T item)
        {
            lock (_locker)
            {
                return _collection.Contains(item); 
            }
        }

        #endregion Public method - Check

        #region Public method - Edit

        public void Clear()
        {
            lock (_locker)
            {
                _ids.Clear();
                _collection.Clear(); 
            }
        }

        /// <inheritdoc cref="ICollection.CopyTo(System.Array, int)"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                _collection.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc cref="FemObjectCollection{T}.Remove(T)"/>
        public bool Remove(T item)
        {
            lock (_locker)
            {
                if (_collection.Remove(item))
                {
                    _ids.Remove(item.Id);
                    return true;
                }
                else
                {
                    return false;
                } 
            }
        }

        /// <inheritdoc cref="FemObjectCollection{T}.Remove(int)"/>
        public bool Remove(int id)
        {
            lock (_locker)
            {
                int removed = (_collection as List<T>).RemoveAll(i => i.Id.Equals(id));

                if (removed > 0)
                {
                    _ids.Remove(id);
                    return true;
                }
                else
                {
                    return false;
                } 
            }
        }

        #endregion Public method - Edit

        #region Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is FemObjectCollection<T> collection && _collection.ScrambledEquals(collection._collection); 
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
                        hashCode += EqualityComparer<FEMObject>.Default.GetHashCode(element);
                    }

                    return hashCode;  
                }
            }
        }

        public static bool operator ==(FemObjectCollection<T> obj1, FemObjectCollection<T> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FemObjectCollection<T> obj1, FemObjectCollection<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals - HashCode - Operators
    }
}