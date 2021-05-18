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
    public class PointResultLocation : ResultLocationId, ISerializable
    {
        private readonly Point2d _location;

        public Point2d Location => _location;

        public PointResultLocation(int id, Point2d location) 
            : this(id, string.Empty)
        {
            _location = location;
        }

        public PointResultLocation(int id, string name) 
            : base(id, name)
        {

        }

        public PointResultLocation(int id)
            : base(id, string.Empty)
        {

        }


        public PointResultLocation(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is PointResultLocation point &&
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }
    }
}
