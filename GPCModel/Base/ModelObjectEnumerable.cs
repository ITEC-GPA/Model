using System.Collections;
using System.Collections.Generic;

namespace GPC.Model
{
    public class ModelObjectEnumerable<T> : IEnumerable<T> where T : ModelObject
    {

        protected readonly object _locker = new object();

        protected ICollection<T> _collection;

        public ModelObjectEnumerable()
        {
            _collection = new List<T>();
        }

        /// <inheritdoc cref="ICollection{T}.Add(T)"/>
        public virtual bool Add(T item)
        {
            lock (_locker)
            {
                _collection.Add(item);
                return true; 
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_collection).GetEnumerator();
        }
    }
}
