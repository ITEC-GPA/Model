using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A uniform pressure normal to an area (along the Z axis of the coordinate system of the shape)
    /// </summary>
    [Serializable]
    public class NormalAreaLoad : Load, IAreaLoad
    {
        #region Variables

        /// <summary>
        /// The pressure
        /// </summary>
        protected double _pressure;
        /// <summary>
        /// The area, in the global coordinates
        /// </summary>
        protected Shape _shape;

        #endregion

        #region Properties

        /// <summary>
        /// The pressure (positive along the normal of the shape)
        /// </summary>
        public double Pressure { get => _pressure; set => _pressure = value; }

        /// <summary>
        /// The area, in the global coordinates
        /// </summary>
        public Shape Shape { get => _shape; set => _shape = value; }

        #endregion

        #region Public constructors 

        /// <summary>
        /// Creates a normal area load
        /// </summary>
        /// <param name="pressure">The pressure</param>
        /// <param name="shape">The area, in the global coordinates</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">Not used: the coordinate system is the one of the shape (<see cref="Shape.GetCoordinateSystem"/>)</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If the shape is null</exception>
        public NormalAreaLoad(double pressure, Shape shape, LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            _pressure = pressure;
            _shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
            _coordinateSystem = shape.GetCoordinateSystem();
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Load"/>, pressure, shape and coordinate system
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected NormalAreaLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _pressure = info.GetDouble("Pressure");
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The area of the load
        /// </summary>
        /// <returns>The shape, in the global coordinates</returns>
        public Shape GetGeometry() => _shape;

        /// <summary>
        /// The area of the load (see <see cref="GetGeometry"/>)
        /// </summary>
        /// <returns>The shape</returns>
        public override GeometryBase GetGeometryBase() => GetGeometry();

        /// <summary>
        /// The total load in the coordinate system of the shape
        /// </summary>
        /// <returns>(0, 0, pressure * area)</returns>
        public Vector3d GetLocalLoadVector()
        {
            return new Vector3d(0, 0, _pressure * _shape.GetArea());
        }

        /// <summary>
        /// The total load in the global system
        /// </summary>
        /// <returns>(0, 0, pressure * area) rotated from the coordinate system of the shape to the global one</returns>
        public Vector3d GetGlobalLoadVector()
        {
            if (_shape.GetCoordinateSystem() == CoordinateSystem.Global)
                return GetLocalLoadVector();
            else
                return _shape.GetCoordinateSystem().ToGlobal(GetLocalLoadVector());
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes the data of <see cref="Load"/>, pressure, shape and the coordinate system (a second time: duplicated name, the serialization throws)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Pressure", _pressure);
            info.AddValue("Shape", _shape);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        /// <summary>
        /// Equality of name, load case, coordinate system, shape and pressure (exact)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal normal area load</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is NormalAreaLoad objCasted) &&
                _shape.Equals(objCasted._shape) &&
                _pressure.Equals(objCasted._pressure) &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of name, load case, coordinate system, pressure and shape
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _pressure.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Shape>.Default.GetHashCode(_shape);
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null loads are equal
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are equal</returns>
        public static bool operator ==(NormalAreaLoad obj1, NormalAreaLoad obj2)
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
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are different</returns>
        public static bool operator !=(NormalAreaLoad obj1, NormalAreaLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
