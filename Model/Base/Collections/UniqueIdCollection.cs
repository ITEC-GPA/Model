using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Collections
{
    /// <summary>
    /// A dictionary of model objects by id: the objects without id get the largest id + 1
    /// </summary>
    /// <typeparam name="T">The type of the objects</typeparam>
    [Serializable]
    public class UniqueIdCollection<T> : Dictionary<int, T>, IEquatable<UniqueIdCollection<T>> where T : ModelObjectId, ISerializable
    {
        #region Variables

        /// <summary>
        /// The largest id assigned or added
        /// </summary>
        protected int _maxId;

        #endregion

        #region Properties

        /// <summary>
        /// The largest id assigned or added (not decreased by the removals)
        /// </summary>
        public int MaxId { get => _maxId; protected set => _maxId = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates an empty collection
        /// </summary>
        public UniqueIdCollection()
            : base()
        {
            _maxId = 0;
        }

        /// <summary>
        /// Deserialization constructor: reads the items and the largest id
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected UniqueIdCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _maxId = info.GetInt32("maxId");
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds an item. An item without id (<see cref="ModelObjectId.IDUNASSIGNED"/>) gets the largest id + 1; an item with the id of an existing
        /// one replaces it
        /// </summary>
        /// <param name="item">The item to add (its id can be changed)</param>
        /// <returns>Always true</returns>
        public bool Add(T item)
        {
            if (item.Id <= ModelObjectId.IDUNASSIGNED)
            {
                item.Id = ++_maxId;
                Add(item.Id, item);
                return true;
            }
            else
            {
                if (ContainsKey(item.Id))
                {
                    this[item.Id] = item;
                    return true;
                }

                if (item.Id > _maxId)
                    _maxId = item.Id;

                Add(item.Id, item);
                return true;
            }
        }

        /// <summary>
        /// Adds items (see <see cref="Add(T)"/>)
        /// </summary>
        /// <param name="items">The items to add</param>
        /// <returns>Always true</returns>
        public bool AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (!Add(item))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Get

        /// <summary>
        /// The item with an id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The item; null if no item has the id</returns>
        public virtual T GetById(int id)
        {
            if (ContainsKey(id))
                return this[id];
            else
                return null;
        }

        /// <summary>
        /// Tell if an item has the id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>True if the id is present</returns>
        public bool Contains(int id)
        {
            return ContainsKey(id);
        }

        #endregion

        #region Edit

        /// <summary>
        /// Removes all the items; the largest id becomes 0
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            _maxId = 0;
        }

        /// <summary>
        /// Removes the item with an id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>True if the item was removed, false if no item has the id</returns>
        public new bool Remove(int id)
        {
            if (ContainsKey(id))
            {
                if (id == _maxId)
                    _maxId = Keys.Max();
                return base.Remove(id);
            }
            return false;
        }

        /// <summary>
        /// Removes the given item (all the items equal to it, see <see cref="ModelObjectId.Equals(object)"/>). O(n)
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>True if the items were removed, false if there were none</returns>
        public bool Remove(T item)
        {
            if (ContainsValue(item))
            {
                foreach (var i in this.Where(kvp => kvp.Value == item).ToList())
                {
                    if (i.Value.Id == _maxId)
                        _maxId = Keys.Max();
                    if (!Remove(i.Key))
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes items (see <see cref="Remove(T)"/>)
        /// </summary>
        /// <param name="items">The items to remove</param>
        /// <returns>False at the first item not found (the following ones are not removed)</returns>
        public bool RemoveRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (!Remove(item))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes the items with the given ids
        /// </summary>
        /// <param name="ids">The ids</param>
        /// <returns>False at the first id not found (the following ones are not removed)</returns>
        public bool RemoveRange(IEnumerable<int> ids)
        {
            foreach (var item in ids)
            {
                if (!Remove(item))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Methos

        /// <summary>
        /// Serializes the items and the largest id
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("maxId", _maxId, typeof(int));
        }

        /// <summary>
        /// Equality of the pairs id - item, in the same order
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a collection with the same pairs</returns>
        public override bool Equals(object obj)
        {
            return obj is UniqueIdCollection<T> collection && collection.SequenceEqual(this);
        }

        /// <summary>
        /// Equality of the comparer, of the ids, of the items (in the same order) and of the largest id
        /// </summary>
        /// <param name="other">The collection to compare</param>
        /// <returns>True if the collections are equal</returns>
        public bool Equals(UniqueIdCollection<T> other)
        {
            return !(other is null) &&
                EqualityComparer<IEqualityComparer<int>>.Default.Equals(Comparer, other.Comparer) &&
                Count == other.Count &&
                Keys.SequenceEqual(other.Keys) &&
                Values.SequenceEqual(other.Values) &&
                _maxId == other._maxId;
        }

        /// <summary>
        /// The hash code of the comparer, of the count, of the key and value collections (as instances) and of the largest id
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + EqualityComparer<IEqualityComparer<int>>.Default.GetHashCode(Comparer);
                hashCode = hashCode * -17 + Count.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<KeyCollection>.Default.GetHashCode(Keys);
                hashCode = hashCode * -17 + EqualityComparer<ValueCollection>.Default.GetHashCode(Values);
                hashCode = hashCode * -17 + MaxId.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(UniqueIdCollection{T})"/>)
        /// </summary>
        /// <param name="left">The first collection</param>
        /// <param name="right">The second collection</param>
        /// <returns>True if the collections are equal</returns>
        public static bool operator ==(UniqueIdCollection<T> left, UniqueIdCollection<T> right)
        {
            return EqualityComparer<UniqueIdCollection<T>>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(UniqueIdCollection{T})"/>)
        /// </summary>
        /// <param name="left">The first collection</param>
        /// <param name="right">The second collection</param>
        /// <returns>True if the collections are different</returns>
        public static bool operator !=(UniqueIdCollection<T> left, UniqueIdCollection<T> right)
        {
            return !(left == right);
        }

        #endregion
    }
}
