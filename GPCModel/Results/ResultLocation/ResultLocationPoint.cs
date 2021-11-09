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
    public class ResultLocationPoint : ResultLocation, ISerializable
    {
        private readonly Point2d _location;

        public Point2d Location => _location;

        public ResultLocationPoint(IEnumerable<ResultType> results, Point2d location, int id = ModelObjectId.IDUNASSIGNED)
            : base(results.ToArray(), id)
        {
            _location = location;
        }

        public ResultLocationPoint(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _location = (Point2d)info.GetValue("Location", typeof(Point2d));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Location", _location, typeof(Point2d));
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
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                hashCode = hashCode * -19 + _location.GetHashCode();
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
