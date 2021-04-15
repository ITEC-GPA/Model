using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    public class PointLoad : Load, IPointLoad
    {
        #region Variables

        private double _f1;
        private double _f2;
        private double _f3;
        private double _m1;
        private double _m2;
        private double _m3;

        private CoordinateSystem _coordinateSystem;

        private Point3d _point;

        #endregion


        #region Properties

        public double F1 => _f1;
        public double F2 => _f2;
        public double F3 => _f3;
        public double M1 => _m1;
        public double M2 => _m2;
        public double M3 => _m3;

        public Point3d Point => _point;

        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        #endregion


        #region Public Constructors

        public PointLoad(double f1, double f2, double f3, double m1, double m2, double m3, Point3d point, LoadCase loadCase, CoordinateSystem coordinateSystem) 
            : base(loadCase, Guid.NewGuid())
        {
            _f1 = f1;                                       
            _f2 = f2;
            _f3 = f3;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException("Coordinate system cannot be null");
            _point = point ?? throw new ArgumentNullException("Point cannot be null") ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="f3"></param>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <param name="m3"></param>
        /// <param name="point"></param>
        /// <param name="loadCase"></param>
        /// <remarks> <see cref="CoordinateSystem"/> set to Global </remarks>
        public PointLoad(double f1, double f2, double f3, double m1, double m2, double m3, Point3d point, LoadCase loadCase)
            : this(f1, f2, f3, m1, m2, m3, point, loadCase, CoordinateSystem.Global)
        {

        }


        public PointLoad(Vector3d force, Vector3d moment, Point3d point, LoadCase loadCase, CoordinateSystem cSys)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, point, loadCase, cSys)
        {
            
        }

        public Point3d GetGeometry() => _point;

        public override GeometryBase GetGeometryBase() => GetGeometry();


        public PointLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _f1 = info.GetDouble("F1");
            _f2 = info.GetDouble("F2");
            _f3 = info.GetDouble("F3");
            _m1 = info.GetDouble("M1");
            _m2 = info.GetDouble("M2");
            _m3 = info.GetDouble("M3");
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
        }

        #endregion


        #region Public Methods Specific

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

        /// <summary>
        /// Return an array with forces and moments in global coordinate system 
        /// </summary>
        /// <param name="cSys"></param>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        public double[] GetLocalForces(CoordinateSystem cSys)
        {
            Point3d forceGlobal = new Point3d(_f1, _f2, _f3);                               // 
            Point3d momentglobal = new Point3d(_m1, _m2, _m3);                              // Crea un array di double in le 3 componenti di forza e di momento
                                                                                            // nelle 3 direzioni del sistema di coordinate locali.
            Point3d PointForceLocal = cSys.ToLocal(forceGlobal);                            // 
            Point3d PointMomentLocal = cSys.ToLocal(momentglobal);

            Point3d OriginGlobal = cSys.ToLocal(CoordinateSystem.Global.Origin);
        
            Vector3d forceLocal = new Vector3d(PointForceLocal - OriginGlobal);
            Vector3d momentLocal = new Vector3d(PointMomentLocal - OriginGlobal);

            double[] PointLoad = new double[6];
            PointLoad[0] = forceLocal.X;
            PointLoad[1] = forceLocal.Y;
            PointLoad[2] = forceLocal.Z;
            PointLoad[3] = momentLocal.X;
            PointLoad[4] = momentLocal.Y;
            PointLoad[5] = momentLocal.Z;

            return PointLoad;
        }

        /// <summary>
        /// Change the PointLoad property moving from a local coordinate system to a global coordinate system
        /// </summary>
        public void ToGlobal()
        {
            Vector3d forceLocal = new Vector3d(_f1, _f2, _f3);                              // 
            Vector3d momentLocal = new Vector3d(_m1, _m2, _m3);                             // Cambia le proprietà del PointLoad passando da un sistema di riferimento globale
                                                                                            // ad un sistema di rifarimento locale.
            _point = _coordinateSystem.ToGlobal(_point);                                    // 
            _f1 = _coordinateSystem.ToGlobal(forceLocal).X - _coordinateSystem.Origin.X;
            _f2 = _coordinateSystem.ToGlobal(forceLocal).Y - _coordinateSystem.Origin.Y;
            _f3 = _coordinateSystem.ToGlobal(forceLocal).Z - _coordinateSystem.Origin.Z;
            _m1 = _coordinateSystem.ToGlobal(momentLocal).X - _coordinateSystem.Origin.X;
            _m2 = _coordinateSystem.ToGlobal(momentLocal).Y - _coordinateSystem.Origin.Y;
            _m3 = _coordinateSystem.ToGlobal(momentLocal).Z - _coordinateSystem.Origin.Z;
            _coordinateSystem = CoordinateSystem.Global;
        }

        /// <summary>
        /// Return an array with forces and moments in global coordinate system 
        /// </summary>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        public double[] GetGlobalForces()
        {
            Vector3d forceLocal = new Vector3d(_f1, _f2, _f3);                                   // 
            Vector3d momentLocal = new Vector3d(_m1, _m2, _m3);                                   // Crea un array di double in le 3 componenti di forza e di momento
                                                                                                  // nelle 3 direzioni del sistema di coordinate globali.
                                                                                                  // Point3d point = _coordinateSystem.ToGlobal(_point);                                  // 
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
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            if (obj is null)
                return false;
                
            var objCasted = obj as PointLoad;

            return objCasted != null && _point.Equals(objCasted._point) && _coordinateSystem.Equals(objCasted._coordinateSystem)
                                                                        && _f1.Equals(objCasted._f1) && _f2.Equals(objCasted._f2) && _f3.Equals(objCasted._f3)
                                                                        && _m1.Equals(objCasted._m1) && _m2.Equals(objCasted._m2) && _m3.Equals(objCasted._m3) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _f1.GetHashCode();
            hashCode = hashCode * -17 + _f2.GetHashCode();
            hashCode = hashCode * -17 + _f3.GetHashCode();
            hashCode = hashCode * -17 + _m1.GetHashCode();
            hashCode = hashCode * -17 + _m2.GetHashCode();
            hashCode = hashCode * -17 + _m3.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            hashCode = hashCode * -17 + EqualityComparer<Point3d>.Default.GetHashCode(_point);
            return hashCode;
        }

        public static bool operator ==(PointLoad obj1, PointLoad obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PointLoad obj1, PointLoad obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}
