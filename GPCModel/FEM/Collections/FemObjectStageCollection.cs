using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.FEM;
using GPC.Model.FEM.FiniteElements;

namespace GPC.Model.FEM.Collections
{
    public class FemObjectStageCollection<T, D> : FemObjectCollection<T> where T : FEMObject where D : Stage.StageProperty
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
    }


    public class FiniteElementStageCollection<T, D>
        : FemObjectStageCollection<T, D> where T : FiniteElement where D : Stage.StageFiniteElementProperty
    {

    }

    public class NodeStageCollection<T, D>
        : FemObjectStageCollection<T, D> where T : Node where D : Stage.StageProperty
    {

    }
}
