using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    [Serializable]
    public sealed class PointResultDisplacement : ResultLocation, ISerializable, IEquatable<PointResultDisplacement>, IPlateResultLocation
    {
        #region Variables

        private Point2d _location;

        #endregion

        #region Properties

        public Point2d Location { get => _location; set => _location = value; }

        public ResultDisplacement ResultDisplacement { get => (ResultDisplacement)_resultTypes; set => _resultTypes = value; }

        #endregion

        #region Public Constructors

        public PointResultDisplacement(ILoadCase loadCase, ResultDisplacement result, Point2d location, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, result, id, name)
        {
            _location = location;
        }

        private PointResultDisplacement(SerializationInfo info, StreamingContext context)
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
            return Equals((PointResultDisplacement)obj);
        }

        public bool Equals(PointResultDisplacement other)
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

        public static bool operator ==(PointResultDisplacement obj1, PointResultDisplacement obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PointResultDisplacement obj1, PointResultDisplacement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
