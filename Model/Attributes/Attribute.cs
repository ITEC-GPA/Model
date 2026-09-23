using System;
using System.Runtime.Serialization;

namespace GPC.Model.Attributes
{
    [Serializable]
    public abstract class Attribute : ModelObjectId, ISerializable
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Constructor

        protected Attribute(string name = "", int id = IDUNASSIGNED)
            : base(id, name)
        {
        }

        protected Attribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Equals, HashCode and operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return obj is Attribute load && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23 * -17 + base.GetHashCode();

                return hashCode;
            }
        }

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

        public static bool operator !=(Attribute obj1, Attribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals, HashCode and operators
    }
}