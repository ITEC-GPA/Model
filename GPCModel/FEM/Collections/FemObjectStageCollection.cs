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
    public abstract class FemObjectStageCollection<T, D> : FemObjectCollection<T> where T : FEMObject where D : Stage.StageProperty
    {
        protected List<D> _stageFiniteElementProperty;

        public FemObjectStageCollection()
        {
            _collection = new List<T>();
            _stageFiniteElementProperty = new List<D>();
            
        }


        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)""/>
        public int Add(T item, D stageFiniteElementProperty)
        {
            base.Add(item);

            _stageFiniteElementProperty.Add(stageFiniteElementProperty);

            return item.Id;
        }

        public void SetStageProperty(T item, D stageFiniteElementProperty)
        {
            var i = _collection.ToList().IndexOf(item);
            _stageFiniteElementProperty[i] = stageFiniteElementProperty;

        }


        public D GetStageProperty(T item)
        {
            var i = _collection.ToList().IndexOf(item);

            return _stageFiniteElementProperty[i];
        }
        public D GetStageProperty(int index)
        {
            return _stageFiniteElementProperty[index];
        }
    }


    public class FiniteElementStageCollection<T, D>
        : FemObjectStageCollection<FiniteElement, Stage.StageFiniteElementProperty>
    {

        public override int Add(FiniteElement item)
        {
            var sfep = new Stage.StageFiniteElementProperty(item.Property);
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase);

            this.Add(item, sfep);

            return item.Id;
        }
    }

    public class NodeStageCollection<T, D>
        : FemObjectStageCollection<Node, Stage.StageProperty>
    {

        public override int Add(Node item)
        {
            var sfep = new Stage.StageProperty();
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

            this.Add(item, sfep);

            return item.Id;
        }
    }
}
