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

        public FemObjectStageCollection() 
            : base()
        {
            // Chiamo il costruttore di FemObjectCollection per cui uso la sua _collection e uso il costruttore di default di T.

            // _stageFiniteElementProperty è un dizionario che usa lo stesso equality comparer sulla chiave,
            // quindi le chiavi sono femobject con id diversi
            _stageFiniteElementProperty = new Dictionary<T, D>(new FEMObject.FemObjectOnlyIdComparer());
        }

        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        public int Add(T item, D stageFiniteElementProperty)
        {
            var baseItemId = base.Add(item);

            // l'add cambia l'id se già presente.
            // mi prendo l'istanza di quello presente nella base.collection dato che non è detto che item venga aggiunto (se già presente)
            var baseItem = base.GetElementById(baseItemId); 

            if (!_stageFiniteElementProperty.ContainsKey(baseItem))
            {
                // se la chiave non è presente aggiungo il valore
                _stageFiniteElementProperty.Add(baseItem, stageFiniteElementProperty);
            }
            else
            {
                // Se già presente, sommo gli attributi
                var a = _stageFiniteElementProperty[baseItem].Merge(stageFiniteElementProperty);
                _stageFiniteElementProperty[baseItem] = (D)a;
            }

            return baseItemId;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID">The id of the element to get the stageProperty</param>
        /// <returns></returns>
        public D GetStageProperty(int elementID)
        {
            return _stageFiniteElementProperty[base.GetElementById(elementID)];
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