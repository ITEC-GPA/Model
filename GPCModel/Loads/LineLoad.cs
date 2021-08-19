using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    public class LineLoad : Load, ILineLoad
    {
        // Classe load e derivate deve rimanere immutabile 

        // sono forze per unità di lunghezza ( F / L )
        protected readonly double _f1;                 
        protected readonly double _f2;
        protected readonly double _f3;

        // sono momenti per unità di lunghezza ( F * L / L )
        protected readonly double _m1;                
        protected readonly double _m2;
        protected readonly double _m3;

        protected readonly CoordinateSystem _coordinateSystem;

        protected readonly Line3d _line;

        public double F1 => _f1;
        public double F2 => _f2;
        public double F3 => _f3;
        public double M1 => _m1;
        public double M2 => _m2;
        public double M3 => _m3;

        public Line3d Line => _line;

        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        #region Costruttori

        /// <param name="f1">Unit measure [F/L]</param>
        /// <param name="f2">Unit measure [F/L]</param>
        /// <param name="f3">Unit measure [F/L]</param>
        /// <param name="m1">Unit measure [FL/L]</param>
        /// <param name="m2">Unit measure [FL/L]</param>
        /// <param name="m3">Unit measure [FL/L]</param>
        /// <param name="line"></param>
        /// <param name="loadCase"></param>
        /// <param name="coordinateSystem"></param>
        public LineLoad(double f1, double f2, double f3, double m1, double m2, double m3,
                        Line3d line, LoadCaseBase loadCase, CoordinateSystem coordinateSystem)
            : base(loadCase, Guid.NewGuid())
        {
            _f1 = f1;
            _f2 = f2;
            _f3 = f3;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException("Coordinate system cannot be null");
            _line = line ?? throw new ArgumentNullException("Line cannot be null");
        }


        /// <param name="f1">Unit measure [F/L]</param>
        /// <param name="f2">Unit measure [F/L]</param>
        /// <param name="f3">Unit measure [F/L]</param>
        /// <param name="m1">Unit measure [FL/L]</param>
        /// <param name="m2">Unit measure [FL/L]</param>
        /// <param name="m3">Unit measure [FL/L]</param>
        /// <param name="line"></param>
        /// <param name="loadCase"></param>
        /// <remarks> <see cref="CoordinateSystem"/> set to Global </remarks>
        public LineLoad(double f1, double f2, double f3, double m1, double m2, double m3, Line3d line, LoadCaseBase loadCase)
            : this(f1, f2, f3, m1, m2, m3, line, loadCase, CoordinateSystem.Global)
        {

        }

        public LineLoad(Vector3d force, Vector3d moment, Line3d line, LoadCaseBase loadCase, CoordinateSystem cSys)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, line, loadCase, cSys)
        {

        } 
        #endregion

        public Line3d GetGeometry() => _line;

        public override GeometryBase GetGeometryBase() => GetGeometry();


        public LineLoad(SerializationInfo info, StreamingContext context)
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
            info.AddValue("Line3d", _line);
        }

        /// <returns>The total local load vector in the local system. i.e. _f1 * lenght , _f2 * lenght, _f3 * lenght</returns>
        public (Vector3d force, Vector3d moment) GetLocalLoadVector()
        {
            double line = _line.GetLength();
            return (new Vector3d(_f1 * line, _f2 * line, _f3 * line), new Vector3d(_m1 * line, _m2 * line, _m3 * line));
        }


        /// <returns>The total global load vector in the local system. i.e. _f1 * lenght , _f2 * lenght, _f3 * lenght</returns>
        public (Vector3d force, Vector3d moment) GetGlobalLoadVector()
        {
            if (_coordinateSystem == CoordinateSystem.Global)
                return GetLocalLoadVector();
            else
            {
                return (_coordinateSystem.ToGlobal(GetLocalLoadVector().force), _coordinateSystem.ToGlobal(GetLocalLoadVector().moment));
            }
        }

        /// <inheritdoc cref="GetLocalForces(CoordinateSystem)"/>
        public double[] GetLocalForces()
        {
            return GetLocalForces(_coordinateSystem);
        }

        /// <summary>
        /// Return an array with forces and moments in local coordinate system 
        /// </summary>
        /// <param name="cSys"></param>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
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
        public LineLoad ToGlobal()
        {
            Vector3d forceLocal = new Vector3d(_f1, _f2, _f3);
            Vector3d momentLocal = new Vector3d(_m1, _m2, _m3);

            // Cambia le proprietà del LineLoad passando da un sistema di riferimento globale
            // ad un sistema di rifarimento locale.

            return new LineLoad(_coordinateSystem.ToGlobal(forceLocal) - _coordinateSystem.Origin,
                                _coordinateSystem.ToGlobal(momentLocal) - _coordinateSystem.Origin,
                                _coordinateSystem.ToGlobal(_line),
                                LoadCase, CoordinateSystem.Global);
        }

        /// <summary>
        /// Return an array with forces and moments in global coordinate system 
        /// </summary>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
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


        #region Equals, HasCode and operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is LineLoad objCasted) && _line.Equals(objCasted._line) && _coordinateSystem.Equals(objCasted._coordinateSystem)
                                               && _f1.Equals(objCasted._f1) && _f2.Equals(objCasted._f2) && _f3.Equals(objCasted._f3)
                                               && _m1.Equals(objCasted._m1) && _m2.Equals(objCasted._m2) && _m3.Equals(objCasted._m3) && base.Equals(objCasted);
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
                hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
                hashCode = hashCode * -17 + EqualityComparer<Line3d>.Default.GetHashCode(_line);
                return hashCode;
            }
        }

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

        public static bool operator !=(LineLoad obj1, LineLoad obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}
