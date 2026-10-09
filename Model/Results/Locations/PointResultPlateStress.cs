using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// The stresses of a plate (area) element at a point of its plane
    /// </summary>
    [Serializable]
    public sealed class PointResultPlateStress : ResultLocation, ISerializable, IEquatable<PointResultPlateStress>, IPlateResultLocation
    {
        #region Variables

        /// <summary>
        /// The point of the result
        /// </summary>
        private Point2d _location;

        #endregion

        #region Properties

        /// <summary>
        /// The point of the result (2D coordinates in the plane of the element)
        /// </summary>
        public Point2d Location { get => _location; set => _location = value; }

        /// <summary>
        /// The result as <see cref="ResultPlateStress"/> (the constructor takes a <see cref="ResultStress"/>: the getter throws <see cref="InvalidCastException"/>)
        /// </summary>
        public ResultPlateStress ResultBeamForces { get => _resultTypes as ResultPlateStress; set => _resultTypes = value; }
        public ResultPlateStress ResultPlateStress { get => _resultTypes as ResultPlateStress; set => _resultTypes = value; }
        /// <summary>The legacy constructor supplied one unlocated stress tensor. Faces cannot be inferred.</summary>
        public ResultStress LegacyUnlocatedStress => _resultTypes as ResultStress;

        #endregion

        #region Public Constructors

        public PointResultPlateStress(ILoadCase loadCase, ResultPlateStress result, Point2d location, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, result, id, name) { _location = location; }

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="loadCase">The load case or combination</param>
        /// <param name="result">The stresses</param>
        /// <param name="location">The point of the result (2D coordinates in the plane of the element)</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        public PointResultPlateStress(ILoadCase loadCase, ResultStress result, Point2d location, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, result, id, name)
        {
            _location = location;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private PointResultPlateStress(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _location = (Point2d)info.GetValue("Location", typeof(Point2d));
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
            info.AddValue("Location", _location, typeof(Point2d));
        }

        /// <summary>
        /// Equality with another result of the same type (an object of another type throws <see cref="InvalidCastException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the results are equal</returns>
        public override bool Equals(object obj)
        {
            return Equals((PointResultPlateStress)obj);
        }

        /// <summary>
        /// Equality of the result values and of the name and of the point (including the load case and analysis state)
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(PointResultPlateStress other)
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
        /// Equality operator (see <see cref="Equals(PointResultPlateStress)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(PointResultPlateStress obj1, PointResultPlateStress obj2)
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
        /// Inequality operator (see <see cref="Equals(PointResultPlateStress)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(PointResultPlateStress obj1, PointResultPlateStress obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
