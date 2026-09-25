using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    public enum EdgeType
    {
        Sharp = 0, // Without working, simple corner. Default.
        Fillet = 1, // Rounded corners with circumference arc.
        Chamfer = 2 // Straight line.
    }

    /// <summary>
    /// List of rectangular thin wall.
    /// Useful for approximating thin steel profiles. They do not use fillets between wall elements.
    /// 2023-06-20: Refactoring, moved points from ThinWallSection to ThinWall. Added version=2 in serialization.
    /// </summary>
    [Serializable]
    public abstract class ThinWallSection : Section, ISerializable
    {
        #region Variables

        protected ThinWall[] _thinWalls;
        protected EdgeType _edgeWorking;

        #endregion

        #region Properties

        public override double Height { get; set; }

        public override ThinWall[] ThinWalls => _thinWalls;

        /// <summary>
        /// Edge workings, used to define the type of workings for inside corners.
        /// For steel, EdgeType.Chamfer can be used to represent welds
        /// or EdgeType.Fillet for simple arc fillets.
        /// </summary>
        internal EdgeType EdgeWorking => _edgeWorking;

        #endregion

        #region Public Constructors

        /// <summary>
        /// The default constructor of generic ThinWallSection
        /// </summary>
        /// <param name="name">The name of the section</param>
        internal ThinWallSection(string name)
            : base(name)
        {
        }

        protected ThinWallSection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ThinWallSectionVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version == 1)
            {
                var thinWallsWithoutPoint = (ThinWall[])info.GetValue("ThinWalls", typeof(ThinWall[]));
                var points = (Point2d[])info.GetValue("Points", typeof(Point2d[]));

                if (thinWallsWithoutPoint.Length != points.Length)
                    throw new ArgumentException("Different lenght between thin walls and points.");

                _thinWalls = new ThinWall[thinWallsWithoutPoint.Length];
                for (int i = 0; i < thinWallsWithoutPoint.Length; i++)
                    _thinWalls[i] = new ThinWall(thinWallsWithoutPoint[i].L, thinWallsWithoutPoint[i].T, thinWallsWithoutPoint[i].Angle, points[i]);
            }
            else if (version == 2)
            {
                _thinWalls = (ThinWall[])info.GetValue("ThinWalls", typeof(ThinWall[]));
            }
        }

        #endregion

        #region Protected methods

        protected void SetThinWalls(ThinWall[] thinWalls)
        {
            _thinWalls = thinWalls ?? throw new ArgumentNullException(nameof(thinWalls));
        }

        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
            if (sectionType == SectionTypes.Rolled)
                _edgeWorking = EdgeType.Fillet;
            else if (sectionType == SectionTypes.Welded)
                _edgeWorking = EdgeType.Chamfer;
        }

        #endregion

        #region Mesh

        protected override Mesh CreateMesh() => GetMesh();

        public Mesh GetMesh()
        {
            Mesh mesh = new Mesh();
            double tol = 0.01;

            for (int i = 0; i < _thinWalls.Count(); i++)
            {
                List<int> idList = new List<int>();
                mesh.UpdateVertexBVH();
                Polygon2d polygon = (Polygon2d)_thinWalls[i].GetPerimeter().Clone();

                for (int j = 0; j < polygon.Count; j++)
                {
                    List<int> neighboursId = mesh.FindNeighbours(polygon[j], tol);

                    if (neighboursId.Count == 0)
                    {
                        MeshVertex meshVertex = new MeshVertex(polygon[j]);
                        idList.Add(mesh.Vertices.Add(meshVertex));
                    }
                    else
                    {
                        idList.Add(neighboursId.FirstOrDefault());
                    }
                }

                mesh.Edges.Add(new MeshEdge(idList[0], idList[1]));
                mesh.Edges.Add(new MeshEdge(idList[1], idList[2]));
                mesh.Edges.Add(new MeshEdge(idList[2], idList[0]));

                mesh.Edges.Add(new MeshEdge(idList[2], idList[3]));
                mesh.Edges.Add(new MeshEdge(idList[3], idList[0]));
                mesh.Edges.Add(new MeshEdge(idList[0], idList[2]));

                mesh.Faces.Add(new MeshFace(new int[] { idList[0], idList[1], idList[2] }));
                mesh.Faces.Add(new MeshFace(new int[] { idList[2], idList[3], idList[0] }));
            }

            return mesh;
        }

        #endregion

        #region Public abstract method

        protected abstract override double CalculateJw();

        protected abstract override Point2d CalculateShearCenter();

        #endregion

        #region Public method

        protected override abstract Shape2d GetShape();

        /// <inheritdoc cref="Section.CalculateCentroid()"/>
        protected override Point2d CalculateCentroid()
        {
            double xSum = 0;
            double ySum = 0;
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                xSum += _thinWalls[i].CalculateSy();
                ySum += _thinWalls[i].CalculateSx();
                area += _thinWalls[i].Area;
            }

            return new Point2d(xSum / area, ySum / area);
        }

        protected override double CalculateJt()
        {
            double jt = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                jt += _thinWalls[i].CalculateJt();
            }

            return jt;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the X-axis (the Y-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        protected override double CalculateJ11() => SectionHelper.CalculateJ11(_jxx, _jyy, _jxy);

        /// <summary>
        /// Calculate the first moment of inertia respect the Y-axis (the Z-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        protected override double CalculateJ22() => SectionHelper.CalculateJ22(_jxx, _jyy, _jxy);

        private double CalculateAreaThinWallSection()
        {
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                area += _thinWalls[i].Area;

            return area;
        }

        protected override double CalculateArea()
        {
            return CalculateAreaThinWallSection();
        }

        /// <summary>
        /// Moment of inertia with respect to the X axis passing through the center of gravity
        /// of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <returns></returns>
        private double CalculateJxxThinWall()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJx();

            j -= CalculateAreaThinWallSection() * _centroid.Y * _centroid.Y;

            return j;
        }

        protected override double CalculateJxx() => CalculateJxxThinWall();

        /// <summary>
        /// Moment of inertia with respect to the Y axis passing through the center of gravity
        /// of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <returns></returns>
        private double CalculateJyyThinWall()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJy();

            j -= CalculateAreaThinWallSection() * _centroid.X * _centroid.X;

            return j;
        }

        protected override double CalculateJyy() => CalculateJyyThinWall();

        /// <summary>
        /// Product of inertia with respect to the X and Y axes passing through the center of
        /// gravity of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <returns></returns>
        protected override double CalculateJxy()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJxy();

            j -= CalculateAreaThinWallSection() * _centroid.X * _centroid.Y;

            return j;
        }

        /// <summary>
        /// Calculate plastic modulus for a list of thinwall.
        /// The modulus is calculated with respect to the barycenter, relative to a rotated axis of Alpha.
        /// Approximate method using thinwall axis.
        /// </summary>
        /// <param name="angle">Angle in radians, counterclockwise, is zero for the x-positive direction.</param>
        /// <returns>The plastic modulus of the thin walls when the axis through the centroid divides their area in two equal parts (it is the
        /// plastic neutral axis: the sections symmetric respect to the axis); otherwise <see cref="double.NaN"/>: the exact modulus of the shape is
        /// computed at the first access (before, the static moment respect to the axis through the centroid, that is not the plastic neutral
        /// axis: the modulus was overestimated, e.g. +21% for a RHS 400x200 with webs 30 and 10 about the vertical axis)</returns>
        protected double CalculateWplAngle(in double angle)
        {
            // Axis with respect to which to calculate the plastic modulus.
            var centroid = CalculateCentroid();
            var versor = Vector2d.XAxis;
            versor.Rotate(angle);
            var axis = new Line2d(centroid, centroid + versor);

            if (!DividesInEqualAreas(axis))
                return double.NaN;

            // Redefine a list of thin walls.
            var thinWalls = new List<ThinWall>();
            foreach (var tw_original in _thinWalls)
            {
                var tw_originalMid = tw_original.GetMiddleLine();
                bool startIsLeft = axis.OrientedDistFromSegment2D(tw_originalMid[0]) >= 0.0;
                bool endIsLeft = axis.OrientedDistFromSegment2D(tw_originalMid[1]) >= 0.0;

                if (startIsLeft && endIsLeft)
                {
                    thinWalls.Add(tw_original);
                }
                else if (startIsLeft || endIsLeft)
                {
                    if (axis.GetIntersectionWithInfiniteLine(new Line2d(tw_originalMid[0], tw_originalMid[1]), out Point2d intersection))
                    {
                        if (startIsLeft)
                            thinWalls.Add(new ThinWall(tw_originalMid[0], intersection, tw_original.T));
                        else
                            thinWalls.Add(new ThinWall(intersection, tw_originalMid[1], tw_original.T));
                    }
                }
            }

            // Calculate the static moment of the half section with respect to the axis.
            double S_axis = 0.0;
            foreach (var tw in thinWalls)
            {
                double area = tw.Area;
                double distance = axis.OrientedDistFromSegment2D(tw.Point); // This value is always positive.

                S_axis += area * distance;
            }

            return S_axis * 2.0;
        }

        /// <returns>True if the thin walls have the same area on the two sides of <paramref name="axis"/> (a wall on the axis is half on each side)</returns>
        private bool DividesInEqualAreas(Line2d axis)
        {
            double left = 0.0, right = 0.0;
            foreach (var wall in _thinWalls)
            {
                var middle = wall.GetMiddleLine();
                double d0 = axis.OrientedDistFromSegment2D(middle[0]);
                double d1 = axis.OrientedDistFromSegment2D(middle[1]);
                double area = wall.Area;

                if (d0 >= 0.0 && d1 >= 0.0 || d0 <= 0.0 && d1 <= 0.0)
                {
                    double dMax = Math.Max(Math.Abs(d0), Math.Abs(d1));
                    if (dMax == 0.0)
                    {
                        left += area / 2.0;
                        right += area / 2.0;
                    }
                    else if (d0 + d1 > 0.0)
                        left += area;
                    else
                        right += area;
                }
                else
                {
                    // the wall crosses the axis: the parts are proportional to the distances of the ends
                    double fraction = Math.Abs(d0) / (Math.Abs(d0) + Math.Abs(d1));
                    double partStart = area * fraction;
                    if (d0 > 0.0)
                    {
                        left += partStart;
                        right += area - partStart;
                    }
                    else
                    {
                        right += partStart;
                        left += area - partStart;
                    }
                }
            }

            return Math.Abs(left - right) <= 1e-9 * (left + right);
        }

        protected override double CalculateWpl1()
        {
            return CalculateWplAngle(AngleX1);
        }

        protected override double CalculateWpl2()
        {
            return CalculateWplAngle(AngleX1 + Math.PI * 0.5);
        }

        protected override double CalculateWplX()
        {
            return CalculateWplAngle(0);
        }

        protected override double CalculateWplY()
        {
            return CalculateWplAngle(Math.PI * 0.5);
        }

        protected abstract override double CalculateWel1Max();

        protected abstract override double CalculateWel1Min();

        protected abstract override double CalculateWel2Max();

        protected abstract override double CalculateWel2Min();

        public override Point2d[] GetSectionPoints()
        {
            List<Point2d> points = new List<Point2d>();

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                List<Point2d> _pointBuffer = _thinWalls[i].GetPerimeter().Select(j => j).ToList();

                points.AddRange(_pointBuffer.Select(k => (Point2d)k.Clone()));
            }

            return points.ToArray();
        }

        /// <summary>
        /// Break all thiwall in lines.
        /// </summary>
        /// <param name="edges">List of perimeter sides in the local system of ThinWallSection.</param>
        /// <returns>A new list with all breaked thinwalls.</returns>
        public ThinWall[] BreakThinWallsInEdges(IList<Line2d> edges)
        {
            var thinWallsBreaked = new LinkedList<ThinWall>();
            // Clone all thinwalls.
            for (int j = 0; j < _thinWalls.Length; j++)
                thinWallsBreaked.AddLast((ThinWall)_thinWalls[j].Clone());

            for (int j = 0; j < edges.Count(); j++)
            {
                for (LinkedListNode<ThinWall> thinWallnode = thinWallsBreaked.First; thinWallnode != null; thinWallnode = thinWallnode.Next)
                {
                    // Build middle line in global position.
                    var thinWall = thinWallnode.Value;
                    var midLinePoint = thinWall.GetMiddleLine();
                    var midLine = new Line2d(midLinePoint[0], midLinePoint[1]);

                    if (edges[j].GetIntersection(midLine, out Point2d intersection) &&
                        intersection.DistanceTo(midLine.Start) > 1 &&
                        intersection.DistanceTo(midLine.End) > 1)
                    {
                        // Break in local position.
                        var localBreaked = thinWall.BreaksAtAnIntermediatePoint(intersection);
                        for (int k = localBreaked.Length - 1; k >= 0; k--)
                            thinWallsBreaked.AddAfter(thinWallnode, localBreaked[k]);

                        // Save the position to be deleted and move to the next one.
                        // A thinwall can only be broken once on one side of the concrete so it can move to the next thinwall.
                        var thinWallnodeToRemove = thinWallnode;
                        thinWallnode = thinWallnode.Next;
                        thinWallsBreaked.Remove(thinWallnodeToRemove);
                    }
                }
            }

            return thinWallsBreaked.ToArray();
        }

        #endregion

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {

            return obj is ThinWallSection section &&
                   base.Equals(obj) &&
                   _thinWalls.SequenceEqual(section._thinWalls);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _thinWalls.GetHashCodeSequence();
                return hashCode;
            }
        }

        public static bool operator ==(ThinWallSection left, ThinWallSection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ThinWallSection left, ThinWallSection right)
        {
            return !(left == right);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            int version = 2;
            info.AddValue("ThinWallSectionVersion", version);
            info.AddValue("ThinWalls", _thinWalls, typeof(ThinWall[]));
        }

        #endregion

        #region Nested classes ThinWall

        /// With _angle = 0:
        ///    ┌-----------------┐
        /// _t |                 |
        ///    └-----------------┘
        ///            _l
        /// </summary>
        [Serializable]
        public class ThinWall : ICloneable
        {
            #region Variables

            private readonly double _t;
            private readonly double _l;
            private readonly double _angle;
            private readonly Point2d _point;

            #endregion

            #region Properties

            /// <summary>
            /// The _thickness of the wall
            /// </summary>
            public double T => _t;

            /// <summary>
            /// The lenght of the wall
            /// </summary>
            public double L => _l;

            /// <summary>
            /// The angle of rotation of the principal axis. Angle = 0 is the X-axis, PI.GRECO/2 is the y-axis
            /// Angle in radians, counterclockwise, is zero for the x-positive direction.
            /// </summary>
            public double Angle => _angle;

            /// <summary>
            /// The position of the thin wall, the center of gravity point.
            /// </summary>
            public Point2d Point => _point;

            /// <summary>
            /// The area og the thin wal
            /// </summary>
            public double Area => CalculateArea();

            /// <summary>
            /// Defines whether the ThinWall is entirely outside or inside the concrete area.
            /// </summary>
            public bool IsInsideConcrete { get; set; }

            #endregion

            #region Protected constructor

            /// <summary>
            /// Older version of the constructor, kept for compatibility.
            /// </summary>
            /// <param name="length"></param>
            /// <param name="thickness"></param>
            /// <param name="angle"></param>
            internal ThinWall(double length, double thickness, double angle)
                : this(length, thickness, angle, Point2d.Origin)
            {
            }

            /// <summary>
            /// The default constructor of generic ThinWall
            /// </summary>
            /// <param name="length">The length of the ThinWall</param>
            /// <param name="thickness">The _thickness of the ThinWall</param>
            /// <param name="angle">The angle of the ThinWall. 0 is orizontal, Math.PI / 2.0 is vertical</param>
            /// <param name="point">Position, barycenter/centroid of rectangular.</param>
            internal ThinWall(double length, double thickness, double angle, Point2d point)
                : base()
            {
                _t = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;
                _l = length < 0 ? throw new ArgumentException($"Lenght cannot be lower than zero") : length;
                _angle = angle;
                _point = point ?? throw new ArgumentException($"Position cannot be null.");
            }

            /// <summary>
            /// Constructor using axis end points and thickness.
            /// </summary>
            /// <param name="startPoint"></param>
            /// <param name="endPoint"></param>
            /// <param name="thickness"></param>
            internal ThinWall(in Point2d startPoint, in Point2d endPoint, in double thickness)
            {
                _t = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;
                Vector2d vector = endPoint - startPoint;
                _l = vector.Length;
                _angle = Math.Atan2(vector.Y, vector.X);
                _point = 0.5 * (startPoint + endPoint);
            }

            protected ThinWall(SerializationInfo info, StreamingContext _)
            {
                int version;
                try
                {
                    version = info.GetInt32("ThinWallVersion");
                }
                catch (Exception)
                {
                    version = 1;
                }

                _t = info.GetDouble("T");
                _l = info.GetDouble("L");
                _angle = info.GetDouble("Angle");
                try
                {
                    _point = (Point2d)info.GetValue("Point", typeof(Point2d));
                }
                catch (Exception)
                {
                    _point = Point2d.Origin;
                }

                if (version >= 2)
                    IsInsideConcrete = info.GetBoolean("IsInsideConcrete");
                else
                    IsInsideConcrete = false;
            }

            #endregion

            #region Internal method

            internal Polygon2d GetPerimeter()
            {
                var sinAngle = Math.Sin(_angle);
                var cosAngle = Math.Cos(_angle);
                var lHalf = _l / 2.0;
                var tHalf = _t / 2.0;

                var poly = new Polygon2d(new Point2d[] {
                    new Point2d(- lHalf * cosAngle - tHalf * sinAngle, - lHalf * sinAngle - tHalf * cosAngle),
                    new Point2d(lHalf * cosAngle - tHalf * sinAngle, lHalf * sinAngle - tHalf * cosAngle),
                    new Point2d(lHalf * cosAngle + tHalf * sinAngle, lHalf * sinAngle + tHalf * cosAngle),
                    new Point2d(- lHalf * cosAngle + tHalf * sinAngle, - lHalf * sinAngle + tHalf * cosAngle)
                    });
                poly.Move(_point.X, _point.Y);
                return poly;
            }

            public Point2d[] GetMiddleLine()
            {
                var sinAngle = Math.Sin(_angle);
                var cosAngle = Math.Cos(_angle);
                var lHalf = _l / 2.0;
                return new Point2d[] {  _point + new Point2d(-lHalf * cosAngle, -lHalf * sinAngle),
                                        _point + new Point2d(lHalf * cosAngle, lHalf * sinAngle) };
            }

            /// <summary>
            /// Breaks at an intermediate point.
            /// Returns two thinwalls if the point is intermediate, otherwise returns the thinwall itself.
            /// </summary>
            /// <returns></returns>
            public ThinWall[] BreaksAtAnIntermediatePoint(in Point2d intermediatePoint)
            {
                var midLinePoints = GetMiddleLine();
                var midLine = new Line2d(midLinePoints[0], midLinePoints[1]);
                if (midLine.IsPointOnLine(intermediatePoint) &&
                    intermediatePoint.DistanceTo(midLinePoints[0]) > 1 &&
                    intermediatePoint.DistanceTo(midLinePoints[1]) > 1)
                {
                    return new ThinWall[]
                    {
                        new ThinWall(midLinePoints[0], intermediatePoint, _t),
                        new ThinWall(intermediatePoint, midLinePoints[1], _t)
                    };
                }
                else
                {
                    return new ThinWall[]
                    {
                        this
                    };
                }
            }

            /// <summary>
            /// Calculate the area of the wall 
            /// </summary>
            /// <returns></returns>
            internal double CalculateArea()
            {
                return _t * _l;
            }

            /// <summary>
            /// Moment of inertia with respect to the X and Y axes.
            /// </summary>
            /// <returns></returns>
            internal double CalculateJxy()
            {
                double Jxx = _l * Math.Pow(_t, 3) / 12.0;
                double Jyy = _t * Math.Pow(_l, 3) / 12.0;
                double Jxy = 0.0;

                return SectionHelper.CalculateJxyAlpha(Jxx, Jyy, Jxy, -_angle) + _point.X * _point.Y * Area;
            }

            /// <summary>
            /// Moment of inertia with respect to the Y-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateJy()
            {
                double momentTranslation = Area * _point.X * _point.X;
                if (Math.Abs(_angle) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(_angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _t * Math.Pow(_l, 3) / 12.0;

                else if (Math.Abs(_angle - Math.PI / 2) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _l * Math.Pow(_t, 3) / 12.0;

                else
                {
                    double Jxx = _l * Math.Pow(_t, 3) / 12.0;
                    double Jyy = _t * Math.Pow(_l, 3) / 12.0;
                    double Jxy = 0.0;
                    return momentTranslation + SectionHelper.CalculateJAlpha(Jxx, Jyy, Jxy, -_angle + 0.5 * Math.PI);
                }
            }

            /// <summary>
            /// Moment of inertia with respect to the X-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateJx()
            {
                double momentTranslation = Area * _point.Y * _point.Y;
                if (Math.Abs(_angle) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(_angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _l * Math.Pow(_t, 3) / 12.0;

                else if (Math.Abs(_angle - Math.PI / 2) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _t * Math.Pow(_l, 3) / 12.0;

                else
                {
                    double Jxx = _l * Math.Pow(_t, 3) / 12.0;
                    double Jyy = _t * Math.Pow(_l, 3) / 12.0;
                    double Jxy = 0.0;
                    return momentTranslation + SectionHelper.CalculateJAlpha(Jxx, Jyy, Jxy, -_angle);
                }
            }

            /// <summary>
            /// Static moment with respect to X-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateSx() => Area * _point.Y;

            /// <summary>
            /// Static moment with respect to Y-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateSy() => Area * _point.X;

            internal virtual double CalculateJt()
            {
                return L * Math.Pow(T, 3) / GetAlpha();
            }

            internal virtual double CalculateJw()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Calculate the polar moment of inertia 
            /// </summary>
            /// <returns></returns>
            internal virtual double CalculateJpolar()
            {
                return CalculateJx() + CalculateJy();
            }

            internal double GetAlpha()
            {
                return 3 + 1.8 * T / L;
            }

            public override bool Equals(object obj)
            {
                return obj is ThinWall wall &&
                       _t == wall._t &&
                       _l == wall._l &&
                       _angle == wall._angle &&
                       _point == wall._point;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = -23;
                    hashCode = hashCode * -17 + _t.GetHashCode();
                    hashCode = hashCode * -17 + _l.GetHashCode();
                    hashCode = hashCode * -17 + _angle.GetHashCode();
                    hashCode = hashCode * -17 + _point.GetHashCode();
                    return hashCode;
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                int version = 2;
                info.AddValue("ThinWallVersion", version);
                info.AddValue("T", _t);
                info.AddValue("L", _l);
                info.AddValue("Angle", _angle);
                info.AddValue("Point", _point);
                info.AddValue("IsInsideConcrete", IsInsideConcrete);
            }

            public object Clone()
            {
                return (ThinWall)MemberwiseClone();
            }

            #endregion
        }

        #endregion
    }
}
