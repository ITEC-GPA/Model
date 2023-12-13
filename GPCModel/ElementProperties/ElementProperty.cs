using System;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    /// <summary>
    /// All beam, plate and brick properties for FEM.
    /// </summary>
    [Serializable]
    public abstract class ElementProperty : ModelObjectId, ISerializable
    {
        #region Public Constructors

        protected ElementProperty(string name = "", int id = IDUNASSIGNED)
            : base(id, name, Guid.NewGuid())
        {

        }

        protected ElementProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Equals - Override - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            return (obj is ElementProperty objCasted) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return -391 * base.GetHashCode();
            }
        }

        public static bool operator ==(ElementProperty obj1, ElementProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(ElementProperty obj1, ElementProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}