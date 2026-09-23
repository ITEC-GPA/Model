using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    public class PointLoad : Load, IPointLoad, IConvertibleLoad
    {
        #region Variables

        protected double _f1;
        protected double _f2;
        protected double _f3;
        protected double _m1;
        protected double _m2;
        protected double _m3;

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

        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="f3"></param>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <param name="m3"></param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase"></param>
        /// <param name="coordinateSystem">Reference system of the load</param>
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

        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="f3"></param>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <param name="m3"></param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase"></param>
        /// <remarks> <see cref="CoordinateSystem"/> set to Global </remarks>
        public PointLoad(double f1, double f2, double f3, double m1, double m2, double m3, Point3d point, LoadCaseBase loadCase,
            string name = "", int id = IDUNASSIGNED)
            : this(f1, f2, f3, m1, m2, m3, point, loadCase, CoordinateSystem.Global, name, id)
        {

        }

        /// <param name="force"></param>
        /// <param name="moment"></param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase"></param>
        /// <param name="coordinateSystem">Reference system of the load</param>
        public PointLoad(Vector3d force, Vector3d moment, Point3d point, LoadCaseBase loadCase, CoordinateSystem coordinateSystem,
            string name = "", int id = IDUNASSIGNED)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, point, loadCase, coordinateSystem, name, id)
        {

        }

        /// <param name="force"></param>
        /// <param name="moment"></param>
        /// <param name="point">In the global reference system</param>
        /// <param name="loadCase"></param>
        public PointLoad(Vector3d force, Vector3d moment, Point3d point, LoadCaseBase loadCase, string name = "", int id = IDUNASSIGNED)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, point, loadCase, CoordinateSystem.Global, name, id)
        {

        }

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

        public Point3d GetGeometry() => _point;

        public override GeometryBase GetGeometryBase() => GetGeometry();

        /// <returns>The total local load vector in the local system. i.e. _f1 , _f2 , _f3 </returns>
        public (Vector3d force, Vector3d moment) GetLocalLoadVector()
        {
            return (new Vector3d(_f1, _f2, _f3), new Vector3d(_m1, _m2, _m3));
        }

        /// <returns>The total global load vector in the local system. i.e. _f1  , _f2 , _f3 </returns>
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
        /// <param name="cSys"></param>
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

        /// <inheritdoc cref="IConvertibleLoad.ConvertToAreaLoad(Plane, double)"/>
        /// <remarks>Moments will be lost. Reference system of the load is the global system</remarks>
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

        public static bool operator !=(PointLoad obj1, PointLoad obj2)
        {
            return !(obj1 == obj2);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("F1", _f1);
            info.AddValue("F2", _f2);
            info.AddValue("F3", _f3);
            info.AddValue("M1", _m1);
            info.AddValue("M2", _m2);
            info.AddValue("M3", _m3);
            info.AddValue("CoordinateSystem", _coordinateSystem);
            info.AddValue("Point", _point);
        }

        #endregion
    }
}
