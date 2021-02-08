using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    public class LineLoad : Load, ILineLoad
    {
        private double _fX;                 // sono forze per unità di lunghezza ( F / L )
        private double _fY;
        private double _fZ;
        private double _mX;                 // sono momenti per unità di lunghezza ( F / L )
        private double _mY;
        private double _mZ;

        private CoordinateSystem _coordinateSystem;

        private Line3d _line;

        public double Fx => _fX;
        public double Fy => _fY;
        public double Fz => _fZ;
        public double Mx => _mX;
        public double My => _mY;
        public double Mz => _mZ;

        public Line3d Line => _line;

        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        public LineLoad(double fx, double fy, double fz, double mx, double my, double mz, Line3d line, LoadCase loadCase, CoordinateSystem coordinateSystem, Guid guid) 
            : base(loadCase, guid)
        {
            _fX = fx;                                       
            _fY = fy;
            _fZ = fz;
            _mX = mx;
            _mY = my;
            _mZ = mz;
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException("Coordinate system cannot be null");
            _line = line ?? throw new ArgumentNullException("Line cannot be null") ;
        }

        public LineLoad(double fx, double fy, double fz, double mx, double my, double mz, Line3d line, LoadCase loadCase, Guid guid)
            : this(fx, fy, fz, mx, my, mz, line, loadCase, CoordinateSystem.Global, guid)
        {

        }

        public LineLoad(Vector3d force, Vector3d moment, Line3d line, LoadCase loadCase, CoordinateSystem cSys, Guid guid)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, line, loadCase, cSys, guid)
        {
            
        }

        public override GeometryBase GetGeometry() => _line;


        public LineLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _fX = info.GetDouble("Fx");
            _fY = info.GetDouble("Fy");
            _fZ = info.GetDouble("Fz");
            _mX = info.GetDouble("Mx");
            _mY = info.GetDouble("My");
            _mZ = info.GetDouble("Mz");
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fx", _fX);
            info.AddValue("Fy", _fY);
            info.AddValue("Fz", _fZ);
            info.AddValue("Mx", _mX);
            info.AddValue("My", _mY);
            info.AddValue("Mz", _mZ);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        /// <summary>
        /// Return an array with forces and moments in global coordinate system 
        /// </summary>
        /// <param name="cSys"></param>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        public double[] GetLocalForces(CoordinateSystem cSys)
        {
            Point3d forceGlobal = new Point3d(_fX, _fY, _fZ);                               // 
            Point3d momentglobal = new Point3d(_mX, _mY, _mZ);                              // Crea un array di double in le 3 componenti di forza e di momento
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
            Vector3d forceLocal = new Vector3d(_fX, _fY, _fZ);                              // 
            Vector3d momentLocal = new Vector3d(_mX, _mY, _mZ);                             // Cambia le proprietà del PointLoad passando da un sistema di riferimento globale
                                                                                            // ad un sistema di rifarimento locale.
            _line = _coordinateSystem.ToGlobal(_line);                                      // 
            _fX = _coordinateSystem.ToGlobal(forceLocal).X - _coordinateSystem.Origin.X;
            _fY = _coordinateSystem.ToGlobal(forceLocal).Y - _coordinateSystem.Origin.Y;
            _fZ = _coordinateSystem.ToGlobal(forceLocal).Z - _coordinateSystem.Origin.Z;
            _mX = _coordinateSystem.ToGlobal(momentLocal).X - _coordinateSystem.Origin.X;
            _mY = _coordinateSystem.ToGlobal(momentLocal).Y - _coordinateSystem.Origin.Y;
            _mZ = _coordinateSystem.ToGlobal(momentLocal).Z - _coordinateSystem.Origin.Z;
            _coordinateSystem = CoordinateSystem.Global;
        }

        /// <summary>
        /// Return an array with forces and moments in global coordinate system 
        /// </summary>
        /// <returns>The array [fx, fy, fz, mx, my, mz]</returns>
        public double[] GetGlobalForces()
        {
            Vector3d forceLocal = new Vector3d(_fX, _fY, _fZ);                                      // 
            Vector3d momentLocal = new Vector3d(_mX, _mY, _mZ);                                     // Crea un array di double in le 3 componenti di forza e di momento
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
    }
}
