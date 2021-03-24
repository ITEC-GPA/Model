using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.FEM;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.LoadCases;
using GPC.Model.FreedomCases;

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
    }


    public class FiniteElementStageCollection<T, D> : FemObjectStageCollection<FiniteElement, Stage.StageFiniteElementProperty>
    {

        /// <summary>
        /// <inheritdoc />
        /// The <see cref="FiniteElement.AttributesLoadCase"/> and <see cref="FiniteElement.AttributesFreedomCase"/> 
        /// will be copied to the <see cref="Stage.StageFiniteElementProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc />
        public override int Add(FiniteElement item)
        {
            var sfep = new Stage.StageFiniteElementProperty(item.Property);
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase);
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase);
            
            Add(item, sfep);

            return item.Id;
        }
    }


    public class NodeStageCollection<T, D> : FemObjectStageCollection<Node, Stage.StageProperty>
    {

        /// <summary>
        /// <inheritdoc />
        /// The <see cref="Node.AttributesLoadCase"/> and <see cref="Node.AttributesFreedomCase"/> 
        /// will be copied to the <see cref="Stage.StageProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc />
        public override int Add(Node item)
        {
            var sfep = new Stage.StageProperty();
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

            Add(item, sfep);

            return item.Id;
        }

    }
}
