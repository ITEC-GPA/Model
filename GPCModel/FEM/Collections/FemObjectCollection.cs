using GPC.Utilities.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// A collection of FemObject.
    /// <para>This collection does not contains elements with a duplicated ID</para>
    /// <para>This collection can not contain duplicate element (even with different ID)</para>
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
        /// Inizialize the collection using <see cref="FEMObject.FemObjectOnlyIdComparer"/> as equality comparer
        /// </summary>
        public FemObjectCollection()
        {
            // Usiamo l'equality comparer di default di T che non confronta gli ID, usiamo una lista a parte per confrontare gli ID
            // l'equality di default usa l'equals degli oggetti 
            _collection = new HashSet<T>();
        }

        /// <summary>
        /// Inizialize with a custom Equality Comparer <paramref name="customEqualityComparer"/>
        /// </summary>
        public FemObjectCollection(IEqualityComparer<T> customEqualityComparer)
        {
            _collection = new HashSet<T>(customEqualityComparer);
        }


        public int Count => _collection.Count();

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">The <see cref="Elements.Element.Id"/> of the FemObject</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"> If collection does not contain a element with Id: <param name="id" /> </exception>
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
        /// <inheritdoc cref="this[int]"/>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">If collection does not contain a element with Id:<param name="id"></param> </exception>
        public virtual T GetElementById(int id)
        {
            return this[id];
        }

        /// <summary>
        /// Add a FEMObject to the collection.
        /// <para>Object will be added only if not already present, using the equality comparer chosed on the the constructor</para>
        /// <para>In any case, if the <paramref name="item"/> id already exist in the collection, its ID will be replaced with the collection maximum index + 1</para>
        /// </summary>
        /// <returns>The Id of the item</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null </exception>
        public virtual int Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());
            

            if (!_collection.Contains(item) || _collection.Count == 0)
            {
                // obj non presente
                if (_ids.Contains(item.Id))
                {
                    // id già presente
                    // cambio id e aggiungo obj

                    item.SetId(++_maxId);

                    _collection.Add(item);
                    _ids.Add(item.Id);

                    return item.Id;
                }
                else
                {
                    // id non presente
                    // aggiungo obj

                    _collection.Add(item);
                    _ids.Add(item.Id);

                    if (item.Id > _maxId)
                        _maxId = item.Id;

                    return item.Id;
                }
            }
            else
            {
                // obj già presente
                if (_ids.Contains(item.Id))
                {
                    // id già presente
                    // non aggiungo, ritorno id dell'elemento già presente

                    (_collection as HashSet<T>).TryGetValue(item, out T itemFound);

                    return itemFound.Id;
                }
                else
                {
                    // id non presente
                    // ritorno id dell'elemento già presente
                    (_collection as HashSet<T>).TryGetValue(item, out T itemFound);
                    item.SetId(itemFound.Id);
                    return itemFound.Id;
                }
            }


            //if (!_ids.Contains(item.Id))
            //{
            //    if (!_collection.Contains(item))
            //    {
            //        // id non presente, obj non presente
            //        // va aggiunto

            //        _collection.Add(item);
            //        _ids.Add(item.Id);

            //        if (item.Id > _maxId)
            //            _maxId = item.Id;

            //        return item.Id;
            //    }
            //    else
            //    {
            //        // id non presente, obj già presente
            //        // non va aggiunto

            //        (_collection as HashSet<T>).TryGetValue(item, out T itemFound);
            //        item.SetId(itemFound.Id);
            //        return itemFound.Id;
            //    }
            //}
            //else
            //{
            //    if (!_collection.Contains(item))
            //    {
            //        item.SetId(++_maxId);

            //        _collection.Add(item);
            //        _ids.Add(item.Id);

            //        // id già presente ma obj diverso
            //        // va aggiunto, ma cambio ID
            //        return item.Id;
            //    }
            //    else
            //    {
            //        // id già presente, obj già presente
            //        // non aggiunto
            //        return item.Id;
            //    }
            //}

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

        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        public virtual bool Remove(T item)
        {
            return _collection.Remove(item);
        }

        /// <inheritdoc cref="ICollection{T}.Remove(T)"/>
        /// <inheritdoc cref="this[int]"/>
        public virtual bool Remove(int id)
        {
            return _collection.Remove(this[id]);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

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
    }
}