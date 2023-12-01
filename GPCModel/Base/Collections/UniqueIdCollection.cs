using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Collections
{
    [Serializable]
    public class UniqueIdCollection<T> : Dictionary<int, T> where T : ModelObjectId, ISerializable
    {
        #region Variables

        protected int _maxId;

        #endregion

        #region Properties

        public int MaxId { get => _maxId; protected set => _maxId = value; }

        #endregion

        #region Constructor

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

        #endregion

        #region Add

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

        #endregion

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

        #region Methos

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("maxId", _maxId, typeof(int));
        }

        public override bool Equals(object obj)
        {
            return obj is UniqueIdCollection<T> collection && collection.SequenceEqual(this);
        }

        #endregion
    }
}
