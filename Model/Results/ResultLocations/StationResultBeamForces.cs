using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// The internal forces of a beam element at a station
    /// </summary>
    [Serializable]
    public class StationResultBeamForces : ResultLocation, ISerializable, IBeamResultLocation
    {
        #region Variables

        /// <summary>
        /// The position of the station along the beam: 0 at the start point, 1 at the end point
        /// </summary>
        private double _parametricCoordinate;

        #endregion

        #region Properties

        /// <summary>
        /// The position of the station along the beam: 0 at the start point, 1 at the end point (the setter does not check the range)
        /// </summary>
        public double ParametricDistance { get => _parametricCoordinate; set => _parametricCoordinate = value; }

        /// <summary>
        /// The internal forces
        /// </summary>
        public ResultBeamForces ResultBeamForces { get => (ResultBeamForces)_resultTypes; set => _resultTypes = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="loadCase">The load case or combination</param>
        /// <param name="results">The internal forces</param>
        /// <param name="parametricCoordinate">The position of the station along the beam: 0 at the start point, 1 at the end point</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If <paramref name="parametricCoordinate"/> is out of [0, 1]</exception>
        public StationResultBeamForces(ILoadCase loadCase, ResultBeamForces results, double parametricCoordinate, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, results, id, name)
        {
            if (parametricCoordinate < 0.0 || parametricCoordinate > 1.0)
                throw new ArgumentException($"distanceFromStartPoint can not lower than 0 or higher than 1");
            _parametricCoordinate = parametricCoordinate;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected StationResultBeamForces(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _parametricCoordinate = info.GetDouble("ParametricDistance");
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
            info.AddValue("ParametricDistance", _parametricCoordinate);
        }

        /// <summary>
        /// Equality with another result of the same type (an object of another type throws <see cref="InvalidCastException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the results are equal</returns>
        public override bool Equals(object obj)
        {
            return Equals((StationResultBeamForces)obj);
        }

        /// <summary>
        /// Equality of the result values and of the name and of the station (the load case is not compared, see <see cref="ResultLocation.Equals(object)"/>)
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(StationResultBeamForces other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) &&
                _parametricCoordinate == other._parametricCoordinate;
        }

        /// <summary>
        /// The hash code of the base and of the station
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                hashCode = hashCode * -19 + _parametricCoordinate.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(StationResultBeamForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(StationResultBeamForces obj1, StationResultBeamForces obj2)
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
        /// Inequality operator (see <see cref="Equals(StationResultBeamForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(StationResultBeamForces obj1, StationResultBeamForces obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}