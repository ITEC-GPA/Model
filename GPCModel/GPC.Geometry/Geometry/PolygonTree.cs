using System;
using System.Collections.Generic;
using System.Linq;


namespace GPC.Geometry
{
    public class PolygonTree
    {
        private PolygonTree _owner;
        public readonly Polygon Polygon;
        public readonly bool IsHole;
        public readonly PolygonTree[] Childs;

        private PolygonTree(Polygon poly, bool isHole, PolygonTree[] childs)
        {
            Polygon = poly;
            IsHole = isHole;
            Childs = childs;
            if (childs != null && childs.Length > 0)
            {
                if (isHole)
                {
                    foreach (PolygonTree child in childs)
                    {
                        if (child.IsHole)
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
                else
                {
                    foreach (PolygonTree child in childs)
                    {
                        if (!child.IsHole)
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }
        }

        public PolygonTree(Polygon polygon, Polygon[] holesCanBeNull)
        {
            Polygon = polygon;
            IsHole = false;
            if (holesCanBeNull == null)
            {
                Childs = null;
            }
            else
            {
                List<PolygonTree> childs;
                childs = new List<PolygonTree>();
                foreach (Polygon hole in holesCanBeNull)
                {
                    childs.Add(new PolygonTree(hole, true, null));
                }
                Childs = childs.ToArray();
            }
        }

        internal void SetOwner(PolygonTree p)
        {
            _owner = p;
        }

        public PolygonTree Owner
        {
            get => _owner;
        }

        public PolygonTree RootOwner
        {
            get
            {
                if (Owner == null)
                {
                    return this;
                }
                else
                {
                    return Owner.RootOwner;
                }
            }
        }

        public static Shape[] GetShapes(PolygonTree tree)
        {
            List<Shape> shapes = new List<Shape>();
            if (tree.IsHole) throw new NotSupportedException("RootOwner must be a fill");

            shapes.AddRange(tree.GetSubShape());
            return shapes.ToArray();
        }

        private List<Shape> GetSubShape()
        {
            List<Shape> result;
            Polygon owner;
            if (Owner is null)
            {
                owner = null;
            }
            else
            {
                owner = Owner.Polygon;
            }

            result = new List<Shape>();
            if (IsHole)
            {
                if (Childs != null)
                {
                    foreach (PolygonTree fill in Childs)
                    {
                        result.AddRange(fill.GetSubShape());
                    }
                }
            }
            else
            {
                Polygon fill;
                List<Polygon> holesCanBeNull;
                fill = Polygon;
                if (Childs == null)
                {
                    holesCanBeNull = null;
                }
                else
                {
                    holesCanBeNull = new List<Polygon>();
                    foreach (PolygonTree hole in Childs)
                    {
                        holesCanBeNull.Add(hole.Polygon);
                    }
                }
                result.Add(new Shape(fill, holesCanBeNull == null ? null : holesCanBeNull.ToArray()));
                if (Childs != null)
                {
                    foreach (PolygonTree hole in Childs)
                    {
                        result.AddRange(hole.GetSubShape());
                    }
                }
            }
            return result;
        }

        public static PolygonTree[] Union(Polygon a, Polygon b)
        {
            List<PolygonTree> aTs, bTs;
            aTs = new List<PolygonTree>();
            aTs.Add(new PolygonTree(a, false, null));
            bTs = new List<PolygonTree>();
            bTs.Add(new PolygonTree(b, false, null));
            return Boolean(aTs.ToArray(), bTs.ToArray(), ClipType.ctUnion);
        }

        public static PolygonTree[] Difference(Polygon a, Polygon b)
        {
            List<PolygonTree> aTs, bTs;
            aTs = new List<PolygonTree>();
            aTs.Add(new PolygonTree(a, false, null));
            bTs = new List<PolygonTree>();
            bTs.Add(new PolygonTree(b, false, null));
            return Boolean(aTs.ToArray(), bTs.ToArray(), ClipType.ctDifference);
        }

        public static PolygonTree[] Intersection(Polygon a, Polygon b)
        {
            List<PolygonTree> aTs, bTs;
            aTs = new List<PolygonTree>();
            aTs.Add(new PolygonTree(a, false, null));
            bTs = new List<PolygonTree>();
            bTs.Add(new PolygonTree(b, false, null));
            return Boolean(aTs.ToArray(), bTs.ToArray(), ClipType.ctIntersection);
        }

        public static PolygonTree[] NotIntersection(Polygon a, Polygon b)
        {
            List<PolygonTree> aTs, bTs;
            aTs = new List<PolygonTree>();
            aTs.Add(new PolygonTree(a, false, null));
            bTs = new List<PolygonTree>();
            bTs.Add(new PolygonTree(b, false, null));
            return Boolean(aTs.ToArray(), bTs.ToArray(), ClipType.ctXor);
        }

        public static PolygonTree[] Union(PolygonTree[] a, PolygonTree[] b)
        {
            return Boolean(a, b, ClipType.ctUnion);
        }

        public static PolygonTree[] Difference(PolygonTree[] a, PolygonTree[] b)
        {
            return Boolean(a, b, ClipType.ctDifference);
        }

        public static PolygonTree[] Intersection(PolygonTree[] a, PolygonTree[] b)
        {
            return Boolean(a, b, ClipType.ctIntersection);
        }

        public static PolygonTree[] NotIntersection(PolygonTree[] a, PolygonTree[] b)
        {
            return Boolean(a, b, ClipType.ctXor);
        }

        private static PolygonTree[] Boolean(PolygonTree[] a, PolygonTree[] b, ClipType code)
        {
            const int FACTOR = 1000;
            Clipper clipper;
            PolyTree resultI;
            clipper = new Clipper();
            foreach (PolygonTree t in a)
            {
                AddToPath(t, FACTOR, clipper, PolyType.ptSubject);
            }
            foreach (PolygonTree t in b)
            {
                AddToPath(t, FACTOR, clipper, PolyType.ptClip);
            }
            resultI = new PolyTree();
            if (clipper.Execute(code, resultI, PolyFillType.pftEvenOdd))
            {
                if (resultI.Contour.Count > 0)
                {
                    throw new NotSupportedException();
                }
                if (resultI.ChildCount > 0)
                {
                    List<PolygonTree> bufferResult;
                    bufferResult = new List<PolygonTree>();
                    foreach (PolyNode n in resultI.Childs)
                    {
                        bufferResult.Add(GetPolygonTree(n, FACTOR));
                    }
                    return bufferResult.ToArray();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static void AddToPath(PolygonTree t, double FACTOR, Clipper clipper, PolyType type)
        {
            List<IntPoint> buffer;
            buffer = GetIntPointList(t.Polygon, FACTOR);
            clipper.AddPath(buffer, type, true);
            if (t.Childs != null)
            {
                foreach (PolygonTree p in t.Childs)
                {
                    AddToPath(p, FACTOR, clipper, type);
                }
            }
        }

        private static List<IntPoint> GetIntPointList(Polygon p, double FACTOR)
        {
            List<IntPoint> result;
            result = new List<IntPoint>();
            foreach (Point2d v in p)
            {
                result.Add(new IntPoint(v.X * FACTOR, v.Y * FACTOR));
            }
            return result;
        }

        private static PolygonTree GetPolygonTree(PolyNode resultI, double FACTOR)
        {
            Polygon bufferPoly;
            PolygonTree[] childs;
            PolygonTree result;
            bufferPoly = new Polygon();
            foreach (IntPoint p in resultI.Contour)
            {
                bufferPoly.Add(p.X / FACTOR, p.Y / FACTOR);
            }

            if (resultI.ChildCount > 0)
            {
                childs = new PolygonTree[resultI.ChildCount];
                for (int i = 0; i < resultI.ChildCount; i++)
                {
                    childs[i] = GetPolygonTree(resultI.Childs[i], FACTOR);
                }
            }
            else
            {
                childs = null;
            }
            result = new PolygonTree(bufferPoly, resultI.IsHole, childs);
            if (result.Childs != null)
            {
                foreach (PolygonTree p in result.Childs)
                {
                    p.SetOwner(result);
                }
            }
            return result;
        }
    }
}