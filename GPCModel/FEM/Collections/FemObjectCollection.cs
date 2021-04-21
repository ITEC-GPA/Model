using GPC.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// A collection of FemObject.
    /// <para>This collection does not contains elements with a duplicated ID</para>
    /// </summary>
    /// <typeparam name="T">A <see cref="FEMObject"/></typeparam>
    public class FemObjectCollection<T> : FemObjectBaseCollection<T> where T : FEMObject
    {
        // TODO: la classe FemObjectBaseCollection non serve più, toglierla e copiare tutto il contenuto qua
        public FemObjectCollection() : base()
        {
            _collection = new List<T>();
        }


        #region Public method - Setter


        /// <inheritdoc cref="FemObjectBaseCollection{T}.BaseAdd(T)"/>
        public virtual int Add(T item)
        {
            return base.BaseAdd(item);
        }


        /// <inheritdoc cref="FemObjectBaseCollection{T}.BaseSetItem(T)"/>
        public virtual int SetItem(T item)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());

            return base.BaseSetItem(item);
        }

        /// <inheritdoc cref="FemObjectBaseCollection{T}.BaseSetItems(T[])"/>
        public virtual int[] SetItem(T[] item)
        {
            if (item is null)
                throw new ArgumentNullException(item.ToString());

            return base.BaseSetItems(item);
        }


        #endregion Public method - Setter

        #region Public method - Getter

        /// <inheritdoc cref="FemObjectBaseCollection{T}.BaseGetElementById(int)"/>
        public virtual T this[int id]
        {
            get
            {
                return base.BaseGetElementById(id);
            }
        }


        /// <inheritdoc cref="FemObjectBaseCollection{T}.BaseGetElementById(int)"/>
        public virtual T GetElementById(int id)
        {
            return base.BaseGetElementById(id);
        }


        #endregion Public method - Getter

        #region Public method - Check

        /// <summary>
        /// Check if <paramref name="item"/> is contained in the collection
        /// </summary>
        /// <remarks>This is an O(n) operation</remarks>
        public virtual bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        #endregion Public method - Check

        #region Public method - Edit

        public override void Clear()
        {
            base.Clear();
        }

        /// <inheritdoc cref="ICollection.CopyTo(System.Array, int)"/>
        public override void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="FemObjectBaseCollection{T}.Remove(T)"/>
        public override bool Remove(T item)
        {
            return base.Remove(item);
        }

        /// <inheritdoc cref="FemObjectBaseCollection{T}.Remove(int)"/>
        public override bool Remove(int id)
        {
            return base.Remove(id);
        }


        #endregion Public method - Edit

        #region Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(FemObjectCollection<T> obj1, FemObjectCollection<T> obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FemObjectCollection<T> obj1, FemObjectCollection<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals - HashCode - Operators
    }
}
