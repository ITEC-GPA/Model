using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Fem.FemObjects;
using GPC.Utilities.Extensions;

namespace GPC.Model.Fem.Collections
{
    /// <summary>
    /// This class rapresent an association between a <see cref="FemObject"/> and a property override <see cref="Stage.StageProperty"/>
    /// </summary>
    /// <remarks>This should be accessed only from the class <see cref="Stage"/> since it does not implement any check on the element duplicates</remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <remarks>The collection is thread-safe</remarks>
    [Serializable]
    public abstract class FemObjectStageCollection<T, D> where T : FemObject where D : Stage.StageProperty, ISerializable
    {
        protected readonly object _locker = new object();

        /// <summary>
        /// Association between an element and Stage.StageProperty of that element
        /// </summary>
        protected List<KeyValuePair<T, D>> _stageFiniteElementProperty;

        public int Count => _stageFiniteElementProperty.Count();

        public FemObjectStageCollection()
        {
            _stageFiniteElementProperty = new List<KeyValuePair<T, D>>();
        }


        public FemObjectStageCollection(SerializationInfo info, StreamingContext context)
        {
            _stageFiniteElementProperty = (List<KeyValuePair<T, D>>)info.GetValue("StageFiniteElementProperty", typeof(List<KeyValuePair<T, D>>));
        }


        /// <summary>Add an <paramref name="item"/> to the collection. If the <paramref name="item"/> already exist the <paramref name="stageFiniteElementProperty"/> will be merged with the one already assigned</summary>
        /// <remarks>This is a O(n) operations</remarks>
        private void AddStageFiniteElementProperty(T item, D stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            lock (_locker)
            {
                var el = _stageFiniteElementProperty.Where(i => i.Key == item).SingleOrDefault();

                if (el.Equals(default(KeyValuePair<T, D>)))
                {
                    // Item non esiste
                    _stageFiniteElementProperty.Add(new KeyValuePair<T, D>(item, stageFiniteElementProperty));
                }
                else
                {
                    // Item già presente, faccio merge
                    var kvp = new KeyValuePair<T, D>(el.Key, (D)el.Value.Merge(stageFiniteElementProperty));
                    _stageFiniteElementProperty.Remove(el);
                    _stageFiniteElementProperty.Add(kvp);
                }
            }
        }

        #region Public methods - Add / Set

        /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null</exception>
        /// <remarks>This is an O(1) operation</remarks>
        public virtual void Add(T item, D stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            _stageFiniteElementProperty.Add(new KeyValuePair<T, D>(item, stageFiniteElementProperty));
        }

        /// <inheritdoc cref = "FemObjectStageCollection{T, D}.AddStageFiniteElementProperty(T, D)" />
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void AddUnique(T item, D stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            AddStageFiniteElementProperty(item, stageFiniteElementProperty);
        }



        /// <summary>
        /// Set the <paramref name="stageFiniteElementProperty"/> of <paramref name="item"/>
        /// </summary>
        /// <returns><see langword="false"/> if the <paramref name="item"/> does not exist in the collection </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> or <paramref name="stageFiniteElementProperty"/> is null</exception>
        /// <remarks>This is an O(2n) operation         
        /// <para>The <paramref name="stageFiniteElementProperty"/> will ovveride the existing one if present</para>
        /// </remarks>
        public virtual bool SetStageProperty(T item, D stageFiniteElementProperty)
        {

            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            lock (_locker)
            {
                KeyValuePair<T, D> el = _stageFiniteElementProperty.Where(i => i.Key == item).SingleOrDefault();

                if (el.Equals(default(KeyValuePair<T, D>)))
                {
                    // Elemento non presente
                    return false;
                }
                else
                {
                    _stageFiniteElementProperty.Remove(el);

                    var kvp = new KeyValuePair<T, D>(el.Key, stageFiniteElementProperty);
                    _stageFiniteElementProperty.Add(kvp);
                    return true;
                }
            }
        }


        #endregion Public methods - Add / Set

        #region Public methods - Getter

        /// <param name="id"></param>
        /// <returns>The <typeparamref name="T"/> with id equal to <paramref name="id"/></returns>
        public virtual T this[int id]
        {
            get
            {
                lock (_locker)
                {
                    return _stageFiniteElementProperty.Where(i => i.Key.Id == id).FirstOrDefault().Key;
                }
            }
        }


        /// <summary>
        /// Get the property associated to <paramref name="item"/>
        /// </summary>
        /// <remarks>This is a O(n) operations</remarks>
        /// <returns>The property associated to <paramref name="item"/></returns>
        public D GetStageProperty(T item)
        {
            lock (_locker)
            {
                var el = _stageFiniteElementProperty.Where(i => i.Key == item).SingleOrDefault();

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

        /// <summary>
        /// Get the property associated to the item that correspond to the <paramref name="elementID"/>
        /// </summary>
        /// <param name="elementID">The id of the element to get the stageProperty</param>
        /// <remarks>This is a O(n^2) operations</remarks>
        /// <returns>The property associated to <paramref name="elementID"/></returns>
        public D GetStageProperty(int elementID)
        {
            lock (_locker)
            {
                var el = _stageFiniteElementProperty.Where(i => i.Key == this[elementID]).SingleOrDefault();

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

        public IEnumerator<KeyValuePair<T, D>> GetEnumerator()
        {
            return _stageFiniteElementProperty.GetEnumerator();
        }

        public IEnumerable<KeyValuePair<T, D>> GetEnumerable()
        {
            return _stageFiniteElementProperty;
        }


        #endregion Public methods - Getter

        #region Public method - Edit

        /// <remarks>This is a O(n^2) operations</remarks>
        public bool Remove(T item)
        {
            lock (_locker)
            {
                return _stageFiniteElementProperty.Remove(_stageFiniteElementProperty.SingleOrDefault(i => i.Key == item));
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _stageFiniteElementProperty.Clear();
            }
        }

        #endregion Public method - Edit

        #region Equals - hashcode - Operators

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("StageFiniteElementProperty", _stageFiniteElementProperty);
        }


        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is FemObjectStageCollection<T, D> collection
                                    && _stageFiniteElementProperty.ScrambledEquals(collection._stageFiniteElementProperty)
                                    && base.Equals(collection);
            }
        }

        public override int GetHashCode()
        {
            lock (_locker)
            {
                unchecked
                {
                    int hashCode = -391 + base.GetHashCode();

                    foreach (var element in _stageFiniteElementProperty)
                    {
                        hashCode += EqualityComparer<FemObject>.Default.GetHashCode(element.Key);
                        hashCode += EqualityComparer<Stage.StageProperty>.Default.GetHashCode(element.Value);
                    }

                    return hashCode;
                }
            }
        }


        public static bool operator ==(FemObjectStageCollection<T, D> obj1, FemObjectStageCollection<T, D> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FemObjectStageCollection<T, D> obj1, FemObjectStageCollection<T, D> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals - hashcode - Operators
    }
}
