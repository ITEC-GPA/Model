using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Extensions;

namespace GPC.Model.Base
{

    [Serializable]
    public class KeyValuePairCollection<T, D> : IEnumerable<KeyValuePair<T, D>>, ISerializable where T : ModelObjectId where D : class
    {

        protected readonly object _locker = new object();


        protected ICollection<KeyValuePair<T, D>> _collection;

        public int Count => _collection.Count();

        public KeyValuePairCollection()
        {
            _collection = new List<KeyValuePair<T, D>>();
        }

        #region Private method

        /// <summary>
        /// Check if an item with key = <paramref name="key"/> and add the pair. If already exist it will be replaced 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
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


        #endregion

        #region Public method - Add

        /// <inheritdoc cref="AddKeyValuePair(T, D)"/>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void AddUnique(T key, D value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(T));
            }
            else
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(D));
                }
            }


            AddKeyValuePair(key, value);
        }

        /// <summary>
        /// Add the pair without checking if the <paramref name="key"/> already exist
        /// </summary>
        /// <remarks>Null checks not performed</remarks>
        public virtual void Add(T key, D value)
        {
            _collection.Add(new KeyValuePair<T, D>(key, value));
        }


        /// <summary>
        /// Set the <paramref name="value"/> of <paramref name="key"/>
        /// </summary>
        /// <returns><see langword="false"/> if the <paramref name="key"/> does not exist in the collection </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> or <paramref name="value"/> is null</exception>
        /// <remarks>This is an O(2n) operation         
        /// <para>The <paramref name="value"/> will ovveride the existing one if present</para>
        /// </remarks>
        public virtual bool SetValue(T key, D value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(T));
            }
            else
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(D));
                }
            }


            lock (_locker)
            {
                KeyValuePair<T, D> el = _collection.Where(i => i.Key.Equals(key)).SingleOrDefault();

                if (el.Equals(default(KeyValuePair<T, D>)))
                {
                    // Elemento non presente
                    return false;
                }
                else
                {
                    _collection.Remove(el);

                    var kvp = new KeyValuePair<T, D>(el.Key, value);
                    _collection.Add(kvp);
                    return true;
                }
            }
        }

        #endregion

        #region Public method - Get


        /// <summary>
        /// Get the value associated to <paramref name="key"/>
        /// </summary>
        /// <remarks>This is a O(n) operations</remarks>
        /// <returns>The property associated to <paramref name="key"/></returns>
        public D GetValue(T key)
        {

            lock (_locker)
            {
                var el = _collection.Where(i => i.Key.Equals(key)).SingleOrDefault();

                if (el.Equals(default(KeyValuePair<T, D>)))
                {
                    // Item non esiste
                    return null;
                }
                else
                {
                    return el.Value;
                }
            }
        }


        IEnumerator<KeyValuePair<T, D>> IEnumerable<KeyValuePair<T, D>>.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        #endregion

        #region Public method - Edit

        /// <remarks>This is a O(n^2) operations</remarks>
        public bool Remove(T key)
        {
            lock (_locker)
            {
                return _collection.Remove(_collection.SingleOrDefault(i => i.Key.Equals(key)));
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _collection.Clear();
            }
        }

        #endregion 

        #region Equals - hashcode - Operators


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _collection = (ICollection<KeyValuePair<T, D>>)info.GetValue("Collection", typeof(ICollection<KeyValuePair<T, D>>));
        }

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is KeyValuePairCollection<T, D> collection
                                    && _collection.ScrambledEquals(collection._collection);
            }
        }

        public override int GetHashCode()
        {
            lock (_locker)
            {
                unchecked
                {
                    int hashCode = -391 + base.GetHashCode();

                    foreach (KeyValuePair<T, D> element in _collection)
                    {
                        hashCode += element.Key.GetHashCode();
                        hashCode += element.Value.GetHashCode();
                    }

                    return hashCode;
                }
            }
        }

        public static bool operator ==(KeyValuePairCollection<T, D> obj1, KeyValuePairCollection<T, D> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(KeyValuePairCollection<T, D> obj1, KeyValuePairCollection<T, D> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion 

    }
}
