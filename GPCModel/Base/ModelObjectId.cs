using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace GPC.Model
{
    /// <summary>
    /// This class add the attribute Id to the <see cref="ModelObject"/> base class
    /// </summary>

    [Serializable]
    public abstract class ModelObjectId : ModelObject, ISerializable
    {
        protected int _id;

        /// <summary>
        /// Setter is available only internally. If needed, a derived class can expose the id setter by means of methods. A constructor with id attribute is available
        /// </summary>
        public virtual int Id { get => _id; internal set { _id = value; } }

        public ModelObjectId()
        {

        }

        public ModelObjectId(int id)
            : this(id, "", Guid.NewGuid())
        {

        }

        public ModelObjectId(Guid guid)
            : base(guid)
        {

        }

        public ModelObjectId(string name)
            : base(name)
        {

        }

        public ModelObjectId(int id, string name)
            : this(id, name, Guid.NewGuid())
        {

        }

        public ModelObjectId(int id, string name, Guid guid)
            : base(guid, name)
        {
            _id = id;
        }

        public ModelObjectId(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _id = info.GetInt32("Id");
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Id", _id);
        }


        /// <remarks>Equality is not checked against <see cref="ModelObjectId.Id"/> </remarks>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            // ID non viene messo in equals in quanto non tutte le derivate devono ritornare true se gli id sono uguali. 
            // Se ne deve occupare la derivata

            return (obj is ModelObjectId modelId) && base.Equals(modelId);
        }

        public override int GetHashCode()
        {
            return 17 * base.GetHashCode();
        }

        public static bool operator ==(ModelObjectId obj1, ModelObjectId obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ModelObjectId obj1, ModelObjectId obj2)
        {
            return !(obj1 == obj2);
        }


        #region Custom equality comparer

        /// <summary>
        /// Compare two <see cref="ModelObjectId"/> using only <see cref="ModelObjectId.Id"/> as equality parameter
        /// </summary>
        public class ModelObjectIdEqualityComparer : IEqualityComparer<ModelObjectId>
        {
            /// <returns>
            /// <para> true if both <paramref name="x"/> and <paramref name="y"/> are null </para>
            /// </returns>
            /// <remarks> Only <see cref="ModelObjectId.Id"/> is used as equality parameter </remarks>
            bool IEqualityComparer<ModelObjectId>.Equals(ModelObjectId x, ModelObjectId y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                if (ReferenceEquals(x, y))
                    return true;

                if (x.Id.Equals(y.Id))
                    return true;

                return false;
            }


            /// <remarks> Only <see cref="ModelObjectId.Id"/> is used as equality parameter </remarks>
            int IEqualityComparer<ModelObjectId>.GetHashCode(ModelObjectId obj)
            {
                return -17 * obj.Id.GetHashCode();
            }
        }

        #endregion
    }
}
