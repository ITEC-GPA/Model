using System;
using GPC.Model.FEM.FiniteElements;
using static GPC.Model.FEM.Stage;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="FiniteElement"/> associated to <see cref="Stage.StageFiniteElementProperty"/>
    /// </summary>
    /// <remarks>This should be accessed only from the class <see cref="Stage"/> since it does not implement any check on the element duplicates</remarks>
    public class FiniteElementStageCollection<T, D> : FemObjectStageCollection<FiniteElement, Stage.StageFiniteElementProperty>
    {
        public FiniteElementStageCollection() : base()
        {
        }


        /// <inheritdoc cref="FemObjectStageCollection{T, D}.Add(T, D)"/>
        public override void Add(FiniteElement item, Stage.StageFiniteElementProperty stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            base.Add(item, stageFiniteElementProperty);
        }


        /// <summary>
        /// The <see cref="FiniteElement.AttributesLoadCase"/> and <see cref="FiniteElement.AttributesFreedomCase"/> 
        /// will be copied to the <see cref="Stage.StageFiniteElementProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.Add(T, D)"/>
        public void Add(FiniteElement item)
        {
            if (item is null)
                throw new ArgumentNullException();

            StageFiniteElementProperty sp = new StageFiniteElementProperty(item.Property.Name);

            sp.AddLoadCaseAttributes(item.AttributesLoadCase);
            sp.AddFreedomCaseAttributes(item.AttributesFreedomCase);

            base.Add(item, sp);
        }


        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetStageProperty(T, D)"/>
        public override bool SetStageProperty(FiniteElement item, Stage.StageFiniteElementProperty stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            return base.SetStageProperty(item, stageFiniteElementProperty);
        }

    }
}
