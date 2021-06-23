using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{
    [Serializable]
    public abstract class SortedCollection<T> : IEnumerable<T>, ISerializable where T : ModelObjectId, INotifyPropertyChanged 
    {
        protected readonly object _locker = new object();
        protected readonly List<T> _collection;
        protected int _lastId;
        protected bool _autoSort;

        /// <summary>
        /// Get the private static comparer used to sort the collection
        /// </summary>
        public abstract IComparer<T> Comparer { get; }

        /// <summary>
        /// Get an item by his id
        /// </summary>
        /// <param name="id">The Id of the item to retrieve</param>
        /// <returns>The node foud or null if it not exists</returns>
        public T this[int id] => GetById(id);

        public int Count => _collection.Count;

        public bool AutoSort
        {
            get => _autoSort;
            set
            {
                if (_autoSort == false && value == true) // Force sorting when the AutoSort is activated
                    Sort();
                _autoSort = value;
            }
        }

        public SortedCollection()
        {
            _collection = new List<T>();
            _lastId = 1;
            _autoSort = true;
        }

        public SortedCollection(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info can't be null");

            _collection = (List<T>)info.GetValue("Collection", typeof(List<T>));
            _lastId = info.GetInt32("LastId");
            _autoSort = info.GetBoolean("AutoSort");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info can't be null");
            info.AddValue("Collection", _collection, typeof(List<T>));
            info.AddValue("LastId", _lastId);
            info.AddValue("AutoSort", _autoSort);
        }

        /// <summary>
        /// Get the node index by his id
        /// </summary>
        /// <param name="id">The node Id</param>
        /// <returns></returns>
        public int GetIndexById(int id)
        {
            int pos = -1;
            Parallel.For(0, _collection.Count, (i, state) =>
            {
                if (_collection[i].Id == id)
                {
                    pos = i;
                    state.Stop();
                }
            });
            return pos;
        }

        /// <summary>
        /// Search an item by his value (the id is not considered).
        /// </summary>
        /// <param name="item">The item to search</param>
        /// <returns>The index of the item if found, or a negative value if the item does not exist</returns>
        protected int GetIndexByValue(T item)
        {
            return _collection.BinarySearch(0, 1, item, Comparer);
        }

        /// <summary>
        /// Get an item by his Id.
        /// </summary>
        /// <param name="id">The id of the item to get</param>
        /// <returns>The item or null if not exists</returns>
        public T GetById(int id)
        {
            int pos = GetIndexById(id);
            if (pos >= 0)
                return _collection[pos];
            return null;
        }

        /// <summary>
        /// Get an item by his index. 
        /// </summary>
        /// <param name="index">The node index</param>
        /// <returns>The note or null if out of range</returns>
        public T GetByIndex(int index)
        {
            if (index > _collection.Count - 1)
                return null;
            return _collection[index];
        }

        /// <summary>
        /// Tell if an item exists or not (the id is not considered)
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>True if the collection contains the given item</returns>
        public bool Contains(T item)
        {
            return _collection.BinarySearch(item, Comparer) >= 0;
        }

        /// <summary>
        /// Reorder the collection based on the items value
        /// </summary>
        public void Sort()
        {
            lock (_locker)
            {
                T[] items = _collection.ToArray();
                Array.Sort(items, Comparer);
                _collection.Clear();
                _collection.AddRange(items);
            }
        }

        /// <summary>
        /// Remove all the items from the collection
        /// </summary>
        public void Clear()
        {
            lock (_locker)
            {
                _collection.Clear();
            }
        }

        /// <summary>
        /// Copy the ordered collection in the given array
        /// </summary>
        /// <param name="array">The array to fill with the nodes of the collection</param>
        /// <param name="arrayIndex">The 0 based index where to start the copy</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                _collection.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Returns the inxed of an item
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The index of the node or -1 if not exist</returns>
        public int IndexOf(T item)
        {
            lock (_locker)
            {
                int pos = _collection.BinarySearch(item, Comparer);
                return pos >= 0 ? pos : -1;
            }
        }

        /// <summary>
        /// Adds a new item and returns the new Id. If the item already exists return his Id
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>The item Id</returns>
        public int Add(T item)
        {
            lock (_locker)
            {
                if (AutoSort == false) // When a new node is added force AutoSort enabled
                    AutoSort = true;

                int pos = _collection.BinarySearch(item, Comparer);
                if (pos < 0) // New not existing item
                {
                    item.Id = _lastId++;

                    if (pos == -_collection.Count - 1)
                        _collection.Add(item); // Append to the end of the collection
                    else
                        _collection.Insert(-pos - 1, item); // Insert inside to keep the collection ordered

                    item.PropertyChanged += OnItemChanged;

                    return item.Id;
                }
                return _collection[pos].Id;
            }
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_autoSort)
                return;
            if (sender is T item)
            {
                bool needToMove = false;
                int currPos = _collection.IndexOf(item);
                if (currPos > 0 && Comparer.Compare(_collection[currPos - 1], item) >= 0)
                    needToMove = true;
                if (currPos < _collection.Count - 1 && Comparer.Compare(_collection[currPos + 1], item) <= 0)
                    needToMove = true;
                if (needToMove)
                {
                    _collection.RemoveAt(currPos);
                    int newPos = _collection.BinarySearch(item, Comparer);
                    if (newPos == -_collection.Count - 1)
                        _collection.Add(item); // Append to the end of the collection
                    else
                        _collection.Insert(-newPos - 1, item); // Insert inside to keep the collection ordered           
                }
            }
        }

        /// <summary>
        /// Replace an item with another and reposition it basing on its value
        /// </summary>
        /// <param name="id">The id of the node to replace</param>
        /// <param name="item">The new item</param>
        /// <returns>The new item index</returns>
        public int Replace(int id, T item)
        {
            int i = GetIndexById(id);
            _collection.RemoveAt(i);
            int newPos = _collection.BinarySearch(item, Comparer);
            item.Id = id;
            if (newPos == -_collection.Count - 1)
            {
                _collection.Add(item); // Append to the end of the collection
                return _collection.Count - 1;
            }
            else
            {
                _collection.Insert(-newPos - 1, item); // Insert inside to keep the collection ordered
                return -newPos - 1;
            }
        }

        /// <summary>
        /// Remove the given item from the collection
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>True id success</returns>
        public bool Remove(T item)
        {
            lock (_locker)
            {
                int pos = _collection.BinarySearch(item, Comparer);
                if (pos >= 0)
                {
                    _collection.RemoveAt(pos);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Remove the item at the given index
        /// </summary>
        /// <param name="index">The index of the node to remove</param>
        public void RemoveAt(int index)
        {
            lock (_locker)
            {
                _collection.RemoveAt(index);
            }
        }

        /// <summary>
        /// Remove an item by his id
        /// </summary>
        /// <param name="id">The id of the item to remove</param>
        /// <returns>True id success</returns>
        public bool RemoveById(int id)
        {
            lock (_locker)
            {
                int pos = GetIndexById(id);
                if (pos >= 0)
                {
                    _collection.RemoveAt(pos);
                    return true;
                }
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public int AddUnique(T item)
        {
            return Add(item);
        }

        public T GetElementById(int id)
        {
            return GetById(id);
        }
    }
}
