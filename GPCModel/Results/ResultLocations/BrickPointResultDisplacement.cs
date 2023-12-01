using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    [Serializable]
    public sealed class BrickPointResultDisplacement : ResultLocation, ISerializable, IEquatable<BrickPointResultDisplacement>, IBrickResultLocation
    {
        #region Variables

        private Point3d _location;

        #endregion

        #region Properties

        public Point3d Location { get => _location; set => _location = value; }

        public ResultDisplacement ResultDisplacement { get => (ResultDisplacement)_resultTypes; set => _resultTypes = value; }

        #endregion

        #region Public Constructors

        public BrickPointResultDisplacement(ILoadCase loadCase, ResultDisplacement result, Point3d location, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, result, id, name)
        {
            _location = location;
        }

        private BrickPointResultDisplacement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _location = (Point3d)info.GetValue("Location", typeof(Point3d));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Location", _location, typeof(Point3d));
        }

        public override bool Equals(object obj)
        {
            return Equals((BrickPointResultDisplacement)obj);
        }

        public bool Equals(BrickPointResultDisplacement other)
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

        public static bool operator ==(BrickPointResultDisplacement obj1, BrickPointResultDisplacement obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(BrickPointResultDisplacement obj1, BrickPointResultDisplacement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
