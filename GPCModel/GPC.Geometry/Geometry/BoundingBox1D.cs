using System.Collections.Generic;

namespace GPC.Geometry
{
    public class BoundingBox1D
    {
        private double _min;
        private double _max;
        private bool _isEmpty;

        public BoundingBox1D()
        {
            Reset();
        }

        public double Min
        {
            get { return _min; }
        }

        public double Max
        {
            get { return _max; }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public double Size
        {
            get { return _max - _min; }
        }

        public double Center()
        {
            return _min + 0.5 * Size;
        }

        public void Update(double x)
        {
            if (_isEmpty)
            {
                _min = x;
                _max = x;
            }
            else
            {
                if (x < _min)
                {
                    _min = x;
                }
                else if (x > _max)
                {
                    _max = x;
                }
            }
            _isEmpty = false;
        }

        public void Reset()
        {
            _min = 0;
            _max = 0;
            _isEmpty = true;
        }

        public static List<BoundingBox1D> GetUnion(BoundingBox1D a, BoundingBox1D b)
        {
            BoundingBox1D buffer;
            buffer = GetIntersection(a, b, 0);
            if (buffer != null)
            {
                buffer = new BoundingBox1D();
                buffer.Update(System.Math.Min(a.Min, b.Min));
                buffer.Update(System.Math.Max(a.Max, b.Max));
                return new List<BoundingBox1D>() { buffer };
            }
            else
            {
                return new List<BoundingBox1D>() { a, b };
            }
        }

        public static List<BoundingBox1D> GetDifference(BoundingBox1D a, BoundingBox1D b, double minLength)
        {
            BoundingBox1D buffer;
            buffer = GetIntersection(a, b, 0);
            if (buffer != null)
            {
                List<BoundingBox1D> result;
                result = null;
                if (b.Max <= a.Max - minLength)
                {
                    if (result == null)
                    {
                        result = new List<BoundingBox1D>();
                    }
                    buffer = new BoundingBox1D();
                    buffer.Update(b.Max);
                    buffer.Update(a.Max);
                    result.Add(buffer);
                }
                if (b.Min >= a.Min + minLength)
                {
                    if (result == null)
                    {
                        result = new List<BoundingBox1D>();
                    }
                    buffer = new BoundingBox1D();
                    buffer.Update(b.Min);
                    buffer.Update(a.Min);
                    result.Add(buffer);
                }
                return result;
            }
            else
            {
                return new List<BoundingBox1D>() { a };
            }
        }

        public static List<BoundingBox1D> GetIntersection(List<BoundingBox1D> a, List<BoundingBox1D> b, double minLength)
        {
            List<BoundingBox1D> result;
            result = null;
            foreach (BoundingBox1D aBBox in a)
            {
                foreach (BoundingBox1D bBbox in b)
                {
                    BoundingBox1D inters;
                    inters = GetIntersection(aBBox, bBbox, minLength);
                    if (inters != null)
                    {
                        if (result == null)
                        {
                            result = new List<BoundingBox1D>();
                        }
                        result.Add(inters);
                    }
                }
            }
            return result;
        }

        public static BoundingBox1D GetIntersection(BoundingBox1D a, BoundingBox1D b, double minLength)
        {
            double maxOfMin = System.Math.Max(a.Min, b.Min);
            double minOfMax = System.Math.Min(a.Max, b.Max);
            if (minOfMax - minLength >= maxOfMin)
            {
                BoundingBox1D result;
                result = new BoundingBox1D();
                result.Update(maxOfMin);
                result.Update(minOfMax);
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}