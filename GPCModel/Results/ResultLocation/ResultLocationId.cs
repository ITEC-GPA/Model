using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultLocationId : ResultLocation, ISerializable
    {

        public ResultLocationId(ResultType[] results, int id) 
            : base(results, id)
        {

        }

        public ResultLocationId(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public ResultLocationId(ResultType[] results, int id, string name) 
            : base(results, id, name)
        {

        }

        public override bool Equals(object obj)
        {
            return obj is ResultLocationId id &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode(); 
            }
        }

        public static bool operator ==(ResultLocationId left, ResultLocationId right)
        {
            return EqualityComparer<ResultLocationId>.Default.Equals(left, right);
        }

        public static bool operator !=(ResultLocationId left, ResultLocationId right)
        {
            return !(left == right);
        }
    }
}
