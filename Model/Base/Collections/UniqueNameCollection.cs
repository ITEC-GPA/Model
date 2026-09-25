using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Collections
{
    /// <summary>
    /// A dictionary of model objects by name (the names must be unique)
    /// </summary>
    /// <typeparam name="T">The type of the objects</typeparam>
    [Serializable]
    public class UniqueNameCollection<T> : Dictionary<string, T> where T : ModelObject, ISerializable
    {
        #region Constructor

        /// <summary>
        /// Creates an empty collection
        /// </summary>
        public UniqueNameCollection()
        {

        }

        /// <summary>
        /// Deserialization constructor (see <see cref="Dictionary{TKey, TValue}"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected UniqueNameCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an item with its name as key
        /// </summary>
        /// <param name="item">The item</param>
        /// <exception cref="ArgumentException">If an item with the same name exists</exception>
        public void Add(T item)
        {
            Add(item.Name, item);
        }

        /// <summary>
        /// Adds items with their names as keys
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>False if <paramref name="items"/> is null</returns>
        /// <exception cref="ArgumentException">If an item with the same name exists</exception>
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

        /// <summary>
        /// The item with a name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The item; null if no item has the name</returns>
        public virtual T GetElementByName(string name)
        {
            if (ContainsKey(name))
                return this[name];
            else
                return null;
        }

        /// <summary>
        /// The names of the items
        /// </summary>
        /// <returns>A new list with the names</returns>
        public List<string> GetNames()
        {
            return Keys.ToList();
        }

        #endregion
    }
}
