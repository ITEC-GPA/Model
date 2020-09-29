using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    [Serializable]
    public class Polygon : IEnumerable<Point2d>, ISerializable
    {
        private readonly List<Point2d> _items;

        public Polygon()
        {
            _items = new List<Point2d>();
        }

        public Polygon(Polygon polygon) : this()
        {
            foreach (Point2d p in polygon)
            {
                Add(p);
            }
        }

        public void Add(Point2d p)
        {
            Add(p.X, p.Y);
        }

        public void Add(double x, double y)
        {
            _items.Add(new Point2d(x, y));
        }

        public Point2d this[int index]
        {
            get { return _items[index]; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public int GetNextIndex(int i)
        {
            if (i < _items.Count - 1)
            {
                return i + 1;
            }
            else if (i == _items.Count - 1)
            {
                return 0;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public Point2d GetNextItem(int i)
        {
            return this[GetNextIndex(i)];
        }

        public double GetSignedArea()
        {
            double result;
            result = 0;
            for (int i = 0; i < Count; i++)
            {
                Point2d item;
                Point2d nextItem;
                item = this[i];
                nextItem = GetNextItem(i);
                result += 0.5 * (item.X * nextItem.Y - nextItem.X * item.Y);
            }
            return result;
        }

        public bool IsRightHandOrdered()
        {
            return GetSignedArea() > 0;
        }

        public void Reverse()
        {
            _items.Reverse();
        }

        /// <summary>
        /// Cut the polygon with a line
        /// </summary>
        /// <param name="p0">first line point</param>
        /// <param name="p1">second line point</param>
        /// <returns>The cutted polygon, that stay at right of line from p0 to p1</returns>
        public Polygon GetRightPolygon(Point2d p0, Point2d p1)
        {
            const double TOLL = 1e-6;
            Polygon cutted;
            Trans2d t;
            int lastIndex;
            Polygon localPoly;
            t = new Trans2d(p0, p1, p0);
            cutted = null;
            if (Geom.SquareDistance(this[0], this[Count - 1]) < TOLL)
            {
                //firstlast coincident
                lastIndex = Count - 2;
            }
            else
            {
                lastIndex = Count - 1;
            }
            localPoly = new Polygon(this);
            t.ToLocal(ref localPoly);
            for (int i = 0; i <= lastIndex; i++)
            {
                Point2d p, nextP;
                p = localPoly[i];
                nextP = localPoly.GetNextItem(i);
                if (p.Y < TOLL)
                {
                    if (cutted == null) cutted = new Polygon();
                    cutted.Add(this[i]);
                }
                if ((System.Math.Abs(p.Y - nextP.Y) > TOLL) && (p.Y * nextP.Y < 0))
                {
                    Point2d dir;
                    double length;
                    double k;
                    //points on different sides
                    k = System.Math.Abs(p.Y / (nextP.Y - p.Y));
                    dir = GetNextItem(i) - this[i];
                    length = Geom.GetLength(dir);
                    dir.Scale(1 / length);
                    if (cutted == null) cutted = new Polygon();
                    cutted.Add(this[i] + dir * (length * k));
                }
            }
            return cutted;
        }

        /// <summary>
        /// Cut the polygon with a line
        /// </summary>
        /// <param name="p0">first line point</param>
        /// <param name="p1">second line point</param>
        /// <returns>The cutted polygon, that stay at left of line from p0 to p1</returns>
        public Polygon GetLeftPolygon(Point2d p0, Point2d p1)
        {
            return GetRightPolygon(p1, p0);
        }

        public void RemoveDoublePoints(double toll)
        {
            for (int i = Count - 1; i > -1; i--)
            {
                int nextI;
                if (i == Count - 1)
                {
                    nextI = 0;
                }
                else
                {
                    nextI = i + 1;
                }
                if (Geom.SquareDistance(this[nextI], this[i]) < toll * toll)
                {
                    if (i == Count - 1)
                    {
                        _items.RemoveAt(Count - 1);
                    }
                    else
                    {
                        _items.RemoveAt(i + 1);
                    }
                }
            }
        }

        public void RemoveAlignedPoints(double distOnLine)
        {
            //Simplify polyline
            int startingIndex;
            Point2d prevDir, nextDir;
            List<Point2d> cleanedList;
            nextDir = null;
            startingIndex = -1;
            //finding a point on a kink
            for (int i = 0; i < _items.Count - 2; i++)
            {
                if (nextDir == null)
                {
                    prevDir = Geom.Direction(_items[i], _items[i + 1]);
                }
                else
                {
                    prevDir = nextDir;
                }
                nextDir = Geom.Direction(_items[i + 1], _items[i + 2]);

                if (nextDir * prevDir < 0.95)
                {
                    //kink at i+1
                    startingIndex = i + 1;
                    break;
                }
            }
            if (startingIndex == -1) return;
            int i0, iNext;
            bool start = true;
            i0 = startingIndex;
            iNext = i0;
            cleanedList = new List<Point2d>();
            cleanedList.Add(_items[startingIndex]);
            do
            {
                Point2d dir;
                Point2d diff;
                Point2d projection;
                Point2d ortoProjection;
                if ((!start) && (iNext == startingIndex))
                {
                    break;
                }
                iNext++;
                start = false;
                bool add;
                if (iNext >= _items.Count)
                {
                    iNext = 0;
                }
                add = false;
                dir = Geom.Direction(_items[i0], _items[iNext]);
                if (iNext < i0)
                {
                    for (int i = i0 + 1; i < _items.Count; i++)
                    {
                        diff = _items[i] - _items[i0];
                        projection = diff * dir * dir;
                        ortoProjection = diff - projection;
                        if (Geom.SquareDistance(Point2d.Origin, ortoProjection) > distOnLine * distOnLine)
                        {
                            add = true;
                            break;
                        }
                    }

                    for (int i = 0; i < iNext; i++)
                    {
                        diff = _items[i] - _items[i0];
                        projection = diff * dir * dir;
                        ortoProjection = diff - projection;
                        if (Geom.SquareDistance(Point2d.Origin, ortoProjection) > distOnLine * distOnLine)
                        {
                            add = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = i0 + 1; i < iNext; i++)
                    {
                        diff = _items[i] - _items[i0];
                        projection = diff * dir * dir;
                        ortoProjection = diff - projection;
                        if (Geom.SquareDistance(Point2d.Origin, ortoProjection) > distOnLine * distOnLine)
                        {
                            add = true;
                            break;
                        }
                    }
                }
                if (add)
                {
                    if (iNext == 0)
                    {
                        iNext = _items.Count - 1;
                    }
                    else
                    {
                        iNext -= 1;
                    }
                    i0 = iNext;
                    cleanedList.Add(_items[iNext]);
                }
            } while (true == true);

            _items.Clear();
            _items.AddRange(cleanedList);
        }

        #region IEnumerable<Point2d>

        public IEnumerator<Point2d> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable<Point2d>

        public Polygon Mirror(double a, double b, double c)
        {
            Polygon mirror = new Polygon();
            for (int i = 0; i < _items.Count; i++)
            {
                Point2d pt = _items[i].Mirror(a, b, c);
                mirror.Add(pt);
            }
            return mirror;
        }

        public void Pan(double dx, double dy)
        {
            foreach (Point2d p in this)
            {
                p.Pan(dx, dy);
            }
        }

        #region Serializzazione

        private static int SERIALIZATION_RELEASE = 1;
        protected static string SERIALIZATION_RELEASE_NAME = "SERIALIZATION_RELEASE_NAME";
        private static string PREFIX = "Utilities.Polygon/";

        protected Polygon(SerializationInfo info, StreamingContext context)
        {
            _items = (List<Point2d>)info.GetValue(PREFIX + "_items", typeof(List<Point2d>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(PREFIX + SERIALIZATION_RELEASE_NAME, SERIALIZATION_RELEASE);
            info.AddValue(PREFIX + "_items", _items, typeof(List<Point2d>));
        }

        #endregion Serializzazione
    }
}