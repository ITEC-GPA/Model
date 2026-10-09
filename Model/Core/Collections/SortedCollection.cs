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
    public class SortedCollection<T> : SortedDictionary<int, T>, ISerializable, IDeserializationCallback where T : ModelObjectId
    {
        #region Variables

        /// <summary>
        /// The largest id assigned or added
        /// </summary>
        protected int _lastId;
        [NonSerialized] private KeyValuePair<int, T>[] _pendingItems;
        /// <summary>Legacy v0 stored no collection entries. Restore these from an authoritative source.</summary>
        public bool MissingLegacyPayload { get; private set; }

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
        /// Reads the items and last id; records unrecoverable omissions in legacy archives.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SortedCollection(SerializationInfo info, StreamingContext context)
        {
            _lastId = info.GetInt32("LastId");
            _pendingItems = SerializationFields.Read<KeyValuePair<int, T>[]>(info, "Items");
            MissingLegacyPayload = SerializationFields.Read(info, "MissingLegacyPayload", _pendingItems == null);
        }

        #endregion

        #region Methos

        /// <summary>
        /// Serializes all entries and the last assigned id.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LastId", _lastId);
            info.AddValue("CollectionVersion", 1);
            info.AddValue("Items", this.ToArray());
            info.AddValue("MissingLegacyPayload", MissingLegacyPayload);
        }

        /// <summary>
        /// Tests registered entity identity (registry ID and persistent GUID), independently of mutable content. O(log n).
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>True if the collection contains the given item</returns>
        public bool Contains(T item)
        {
            return !(item is null) && TryGetValue(item.Id, out var existing) && existing.Guid == item.Guid;
        }

        /// <summary>
        /// Removes all items and resets the last id.
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            _lastId = 0;
        }

        /// <summary>
        /// Adds an item and returns its Id. An item without id (<see cref="ModelObjectId.IDUNASSIGNED"/>) gets the last id + 1; an item with the id
        /// of a different existing instance is rejected. Replacement must be explicit via <see cref="Replace"/>.
        /// </summary>
        /// <param name="item">The item to add (its id can be changed)</param>
        /// <returns>The item Id</returns>
        public int Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (Count > 0) _lastId = Math.Max(_lastId, Keys.Max());
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
                    if (!ReferenceEquals(this[item.Id], item))
                        throw new InvalidOperationException("DuplicateEntityId: use an explicit replacement operation.");
                    return item.Id;
                }

                if (item.Id > _lastId)
                    _lastId = item.Id;

                base.Add(item.Id, item);
                return item.Id;
            }
        }

        /// <summary>
        /// Replaces an item while preserving its registry id.
        /// </summary>
        /// <param name="id">The id of the node to replace</param>
        /// <param name="item">The new item</param>
        /// <returns>The id of the new item; <see cref="ModelObjectId.IDUNASSIGNED"/> if no item has the id</returns>
        public int Replace(int id, T item)
        {
            if (item == null || item.Id != id) throw new ArgumentException("Replacement must retain the registry ID.", nameof(item));
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
        /// Removes only the entity with the given registry ID and GUID, never other objects with equal content.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>True if the items were removed, false if there were none</returns>
        public bool Remove(T item)
        {
            return Contains(item) && Remove(item.Id);
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

        public void OnDeserialization(object sender)
        {
            if (_pendingItems == null) return; // v0 wrote no items; they cannot be recovered.
            foreach (var pair in _pendingItems) base.Add(pair.Key, pair.Value);
            _pendingItems = null;
            if (Count > 0) _lastId = Math.Max(_lastId, Keys.Max());
        }

        #endregion
    }
}
