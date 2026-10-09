using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A uniform load along a line: force and moment per unit length, with components in the coordinate system of the load
    /// </summary>
    [Serializable]
    public class LineLoad : Load, ILineLoad, IConvertibleLoad
    {
        #region Variables

        /// <summary>
        /// The force per unit length along V1 [F/L]
        /// </summary>
        protected double _f1;
        /// <summary>
        /// The force per unit length along V2 [F/L]
        /// </summary>
        protected double _f2;
        /// <summary>
        /// The force per unit length along V3 [F/L]
        /// </summary>
        protected double _f3;

        /// <summary>
        /// The moment per unit length around V1 [FL/L]
        /// </summary>
        protected double _m1;
        /// <summary>
        /// The moment per unit length around V2 [FL/L]
        /// </summary>
        protected double _m2;
        /// <summary>
        /// The moment per unit length around V3 [FL/L]
        /// </summary>
        protected double _m3;

        /// <summary>
        /// The line, in the global coordinates
        /// </summary>
        protected Line3d _line;

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
        /// Line in the global reference system
        /// </summary>
        public Line3d Line { get => _line; set => _line = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a line load
        /// </summary>
        /// <param name="f1">Force per unit length along V1 of the coordinate system [F/L]</param>
        /// <param name="f2">Force per unit length along V2 of the coordinate system [F/L]</param>
        /// <param name="f3">Force per unit length along V3 of the coordinate system [F/L]</param>
        /// <param name="m1">Moment per unit length around V1 of the coordinate system [FL/L]</param>
        /// <param name="m2">Moment per unit length around V2 of the coordinate system [FL/L]</param>
        /// <param name="m3">Moment per unit length around V3 of the coordinate system [FL/L]</param>
        /// <param name="line">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">Orientation of the load</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If the line is null</exception>
        public LineLoad(double f1, double f2, double f3, double m1, double m2, double m3, Line3d line, LoadCaseBase loadCase,
            CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            _f1 = f1;
            _f2 = f2;
            _f3 = f3;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
            _line = line ?? throw new ArgumentNullException("Line cannot be null");
        }

        /// <summary>
        /// Creates a line load with components in the global system
        /// </summary>
        /// <param name="f1">Force per unit length along V1 of the coordinate system [F/L]</param>
        /// <param name="f2">Force per unit length along V2 of the coordinate system [F/L]</param>
        /// <param name="f3">Force per unit length along V3 of the coordinate system [F/L]</param>
        /// <param name="m1">Moment per unit length around V1 of the coordinate system [FL/L]</param>
        /// <param name="m2">Moment per unit length around V2 of the coordinate system [FL/L]</param>
        /// <param name="m3">Moment per unit length around V3 of the coordinate system [FL/L]</param>
        /// <param name="line">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <remarks> <see cref="CoordinateSystem"/> set to Global </remarks>
        public LineLoad(double f1, double f2, double f3, double m1, double m2, double m3, Line3d line, LoadCaseBase loadCase, string name = "", int id = IDUNASSIGNED)
            : this(f1, f2, f3, m1, m2, m3, line, loadCase, CoordinateSystem.Global, name, id)
        {

        }

        /// <summary>
        /// Creates a line load from vectors
        /// </summary>
        /// <param name="force">The force per unit length, in the coordinate system of the load</param>
        /// <param name="moment">The moment per unit length, in the coordinate system of the load</param>
        /// <param name="line">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">Orientation of the load</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public LineLoad(Vector3d force, Vector3d moment, Line3d line, LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, line, loadCase, coordinateSystem, name, id)
        {

        }

        /// <summary>
        /// Creates a line load from vectors in the global system
        /// </summary>
        /// <param name="force">The force per unit length, in the global system</param>
        /// <param name="moment">The moment per unit length, in the global system</param>
        /// <param name="line">In the global reference system</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public LineLoad(Vector3d force, Vector3d moment, Line3d line, LoadCaseBase loadCase, string name = "", int id = IDUNASSIGNED)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, line, loadCase, CoordinateSystem.Global, name, id)
        {

        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Load"/>, the components, the coordinate system and the line
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected LineLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _f1 = info.GetDouble("F1");
            _f2 = info.GetDouble("F2");
            _f3 = info.GetDouble("F3");
            _m1 = info.GetDouble("M1");
            _m2 = info.GetDouble("M2");
            _m3 = info.GetDouble("M3");
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
            _line = (Line3d)info.GetValue("Line3d", typeof(Line3d));
        }

        #endregion

        #region Convert to area load

        /// <summary>
        /// Convert this load into an area load: the force is spread on a strip of width <paramref name="width"/> centered on the line, on the
        /// reference plane (normal × line direction)
        /// </summary>
        /// <param name="referencePlane">The plane of the strip</param>
        /// <param name="width">The width of the strip</param>
        /// <returns>The area load with the same total force, components and shape in the global system</returns>
        /// <remarks>Moments will be lost. Reference system of the load is the global system</remarks>
        public virtual AreaLoad ConvertToAreaLoad(Plane referencePlane, double width)
        {

            Line3d line = (Line3d)_line.Clone();
            double lineLenght = _line.GetLength();

            //CoordinateSystem referenceCoordinateSystem = referencePlane.GetCoordinateSystem();

            Vector3d normalVector = referencePlane.Normal;
            normalVector.Unitize();

            //var lineGlobal = CoordinateSystem.ToGlobal(line);

            Vector3d lineVector = new Vector3d(line.Start, line.End);
            Vector3d movementVector = normalVector.CrossProduct(lineVector); // calcolo il vettore spostamento come prodottovettore tra la normale del piano di rif e la linea

            movementVector.Unitize();
            movementVector *= width;
            movementVector /= 2.0;

            Point3d p1 = line.Start.CloneAndMove(movementVector);
            Point3d p2 = line.End.CloneAndMove(movementVector);
            movementVector.Reverse();
            Point3d p3 = line.End.CloneAndMove(movementVector);
            Point3d p4 = line.Start.CloneAndMove(movementVector);

            var loadPerimeterGlobal = new Polygon3d()
                                    {
                                        p1,
                                        p2,
                                        p3,
                                        p4
                                    };

            if (!loadPerimeterGlobal.IsRightHandOrdered())
            {
                loadPerimeterGlobal.Reverse();
            }

            // creo shape con coordinate nel globale
            Shape loadShape = new Shape(loadPerimeterGlobal); // TODO: tagliare con bordo esterno shape nel caso sbordi


            // devo convertire il carico dal sistema _coordinateSystem
            // al sistema globale

            Vector3d loadVector = new Vector3d(F1, F2, F3); // carico nel _coordinateSystem            
            var loadVectorGlobal = _coordinateSystem.ToGlobal(loadVector); // da locale a globale

            double loadArea = loadShape.GetArea();

            return new AreaLoad(loadVectorGlobal.X * lineLenght / loadArea,
                                loadVectorGlobal.Y * lineLenght / loadArea,
                                loadVectorGlobal.Z * lineLenght / loadArea,
                                loadShape,
                                LoadCase,
                                CoordinateSystem.Global);
        }


        /// <summary>
        /// Convert this load into a pressure normal to the reference plane on a strip of width <paramref name="width"/> centered on the line
        /// </summary>
        /// <param name="referencePlane">The plane of the strip</param>
        /// <param name="width">The width of the strip</param>
        /// <returns>The normal area load with the same total normal force</returns>
        /// <remarks>Moments will be lost. Only the force normal part will be kept. The shape is in the local coordinates of the plane, while the
        /// normal area load expects global coordinates (see the list of the defects found)</remarks>
        public virtual NormalAreaLoad ConvertToNormalAreaLoad(Plane referencePlane, double width)
        {
            Line3d line = (Line3d)_line.Clone();
            double lineLenght = _line.GetLength();

            CoordinateSystem referenceCoordinateSystem = referencePlane.GetCoordinateSystem();

            Vector3d normalVector = referencePlane.Normal;
            normalVector.Unitize();

            //var lineGlobal = CoordinateSystem.ToGlobal(line);

            Vector3d lineVector = new Vector3d(line.Start, line.End); // converto linea nel globale
            Vector3d movementVector = normalVector.CrossProduct(lineVector); // calcolo il vettore spostamento come prodottovettore tra la normale del piano di rif e la linea

            movementVector.Unitize();
            movementVector *= width;
            movementVector /= 2.0;

            Point3d p1 = line.Start.CloneAndMove(movementVector);
            Point3d p2 = line.End.CloneAndMove(movementVector);
            movementVector.Reverse();
            Point3d p3 = line.End.CloneAndMove(movementVector);
            Point3d p4 = line.Start.CloneAndMove(movementVector);

            Polygon3d loadPerimeterGlobal = new Polygon3d() { p1, p2, p3, p4 };

            if (!loadPerimeterGlobal.IsRightHandOrdered())
            {
                loadPerimeterGlobal.Reverse();
            }

            Polygon2d loadPerimeterReference = referenceCoordinateSystem.ToLocal(loadPerimeterGlobal);

            Shape loadShape = new Shape(loadPerimeterReference); // TODO: tagliare con bordo esterno shape nel caso sbordi
            double loadArea = loadShape.GetArea();

            Vector3d loadVector = new Vector3d(F1, F2, F3);
            Vector3d loadVectorGlobal = _coordinateSystem.ToGlobal(loadVector);
            Vector3d loadVectorReference = referenceCoordinateSystem.ToLocal(loadVectorGlobal);

            return new NormalAreaLoad(loadVectorReference.Z * lineLenght / loadArea, loadShape, LoadCase, referenceCoordinateSystem);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The line of the load
        /// </summary>
        /// <returns>The segment, in the global coordinates</returns>
        public Line3d GetGeometry() => _line;

        /// <summary>
        /// The line of the load (see <see cref="GetGeometry"/>)
        /// </summary>
        /// <returns>The segment</returns>
        public override GeometryBase GetGeometryBase() => GetGeometry();

        /// <summary>
        /// The total force and moment in the coordinate system of the load
        /// </summary>
        /// <returns>The total local load vector in the local system. i.e. _f1 * lenght , _f2 * lenght, _f3 * lenght</returns>
        public (Vector3d force, Vector3d moment) GetLocalLoadVector()
        {
            double line = _line.GetLength();
            return (new Vector3d(_f1 * line, _f2 * line, _f3 * line), new Vector3d(_m1 * line, _m2 * line, _m3 * line));
        }

        /// <summary>
        /// The total force and moment in the global system
        /// </summary>
        /// <returns>The total load vectors rotated to the global system</returns>
        public (Vector3d force, Vector3d moment) GetGlobalLoadVector()
        {
            if (_coordinateSystem == CoordinateSystem.Global)
                return GetLocalLoadVector();
            else
            {
                return (_coordinateSystem.ToGlobal(GetLocalLoadVector().force), _coordinateSystem.ToGlobal(GetLocalLoadVector().moment));
            }
        }

        /// <summary>
        /// Forces and moments per unit length in the coordinate system of the load (see <see cref="GetLocalForces(CoordinateSystem)"/>)
        /// </summary>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        public double[] GetLocalForces()
        {
            return GetLocalForces(_coordinateSystem);
        }

        /// <summary>
        /// Return an array with forces and moments per unit length in the local coordinate system <paramref name="cSys"/>
        /// </summary>
        /// <param name="cSys">The coordinate system</param>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        /// <remarks>The components are rotated as if they were in the global system, not in the coordinate system of the load (see the list of the defects found)</remarks>
        public double[] GetLocalForces(CoordinateSystem cSys)
        {
            // Crea un array di double in le 3 componenti di forza e di momento
            // nelle 3 direzioni del sistema di coordinate locali.

            Point3d forceGlobal = new Point3d(_f1, _f2, _f3);
            Point3d momentglobal = new Point3d(_m1, _m2, _m3);

            Point3d PointForceLocal = cSys.ToLocal(forceGlobal);
            Point3d PointMomentLocal = cSys.ToLocal(momentglobal);

            Point3d OriginGlobal = cSys.ToLocal(CoordinateSystem.Global.Origin);

            Vector3d forceLocal = new Vector3d(PointForceLocal - OriginGlobal);
            Vector3d momentLocal = new Vector3d(PointMomentLocal - OriginGlobal);

            double[] pointLoad = new double[6];
            pointLoad[0] = forceLocal.X;
            pointLoad[1] = forceLocal.Y;
            pointLoad[2] = forceLocal.Z;
            pointLoad[3] = momentLocal.X;
            pointLoad[4] = momentLocal.Y;
            pointLoad[5] = momentLocal.Z;

            return pointLoad;
        }

        /// <summary>
        /// Return the lineload in a global coordinate system
        /// </summary>
        /// <returns>A new load with the components in the global system (same line and load case, without name and id)</returns>
        public LineLoad ToGlobal()
        {
            // Cambia le proprietà del LineLoad passando da un sistema di riferimento globale
            // ad un sistema di rifarimento locale.

            return new LineLoad(_coordinateSystem.ToGlobal(new Vector3d(_f1, _f2, _f3)), _coordinateSystem.ToGlobal(new Vector3d(_m1, _m2, _m3)),
                _line, LoadCase, CoordinateSystem.Global);
        }

        /// <summary>
        /// Return the lineload in a local coordinate system
        /// </summary>
        /// <param name="cSys">The coordinate system</param>
        /// <returns>A new load with the components in <paramref name="cSys"/> (same line and load case, without name and id)</returns>
        public LineLoad ToLocal(CoordinateSystem cSys)
        {
            Vector3d forceLocal = cSys.ToLocal(_coordinateSystem.ToGlobal(new Vector3d(_f1, _f2, _f3)));
            Vector3d momentLocal = cSys.ToLocal(_coordinateSystem.ToGlobal(new Vector3d(_m1, _m2, _m3)));
            // Line3d lineLocal = cSys.ToLocal(_coordinateSystem.ToGlobal(_line)); la linea non va cambiata dato che sempre riferita al globale

            return new LineLoad(forceLocal, momentLocal, _line, LoadCase, cSys);
        }

        /// <summary>
        /// Return an array with forces and moments per unit length in global coordinate system
        /// </summary>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        /// <remarks>The origin of the coordinate system is subtracted from the rotated vectors: the result is wrong if the origin is not at
        /// the origin of the axes (see the list of the defects found)</remarks>
        public double[] GetGlobalForces()
        {
            // Crea un array di double in le 3 componenti di forza e di momento nelle 3 direzioni del sistema di coordinate globali
            // Point3d point = _coordinateSystem.ToGlobal(_point);

            Vector3d forceLocal = new Vector3d(_f1, _f2, _f3);
            Vector3d momentLocal = new Vector3d(_m1, _m2, _m3);

            double fX = _coordinateSystem.ToGlobal(forceLocal).X - _coordinateSystem.Origin.X;
            double fY = _coordinateSystem.ToGlobal(forceLocal).Y - _coordinateSystem.Origin.Y;
            double fZ = _coordinateSystem.ToGlobal(forceLocal).Z - _coordinateSystem.Origin.Z;
            double mX = _coordinateSystem.ToGlobal(momentLocal).X - _coordinateSystem.Origin.X;
            double mY = _coordinateSystem.ToGlobal(momentLocal).Y - _coordinateSystem.Origin.Y;
            double mZ = _coordinateSystem.ToGlobal(momentLocal).Z - _coordinateSystem.Origin.Z;

            double[] PointLoad = new double[6];
            PointLoad[0] = fX;
            PointLoad[1] = fY;
            PointLoad[2] = fZ;
            PointLoad[3] = mX;
            PointLoad[4] = mY;
            PointLoad[5] = mZ;

            return PointLoad;
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes the data of <see cref="Load"/>, the components and the line
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
            info.AddValue("Line3d", _line);
        }

        /// <summary>
        /// Equality of name, load case, coordinate system, line and components (exact)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal line load</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is LineLoad objCasted) &&
                _line.Equals(objCasted._line) &&
                _f1.Equals(objCasted._f1) &&
                _f2.Equals(objCasted._f2) &&
                _f3.Equals(objCasted._f3) &&
                _m1.Equals(objCasted._m1) &&
                _m2.Equals(objCasted._m2) &&
                _m3.Equals(objCasted._m3) &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of name, load case, coordinate system, components and line
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
                hashCode = hashCode * -17 + EqualityComparer<Line3d>.Default.GetHashCode(_line);
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null loads are equal
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are equal</returns>
        public static bool operator ==(LineLoad obj1, LineLoad obj2)
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
        public static bool operator !=(LineLoad obj1, LineLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
