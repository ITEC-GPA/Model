using GPC.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// This class rapresent an association between a <see cref="FEMObject"/> and a property override <see cref="Stage.StageProperty"/>
    /// </summary>
    /// <remarks>This should be accessed only from the class <see cref="Stage"/> since it does not implement any check on the element duplicates</remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public abstract class FemObjectStageCollection<T, D>  where T : FEMObject where D : Stage.StageProperty
    {
        /// <summary>
        /// Association between an element and Stage.StageProperty of that element
        /// </summary>
        protected List<KeyValuePair<T, D>> _stageFiniteElementProperty;

        public int Count => _stageFiniteElementProperty.Count();

        public FemObjectStageCollection()
        {
            _stageFiniteElementProperty = new List<KeyValuePair<T, D>>();
        }


        /// <summary>Add an <paramref name="item"/> to the collection. If the <paramref name="item"/> already exist the <paramref name="stageFiniteElementProperty"/> will be merged with the one already assigned</summary>
        /// <remarks>This is a O(n) operations</remarks>
        private void AddStageFiniteElementProperty(T item, D stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

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

        #region Public methods - Add / Set


        /// <inheritdoc cref = "FemObjectStageCollection{T, D}.AddStageFiniteElementProperty(T, D)" />
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void Add(T item, D stageFiniteElementProperty)
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
            KeyValuePair<T,D> el = _stageFiniteElementProperty.Where(i => i.Key == item).SingleOrDefault();

            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

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


        #endregion Public methods - Add / Set

        #region Public methods - Getter

        /// <param name="id"></param>
        /// <returns>The <typeparamref name="T"/> with id equal to <paramref name="id"/></returns>
        public virtual T this[int id]
        {
            get
            {
                return _stageFiniteElementProperty.Where(i => i.Key.Id == id).FirstOrDefault().Key;
            }
        }


        /// <summary>
        /// Get the property associated to <paramref name="item"/>
        /// </summary>
        /// <remarks>This is a O(n) operations</remarks>
        /// <returns>The property associated to <paramref name="item"/></returns>
        public D GetStageProperty(T item)
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

        /// <summary>
        /// Get the property associated to the item that correspond to the <paramref name="elementID"/>
        /// </summary>
        /// <param name="elementID">The id of the element to get the stageProperty</param>
        /// <remarks>This is a O(n^2) operations</remarks>
        /// <returns>The property associated to <paramref name="elementID"/></returns>
        public D GetStageProperty(int elementID)
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

        public IEnumerator<KeyValuePair<T, D>> GetEnumerator()
        {
            return _stageFiniteElementProperty.GetEnumerator();
        }


        #endregion Public methods - Getter

        #region Public method - Edit

        /// <remarks>This is a O(n^2) operations</remarks>
        public bool Remove(T item)
        {
            return _stageFiniteElementProperty.Remove(_stageFiniteElementProperty.SingleOrDefault(i => i.Key == item));
        }

        public void Clear()
        {
            _stageFiniteElementProperty.Clear();
        }

        #endregion Public method - Edit

        #region Equals - hashcode - Operators

        public override bool Equals(object obj)
        {
            return obj is FemObjectStageCollection<T, D> collection && _stageFiniteElementProperty.ScrambledEquals(collection._stageFiniteElementProperty) && base.Equals(collection);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();

            foreach (var element in _stageFiniteElementProperty)
            {
                hashCode = hashCode + EqualityComparer<FEMObject>.Default.GetHashCode(element.Key);
                hashCode = hashCode + EqualityComparer<Stage.StageProperty>.Default.GetHashCode(element.Value);
            }

            return hashCode;
        }


        public static bool operator ==(FemObjectStageCollection<T, D> obj1, FemObjectStageCollection<T, D> obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FemObjectStageCollection<T, D> obj1, FemObjectStageCollection<T, D> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals - hashcode - Operators
    }
}
