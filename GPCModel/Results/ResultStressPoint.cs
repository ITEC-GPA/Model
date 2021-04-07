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
    public class ResultStressPoint : ResultPoint, ISerializable
    {
        private Point2d _location;

        public Point2d Location => _location;

        public ResultStressPoint(int id, Point2d location) 
            : this(id, string.Empty)
        {
            this._location = location;
        }

        public ResultStressPoint(int id, string name) 
            : base(id, name)
        {

        }

        public ResultStressPoint(int id)
            : base(id, string.Empty)
        {

        }


        public ResultStressPoint(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is ResultStressPoint point &&
                   base.Equals(obj) &&
                   EqualityComparer<Point2d>.Default.Equals(_location, point._location);
        }

        public override int GetHashCode()
        {
            int hashCode = 548696834;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Point2d>.Default.GetHashCode(_location);
            return hashCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }
    }
}
