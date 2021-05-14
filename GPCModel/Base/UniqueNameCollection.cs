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
        private readonly HashSet<string> _names;


        public UniqueNameCollection()
        {
            _collection = new HashSet<T>(new ModelObject.ModelObjectNameEqualityComparer());
            _names = new HashSet<string>();
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
            _names.Add(item.Name);
            return true;
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

        /// <inheritdoc cref="AddRange(IEnumerable{T})"/>
        public virtual bool AddRange(T[] items)
        {
            return this.AddRange(items.ToList());
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


            if (this.Contains(name))
                return _collection.SingleOrDefault(i => i.Name == name);
            else
                throw new KeyNotFoundException($"Collection does not contain a element with name: {name}");
        }

        /// <returns>A list of all element names of this collection</returns>
        public virtual List<string> GetNames()
        {
            return _names.ToList();
        }

        public void Clear()
        {
            _collection.Clear();
            _names.Clear();
        }

        /// <inheritdoc cref="Contains(string)"/>
        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }


        /// <returns><see langword="True" /> if this collection contains an element with <see cref="ModelObject.Name"/> equals to <paramref name="name"/> </returns>
        public bool Contains(string name)
        {
            return _names.Contains(name);
        }

        /// <remarks>This is a O(1) operation</remarks>
        public bool Remove(T item)
        {
            return _collection.Remove(item) && _names.Remove(item.Name) ;
        }

        public bool Remove(string name)
        {
            return _collection.Remove(GetElementByName(name)) && _names.Remove(name); ;
        }


        void ICollection<T>.Add(T item)
        {
            // metodo privato
            this.Add(item);
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

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }
    }
}
