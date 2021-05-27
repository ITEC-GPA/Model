using System;
using System.Collections.Generic;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="Group"/> with unique name.
    /// </summary>
    /// <remarks>
    /// The collection is thread-safe
    /// <para>This class will set the <see cref="ModelObjectId.Id"/> automatically</para>
    /// </remarks>
    public class GroupCollection : UniqueNameCollection<Group>
    {

        protected HashSet<int> _ids;

        protected int _maxId = 0;

        public GroupCollection()
        {
            _ids = new HashSet<int>();
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


    }
}
