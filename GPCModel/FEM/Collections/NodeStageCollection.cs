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
        public void Add(Node item)
        {
            var sfep = new Stage.StageProperty();
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

            base.Add(item, sfep);
        }


        /// <inheritdoc cref = "FemObjectStageCollection{T, D}.Add(T, D)" />
        public override void Add(Node item, Stage.StageProperty stageProperty)
        {
            if (stageProperty is null || item is null)
                throw new System.ArgumentNullException();

            base.Add(item, stageProperty);
        }


        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetStageProperty(T, D)"/>
        public override bool SetStageProperty(Node item, Stage.StageProperty stageProperty)
        {
            return base.SetStageProperty(item, stageProperty);
        }


    }
}
