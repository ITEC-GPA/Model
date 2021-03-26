using System.Collections;
using System.Collections.Generic;
using System;

namespace GPC.Model
{
    public class ModelObjectEnumerable<T> : IEnumerable<T> where T : ModelObject
    {
        protected ICollection<T> _collection;

        public ModelObjectEnumerable()
        {
            _collection = new List<T>();
        }

        public virtual bool Add(T item)
        {
            _collection.Add(item);
            return true;
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