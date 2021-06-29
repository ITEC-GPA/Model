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
    public abstract class HashCollection<T> : IEnumerable<T> where T : ModelObjectId, INotifyPropertyChanged
    {
        protected readonly object _locker = new object();

        protected readonly List<T> _collection; // Questa lista è ordinata per Id
        protected readonly Dictionary<int, List<int>> _hashMap;
        protected int _lastId;

        /// <summary>
        /// Get an item by his id
        /// </summary>
        /// <param name="id">The Id of the item to retrieve</param>
        /// <returns>The node foud or null if it not exists</returns>
        public T this[int id] => GetById(id);

        public int Count => _collection.Count;

        public HashCollection()
        {
            _collection = new List<T>();
            _hashMap = new Dictionary<int, List<int>>();
            _lastId = 1;
        }

        protected abstract int GetItemHashCode(T item);


        /// <summary>
        /// Get the item index by his id. Since the list is ID ordered, implements the non-recursive binary search algorithm.
        /// </summary>
        /// <param name="id">The item index or -1 if not found</param>
        public int GetIndexById(int id)
        {
            int first = 0;
            int last = _collection.Count - 1;
            while (first <= last)
            {
                int mid = (first + last) / 2;
                if (_collection[mid].Id == id)
                    return mid;
                if (_collection[mid].Id < id)
                    first = mid + 1;
                else
                    last = mid - 1;
            }

            return -1;
        }

        /// <summary>
        /// Search an item by his value (the id is not considered).
        /// </summary>
        /// <param name="item">The item to search</param>
        /// <returns>The index of the item if found, or -1 if the item does not exist</returns>
        protected int GetIndexByValue(T item)
        {
            if (_hashMap.TryGetValue(GetItemHashCode(item), out List<int> values))
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (_collection[values[i]].Equals(item))
                        return values[i]; // Returns the index of the item
                }
            }

            return -1;
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
            if (index > _collection.Count - 1 || index < 0)
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

            if (_hashMap.TryGetValue(GetItemHashCode(item), out List<int> values))
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (_collection[values[i]].Equals(item))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Recompute the hash map of the collection
        /// </summary>
        public void UpdateHashes()
        {
            lock (_locker)
            {
                _hashMap.Clear();
                for(int i = 0; i < _collection.Count(); i++)
                {
                    var hash = GetItemHashCode(_collection[i]);

                    if (_hashMap.ContainsKey(hash))
                    {
                        _hashMap[hash].Add(i);
                    }
                    else
                    {
                        _hashMap[hash] = new List<int>() { i };
                    }
                }
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
                _hashMap.Clear();
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
            return GetIndexByValue(item);
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
                var hash = GetItemHashCode(item);

                bool hashAlreadyExisting = false;
                if (_hashMap.TryGetValue(hash, out List<int> indexes))
                {
                    for (int i = 0; i < indexes.Count; i++)
                    {
                        if (_collection[indexes[i]].Equals(item))
                        {
                            return _collection[i].Id;
                        }
                    }

                    hashAlreadyExisting = true;
                }

                item.Id = _lastId++;
                item.PropertyChanged += OnItemChanged;
                _collection.Add(item);

                if (!hashAlreadyExisting)
                    _hashMap.Add(hash, new List<int>() { _collection.Count - 1 });
                else
                    _hashMap[hash].Add(_collection.Count - 1);

                return item.Id;
            }
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is T item)
            {
                // qua si può ottimizzare andando ad aggiornare solo hash relativi alla modifica.
                // per ora aggiorniamo tutto
                UpdateHashes();
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
            lock (_locker)
            {
                int i = GetIndexById(id);
                _collection.RemoveAt(i);
                _hashMap[GetItemHashCode(item)].Remove(i);

                return GetIndexById(Add(item)); 
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
                int hash = GetItemHashCode(item);
                if (!_hashMap.ContainsKey(hash))
                    return false;

                int i = _collection.IndexOf(item);

                if (_hashMap[hash].Count > 1)
                    _hashMap[hash].Remove(i);
                else
                    _hashMap.Remove(hash);
                return _collection.Remove(item);
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
                int hash = GetItemHashCode(_collection[index]);
                if (_hashMap[hash].Count > 1)
                    _hashMap[hash].Remove(index);
                else
                    _hashMap.Remove(hash);
                _collection.RemoveAt(index);
            }
        }

        /// <summary>
        /// Remove an item by its id
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
                    int hash = GetItemHashCode(_collection[pos]);
                    if (_hashMap[hash].Count > 1)
                        _hashMap[hash].Remove(pos);
                    else
                        _hashMap.Remove(hash);

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
    }
}
