using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Collections
{
    [Serializable]
    public class UniqueNameCollection<T> : Dictionary<string, T> where T : ModelObject, ISerializable
    {
        #region Constructor

        public UniqueNameCollection()
        {

        }

        protected UniqueNameCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Methods

        public void Add(T item)
        {
            Add(item.Name, item);
        }

        public bool AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item.Name, item);
                }
                return true;
            }
            return false;
        }

        public virtual T GetElementByName(string name)
        {
            if (ContainsKey(name))
                return this[name];
            else
                return null;
        }

        public List<string> GetNames()
        {
            return Keys.ToList();
        }

        #endregion
    }
}
