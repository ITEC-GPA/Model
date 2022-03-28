using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Utilities.Extensions;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="Group"/> with unique name.
    /// </summary>
    /// <remarks>
    /// The collection is thread-safe
    /// <para>This class will set the <see cref="ModelObjectId.Id"/> automatically</para>
    /// </remarks>
    [Serializable]
    public class GroupCollection : UniqueNameCollection<Group>, ISerializable
    {
        protected HashSet<int> _ids;

        protected int _maxId = 0;

        public GroupCollection()
        {
            _ids = new HashSet<int>();
        }

        public GroupCollection(SerializationInfo info, StreamingContext context)
        {
            _ids = (HashSet<int>)info.GetValue("Ids", typeof(HashSet<int>));
        }

        /// <summary>
        /// If the <paramref name="item"/>.Id already exist in the collection, its ID will be replaced with the collection maximum index + 1
        /// </summary>
        /// <inheritdoc cref="UniqueNameCollection{T}.Add(T)"/>
        public override bool Add(Group item)
        {
            lock (_locker)
            {
                if (item.Id == ModelObjectId.IDUNASSIGNED || _ids.Contains(item.Id))
                {
                    item.Id = ++_maxId;
                }
                else
                {
                    if (item.Id > _maxId)
                        _maxId = item.Id;
                }
            }

            return base.Add(item);
        }

        /// <summary>
        /// If the <paramref name="items"/>.Id already exist in the collection, its ID will be replaced with the collection maximum index + 1
        /// </summary>
        /// <inheritdoc cref="UniqueNameCollection{T}.AddRange(IEnumerable{T})"/>
        public override bool AddRange(IEnumerable<Group> items)
        {
            if (items != null)
            {
                foreach (Group item in items)
                {
                    if (!Add(item))
                        return false;
                }
                return true;
            }
            return false;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Ids", _ids, typeof(HashSet<int>));
        }

        #region Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is GroupCollection collection && _collection.ScrambledEquals(collection._collection);
            }
        }


        public override int GetHashCode()
        {
            lock (_locker)
            {
                return base.GetHashCode();
            }
        }


        public static bool operator ==(GroupCollection obj1, GroupCollection obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(GroupCollection obj1, GroupCollection obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
