using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    /// <summary>
    /// The values of a result in a coordinate system
    /// </summary>
    [Serializable]
    public abstract class ResultType : ModelObjectId, ISerializable
    {
        #region Variables

        /// <summary>
        /// The coordinate system of the values
        /// </summary>
        protected CoordinateSystem _coordinateSystem;

        #endregion

        #region Properties

        /// <summary>
        /// The coordinate system of the values (the setter does not transform the values)
        /// </summary>
        public CoordinateSystem CoordinateSystem { get => _coordinateSystem; set => _coordinateSystem = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system of the values</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If <paramref name="coordinateSystem"/> is null</exception>
        protected ResultType(CoordinateSystem coordinateSystem, string name = "", int id = ModelObjectId.IDUNASSIGNED)
            : base(id, name)
        {
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ResultType(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
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
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        /// <summary>
        /// Equality of the names (the coordinate system is not compared, by choice)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a result with the same name</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return base.Equals(obj); // Coordinate system non messo per scelta
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(ResultType obj1, ResultType obj2)
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
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(ResultType obj1, ResultType obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}