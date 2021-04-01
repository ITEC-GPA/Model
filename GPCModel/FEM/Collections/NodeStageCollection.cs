using GPC.Model.FEM.Attributes;
using System.Linq;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of nodes associate to a <see cref="Stage.StageProperty"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
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