using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model
{
    [Serializable]
    public class SortedCollection2<T> : SortedDictionary<int, T>, ISerializable where T : ModelObjectId
    {
        protected int _lastId;

        public SortedCollection2()
        {
            _lastId = 0;
        }

        protected SortedCollection2(SerializationInfo info, StreamingContext context)
        {
            _lastId = info.GetInt32("LastId");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LastId", _lastId);
        }

        /// <summary>
        /// Tell if an item exists or not (the id is not considered)
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>True if the collection contains the given item</returns>
        public bool Contains(T item)
        {
            return ContainsValue(item);
        }

        /// <summary>
        /// Remove all the items from the collection
        /// </summary>
        public new void Clear()
        {
            Clear();
            _lastId = 0;
        }

        /// <summary>
        /// Adds a new item and returns the new Id. If the item already exists return his Id
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The item Id</returns>
        public int Add(T item)
        {
            if (item.Id <= ModelObjectId.IDUNASSIGNED)
            {
                item.Id = ++_lastId;
                base.Add(item.Id, item);
                return item.Id;
            }
            else
            {
                if (ContainsKey(item.Id))
                {
                    this[item.Id] = item;
                    return item.Id;
                }

                if (item.Id > _lastId)
                    _lastId = item.Id;

                base.Add(item.Id, item);
                return item.Id;
            }
        }

        /// <summary>
        /// Replace an item with another and reposition it basing on its value
        /// </summary>
        /// <param name="id">The id of the node to replace</param>
        /// <param name="item">The new item</param>
        /// <returns>The new item index</returns>
        public int Replace(int id, T item)
        {
            if (ContainsKey(item.Id))
            {
                this[item.Id] = item;
                return item.Id;
            }
            else
            {
                return ModelObjectId.IDUNASSIGNED;
            }
        }

        /// <summary>
        /// Remove the given item from the collection
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>True id success</returns>
        public bool Remove(T item)
        {
            if (ContainsValue(item))
            {
                foreach (var i in this.Where(kvp => kvp.Value == item).ToList())
                {
                    if (i.Value.Id == _lastId)
                        _lastId = Keys.Max();
                    if (!Remove(i.Key))
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove an item by its id
        /// </summary>
        /// <param name="id">The id of the item to remove</param>
        /// <returns>True id success</returns>
        public bool RemoveById(int id)
        {
            return Remove(id);
        }
    }
}
