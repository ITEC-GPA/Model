using System;
using System.Diagnostics;


namespace GPC.Geometry
{
    [DebuggerDisplay("Origin=({Origin.X}, {Origin.Y}, ); XDir=({XDir.X}, {XDir.Y}), YDir=({YDir.X}, {YDir.Y})")]
    public class Trans2d
    {
        private readonly Point2d _origin;
        private readonly Point2d _xDir;
        private readonly Point2d _yDir;

        public Trans2d(Point2d origin, Point2d pointOnXLocalAxes, Point2d pointOnYLocalAxes)
        {
            _origin = new Point2d(origin);
            if (pointOnXLocalAxes == origin)
            {
                _yDir = Geom.Direction(origin, pointOnYLocalAxes);
                _xDir = Geom.Normal(_yDir, false);
            }
            else if (pointOnYLocalAxes == origin)
            {
                _xDir = Geom.Direction(origin, pointOnXLocalAxes);
                _yDir = Geom.Normal(_xDir, true);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public Point2d Origin
        {
            get { return _origin; }
        }

        public Point2d XDir
        {
            get { return _xDir; }
        }

        public Point2d YDir
        {
            get { return _yDir; }
        }

        public Point2d PointGlobal(Point2d localP)
        {
            return PointGlobal(localP.X, localP.Y);
        }

        public Point2d PointGlobal(double localX, double localY)
        {
            return _origin + localX * _xDir + localY * _yDir;
        }

        public Point2d PointLocal(Point2d globalP)
        {
            Point2d diff;
            diff = globalP - _origin;
            return new Point2d(diff * _xDir, diff * _yDir);
        }

        public Point2d PointLocal(double globalX, double globalY)
        {
            return PointLocal(new Point2d(globalX, globalY));
        }

        public void ToGlobal(ref Point2d p)
        {
            Point2d gp;
            gp = PointGlobal(p);
            p.Move(gp.X, gp.Y);
        }

        public void ToLocal(ref Point2d p)
        {
            Point2d lp;
            lp = PointLocal(p);
            p.Move(lp.X, lp.Y);
        }

        public void ToLocal(ref Polygon polygon)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d p;
                p = polygon[i];
                ToLocal(ref p);
            }
        }

        public void ToGlobal(ref Polygon polygon)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d p;
                p = polygon[i];
                ToGlobal(ref p);
            }
        }
    }
}