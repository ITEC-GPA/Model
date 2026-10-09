using System;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    /// <summary>
    /// Base of the properties of the finite elements: beam, plate and brick properties. Two properties are equal if they have the same name
    /// </summary>
    [Serializable]
    public abstract class ElementProperty : ModelObjectId, ISerializable
    {
        #region Public Constructors

        /// <summary>
        /// Creates a property with a new Guid
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        protected ElementProperty(string name = "", int id = IDUNASSIGNED)
            : base(id, name, Guid.NewGuid())
        {

        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ModelObjectId"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ElementProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Equals - Override - Operators

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
        /// Equality with another property: same name
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a property with the same name</returns>
        public override bool Equals(object obj)
        {
            return (obj is ElementProperty objCasted) && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return -391 * base.GetHashCode();
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first property (not null: a null first operand throws <see cref="NullReferenceException"/>)</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the properties are equal</returns>
        public static bool operator ==(ElementProperty obj1, ElementProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first property (not null)</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the properties are different</returns>
        public static bool operator !=(ElementProperty obj1, ElementProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}