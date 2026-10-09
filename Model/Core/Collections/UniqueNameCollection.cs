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
    public class UniqueNameCollection<T> : Dictionary<string, T>, INameIndex where T : ModelObject, ISerializable
    {
        [field: NonSerialized]
        internal event Action<T, string, string> ItemRenamed;
        [field: NonSerialized]
        internal event Action<T, string> ItemRenaming;

        #region Constructor

        /// <summary>
        /// Creates an empty collection
        /// </summary>
        public UniqueNameCollection()
        {
            NameIndexRegistry.Register(this);
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="Dictionary{TKey, TValue}"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected UniqueNameCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            NameIndexRegistry.Register(this);
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
                var values = items.ToArray();
                var names = new HashSet<string>(Keys, Comparer);
                foreach (var item in values)
                {
                    if (item is null || item.Name == null) throw new ArgumentException("Named items are required.", nameof(items));
                    if (!names.Add(item.Name)) throw new ArgumentException("Duplicate item name: " + item.Name, nameof(items));
                }
                foreach (var item in values)
                {
                    Add(item.Name, item);
                }
                return true;
            }
            return false;
        }

        /// <summary>Renames the actual entity and every registered name index that contains it.</summary>
        public void Rename(string name, string newName) => this[name].Name = newName;

        NameIndexChange INameIndex.PrepareRename(ModelObject item, string name)
        {
            if (!(item is T typed)) return null;
            // Reference identity is essential: legacy Equals can equate different entities with the same name.
            var keys = this.Where(p => ReferenceEquals(p.Value, typed)).Select(p => p.Key).ToArray();
            if (keys.Length == 0) return null;
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (keys.Length != 1) throw new InvalidOperationException("AmbiguousNameIndex: the entity has multiple keys.");
            if (TryGetValue(name, out var existing) && !ReferenceEquals(existing, typed))
                throw new ArgumentException("Duplicate item name: " + name, nameof(name));
            ItemRenaming?.Invoke(typed, name);
            string oldName = item.Name;
            return new NameIndexChange
            {
                Apply = () => { base.Remove(keys[0]); base.Add(name, typed); },
                Notify = () => ItemRenamed?.Invoke(typed, oldName, name)
            };
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
