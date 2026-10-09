using System;
using System.Runtime.Serialization;

namespace GPC.Model.FreedomCases
{
    /// <summary>
    /// A freedom case: a named set of restraints of the model (the restraints refer to it)
    /// </summary>
    [Serializable]
    public class FreedomCase : ModelObject, ISerializable
    {
        #region Constructor

        /// <summary>
        /// Creates a freedom case with a new Guid
        /// </summary>
        /// <param name="name">The name</param>
        public FreedomCase(string name)
            : base(Guid.NewGuid(), name)
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ModelObject"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected FreedomCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Equality of the names
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a model object with the same name</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return 17 + base.GetHashCode();
        }

        /// <summary>
        /// Serializes the data of <see cref="ModelObject"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}