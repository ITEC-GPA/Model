using GPC.Model.FEM.Attributes;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.FEM.Collections
{
    /// <summary>
    /// Collection of <see cref="Node"/> associated to a <see cref="Stage.StageProperty"/>
    /// </summary>
    /// <remarks>This should be accessed only from the class <see cref="Stage"/> since it does not implement any check on the element duplicates</remarks>
    /// <remarks>The collection is thread-safe</remarks>
    [Serializable]
    public class NodeStageCollection<D> : NodeCollection, IFEMObjectStageCollection<Node, D>, ISerializable where D : Stage.StageProperty, ISerializable
    {
        public NodeStageCollection() 
            : base()
        {

        }

        public NodeStageCollection(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }


        /*/// <summary>
        /// The <see cref="Node.AttributesLoadCase"/> and <see cref="Node.AttributesFreedomCase"/>
        /// will be copied to the <see cref="Stage.StageProperty"/> associated the <paramref name="item"/>
        /// </summary>
        /// <inheritdoc cref="FemObjectStageCollection{T, D}.AddUnique(T, D)"/>
        public void Add(Node item)
        {
            var sfep = new Stage.StageProperty();
            sfep.AddLoadCaseAttributes(item.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
            sfep.AddFreedomCaseAttributes(item.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

            //base.AddUnique(item, sfep);
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="stageFiniteElementProperty"></param>
        public void Add(Node node, D stageFiniteElementProperty)
        {
            var sfep = new Stage.StageProperty();
            sfep.AddLoadCaseAttributes(node.AttributesLoadCase.Cast<LoadCaseAttribute>().ToList());
            sfep.AddFreedomCaseAttributes(node.AttributesFreedomCase.Cast<FreedomCaseAttribute>().ToList());

            base.Add(node);
        }

        /// <inheritdoc cref = "FemObjectStageCollection{T, D}.AddUnique(T, D)" />
        public /*override */void AddUnique(Node item, Stage.StageProperty stageProperty)
        {
            /*if (stageProperty is null || item is null)
                throw new System.ArgumentNullException();

            base.AddUnique(item, stageProperty);*/
        }


        /// <inheritdoc cref="FemObjectStageCollection{T, D}.SetStageProperty(T, D)"/>
        /*public override bool SetStageProperty(Node item, Stage.StageProperty stageProperty)
        {
            return base.SetStageProperty(item, stageProperty);
        }*/

        public D GetStageProperty(int elementID)
        {
            return null;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #region Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is NodeStageCollection<D> collection && base.Equals(obj);
            }
        }


        public override int GetHashCode()
        {
            lock (_locker)
            {
                return base.GetHashCode();
            }
        }


        public static bool operator ==(NodeStageCollection<D> obj1, NodeStageCollection<D> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(NodeStageCollection<D> obj1, NodeStageCollection<D> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
