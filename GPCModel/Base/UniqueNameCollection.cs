using System.Collections.Generic;
using System.Linq;
using System;

namespace GPC.Model
{
    /// <summary>
    /// Collection of <see cref="ModelObject"/> with unique name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UniqueNameCollection<T> : ModelObjectEnumerable<T>, ICollection<T> where T : ModelObject
    {
        public UniqueNameCollection()
        {
            _collection = new HashSet<T>(new ModelObject.ModelObjectNameEqualityComparer());
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

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

            if (Contains(item)) // stesso nome
                return false;

            _collection.Add(item);
            return true;
        }

        /// <summary><inheritdoc cref="Enumerable.SingleOrDefault"/></summary>
        /// <returns><inheritdoc cref="Enumerable.SingleOrDefault"/></returns>
        /// <exception cref="InvalidOperationException" ></exception>
        /// <remarks>This is a O(n) operation</remarks>
        public virtual T GetElementByName(string name)
        {
            // l'add non fa aggiungere oggetti con nome duplicato.
            // se le istanze variano dopo che sono stati aggiunti e trova un duplicato va in eccezione

            //(_collection as HashSet<T>).TryGetValue(new Elements.GhostElement(name), out T found);

            return _collection.SingleOrDefault(i => i.Name == name);
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        /// <remarks>This is a O(1) operation</remarks>
        public bool Remove(T item)
        {
            return _collection.Remove(item);
        }

        public bool Remove(string name)
        {
            return _collection.Remove(GetElementByName(name));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        void ICollection<T>.Add(T item)
        {
            _collection.Add(item);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            (_collection as HashSet<T>).UnionWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            (_collection as HashSet<T>).IntersectWith(other);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            (_collection as HashSet<T>).ExceptWith(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            (_collection as HashSet<T>).SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return (_collection as HashSet<T>).IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return (_collection as HashSet<T>).IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return (_collection as HashSet<T>).IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return (_collection as HashSet<T>).IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return (_collection as HashSet<T>).Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return (_collection as HashSet<T>).SetEquals(other);
        }
    }
}