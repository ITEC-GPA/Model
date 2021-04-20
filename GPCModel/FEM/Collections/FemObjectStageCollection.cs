using GPC.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    public abstract class FemObjectStageCollection<T, D> : FemObjectBaseCollection<T> where T : FEMObject where D : Stage.StageProperty
    {
        /// <summary>
        /// Association between element and Stage.StageProperty of that element
        /// </summary>
        protected List<KeyValuePair<T, D>> _stageFiniteElementProperty;


        public FemObjectStageCollection() : base()
        {
            // Chiamo il costruttore di FemObjectBaseCollection per cui uso la sua _collection.
            // Mi serve per usare la sua logica di assegnamento ID

            _stageFiniteElementProperty = new List<KeyValuePair<T, D>>();
        }

        /// <remarks>This is a O(n) operations</remarks>
        private void AddStageFiniteElementProperty(T item, D stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null)
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

        /// <remarks>
        /// <para>This is a O(2n) operations</para>
        /// <para>If <paramref name="item"/> already exist, <paramref name="stageFiniteElementProperty"/> will be merged into the one already assigned </para>
        /// </remarks>
        /// <inheritdoc cref="FemObjectBaseCollection{T}.AddItem(T)"/>
        public virtual int Add(T item, D stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null)
                throw new ArgumentNullException();

            var baseItemId = base.BaseAdd(item);

            AddStageFiniteElementProperty(item, stageFiniteElementProperty);

            return baseItemId;
        }

        /// <remarks>
        /// <para>This is a O(n) operations</para>
        /// <para>If <paramref name="item"/> already exist, <paramref name="stageFiniteElementProperty"/> will be merged into the one already assigned </para>
        /// </remarks>
        /// <inheritdoc cref="FemObjectCollection{T}.SetItem(T)"/>
        public virtual int SetItem(T item, D stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null)
                throw new ArgumentNullException();

            int baseItemId = base.BaseSetItem(item);

            AddStageFiniteElementProperty(item, stageFiniteElementProperty);

            return baseItemId;
        }


        /// <summary>
        /// Set the <paramref name="stageFiniteElementProperty"/> of <paramref name="item"/>
        /// <para>The <paramref name="stageFiniteElementProperty"/> will ovveride the existing one</para>
        /// </summary>
        /// <returns><see langword="false"/> if the <paramref name="item"/> does not exist in the collection </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/> or <paramref name="stageFiniteElementProperty"/> is null</exception>
        /// <remarks>This is an O(2n) operation</remarks>
        public virtual bool SetStageProperty(T item, D stageFiniteElementProperty)
        {
            var el = _stageFiniteElementProperty.Where(i => i.Key == item).SingleOrDefault();

            if (stageFiniteElementProperty is null)
                throw new ArgumentNullException();

            if (el.Equals(default(KeyValuePair<T, D>)))
            {
                return false;
            }
            else
            {
                var kvp = new KeyValuePair<T, D>(el.Key, (D)el.Value.Merge(stageFiniteElementProperty));
                _stageFiniteElementProperty.Remove(el);
                _stageFiniteElementProperty.Add(kvp);
                return true;
            }
        }

        #endregion Public methods - Add / Set

        #region Public methods - Getter

        /// <inheritdoc cref="FemObjectBaseCollection{T}.BaseGetElementById(int)"/>
        public virtual T this[int id]
        {
            get
            {
                return base.BaseGetElementById(id);
            }
        }


        /// <summary>
        /// Get the property associated to <paramref name="item"/>
        /// </summary>
        /// <remarks>This is a O(n) operations</remarks>
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
        public D GetStageProperty(int elementID)
        {
            var el = _stageFiniteElementProperty.Where(i => i.Key == base.BaseGetElementById(elementID)).SingleOrDefault();

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

        #endregion Public methods - Getter

        #region Public method - Edit

        /// <remarks>This is a O(n^2) operations</remarks>
        public override bool Remove(T item)
        {
            return base.Remove(item) && this._stageFiniteElementProperty.Remove(_stageFiniteElementProperty.SingleOrDefault(i => i.Key == item));
        }

        public override void Clear()
        {
            base.Clear();
            this._stageFiniteElementProperty.Clear();
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

            foreach (var element in _collection)
            {
                hashCode = hashCode + EqualityComparer<FEMObject>.Default.GetHashCode(element);
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
