using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    public class GlobalPointLoad : Load, IPointLoad
    {
        private double _fX;
        private double _fY;
        private double _fZ;
        private double _mX;
        private double _mY;
        private double _mZ;

        private Point3d _point;

        public double Fx => _fX;
        public double Fy => _fY;
        public double Fz => _fZ;
        public double Mx => _mX;
        public double My => _mY;
        public double Mz => _mZ;

        public Point3d Point => _point;


        public GlobalPointLoad(double fx, double fy, double fz, double mx, double my, double mz, Point3d point, LoadCase loadCase, Guid guid) 
            : base(loadCase, guid)
        {
            _fX = fx;
            _fY = fy;
            _fZ = fz;
            _mX = mx;
            _mY = my;
            _mZ = mz;
            _point = point ?? throw new ArgumentNullException("Point cannot be null") ;
        }


        public GlobalPointLoad(Vector3d force, Vector3d moment, Point3d point, LoadCase loadCase, Guid guid)
            : this(force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z, point, loadCase, guid)
        {

        }

        public override GeometryBase GetGeometry() => _point;


        public GlobalPointLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _fX = info.GetDouble("Fx");
            _fY = info.GetDouble("Fy");
            _fZ = info.GetDouble("Fz");
            _mX = info.GetDouble("Mx");
            _mY = info.GetDouble("My");
            _mZ = info.GetDouble("Mz");
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
        }
    }
}
