using GPC.Model.FEM.FiniteElements;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="FiniteElement"/> associated to <see cref="Stage.StageFiniteElementProperty"/>
    /// </summary>
    public class FiniteElementStageCollection<T, D> : FemObjectStageCollection<FiniteElement, Stage.StageFiniteElementProperty>
    {
        /// <summary>
        /// The <see cref="FiniteElement.AttributesLoadCase"/> and <see cref="FiniteElement.AttributesFreedomCase"/>
        /// will be copied to the <see cref="Stage.StageFiniteElementProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.Add(T, D)"/>
        public override int Add(FiniteElement item)
        {
            var sfep = new Stage.StageFiniteElementProperty(item.Property);
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase);
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase);

            base.Add(item, sfep);

            return item.Id;
        }

        /// <summary>
        /// The <see cref="FiniteElement.AttributesLoadCase"/> and <see cref="FiniteElement.AttributesFreedomCase"/>
        /// will be copied to the <see cref="Stage.StageFiniteElementProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetItem(T, D)"/>
        public new void SetItem(FiniteElement item)
        {
            var sfep = new Stage.StageFiniteElementProperty(item.Property);
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase);
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase);

            base.SetItem(item, sfep);
        }

        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetItem(T, D)"/>
        public override int SetItem(FiniteElement item, Stage.StageFiniteElementProperty stageFiniteElementProperty)
        {
            return base.SetItem(item, stageFiniteElementProperty);
        }
    }
}