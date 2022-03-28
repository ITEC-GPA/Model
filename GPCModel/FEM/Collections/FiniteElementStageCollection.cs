using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.FEM.FiniteElements;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="FiniteElement"/> associated to <see cref="Stage.StageFiniteElementProperty"/>
    /// </summary>
    /// <remarks>This should be accessed only from the class <see cref="Stage"/> since it does not implement any check on the element duplicates</remarks>
    /// <remarks>The collection is thread-safe</remarks>
    [Serializable]
    public class FiniteElementStageCollection<T, D> : FemObjectStageCollection<FiniteElement, Stage.StageFiniteElementProperty>, ISerializable
    {

        public FiniteElementStageCollection()
            : base()
        {

        }

        public FiniteElementStageCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }



        /// <inheritdoc cref="FemObjectStageCollection{T, D}.AddUnique(T, D)"/>
        public override void AddUnique(FiniteElement item, Stage.StageFiniteElementProperty stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            base.AddUnique(item, stageFiniteElementProperty);
        }


        /// <summary>
        /// The <see cref="FiniteElement.AttributesLoadCase"/> and <see cref="FiniteElement.AttributesFreedomCase"/> 
        /// will be copied to the <see cref="Stage.StageFiniteElementProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.AddUnique(T, D)"/>
        public void Add(FiniteElement item)
        {
            if (item is null)
                throw new ArgumentNullException();

            Stage.StageFiniteElementProperty sp = new Stage.StageFiniteElementProperty(item.Property.Name);

            sp.AddLoadCaseAttributes(item.AttributesLoadCase.ToList());
            sp.AddFreedomCaseAttributes(item.AttributesFreedomCase.ToList());

            base.AddUnique(item, sp);
        }


        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetStageProperty(T, D)"/>
        public override bool SetStageProperty(FiniteElement item, Stage.StageFiniteElementProperty stageFiniteElementProperty)
        {
            if (stageFiniteElementProperty is null || item is null)
                throw new ArgumentNullException();

            return base.SetStageProperty(item, stageFiniteElementProperty);
        }



        #region Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is FiniteElementStageCollection<T, D> collection && base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            lock (_locker)
            {
                return base.GetHashCode();
            }
        }

        public static bool operator ==(FiniteElementStageCollection<T, D> obj1, FiniteElementStageCollection<T, D> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FiniteElementStageCollection<T, D> obj1, FiniteElementStageCollection<T, D> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals - HashCode - Operators
    }
}
