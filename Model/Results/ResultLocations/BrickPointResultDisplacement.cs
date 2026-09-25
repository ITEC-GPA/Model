using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// The displacements of a brick (volume) element at a point
    /// </summary>
    [Serializable]
    public sealed class BrickPointResultDisplacement : ResultLocation, ISerializable, IEquatable<BrickPointResultDisplacement>, IBrickResultLocation
    {
        #region Variables

        /// <summary>
        /// The point of the result
        /// </summary>
        private Point3d _location;

        #endregion

        #region Properties

        /// <summary>
        /// The point of the result
        /// </summary>
        public Point3d Location { get => _location; set => _location = value; }

        /// <summary>
        /// The displacements
        /// </summary>
        public ResultDisplacement ResultDisplacement { get => (ResultDisplacement)_resultTypes; set => _resultTypes = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="loadCase">The load case or combination</param>
        /// <param name="result">The displacements</param>
        /// <param name="location">The point of the result</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        public BrickPointResultDisplacement(ILoadCase loadCase, ResultDisplacement result, Point3d location, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, result, id, name)
        {
            _location = location;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private BrickPointResultDisplacement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _location = (Point3d)info.GetValue("Location", typeof(Point3d));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Serializes the result
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Location", _location, typeof(Point3d));
        }

        /// <summary>
        /// Equality with another result of the same type (an object of another type throws <see cref="InvalidCastException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the results are equal</returns>
        public override bool Equals(object obj)
        {
            return Equals((BrickPointResultDisplacement)obj);
        }

        /// <summary>
        /// Equality of the result values and of the name and of the point (the load case is not compared, see <see cref="ResultLocation.Equals(object)"/>)
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(BrickPointResultDisplacement other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) && _location.Equals(other._location);
        }

        /// <summary>
        /// The hash code of the base and of the point
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(BrickPointResultDisplacement)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
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

        /// <summary>
        /// Inequality operator (see <see cref="Equals(BrickPointResultDisplacement)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(BrickPointResultDisplacement obj1, BrickPointResultDisplacement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
