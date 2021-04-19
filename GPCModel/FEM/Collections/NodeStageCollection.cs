using GPC.Model.FEM.Attributes;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="Node"/> associated to a <see cref="Stage.StageProperty"/>
    /// </summary>
    public class NodeStageCollection<T, D> : FemObjectStageCollection<Node, Stage.StageProperty>
    {
        /// <summary>
        /// The <see cref="Node.AttributesLoadCase"/> and <see cref="Node.AttributesFreedomCase"/>
        /// will be copied to the <see cref="Stage.StageProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.Add(T, D)"/>
        public int Add(Node item)
        {
            var sfep = new Stage.StageProperty();
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

            base.Add(item, sfep);

            return item.Id;
        }

        /// <summary>
        /// The <see cref="Node.AttributesLoadCase"/> and <see cref="Node.AttributesFreedomCase"/>
        /// will be copied to the <see cref="Stage.StageProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetItem(T, D)"/>
        public int SetItem(Node item)
        {
            var sfep = new Stage.StageProperty();
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

            return base.SetItem(item, sfep);
        }

        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetItem(T, D)"/>
        public override int SetItem(Node item, Stage.StageProperty stageFiniteElementProperty)
        {
            return base.SetItem(item, stageFiniteElementProperty);
        }
    }
}
