using System.Collections;
using System.Collections.Generic;

namespace GPC.Model
{
    /// <summary>
    /// Collection of unique ModelObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelObjectCollection<T> : ISet<T> where T : ModelObject
    {
        protected ISet<T> _set;

        /// <summary>
        /// Inizialize the <see cref="ModelObjectCollection{T}"/> with a custom equality comparer
        /// </summary>
        public ModelObjectCollection(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        /// <summary>
        /// Inizialize the <see cref="ModelObjectCollection{T}"/> with the default equality comparer of <typeparamref name="T"/>
        /// </summary>
        public ModelObjectCollection()
        {
            _set = new HashSet<T>();
        }

        public virtual int Count => _set.Count;

        public virtual bool IsReadOnly => _set.IsReadOnly;

        public virtual void Add(T item)
        {
            _set.Add(item);
        }

        public virtual void Clear()
        {
            _set.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        public virtual bool Remove(T item)
        {
            return _set.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_set).GetEnumerator();
        }

        bool ISet<T>.Add(T item)
        {
            return _set.Add(item);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _set.UnionWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _set.IntersectWith(other);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _set.ExceptWith(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _set.SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }
    }
}