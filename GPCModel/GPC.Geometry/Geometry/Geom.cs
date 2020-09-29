using System;
using System.Collections.Generic;
using System.Net.Mail;


namespace GPC.Geometry
{
    public class Geom
    {
        public static double Distance(double aX, double aY, double bX, double bY)
        {
            return System.Math.Sqrt(SquareDistance(aX, aY, bX, bY));
        }

        public static double Distance(double aX, double aY, double aZ, double bX, double bY, double bZ)
        {
            return System.Math.Sqrt(SquareDistance(aX, aY, aZ, bX, bY, bZ));
        }

        public static double SquareDistance(double aX, double aY, double bX, double bY)
        {
            return System.Math.Pow(aX - bX, 2d) + System.Math.Pow(aY - bY, 2d);
        }

        public static double SquareDistance(double aX, double aY, double aZ, double bX, double bY, double bZ)
        {
            return System.Math.Pow(aX - bX, 2d) + System.Math.Pow(aY - bY, 2d) + System.Math.Pow(aZ - bZ, 2d);
        }

        public static double Distance(Point2d a, Point2d b)
        {
            return Distance(a.X, a.Y, b.X, b.Y);
        }

        public static double Distance(Point3d a, Point3d b)
        {
            return Distance(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
        }

        public static double SquareDistance(Point2d a, Point2d b)
        {
            return SquareDistance(a.X, a.Y, b.X, b.Y);
        }

        public static double SquareDistance(Point3d a, Point3d b)
        {
            return SquareDistance(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
        }

        public static double GetLength(Point2d p)
        {
            return Distance(p, Point2d.Origin);
        }

        public static double GetLength(Point3d p)
        {
            return Distance(p, Point3d.Origin);
        }

        public static double GetSquareLength(Point2d p)
        {
            return SquareDistance(p, Point2d.Origin);
        }

        public static double GetSquareLength(Point3d p)
        {
            return SquareDistance(p, Point3d.Origin);
        }

        public static Point2d Direction(Point2d p)
        {
            double l = Distance(Point2d.Origin, p);
            return p / l;
        }

        public static Point3d Direction(Point3d p)
        {
            double l = Distance(Point3d.Origin, p);
            return p / l;
        }

        public static Point2d Direction(Point2d a, Point2d b)
        {
            double l = Distance(a, b);
            return (b - a) / l;
        }

        public static Point3d Direction(Point3d a, Point3d b)
        {
            double l = Distance(a, b);
            return (b - a) / l;
        }

        public static Point2d Direction(double angleRad)
        {
            return new Point2d(System.Math.Cos(angleRad), System.Math.Sin(angleRad));
        }

        public static Point2d Normal(Point2d dir, bool onLeftOfdir)
        {
            if (onLeftOfdir)
            {
                return new Point2d(-dir.Y, dir.X);
            }
            else
            {
                return new Point2d(dir.Y, -dir.X);
            }
        }

        public static bool GetLineIntersection(Point2d a1, Point2d a2, Point2d b1, Point2d b2, out Point2d inters)
        {
            //https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
            double denom;
            denom = (a1.X - a2.X) * (b1.Y - b2.Y) - (a1.Y - a2.Y) * (b1.X - b2.X);
            if (denom == 0)
            {
                inters = null;
                return false;
            }
            else
            {
                double t;
                t = ((a1.X - b1.X) * (b1.Y - b2.Y) - (a1.Y - b1.Y) * (b1.X - b2.X)) / denom;
                inters = new Point2d(a1.X + t * (a2.X - a1.X), a1.Y + t * (a2.Y - a1.Y));
                return true;
            }
        }

        /// <summary>
        /// return normal distance from point p, to line defined that pass from a to b.
        /// this is not the distance from point to segment
        /// </summary>
        /// <param name="p">point of interest</param>
        /// <param name="a">start point of line</param>
        /// <param name="b">end point of line</param>
        /// <returns></returns>

        ///https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
        public static double GetPointToLineDistance(Point2d p, Point2d a, Point2d b)
        {
             return Math.Abs((b.Y - a.Y)*p.X - (b.X - a.X)*p.Y + b.X*a.Y - b.Y*a.X) / Distance(a, b);
        }

        /// <summary>
        /// return distance (not normal) from point p, to segment from point A to point B.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>

        /// https://stackoverflow.com/a/6853926
        /// 
        public static double GetPointToSegmentDistance(Point2d p, Point2d a, Point2d b)
        {
            double A = p.X - a.X;
            double B = p.Y - a.Y;
            double C = b.X - a.X;
            double D = b.Y - a.Y;

            double dot = A * C + B * D;
            double squareDistance = SquareDistance(a, b);
            double param = -1;

            if (squareDistance != 0) //in case of 0 length line
                param = dot / squareDistance;

            double xx; double yy;

            if (param < 0)
            {
                xx = a.X;
                yy = a.Y;
            }
            else if (param > 1)
            {
                xx = b.X;
                yy = b.Y;
            }
            else
            {
                xx = a.X + param * C;
                yy = a.Y + param * D;
            }

            var dx = p.X - xx;
            var dy = p.Y - yy;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// return minimum distance from point to border of polygon
        /// </summary>
        /// <param name="p"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static double GetPointToPolygonDistance(Point2d p, Polygon poly)
        {
            double minDistance = double.MaxValue;
            for (int i = 0; i < poly.Count; i++)
            {
                Point2d start = poly[i];
                Point2d end = poly.GetNextItem(i);

                double distance = GetPointToSegmentDistance(p, start, end);

                if (minDistance > distance)
                    minDistance = distance;
            }
            return minDistance;
        }

        public class Volume
        {
            public readonly Face[] Faces;

            public Volume(Face[] faces)
            {
                this.Faces = faces;
            }

            public double GetVolume()
            {
                double result = 0;
                foreach (Face f in Faces)
                {
                    foreach (Face triangle in f.GetTriangledFaces())
                    {
                        result += triangle.RightOrientedVertexes[0] * (triangle.RightOrientedVertexes[1] ^ triangle.RightOrientedVertexes[2]);
                    }
                }
                return 1d / 6d * result;
            }
        }

        public class Face
        {
            public readonly Point3d[] RightOrientedVertexes;

            public Face(Point3d[] rightOrientedVertexes)
            {
                if (rightOrientedVertexes.Length < 3)
                {
                    throw new NotSupportedException("Lines not admitted");
                }
                this.RightOrientedVertexes = rightOrientedVertexes;
            }

            internal Face[] GetTriangledFaces()
            {
                List<Face> faces = new List<Face>();
                for (int i = 2; i < RightOrientedVertexes.Length; i++)
                {
                    Point3d[] buffer;
                    buffer = new Point3d[3];
                    buffer[0] = RightOrientedVertexes[0];
                    buffer[1] = RightOrientedVertexes[i - 1];
                    buffer[2] = RightOrientedVertexes[i];
                    faces.Add(new Face(buffer));
                }
                return faces.ToArray();
            }
        }
    }
}