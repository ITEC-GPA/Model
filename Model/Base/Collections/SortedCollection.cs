using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Collections
{
    /// <summary>
    /// A collection of model objects sorted by id: the objects without id get the last id + 1
    /// </summary>
    /// <typeparam name="T">The type of the objects</typeparam>
    [Serializable]
    public class SortedCollection<T> : SortedDictionary<int, T>, ISerializable where T : ModelObjectId
    {
        #region Variables

        /// <summary>
        /// The largest id assigned or added
        /// </summary>
        protected int _lastId;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates an empty collection
        /// </summary>
        public SortedCollection()
        {
            _lastId = 0;
        }

        /// <summary>
        /// Deserialization constructor: reads only the last id (the items are not serialized by <see cref="GetObjectData"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SortedCollection(SerializationInfo info, StreamingContext context)
        {
            _lastId = info.GetInt32("LastId");
        }

        #endregion

        #region Methos

        /// <summary>
        /// Serializes the last id (the items are not serialized)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LastId", _lastId);
        }

        /// <summary>
        /// Tell if an item exists or not (the id is not considered: the items are compared with their Equals, i.e. by name). O(n)
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>True if the collection contains the given item</returns>
        public bool Contains(T item)
        {
            return ContainsValue(item);
        }

        /// <summary>
        /// Remove all the items from the collection and resets the last id (the method calls itself: it throws <see cref="StackOverflowException"/>)
        /// </summary>
        public new void Clear()
        {
            Clear();
            _lastId = 0;
        }

        /// <summary>
        /// Adds an item and returns its Id. An item without id (<see cref="ModelObjectId.IDUNASSIGNED"/>) gets the last id + 1; an item with the id
        /// of an existing one replaces it
        /// </summary>
        /// <param name="item">The item to add (its id can be changed)</param>
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
        /// Replace the item with a given id (the key does not change, also if the new item has another id)
        /// </summary>
        /// <param name="id">The id of the node to replace</param>
        /// <param name="item">The new item</param>
        /// <returns>The id of the new item; <see cref="ModelObjectId.IDUNASSIGNED"/> if no item has the id</returns>
        public int Replace(int id, T item)
        {
            if (ContainsKey(id))
            {
                this[id] = item;
                return item.Id;
            }
            else
            {
                return ModelObjectId.IDUNASSIGNED;
            }
        }

        /// <summary>
        /// Remove the given item (all the items equal to it) from the collection
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>True if the items were removed, false if there were none</returns>
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
        /// <returns>True if the item was removed, false if no item has the id</returns>
        public bool RemoveById(int id)
        {
            return Remove(id);
        }

        #endregion
    }
}
