

namespace GPC.Geometry
{
    public class BoundingBox2D
    {
        private Point2d _min;
        private Point2d _max;
        private bool _isEmpty;

        public BoundingBox2D()
        {
            Reset();
        }

        public BoundingBox2D(Polygon polygon) : base()
        {
            foreach (Point2d p in polygon)
            {
                Update(p);
            }
        }

        public Point2d Min
        {
            get { return _min; }
        }

        public Point2d Max
        {
            get { return _max; }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public Point2d Size
        {
            get { return _max - _min; }
        }

        public void Update(Polygon poly)
        {
            foreach (Point2d p in poly)
            {
                Update(p);
            }
        }

        public void Update(Point2d p)
        {
            Update(p.X, p.Y);
        }

        public void Update(double x, double y)
        {
            if (_isEmpty)
            {
                _min = new Point2d(x, y);
                _max = new Point2d(x, y);
            }
            else
            {
                if (x < _min.X)
                {
                    _min.Move(x, _min.Y);
                }
                else if (x > _max.X)
                {
                    _max.Move(x, _max.Y);
                }
                if (y < _min.Y)
                {
                    _min.Move(_min.X, y);
                }
                else if (y > _max.Y)
                {
                    _max.Move(_max.X, y);
                }
            }
            _isEmpty = false;
        }

        public void Reset()
        {
            _min = Point2d.Origin;
            _max = Point2d.Origin;
            _isEmpty = true;
        }
    }
}