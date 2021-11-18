using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Base
{

    public class KeyValuePairHashedCollection<T, D> where T : ModelObjectId, INotifyPropertyChanged where D : class, ISerializable
    {
        
        protected readonly object _locker = new object();

        protected readonly List<KeyValuePair<T, D>> _collection;
        protected readonly IDictionary<int, List<int>> _hashMap;

        public int Count => _collection.Count();


        public KeyValuePairHashedCollection()
        {
            _collection = new List<KeyValuePair<T, D>>();
            _hashMap = new Dictionary<int, List<int>>();
        }


        #region Private method

        /// <summary>
        /// Check if an item with key = <paramref name="key"/> and add the pair. If already exist it will be replaced 
        /// </summary>
        protected virtual void AddKeyValuePair(T key, D value)
        {
            lock (_locker)
            {
                var el = _collection.Where(i => i.Key.Equals(key)).SingleOrDefault();

                // Item non esiste
                if (el.Equals(default(KeyValuePair<T, D>)))
                {
                    _collection.Add(new KeyValuePair<T, D>(key, value));
                }
                else
                {
                    // Item già presente, sostituisco
                    _collection.Remove(el);
                    _collection.Add(new KeyValuePair<T, D>(el.Key, value));
                }
            }
        }

        protected bool CollectionsIndexKeyEquals(int index, T key)
        {
            return _collection[index].Key.Equals(key);
        }

        #endregion


        #region Public - Add

        /// <summary>
        /// Adds a new item and returns the Id. If the item already exists return his Id
        /// </summary>
        /// <param name="key">The item to add</param>
        /// <param name="value"></param>
        /// <returns>The item Id</returns>
        public int Add(T key, D value)
        {
            lock (_locker)
            {
                var hash = GetItemHashCode(key);

                bool hashAlreadyExisting = false;
                if (_hashMap.TryGetValue(hash, out List<int> indexes))
                {
                    hashAlreadyExisting = true; 
                    
                    int elementIdToReturn = int.MinValue;
                    bool returnControl = false;

                    for (int i = 0; i < indexes.Count; i++)
                    {
                        if (CollectionsIndexKeyEquals(indexes[i], key))
                        {
                            _collection[indexes[i]] = new KeyValuePair<T, D>(key, value);
                            
                            if (!returnControl)
                            {
                                elementIdToReturn = key.Id; // se l'elemento esiste già ritorna l'id del primo elemento con quel hash
                                returnControl = true;
                            }
                        }
                        // Se non entra nell'if vuol dire che due oggetti hanno lo stesso Hash ma non sono uguali,
                        // può succedere in quanto hascode uguali non garantiscono ugualianza
                    }

                    if (returnControl)
                        return elementIdToReturn;
                }

                key.PropertyChanged += OnItemChanged;
                _collection.Add(new KeyValuePair<T, D>(key, value));

                if (!hashAlreadyExisting)
                {
                    _hashMap.Add(hash, new List<int>() { _collection.Count - 1 });
                }
                else
                {
                    _hashMap[hash].Add(_collection.Count - 1);
                }

                return key.Id;
            }
        }

        #endregion

        #region Public - Get

        /// <summary>
        /// Get the value associated to <paramref name="key"/>
        /// </summary>
        /// <returns>The property associated to <paramref name="key"/></returns>
        public D GetValue(T key)
        {

            lock (_locker)
            {
                var hash = GetItemHashCode(key);

                if (_hashMap.TryGetValue(hash, out List<int> indexes))
                {

                    for (int i = 0; i < indexes.Count; i++)
                    {
                        if (CollectionsIndexKeyEquals(indexes[i], key))
                        {
                            return _collection[indexes[i]].Value;
                        }
                    }
                }

                return null;
            }
        }

        #endregion

        #region Public - Contains


        /// <summary>
        /// Tell if an item exists or not
        /// </summary>
        /// <param name="key">The item to check</param>
        /// <returns>True if the collection contains the given item</returns>
        public bool Contains(T key)
        {

            if (_hashMap.TryGetValue(GetItemHashCode(key), out List<int> values))
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (CollectionsIndexKeyEquals(values[i], key))
                        return true;
                }
            }

            return false;
        }


        #endregion

        #region Public - Remove / Replace

        /// <summary>
        /// Replace an item with another
        /// </summary>
        /// <returns>The new item index</returns>
        public int[] Replace(T key, D value)
        {
            lock (_locker)
            {
                int hash = GetItemHashCode(key);

                if (_hashMap.TryGetValue(hash, out List<int> indexes))
                {
                    int[] ids = new int[indexes.Count];

                    for (int i = 0; i < indexes.Count; i++)
                    {
                        _collection.RemoveAt(i);
                        _collection[i] = new KeyValuePair<T, D>(key, value);
                        ids[i] = i;
                    }

                    return ids;
                }
                else
                {
                    return new int[] { -1 };
                }
            }
        }

        /// <summary>
        /// Remove the given item from the collection
        /// </summary>
        /// <param name="key">The item to remove</param>
        /// <returns>True id success</returns>
        public bool Remove(T key)
        {
            lock (_locker)
            {
                int hash = GetItemHashCode(key);
                if (!_hashMap.ContainsKey(hash))
                    return false;

                int i = IndexOf(key);

                if (i == -1)
                    return false;

                if (_hashMap[hash].Count > 1)
                    _hashMap[hash].Remove(i);
                else
                    _hashMap.Remove(hash);

                _collection.RemoveAt(i);
                return true;
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
                int hash = GetItemHashCode(_collection[index].Key);

                if (_hashMap[hash].Count > 1)
                    _hashMap[hash].Remove(index);
                else
                    _hashMap.Remove(hash);

                _collection.RemoveAt(index);
            }
        }

        #endregion

        #region Public - Collection managment 

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
        /// Returns the inxed of an item
        /// </summary>
        /// <param name="key">The item</param>
        /// <returns>The index of the node or -1 if not exist</returns>
        public int IndexOf(T key)
        {
            return GetIndexByValue(key);
        }

        /// <summary>
        /// Search an item by his value (the id is not considered).
        /// </summary>
        /// <param name="key">The item to search</param>
        /// <returns>The index of the item if found, or -1 if the item does not exist</returns>
        protected int GetIndexByValue(T key)
        {
            if (_hashMap.TryGetValue(GetItemHashCode(key), out List<int> values))
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (CollectionsIndexKeyEquals(values[i], key))
                        return values[i]; // Returns the index of the item
                }
            }

            return -1;
        }

        #endregion

        #region Hash management


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
        /// Calculate the element hash code
        /// </summary>
        protected virtual int GetItemHashCode(T key)
        {
            return key.GetHashCode();
        }

        /// <summary>
        /// Recompute the hash map of the collection
        /// </summary>
        public void UpdateHashes()
        {
            lock (_locker)
            {
                _hashMap.Clear();
                for (int i = 0; i < _collection.Count(); i++)
                {
                    var hash = GetItemHashCode(_collection[i].Key);

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
        #endregion
    }
}
