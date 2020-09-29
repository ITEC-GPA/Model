using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace GPC.Geometry
{
    [Serializable]
    public class Shapes : IEnumerable<Shape>, ISerializable
    {
        private List<Shape> _items;

        public Shapes(IEnumerable<Shape> items)
        {
            _items = new List<Shape>(items);
        }

        public Shape this[int index]
        {
            get => _items[index];
        }

        public IEnumerator<Shape> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public void MoveToMin()
        {
            BoundingBox2D bbox = GetBBox();
            Pan(-bbox.Min.X, -bbox.Min.Y);
        }

        public BoundingBox2D GetBBox()
        {
            BoundingBox2D bbox = new BoundingBox2D();
            foreach (Shape s in this)
            {
                BoundingBox2D shapeBBox = s.GetBBox();
                bbox.Update(shapeBBox.Min);
                bbox.Update(shapeBBox.Max);
            }
            return bbox;
        }

        public void Pan(double dx, double dy)
        {
            foreach (Shape s in this)
            {
                s.Pan(dx, dy);
            }
        }

        public int Count => _items.Count;

        public void GetAreaBarycentre(out double xG, out double yG, out double area)
        {
            List<Polygon> fills = new List<Polygon>();
            List<Polygon> holes = new List<Polygon>();
            foreach (Shape s in this)
            {
                fills.Add(s.Fill);
                if (s.HolesCanBeNull != null)
                {
                    foreach (Polygon p in s.HolesCanBeNull)
                    {
                        holes.Add(p);
                    }
                }
            }
            GetAreaBarycentre(fills, holes, out area, out xG, out yG);
        }

        private void GetAreaBarycentre(List<Polygon> fills, List<Polygon> holes, out double area, out double xG, out double yG)
        {
            area = 0;
            xG = 0;
            yG = 0;
            foreach (List<Polygon> list in new List<Polygon>[] { fills, holes })
            {
                double multiplier;
                if (list == fills)
                {
                    multiplier = 1;
                }
                else
                {
                    multiplier = -1;
                }
                foreach (Polygon poly in list)
                {
                    for (int i = 0; i < poly.Count; i++)
                    {
                        Point2d p = poly[i];
                        Point2d nextP;
                        double ai;
                        if (i == poly.Count - 1)
                        {
                            nextP = poly[0];
                        }
                        else
                        {
                            nextP = poly[i + 1];
                        }

                        ai = (p.X * nextP.Y) - (nextP.X * p.Y);
                        area += multiplier * 0.5d * ai;
                        xG += multiplier * (p.X + nextP.X) * ai;
                        yG += multiplier * (p.Y + nextP.Y) * ai;
                    }
                }
            }

            xG /= 6 * area;
            yG /= 6 * area;
        }

        public void GetGeometricalDatas(bool mIsAlwaysMajorAxes, double mainJToll, out double xG, out double yG, out double area, out double jxG, out double jyG,
                                        out double jxyG, out double jm, out double jn, out double angleXToM, out double wx, out double wy, out double wm, out double wn)
        {
            List<Polygon> fills;
            List<Polygon> holes;
            double cosAlfa, senAlfa;
            BoundingBox2D bbox;
            Trans2d toPrincipal;

            //area = 0;
            //xG = 0;
            //yG = 0;

            fills = new List<Polygon>();
            holes = new List<Polygon>();
            foreach (Shape s in this)
            {
                fills.Add(s.Fill);
                if (s.HolesCanBeNull != null)
                {
                    foreach (Polygon p in s.HolesCanBeNull)
                    {
                        holes.Add(p);
                    }
                }
            }

            GetAreaBarycentre(fills, holes, out area, out xG, out yG);

            //foreach (List<Polygon> list in new List<Polygon>[] { fills, holes })
            //{
            //    double multiplier;
            //    if (list == fills)
            //    {
            //        multiplier = 1;
            //    }
            //    else
            //    {
            //        multiplier = -1;
            //    }
            //    foreach (Polygon poly in list)
            //    {
            //        for (int i = 0; i < poly.Count; i++)
            //        {
            //            Point2d p;
            //            Point2d nextP;
            //            double ai;
            //            p = poly[i];
            //            if (i == poly.Count - 1)
            //            {
            //                nextP = poly[0];
            //            }
            //            else
            //            {
            //                nextP = poly[i + 1];
            //            }

            //            ai = (p.X * nextP.Y) - (nextP.X * p.Y);
            //            area += multiplier * 0.5d * ai;
            //            xG += multiplier * (p.X + nextP.X) * ai;
            //            yG += multiplier * (p.Y + nextP.Y) * ai;
            //        }
            //    }
            //}

            //xG /= 6 * area;
            //yG /= 6 * area;

            //Moving to barycentre
            jxG = 0;
            jyG = 0;
            jxyG = 0;
            bbox = new BoundingBox2D();
            foreach (List<Polygon> list in new List<Polygon>[] { fills, holes })
            {
                double multiplier;
                if (list == fills)
                {
                    multiplier = 1;
                }
                else
                {
                    multiplier = -1;
                }
                foreach (Polygon poly in list)
                {
                    for (int i = 0; i < poly.Count; i++)
                    {
                        Point2d p;
                        Point2d nextP;
                        double ai;
                        double x, y, nextX, nextY;
                        p = poly[i];
                        if (i == poly.Count - 1)
                        {
                            nextP = poly[0];
                        }
                        else
                        {
                            nextP = poly[i + 1];
                        }

                        x = p.X - xG;
                        y = p.Y - yG;
                        nextX = nextP.X - xG;
                        nextY = nextP.Y - yG;

                        ai = (x * nextY) - (nextX * y);
                        jxG += multiplier / 12d * (y * y + y * nextY + nextY * nextY) * ai;
                        jyG += multiplier / 12d * (x * x + x * nextX + nextX * nextX) * ai;
                        jxyG += multiplier / 24d * (x * nextY + 2 * x * y + 2 * nextX * nextY + nextX * y) * ai;
                        bbox.Update(x, y);
                    }
                }
            }

            wx = jxG / System.Math.Max(System.Math.Abs(bbox.Max.Y), System.Math.Abs(bbox.Min.Y));
            wy = jyG / System.Math.Max(System.Math.Abs(bbox.Max.X), System.Math.Abs(bbox.Min.X));

            if (System.Math.Abs(jxG - jyG) < mainJToll)
            {
                if (System.Math.Abs(jxyG) < mainJToll)
                {
                    //circle for example
                    angleXToM = 0;
                }
                else
                {
                    angleXToM = System.Math.PI / 4;
                }
            }
            else
            {
                //formula da sdc. angolo compreso sempre tra +45 e -45 gradi.
                angleXToM = 0.5 * System.Math.Atan(-2 * jxyG / (jxG - jyG));
            }

            //moving angle to I quadrant
            if (angleXToM < -0.000001)
            {
                angleXToM += System.Math.PI / 2;
            }

            cosAlfa = System.Math.Cos(angleXToM);
            senAlfa = System.Math.Sin(angleXToM);
            jm = jxG * cosAlfa * cosAlfa + jyG * senAlfa * senAlfa - 2 * jxyG * senAlfa * cosAlfa;
            jn = jxG * senAlfa * senAlfa + jyG * cosAlfa * cosAlfa + 2 * jxyG * senAlfa * cosAlfa;

            if (mIsAlwaysMajorAxes && jm < jn - mainJToll)
            {
                double buffer;
                buffer = jm;
                jm = jn;
                jn = buffer;
                if (angleXToM < -0.000001)
                {
                    angleXToM += System.Math.PI / 2;
                }
                else
                {
                    angleXToM -= System.Math.PI / 2;
                }
                cosAlfa = System.Math.Cos(angleXToM);
                senAlfa = System.Math.Sin(angleXToM);
            }

            //Principal axes
            Point2d origin;
            origin = new Point2d(xG, yG);
            toPrincipal = new Trans2d(origin, origin + new Point2d(cosAlfa, senAlfa), origin);
            bbox = new BoundingBox2D();
            foreach (List<Polygon> list in new List<Polygon>[] { fills, holes })
            {
                foreach (Polygon poly in list)
                {
                    foreach (Point2d p in poly)
                    {
                        bbox.Update(toPrincipal.PointLocal(p));
                    }
                }
            }
            wm = jm / System.Math.Max(System.Math.Abs(bbox.Max.Y), System.Math.Abs(bbox.Min.Y));
            wn = jn / System.Math.Max(System.Math.Abs(bbox.Max.X), System.Math.Abs(bbox.Min.X));
        }

        #region Serializzazione

        private static int SERIALIZATION_RELEASE = 1;
        protected static string SERIALIZATION_RELEASE_NAME = "SERIALIZATION_RELEASE_NAME";
        private static string PREFIX = "Utilities.Shapes/";

        protected Shapes(SerializationInfo info, StreamingContext context)
        {
            _items = (List<Shape>)info.GetValue(PREFIX + "_items", typeof(List<Shape>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(PREFIX + SERIALIZATION_RELEASE_NAME, SERIALIZATION_RELEASE);
            info.AddValue(PREFIX + "_items", _items, typeof(List<Shape>));
        }

        #endregion Serializzazione
    }

    [Serializable]
    public class Shape : ISerializable
    {
        public readonly Polygon Fill;
        public readonly Polygon[] HolesCanBeNull;

        public Shape(Polygon fill, Polygon[] holesCanBeNull)
        {
            this.Fill = fill;
            this.HolesCanBeNull = holesCanBeNull;
            if (!Fill.IsRightHandOrdered())
            {
                this.Fill.Reverse();
            }
            if (HolesCanBeNull != null)
            {
                foreach (Polygon p in HolesCanBeNull)
                {
                    if (!p.IsRightHandOrdered())
                    {
                        p.Reverse();
                    }
                }
            }
        }

        public Shape(Shape shape)
        {
            this.Fill = new Polygon(shape.Fill);
            if (shape.HolesCanBeNull == null)
            {
                this.HolesCanBeNull = null;
            }
            else
            {
                this.HolesCanBeNull = new Polygon[shape.HolesCanBeNull.Length];
                for (int i = 0; i < shape.HolesCanBeNull.Length; i++)
                {
                    this.HolesCanBeNull[i] = new Polygon(shape.HolesCanBeNull[i]);
                }
            }
        }

        public void Pan(double dx, double dy)
        {
            Fill.Pan(dx, dy);
            if (HolesCanBeNull != null)
            {
                foreach (Polygon hole in HolesCanBeNull)
                {
                    hole.Pan(dx, dy);
                }
            }
        }

        public BoundingBox2D GetBBox()
        {
            BoundingBox2D bbox;
            bbox = new BoundingBox2D();
            bbox.Update(Fill);
            if (HolesCanBeNull != null)
            {
                foreach (Polygon hole in HolesCanBeNull)
                {
                    bbox.Update(hole);
                }
            }
            return bbox;
        }

        public void MoveToMin()
        {
            BoundingBox2D bbox;
            bbox = GetBBox();
            Pan(-bbox.Min.X, -bbox.Min.Y);
        }

        public Shape Mirror(double a, double b, double c)
        {
            Polygon fill = Fill.Mirror(a, b, c);
            Polygon[] hole = null;

            if (HolesCanBeNull != null)
            {
                hole = new Polygon[HolesCanBeNull.Length];
                for (int i = 0; i < HolesCanBeNull.Length; i++)
                    hole[i] = HolesCanBeNull[i].Mirror(a, b, c);
            }

            return new Shape(fill, hole);
        }

        #region Serializzazione

        private static int SERIALIZATION_RELEASE = 1;
        protected static string SERIALIZATION_RELEASE_NAME = "SERIALIZATION_RELEASE_NAME";
        private static string PREFIX = "Utilities.Shape/";

        protected Shape(SerializationInfo info, StreamingContext context)
        {
            Fill = (Polygon)info.GetValue(PREFIX + "Fill", typeof(Polygon));
            if (info.GetBoolean(PREFIX + "HasHoles"))
            {
                HolesCanBeNull = (Polygon[])info.GetValue(PREFIX + "HolesCanBeNull", typeof(Polygon[]));
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(PREFIX + SERIALIZATION_RELEASE_NAME, SERIALIZATION_RELEASE);
            info.AddValue(PREFIX + "Fill", Fill, typeof(Polygon));
            info.AddValue(PREFIX + "HasHoles", HolesCanBeNull != null);
            if (HolesCanBeNull != null)
            {
                info.AddValue(PREFIX + "HolesCanBeNull", HolesCanBeNull, typeof(Polygon[]));
            }
        }

        #endregion Serializzazione
    }
}