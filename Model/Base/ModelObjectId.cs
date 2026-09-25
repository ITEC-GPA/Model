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
        /// <summary>
        /// The value of <see cref="Id"/> not assigned
        /// </summary>
        public const int IDUNASSIGNED = -1;

        /// <summary>
        /// The identifier number
        /// </summary>
        protected int _id;

        /// <summary>
        /// The identifier number (<see cref="IDUNASSIGNED"/> if not assigned). The setter is available only internally: a derived class can expose it
        /// by means of methods; a constructor with id is available
        /// </summary>
        public virtual int Id { get => _id; internal set { _id = value; } }

        /// <summary>
        /// Creates an object with a new Guid, no name and <see cref="IDUNASSIGNED"/>
        /// </summary>
        public ModelObjectId()
            : base()
        {
            _id = IDUNASSIGNED;
        }

        /// <summary>
        /// Creates an object with an id, a new Guid and an empty name
        /// </summary>
        /// <param name="id">The id</param>
        public ModelObjectId(int id)
            : this(id, "", Guid.NewGuid())
        {

        }

        /// <summary>
        /// Creates an object with an id, a Guid and an empty name
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="guid">The Guid</param>
        public ModelObjectId(int id, Guid guid)
            : this(id, "", guid)
        {

        }

        /// <summary>
        /// Creates an object with a Guid, no name and <see cref="IDUNASSIGNED"/>
        /// </summary>
        /// <param name="guid">The Guid</param>
        public ModelObjectId(Guid guid)
            : base(guid)
        {
            _id = IDUNASSIGNED;
        }

        /// <summary>
        /// Creates an object with a name, a new Guid and <see cref="IDUNASSIGNED"/>
        /// </summary>
        /// <param name="name">The name</param>
        public ModelObjectId(string name)
            : base(name)
        {
            _id = IDUNASSIGNED;
        }

        /// <summary>
        /// Creates an object with an id, a name and a new Guid
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        public ModelObjectId(int id, string name)
            : this(id, name, Guid.NewGuid())
        {

        }

        /// <summary>
        /// Creates an object with an id, a name and a Guid
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        /// <param name="guid">The Guid</param>
        public ModelObjectId(int id, string name, Guid guid)
            : base(guid, name)
        {
            _id = id;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="ModelObject"/> and the id
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ModelObjectId(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _id = info.GetInt32("Id");
        }


        /// <summary>
        /// Serializes the data of <see cref="ModelObject"/> and the id
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Id", _id);
        }


        /// <summary>
        /// Equality of the names (see <see cref="ModelObject.Equals(object)"/>): the derived classes decide if the id is compared
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a <see cref="ModelObjectId"/> with the same name</returns>
        /// <remarks>Equality is not checked against <see cref="ModelObjectId.Id"/></remarks>
        public override bool Equals(object obj)
        {
            // ID non viene messo in equals in quanto non tutte le derivate devono ritornare true se gli id sono uguali. 
            // Se ne deve occupare la derivata

            return (obj is ModelObjectId modelId) && base.Equals(modelId);
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return 17 * base.GetHashCode();
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null objects are equal
        /// </summary>
        /// <param name="obj1">The first object</param>
        /// <param name="obj2">The second object</param>
        /// <returns>True if the objects are equal</returns>
        public static bool operator ==(ModelObjectId obj1, ModelObjectId obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first object</param>
        /// <param name="obj2">The second object</param>
        /// <returns>True if the objects are different</returns>
        public static bool operator !=(ModelObjectId obj1, ModelObjectId obj2)
        {
            return !(obj1 == obj2);
        }


        #region Custom equality comparer

        /// <summary>
        /// Compare two <see cref="ModelObjectId"/> using only <see cref="ModelObjectId.Id"/> as equality parameter
        /// </summary>
        [Serializable]
        public class ModelObjectIdEqualityComparer : IEqualityComparer<ModelObjectId>
        {
            /// <summary>
            /// Equality of the ids
            /// </summary>
            /// <param name="x">The first object</param>
            /// <param name="y">The second object</param>
            /// <returns>True if the objects have the same id, or if both <paramref name="x"/> and <paramref name="y"/> are null</returns>
            /// <remarks>Only <see cref="ModelObjectId.Id"/> is used as equality parameter</remarks>
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


            /// <summary>
            /// The hash code of the id
            /// </summary>
            /// <param name="obj">The object</param>
            /// <returns>The hash code</returns>
            /// <remarks>Only <see cref="ModelObjectId.Id"/> is used as equality parameter</remarks>
            int IEqualityComparer<ModelObjectId>.GetHashCode(ModelObjectId obj)
            {
                unchecked
                {
                    return -17 * obj.Id.GetHashCode();
                }
            }
        }

        #endregion
    }
}
