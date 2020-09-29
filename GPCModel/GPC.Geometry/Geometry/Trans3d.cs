using System;
using System.Diagnostics;


namespace GPC.Geometry
{
    [DebuggerDisplay("Origin=({Origin.X}, {Origin.Y}, {Origin.Z}); XDir=({XDir.X}, {XDir.Y}, {XDir.Z}), YDir=({YDir.X}, {YDir.Y}, {YDir.Z}), ZDir=({ZDir.X}, {ZDir.Y}, {ZDir.Z})")]
    public class Trans3d
    {
        private readonly Point3d _origin;
        private readonly Point3d _xDir;
        private readonly Point3d _yDir;
        private readonly Point3d _zDir;

        public Trans3d(Point3d origin, Point3d pointOnXLocalAxes, Point3d pointOnYLocalAxes, Point3d pointOnZLocalAxes)
        {
            _origin = new Point3d(origin);
            if (pointOnXLocalAxes == origin)
            {
                _yDir = Geom.Direction(origin, pointOnYLocalAxes);
                _zDir = Geom.Direction(origin, pointOnZLocalAxes);
                _xDir = _yDir ^ _zDir;
            }
            else if (pointOnYLocalAxes == origin)
            {
                _xDir = Geom.Direction(origin, pointOnXLocalAxes);
                _zDir = Geom.Direction(origin, pointOnZLocalAxes);
                _yDir = _zDir ^ _xDir;
            }
            else if (pointOnZLocalAxes == origin)
            {
                _xDir = Geom.Direction(origin, pointOnXLocalAxes);
                _yDir = Geom.Direction(origin, pointOnYLocalAxes);
                _zDir = _xDir ^ _yDir;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public Point3d Origin
        {
            get { return _origin; }
        }

        public Point3d XDir
        {
            get { return _xDir; }
        }

        public Point3d YDir
        {
            get { return _yDir; }
        }

        public Point3d ZDir
        {
            get { return _zDir; }
        }

        public Point3d PointGlobal(Point3d localP)
        {
            return _origin + (localP.X) * _xDir + (localP.Y) * _yDir + (localP.Z) * _zDir;
        }

        public Point3d PointGlobal(Point2d localP, double z)
        {
            return _origin + (localP.X) * _xDir + (localP.Y) * _yDir + z * _zDir;
        }

        public Point3d PointLocal(Point3d globalP)
        {
            Point3d diff;
            diff = globalP - _origin;
            return new Point3d(diff * _xDir, diff * _yDir, diff * _zDir);
        }

        public void ToGlobal(ref Point3d p)
        {
            Point3d gp;
            gp = PointGlobal(p);
            p.Move(gp.X, gp.Y, gp.Z);
        }

        public void ToLocal(ref Point3d p)
        {
            Point3d lp;
            lp = PointLocal(p);
            p.Move(lp.X, lp.Y, lp.Z);
        }
    }
}