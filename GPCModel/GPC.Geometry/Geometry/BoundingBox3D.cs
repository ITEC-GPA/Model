

namespace GPC.Geometry
{
    public class BoundingBox3D
    {
        private Point3d _min;
        private Point3d _max;
        private bool _isEmpty;

        public BoundingBox3D()
        {
            Reset();
        }

        public Point3d Min
        {
            get { return _min; }
        }

        public Point3d Max
        {
            get { return _max; }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public Point3d Size
        {
            get { return _max - _min; }
        }

        public void Update(Point3d p)
        {
            Update(p.X, p.Y, p.Z);
        }

        public void Update(double x, double y, double z)
        {
            if (_isEmpty)
            {
                _min = new Point3d(x, y, z);
                _max = new Point3d(x, y, z);
            }
            else
            {
                if (x < _min.X)
                {
                    _min.Move(x, _min.Y, _min.Z);
                }
                else if (x > _max.X)
                {
                    _max.Move(x, _max.Y, _max.Z);
                }
                if (y < _min.Y)
                {
                    _min.Move(_min.X, y, _min.Z);
                }
                else if (y > _max.Y)
                {
                    _max.Move(_max.X, y, _max.Z);
                }
                if (z < _min.Z)
                {
                    _min.Move(_min.X, _min.Y, z);
                }
                else if (z > _max.Z)
                {
                    _max.Move(_max.X, _max.Y, z);
                }
            }
            _isEmpty = false;
        }

        public void Reset()
        {
            _min = Point3d.Origin;
            _max = Point3d.Origin;
            _isEmpty = true;
        }
    }
}