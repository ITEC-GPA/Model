using GPC.Model.FEM.FiniteElements;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="FiniteElement"/> associated to <see cref="Stage.StageFiniteElementProperty"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
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
}