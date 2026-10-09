using System;
using System.Runtime.Serialization;

namespace GPC.Model.Attributes
{
    /// <summary>
    /// Base of the attributes that can be assigned to the objects of the model (e.g. the releases of a beam)
    /// </summary>
    [Serializable]
    public abstract class Attribute : ModelObjectId, ISerializable
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Constructor

        /// <summary>
        /// Creates an attribute
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="id">The id (<see cref="ModelObjectId.IDUNASSIGNED"/>: assigned by the collection)</param>
        protected Attribute(string name = "", int id = IDUNASSIGNED)
            : base(id, name)
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ModelObjectId"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Attribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Equals, HashCode and operators

        /// <summary>
        /// Serializes the data of <see cref="ModelObjectId"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Equality with another attribute: same name (see <see cref="ModelObjectId.Equals(object)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal attribute</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return obj is Attribute load && base.Equals(obj);
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23 * -17 + base.GetHashCode();

                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null attributes are equal
        /// </summary>
        /// <param name="obj1">The first attribute</param>
        /// <param name="obj2">The second attribute</param>
        /// <returns>True if the attributes are equal</returns>
        public static bool operator ==(Attribute obj1, Attribute obj2)
        {

            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first attribute</param>
        /// <param name="obj2">The second attribute</param>
        /// <returns>True if the attributes are different</returns>
        public static bool operator !=(Attribute obj1, Attribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals, HashCode and operators
    }
}