using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.Results
{

    [Serializable]
    public class ResultLocationPoint : ResultLocationId, ISerializable, IResultLocation
    {
        private readonly Point2d _location;

        public Point2d Location => _location;

        public ResultLocationPoint(int id, Point2d location) 
            : base(id, string.Empty)
        {
            _location = location;
        }

        public ResultLocationPoint(SerializationInfo info, StreamingContext context) 
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
            return obj is ResultLocationPoint point &&
                   base.Equals(obj) &&
                   EqualityComparer<Point2d>.Default.Equals(_location, point._location);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 548696834;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<Point2d>.Default.GetHashCode(_location);
                return hashCode; 
            }
        }

        public static bool operator ==(ResultLocationPoint obj1, ResultLocationPoint obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultLocationPoint obj1, ResultLocationPoint obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
