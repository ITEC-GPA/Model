using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model
{

    /// <remarks>The collection is thread-safe</remarks>
    [Serializable]
    public abstract class ModelObjectEnumerable<T> : IEnumerable<T> where T : ModelObject, ISerializable
    {

        protected readonly object _locker = new object();

        protected ICollection<T> _collection;

        public ModelObjectEnumerable()
        {
            _collection = new List<T>();
        }

        public ModelObjectEnumerable(SerializationInfo info, StreamingContext context)
        {
            _collection = (ICollection<T>)info.GetValue("Collection", typeof(ICollection<T>));
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

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Collection", _collection);
        }

    }
}
