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
    public sealed class ResultLocationPoint : ResultLocation, ISerializable, IEquatable<ResultLocationPoint>
    {
        #region Variables

        private readonly Point2d _location;

        #endregion

        #region Properties

        public Point2d Location => _location;

        #endregion

        #region Public Constructors

        public ResultLocationPoint(IEnumerable<IPlateResult> results, Point2d location, int id = ModelObjectId.IDUNASSIGNED)
            : base(results.Cast<ResultType>().ToArray(), id)
        {
            _location = location;
        }

        public ResultLocationPoint(IEnumerable<IBrickResult> results, Point2d location, int id = ModelObjectId.IDUNASSIGNED)
            : base(results.Cast<ResultType>().ToArray(), id)
        {
            _location = location;
        }

        private ResultLocationPoint(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _location = (Point2d)info.GetValue("Location", typeof(Point2d));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Location", _location, typeof(Point2d));
        }

        public override bool Equals(object obj)
        {
            return Equals((ResultLocationPoint)obj);
        }

        public bool Equals(ResultLocationPoint other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) && _location.Equals(other._location);
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

		#endregion
	}
}
