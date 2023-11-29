using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model
{

    /// <summary>
    /// Collection of <see cref="ModelObjectId"/> with unique id. This class use an <see cref="HashSet{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>The collection is thread-safe
    /// <para>Id of <typeparamref name="T"/> must be unmutable</para></remarks>
    [Serializable]
    public class UniqueIdCollection<T> : Dictionary<int, T> where T : ModelObjectId, ISerializable
    {
        protected int _maxId;

        public int MaxId { get => _maxId; protected set => _maxId = value; }

        public UniqueIdCollection()
            : base()
        {
            _maxId = 0;
        }


        protected UniqueIdCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _maxId = info.GetInt32("maxId");
        }


        #region Add

        /// <inheritdoc cref="ModelObjectEnumerable{T}.Add(T)" />
        /// <returns><see langword="True"/> if the element has been added
        /// <para><see langword="False"/> if the element has not been added</para>
        /// </returns>
        /// <remarks>This is a O(1) operation
        /// <para> To get the element in the collection use <see cref="GetItem(T, out T)"/> </para>
        /// <para> If an element with same id already exist, <paramref name="item"/> will replace this item</para></remarks>
        public bool Add(T item)
        {
            if (item.Id <= ModelObjectId.IDUNASSIGNED)
            {
                item.Id = ++_maxId;
                Add(item);
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

                Add(item);
                return true;
            }
        }

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


        /// <summary><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></summary>
        /// <returns><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></returns>
        /// <exception cref="InvalidOperationException" ></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <remarks>This is a O(n) operation</remarks>
        public virtual T GetById(int id)
        {
            if (ContainsKey(id))
                return this[id];
            else
                return null;
        }

        public bool Contains(int id)
        {
            return ContainsKey(id);
        }

        #region Edit

        public new void Clear()
        {
            Clear();
            _maxId = 0;
        }

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("maxId", _maxId, typeof(int));
        }
    }
}
