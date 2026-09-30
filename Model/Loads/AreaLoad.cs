using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A uniform load on an area: force per unit area with components in the coordinate system of the load
    /// </summary>
    [Serializable]
    public class AreaLoad : Load, IAreaLoad
    {
        #region Variables

        /// <summary>
        /// The load per unit area along V1
        /// </summary>
        protected double _p1;
        /// <summary>
        /// The load per unit area along V2
        /// </summary>
        protected double _p2;
        /// <summary>
        /// The load per unit area along V3
        /// </summary>
        protected double _p3;
        /// <summary>
        /// The area, in the global coordinates
        /// </summary>
        protected Shape _shape;

        #endregion

        #region Properties

        /// <summary>
        /// Load component in V1 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double P1 { get => _p1; set => _p1 = value; }

        /// <summary>
        /// Load component in V2 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double P2 { get => _p2; set => _p2 = value; }

        /// <summary>
        /// Load component in V3 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double P3 { get => _p3; set => _p3 = value; }

        /// <summary>
        /// The area, in the global reference system
        /// </summary>
        public Shape Shape { get => _shape; set => _shape = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates an area load
        /// </summary>
        /// <param name="p1">The load per unit area along V1 of the coordinate system</param>
        /// <param name="p2">The load per unit area along V2 of the coordinate system</param>
        /// <param name="p3">The load per unit area along V3 of the coordinate system</param>
        /// <param name="shape">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">Reference system of the load</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If the shape is null</exception>
        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;

            _shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
            _coordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Creates an area load with components in the global system
        /// </summary>
        /// <param name="p1">The load per unit area along X</param>
        /// <param name="p2">The load per unit area along Y</param>
        /// <param name="p3">The load per unit area along Z</param>
        /// <param name="shape">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <remarks><see cref="CoordinateSystem"/> set to <see cref="CoordinateSystem.Global"/></remarks>
        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCaseBase loadCase, string name = "", int id = IDUNASSIGNED)
            : this(p1, p2, p3, shape, loadCase, CoordinateSystem.Global, name, id)
        {

        }

        /// <summary>
        /// Reads the base load, intensity components and loaded shape.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected AreaLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _p1 = info.GetDouble("P1"); _p2 = info.GetDouble("P2"); _p3 = info.GetDouble("P3");
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
        }

        #endregion

        #region Public Methods

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
        /// Convert this load into a normal area load: the component of the total load along the normal of the shape (in the coordinate system of
        /// the shape, see <see cref="Shape.GetCoordinateSystem"/>)
        /// </summary>
        /// <returns>The pressure normal to the shape</returns>
        /// <remarks>The not normal portion will be lost</remarks>
        public virtual NormalAreaLoad ConvertToNormalAreaLoad()
        {
            Vector3d globalLoad = this.GetGlobalLoadVector();
            CoordinateSystem coordinateSystem = _shape.GetCoordinateSystem();
            return new NormalAreaLoad(coordinateSystem.ToLocal(globalLoad).Z, _shape, LoadCase, coordinateSystem);
        }

        /// <summary>
        /// The total load in the coordinate system of the load
        /// </summary>
        /// <returns>The total load vector in the local system. i.e. _p1 * area, _p2 * area, _p3 * area</returns>
        public Vector3d GetLocalLoadVector()
        {
            double area = _shape.GetArea();
            return new Vector3d(_p1 * area, _p2 * area, _p3 * area);
        }

        /// <summary>
        /// The total load in the global system
        /// </summary>
        /// <returns>The total load vector (p1, p2, p3) * area rotated to the global system</returns>
        public Vector3d GetGlobalLoadVector()
        {
            if (_coordinateSystem == CoordinateSystem.Global)
                return GetLocalLoadVector();
            else
            {
                return _coordinateSystem.ToGlobal(GetLocalLoadVector());
            }
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes the base load, intensity components and loaded shape.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("P1", _p1); info.AddValue("P2", _p2); info.AddValue("P3", _p3); info.AddValue("Shape", _shape);
        }

        /// <summary>
        /// Equality of name, load case, coordinate system, shape and components (exact)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal area load</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is AreaLoad objCasted) && _shape.Equals(objCasted._shape)
                                               && _coordinateSystem.Equals(objCasted._coordinateSystem)
                                               && _p1.Equals(objCasted._p1) && _p2.Equals(objCasted._p2) && _p3.Equals(objCasted._p3)
                                               && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of name, load case, coordinate system, components and shape
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _p1.GetHashCode();
                hashCode = hashCode * -17 + _p2.GetHashCode();
                hashCode = hashCode * -17 + _p3.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
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
        public static bool operator ==(AreaLoad obj1, AreaLoad obj2)
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
        public static bool operator !=(AreaLoad obj1, AreaLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
