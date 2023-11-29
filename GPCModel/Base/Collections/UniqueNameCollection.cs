using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model
{
    /// <summary>
    /// Collection of <see cref="ModelObject"/> with unique name. This class use an <see cref="HashSet{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>The collection is thread-safe</remarks>
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

        /// <summary><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></summary>
        /// <returns><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></returns>
        /// <exception cref="InvalidOperationException" ></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <remarks>This is a O(n) operation</remarks>
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
