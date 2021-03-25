using GPC.Utilities.Extensions;
using System.Collections.Generic;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public abstract class FemObjectStageCollection<T, D> : FemObjectCollection<T> where T : FEMObject where D : Stage.StageProperty
    {
        protected Dictionary<T, D> _stageFiniteElementProperty;

        public FemObjectStageCollection() : base()
        {
            // Chiamo il costruttore di FemObjectCollection per cui uso la sua _collection.
            // _stageFiniteElementProperty è un dizionario che usa lo stesso equality comparer sulla chiave,
            // quindi le chiavi sono femobject con id diversi
            _stageFiniteElementProperty = new Dictionary<T, D>(new FEMObject.FemObjectOnlyIdComparer());
        }

        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        public int Add(T item, D stageFiniteElementProperty)
        {
            base.Add(item);

            _stageFiniteElementProperty.Add(item, stageFiniteElementProperty);

            return item.Id;
        }

        public void SetStageProperty(T item, D stageFiniteElementProperty)
        {
            if (!_stageFiniteElementProperty.ContainsKey(item))
                this.Add(item, stageFiniteElementProperty);

            _stageFiniteElementProperty[item] = stageFiniteElementProperty;
        }

        public D GetStageProperty(T item)
        {
            if (!_stageFiniteElementProperty.ContainsKey(item))
                return null;

            return _stageFiniteElementProperty[item];
        }

        public D GetStageProperty(int index)
        {
            return _stageFiniteElementProperty[base.GetElementById(index)];
        }

        public override void Clear()
        {
            base.Clear();
            this._stageFiniteElementProperty.Clear();
        }

        public override bool Remove(T item)
        {
            return base.Remove(item) && this._stageFiniteElementProperty.Remove(item);
        }

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
    }
}