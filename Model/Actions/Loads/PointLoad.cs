using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A concentrated load: force and moment in a point, with components in the coordinate system of the load
    /// </summary>
    [Serializable]
    public class PointLoad : Load, IPointLoad, IConvertibleLoad
    {
        #region Variables

        /// <summary>
        /// The force along V1
        /// </summary>
        protected double _f1;
        /// <summary>
        /// The force along V2
        /// </summary>
        protected double _f2;
        /// <summary>
        /// The force along V3
        /// </summary>
        protected double _f3;
        /// <summary>
        /// The moment around V1
        /// </summary>
        protected double _m1;
        /// <summary>
        /// The moment around V2
        /// </summary>
        protected double _m2;
        /// <summary>
        /// The moment around V3
        /// </summary>
        protected double _m3;

        /// <summary>
        /// The point, in the global coordinates
        /// </summary>
        protected Point3d _point;

        #endregion

        #region Properties

        /// <summary>
        /// Force load component in V1 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double F1 { get => _f1; set => _f1 = value; }

        /// <summary>
        /// Force load component in V2 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double F2 { get => _f2; set => _f2 = value; }

        /// <summary>
        /// Force load component in V3 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double F3 { get => _f3; set => _f3 = value; }

        /// <summary>
        /// Moment load component around V1 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double M1 { get => _m1; set => _m1 = value; }

        /// <summary>
        /// Moment load component around V2 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double M2 { get => _m2; set => _m2 = value; }

        /// <summary>
        /// Moment load component around V3 vector <see cref="CoordinateSystem"/>
        /// </summary>
        public double M3 { get => _m3; set => _m3 = value; }

        /// <summary>
        /// Point in the global reference system
        /// </summary>
        public Point3d Point { get => _point; set => _point = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a point load
        /// </summary>
        /// <param name="f1">The force along V1 of the coordinate system</param>
        /// <param name="f2">The force along V2 of the coordinate system</param>
        /// <param name="f3">The force along V3 of the coordinate system</param>
        /// <param name="m1">The moment around V1 of the coordinate system</param>
        /// <param name="m2">The moment around V2 of the coordinate system</param>
        /// <param name="m3">The moment around V3 of the coordinate system</param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">Reference system of the load</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If the coordinate system or the point is null</exception>
        public PointLoad(double f1, double f2, double f3, double m1, double m2, double m3, Point3d point, LoadCaseBase loadCase,
            CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            _f1 = f1;
            _f2 = f2;
            _f3 = f3;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException("Coordinate system cannot be null");
            _point = point ?? throw new ArgumentNullException("Point cannot be null");
        }

        /// <summary>
        /// Creates a point load with components in the global system
        /// </summary>
        /// <param name="f1">The force along V1 of the coordinate system</param>
        /// <param name="f2">The force along V2 of the coordinate system</param>
        /// <param name="f3">The force along V3 of the coordinate system</param>
        /// <param name="m1">The moment around V1 of the coordinate system</param>
        /// <param name="m2">The moment around V2 of the coordinate system</param>
        /// <param name="m3">The moment around V3 of the coordinate system</param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <remarks> <see cref="CoordinateSystem"/> set to Global </remarks>
        public PointLoad(double f1, double f2, double f3, double m1, double m2, double m3, Point3d point, LoadCaseBase loadCase,
            string name = "", int id = IDUNASSIGNED)
            : this(f1, f2, f3, m1, m2, m3, point, loadCase, CoordinateSystem.Global, name, id)
        {

        }

        /// <summary>
        /// Creates a point load from vectors
        /// </summary>
        /// <param name="force">The force, in the coordinate system of the load</param>
        /// <param name="moment">The moment, in the coordinate system of the load</param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">Reference system of the load</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public PointLoad(Vector3d force, Vector3d moment, Point3d point, LoadCaseBase loadCase, CoordinateSystem coordinateSystem,
            string name = "", int id = IDUNASSIGNED)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, point, loadCase, coordinateSystem, name, id)
        {

        }

        /// <summary>
        /// Creates a point load from vectors in the global system
        /// </summary>
        /// <param name="force">The force, in the global system</param>
        /// <param name="moment">The moment, in the global system</param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public PointLoad(Vector3d force, Vector3d moment, Point3d point, LoadCaseBase loadCase, string name = "", int id = IDUNASSIGNED)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, point, loadCase, CoordinateSystem.Global, name, id)
        {

        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Load"/>, the components and the point
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected PointLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _f1 = info.GetDouble("F1");
            _f2 = info.GetDouble("F2");
            _f3 = info.GetDouble("F3");
            _m1 = info.GetDouble("M1");
            _m2 = info.GetDouble("M2");
            _m3 = info.GetDouble("M3");
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// The point of the load
        /// </summary>
        /// <returns>The point, in the global coordinates</returns>
        public Point3d GetGeometry() => _point;

        /// <summary>
        /// The point of the load (see <see cref="GetGeometry"/>)
        /// </summary>
        /// <returns>The point</returns>
        public override GeometryBase GetGeometryBase() => GetGeometry();

        /// <summary>
        /// The force and the moment in the coordinate system of the load
        /// </summary>
        /// <returns>The vectors (F1, F2, F3) and (M1, M2, M3)</returns>
        public (Vector3d force, Vector3d moment) GetLocalLoadVector()
        {
            return (new Vector3d(_f1, _f2, _f3), new Vector3d(_m1, _m2, _m3));
        }

        /// <summary>
        /// The force and the moment in the global system
        /// </summary>
        /// <returns>The vectors of the force and of the moment in the global system</returns>
        public (Vector3d force, Vector3d moment) GetGlobalLoadVector()
        {
            if (_coordinateSystem == CoordinateSystem.Global)
                return GetLocalLoadVector();
            else
                return (_coordinateSystem.ToGlobal(GetLocalLoadVector().force), _coordinateSystem.ToGlobal(GetLocalLoadVector().moment));
        }

        /// <summary>
        /// Return an array with forces and moments in local coordinate system <paramref name="cSys"/>
        /// </summary>
        /// <param name="cSys">The coordinate system</param>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        public double[] GetLocalForces(CoordinateSystem cSys)
        {
            // Crea un array di double in le 3 componenti di forza e di momento
            // nelle 3 direzioni del sistema di coordinate locali.

            PointLoad local = ToLocal(cSys);

            double[] pointLoad = new double[6];
            pointLoad[0] = local.F1;
            pointLoad[1] = local.F2;
            pointLoad[2] = local.F3;
            pointLoad[3] = local.M1;
            pointLoad[4] = local.M2;
            pointLoad[5] = local.M3;

            return pointLoad;
        }

        /// <summary>
        /// Return the PointLoad in a global coordinate system
        /// </summary>
        /// <returns>A new load with the components in the global system (same point and load case, without name and id)</returns>
        public PointLoad ToGlobal()
        {
            Vector3d forceLocal = new Vector3d(_f1, _f2, _f3);
            Vector3d momentLocal = new Vector3d(_m1, _m2, _m3);

            // Cambia le proprietà del PointLoad passando da un sistema di riferimento globale
            // ad un sistema di rifarimento locale.                         

            return new PointLoad(_coordinateSystem.ToGlobal(forceLocal), _coordinateSystem.ToGlobal(momentLocal), _point, LoadCase, CoordinateSystem.Global);
        }

        /// <summary>
        /// Return the PointLoad in a local coordinate system
        /// </summary>
        /// <param name="cSys">The coordinate system</param>
        /// <returns>A new load with the components in <paramref name="cSys"/> (same point and load case, without name and id)</returns>
        public PointLoad ToLocal(CoordinateSystem cSys)
        {
            Vector3d forceLocal = cSys.ToLocal(_coordinateSystem.ToGlobal(new Vector3d(_f1, _f2, _f3)));
            Vector3d momentLocal = cSys.ToLocal(_coordinateSystem.ToGlobal(new Vector3d(_m1, _m2, _m3)));

            return new PointLoad(forceLocal, momentLocal, _point, LoadCase, cSys);
        }

        /// <summary>
        /// Return an array with forces and moments in global coordinate system
        /// </summary>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        public double[] GetGlobalForces()
        {
            // Crea un array di double in le 3 componenti di forza e di momento
            // nelle 3 direzioni del sistema di coordinate globali.
            // NodeElement point = _coordinateSystem.ToGlobal(_point);             

            Vector3d forceLocal = new Vector3d(_f1, _f2, _f3);
            Vector3d momentLocal = new Vector3d(_m1, _m2, _m3);

            double fX = _coordinateSystem.ToGlobal(forceLocal).X;
            double fY = _coordinateSystem.ToGlobal(forceLocal).Y;
            double fZ = _coordinateSystem.ToGlobal(forceLocal).Z;
            double mX = _coordinateSystem.ToGlobal(momentLocal).X;
            double mY = _coordinateSystem.ToGlobal(momentLocal).Y;
            double mZ = _coordinateSystem.ToGlobal(momentLocal).Z;

            double[] pointLoad = new double[6];
            pointLoad[0] = fX;
            pointLoad[1] = fY;
            pointLoad[2] = fZ;
            pointLoad[3] = mX;
            pointLoad[4] = mY;
            pointLoad[5] = mZ;

            return pointLoad;
        }

        /// <summary>
        /// Convert this load into an area load: the force is spread on a square of side <paramref name="width"/> centered in the point, on the
        /// reference plane
        /// </summary>
        /// <param name="referencePlane">The plane where the square is built</param>
        /// <param name="width">The side of the square</param>
        /// <returns>The area load with the same total force, components in the global system</returns>
        /// <remarks>Moments will be lost. The shape of the area load is in the local coordinates of the plane, while the area load expects
        /// global coordinates (see the list of the defects found)</remarks>
        public virtual AreaLoad ConvertToAreaLoad(Plane referencePlane, double width)
        {
            Point3d point = (Point3d)_point.Clone();

            CoordinateSystem referenceCoordinateSystem = referencePlane.GetCoordinateSystem();

            Vector3d normalVector = referencePlane.Normal;
            normalVector.Unitize();

            Vector3d movementVector1 = referenceCoordinateSystem.V1;
            movementVector1.Unitize();
            movementVector1 *= width;
            movementVector1 /= 2.0;

            Vector3d movementVector2 = referenceCoordinateSystem.V2;
            movementVector2.Unitize();
            movementVector2 *= width;
            movementVector2 /= 2.0;

            Point3d p1 = point.CloneAndMove(movementVector1 + movementVector2);
            movementVector1.Reverse();
            Point3d p2 = point.CloneAndMove(movementVector1 + movementVector2);
            movementVector2.Reverse();
            Point3d p3 = point.CloneAndMove(movementVector2 + movementVector1);
            movementVector1.Reverse();
            Point3d p4 = point.CloneAndMove(movementVector2 + movementVector1);

            var loadPerimeterGlobal = new Polygon3d() { p1, p2, p3, p4 };

            if (!loadPerimeterGlobal.IsRightHandOrdered())
                loadPerimeterGlobal.Reverse();

            var loadPerimeterReference = referenceCoordinateSystem.ToLocal(loadPerimeterGlobal);

            // devo convertire il carico dal sistema _coordinateSystem
            // al sistema del referencePlane

            Shape loadShape = new Shape(loadPerimeterReference); // TODO: tagliare con bordo esterno shape nel caso sbordi
            double loadArea = loadShape.GetArea();

            Vector3d loadVector = new Vector3d(F1, F2, F3);
            var loadVectorGlobal = _coordinateSystem.ToGlobal(loadVector); // da locale a globale


            return new AreaLoad(loadVectorGlobal.X / loadArea, loadVectorGlobal.Y / loadArea, loadVectorGlobal.Z / loadArea, loadShape, LoadCase, CoordinateSystem.Global);
        }

        /// <summary>
        /// Convert this load into a pressure normal to the reference plane on a square of side <paramref name="width"/> centered in the point
        /// </summary>
        /// <param name="referencePlane">The plane where the square is built</param>
        /// <param name="width">The side of the square</param>
        /// <returns>The normal area load with the same total normal force</returns>
        /// <remarks>Moments and the force parallel to the plane are lost. The point is moved as if it were in the coordinate system of the load
        /// (it is in the global one) and the shape is in the local coordinates of the plane (see the list of the defects found)</remarks>
        public virtual NormalAreaLoad ConvertToNormalAreaLoad(Plane referencePlane, double width)
        {
            Point3d point = (Point3d)_point.Clone();

            CoordinateSystem referenceCoordinateSystem = referencePlane.GetCoordinateSystem();

            Vector3d normalVector = referencePlane.Normal;
            normalVector.Unitize();

            Point3d pointGlobal = CoordinateSystem.ToGlobal(point);

            Vector3d movementVector1 = referenceCoordinateSystem.V1;
            movementVector1.Unitize();
            movementVector1 *= width;
            movementVector1 /= 2.0;

            Vector3d movementVector2 = referenceCoordinateSystem.V2;
            movementVector2.Unitize();
            movementVector2 *= width;
            movementVector2 /= 2.0;

            Point3d p1 = pointGlobal.CloneAndMove(movementVector1 + movementVector2);
            movementVector1.Reverse();
            Point3d p2 = pointGlobal.CloneAndMove(movementVector1 + movementVector2);
            movementVector2.Reverse();
            Point3d p3 = pointGlobal.CloneAndMove(movementVector2 + movementVector1);
            movementVector1.Reverse();
            Point3d p4 = pointGlobal.CloneAndMove(movementVector2 + movementVector1);

            var loadPerimeterGlobal = new Polygon3d() { p1, p2, p3, p4 };

            if (!loadPerimeterGlobal.IsRightHandOrdered())
                loadPerimeterGlobal.Reverse();

            Polygon2d loadPerimeterReference = referenceCoordinateSystem.ToLocal(loadPerimeterGlobal);

            Shape loadShape = new Shape(loadPerimeterReference); // TODO: tagliare con bordo esterno shape nel caso sbordi
            double loadArea = loadShape.GetArea();

            Vector3d loadVector = new Vector3d(F1, F2, F3);
            Vector3d loadVectorGlobal = _coordinateSystem.ToGlobal(loadVector);
            Vector3d loadVectorReference = referenceCoordinateSystem.ToLocal(loadVectorGlobal);

            return new NormalAreaLoad(loadVectorReference.Z / loadArea, loadShape, LoadCase, referenceCoordinateSystem);
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Equality of name, load case, coordinate system, point and components (exact)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal point load</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is PointLoad objCasted) &&
                _point.Equals(objCasted._point) &&
                _coordinateSystem.Equals(objCasted._coordinateSystem) &&
                _f1.Equals(objCasted._f1) &&
                _f2.Equals(objCasted._f2) &&
                _f3.Equals(objCasted._f3) &&
                _m1.Equals(objCasted._m1) &&
                _m2.Equals(objCasted._m2) &&
                _m3.Equals(objCasted._m3) &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of name, load case, coordinate system, components and point
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _f1.GetHashCode();
                hashCode = hashCode * -17 + _f2.GetHashCode();
                hashCode = hashCode * -17 + _f3.GetHashCode();
                hashCode = hashCode * -17 + _m1.GetHashCode();
                hashCode = hashCode * -17 + _m2.GetHashCode();
                hashCode = hashCode * -17 + _m3.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_point);
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null loads are equal
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are equal</returns>
        public static bool operator ==(PointLoad obj1, PointLoad obj2)
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
        public static bool operator !=(PointLoad obj1, PointLoad obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// Serializes the data of <see cref="Load"/>, the components, the coordinate system (a second time: duplicated name, the serialization
        /// throws) and the point
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("F1", _f1);
            info.AddValue("F2", _f2);
            info.AddValue("F3", _f3);
            info.AddValue("M1", _m1);
            info.AddValue("M2", _m2);
            info.AddValue("M3", _m3);

            info.AddValue("Point", _point);
        }

        #endregion
    }
}
