using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultLocationId : ModelObject, ISerializable
    {

        private readonly int _id;

        public int Id => _id;


        public ResultLocationId(int id) : this(id, "")
        {

        }

        public ResultLocationId(int id, string name) : base(Guid.NewGuid(), name)
        {
            _id = id;
        }

        public ResultLocationId(SerializationInfo info, StreamingContext context) 
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
            return obj is ResultLocationId point && base.Equals(obj) && _id == point._id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -266076855;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _id.GetHashCode();
                return hashCode; 
            }
        }

    }
}
