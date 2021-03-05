using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class ResultPoint : ModelObject, ISerializable
    {

        private int _id;

        public int Id => _id;


        protected ResultPoint(int id, string name) : base(Guid.NewGuid(), name)
        {
            this._id = id;
        }

        protected ResultPoint(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();

        }

        public override bool Equals(object obj)
        {
            return obj is ResultPoint point && base.Equals(obj) && _id == point._id;
        }

        public override int GetHashCode()
        {
            int hashCode = -266076855;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + _id.GetHashCode();
            return hashCode;
        }

    }
}
