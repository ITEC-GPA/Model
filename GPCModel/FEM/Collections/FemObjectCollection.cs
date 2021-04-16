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
    public class FemObjectCollection<T> : IEnumerable<T> where T : FEMObject
    {
        protected ICollection<T> _collection;

        /// <summary>
        /// Set di ID unici, l'indice d'ingresso non è garantito essere quello di uscita
        /// </summary>
        protected HashSet<int> _ids = new HashSet<int>();

        protected int _maxId = 0;

        /// <summary>
        /// Association between T.HashCode and index of T in <see cref="_collection"/>
        /// </summary>
        protected Dictionary<int, int> _hash = new Dictionary<int, int>();

        /// <summary>
        /// Inizialize the collection using <see cref="FEMObject.FemObjectOnlyIdComparer"/> as equality comparer
        /// </summary>
        public FemObjectCollection()
        {
            _collection = new List<T>();
        }

        public int Count => _collection.Count();

        #region Private method

        /// <summary>
        /// If the <paramref name="item"/>.Id already exist in the collection, its ID will be replaced with the collection maximum index + 1
        /// </summary>
        /// <returns>The Id of the item</returns>
        /// <remarks>The item will be added without checking if already exist in <see cref="FemObjectCollection{T}._collection"/></remarks>
        private int AddItem(T item)
        {
            // obj non presente
            if (_ids.Contains(item.Id))
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

        /// <summary>
        /// Add a FEMObject to the collection.
        /// <para>Object will be added in any case. Checks of duplicates not performed</para>
        /// <para>In any case, if the <paramref name="item"/> id already exist in the collection, its ID will be replaced with the collection maximum index + 1</para>
        /// </summary>
        /// <returns>The Id of the item</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null</exception>
        /// <remarks>This is an O(1) operation</remarks>
        public int SetItem(T item)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());

            return AddItem(item);
        }

        /// <inheritdoc cref="SetItem(T)"/>
        public int[] SetItem(T[] item)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());

            int[] ids = new int[item.Count()];
            for (int i = 0; i < item.Count(); i++)
            {
                ids[i] = AddItem(item[i]);
            }

            return ids;
        }

        /// <summary>
        /// Add a value using the hashMap
        /// </summary>
        /// <param name="item"></param>
        /// <param name="recomputeHash"></param>
        /// <remarks>If <paramref name="recomputeHash"/> is <see langword="false"/>, this is a O(1) operation, otherwise O(n)
        /// <para>Ensure that <see cref="RecomputeHash"/> has been called before using this method</para></remarks>
        /// <inheritdoc cref="Add(T)"/>
        internal virtual int AddHashed(T item, bool recomputeHash)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());

            if (recomputeHash)
                RecomputeHash();

            if (!_hash.ContainsKey(item.GetHashCode()) || _collection.Count == 0)
            {
                return AddItem(item);
            }
            else
            {
                // obj già presente
                if (_ids.Contains(item.Id))
                {
                    // id già presente
                    // non aggiungo, ritorno id dell'elemento già presente

                    return (_collection as List<T>).ElementAt(_hash[item.GetHashCode()]).Id;
                }
                else
                {
                    // id non presente
                    // ritorno id dell'elemento già presente

                    return (_collection as List<T>).ElementAt(_hash[item.GetHashCode()]).Id;
                }
            }
        }

        #endregion Public method - Setter

        #region Public method - Getter

        /// <param name="id">The <see cref="Elements.Element.Id"/> of the FemObject</param>
        /// <returns><typeparamref name="T"/> with id equal to <paramref name="id"/></returns>
        /// <exception cref="KeyNotFoundException"> If collection does not contain a element with Id: <paramref name="id"/> </exception>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual T this[int id]
        {
            get
            {
                if (!_ids.Contains(id))
                    throw new KeyNotFoundException($"Collection does not contain a element with Id:{id}");

                return _collection.SingleOrDefault(i => i.Id.Equals(id));
            }
        }

        /// <inheritdoc cref="this[int]"/>
        /// <exception cref="KeyNotFoundException">If collection does not contain a element with Id:<param name="id" /></exception>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual T GetElementById(int id)
        {
            return this[id];
        }

        public IEnumerator<T> GetEnumerator()
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
            return _collection.Contains(item);
        }

        #endregion Public method - Check

        #region Public method - Edit

        public virtual void Clear()
        {
            _ids.Clear();
            _collection.Clear();
        }

        /// <inheritdoc cref="ICollection.CopyTo(System.Array, int)"/>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual bool Remove(T item)
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

        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        /// <inheritdoc cref="this[int]"/>
        public virtual bool Remove(int id)
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

        /// <summary>
        /// Recompute the HashMap
        /// </summary>
        /// <remarks>This is a O(n) operation</remarks>
        internal void RecomputeHash()
        {
            _hash.Clear();
            var list = (_collection as List<T>);
            for (int i = 0; i < _collection.Count; i++)
            {
                _hash.Add(list[i].GetHashCode(), i);
            }
        }

        #endregion Public method - Edit

        #region Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            return obj is FemObjectCollection<T> collection && _collection.ScrambledEquals(collection._collection);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();

            foreach (var element in _collection)
            {
                hashCode = hashCode + EqualityComparer<FEMObject>.Default.GetHashCode(element);
            }

            return hashCode;
        }

        public static bool operator ==(FemObjectCollection<T> obj1, FemObjectCollection<T> obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FemObjectCollection<T> obj1, FemObjectCollection<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals - HashCode - Operators
    }
}
