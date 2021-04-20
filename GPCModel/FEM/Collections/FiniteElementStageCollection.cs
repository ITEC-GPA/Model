using System;
using GPC.Model.FEM.FiniteElements;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="FiniteElement"/> associated to <see cref="Stage.StageFiniteElementProperty"/>
    /// </summary>
    public class FiniteElementStageCollection<T, D> : FemObjectStageCollection<FiniteElement, Stage.StageFiniteElementProperty>
    {


        /// <inheritdoc cref="FemObjectStageCollection{T, D}.Add(T, D)"/>
        public override int Add(FiniteElement item, Stage.StageFiniteElementProperty stageFiniteElementProperty)
        {
            return base.Add(item, stageFiniteElementProperty);
        }



        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetItem(T, D)"/>
        public override int SetItem(FiniteElement item, Stage.StageFiniteElementProperty stageFiniteElementProperty)
        {
            return base.SetItem(item, stageFiniteElementProperty);
        }
    }
}
