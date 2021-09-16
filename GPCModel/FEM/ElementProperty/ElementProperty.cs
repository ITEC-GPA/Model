using System;
using System.Runtime.Serialization;

namespace GPC.Model.FEM.Properties
{
    [Serializable]
    public abstract class ElementProperty : ModelObjectId, ISerializable
    {

        #region Public Constructors

        protected ElementProperty(string name, int id)
            : base(id, name, Guid.NewGuid())
        {

        }

        protected ElementProperty(string name)
            : base(name)
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
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ElementProperty objCasted) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            return -391 * base.GetHashCode();
        }

        public static bool operator ==(ElementProperty obj1, ElementProperty obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ElementProperty obj1, ElementProperty obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}