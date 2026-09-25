using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// The working of the inside corners of a section
    /// </summary>
    public enum EdgeType
    {
        /// <summary>
        /// Without working, simple corner. Default.
        /// </summary>
        Sharp = 0,
        /// <summary>
        /// Rounded corners with circumference arc.
        /// </summary>
        Fillet = 1,
        /// <summary>
        /// Straight line.
        /// </summary>
        Chamfer = 2
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

        /// <summary>
        /// The thin walls
        /// </summary>
        protected ThinWall[] _thinWalls;
        /// <summary>
        /// The working of the inside corners
        /// </summary>
        protected EdgeType _edgeWorking;

        #endregion

        #region Properties

        /// <summary>
        /// The overall height
        /// </summary>
        public override double Height { get; set; }

        /// <summary>
        /// The thin walls
        /// </summary>
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

        /// <summary>
        /// Deserialization constructor (version 1: the thin walls and their points in two arrays; version 2: the thin walls with their points)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        /// <exception cref="ArgumentException">If the version 1 arrays have different lengths</exception>
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

        /// <summary>
        /// Sets the thin walls
        /// </summary>
        /// <param name="thinWalls">The thin walls</param>
        /// <exception cref="ArgumentNullException">If <paramref name="thinWalls"/> is null</exception>
        protected void SetThinWalls(ThinWall[] thinWalls)
        {
            _thinWalls = thinWalls ?? throw new ArgumentNullException(nameof(thinWalls));
        }

        /// <summary>
        /// Sets the working of the corners from the type of the section: fillet for rolled, chamfer for welded
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
            if (sectionType == SectionTypes.Rolled)
                _edgeWorking = EdgeType.Fillet;
            else if (sectionType == SectionTypes.Welded)
                _edgeWorking = EdgeType.Chamfer;
        }

        #endregion

        #region Mesh

        /// <summary>
        /// The mesh of the thin walls (see <see cref="GetMesh()"/>)
        /// </summary>
        /// <returns>The new mesh</returns>
        protected override Mesh CreateMesh() => GetMesh();

        /// <summary>
        /// A mesh with two triangles for each thin wall; the vertices closer than 0.01 are merged
        /// </summary>
        /// <returns>The new mesh</returns>
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

        /// <summary>
        /// Calculate the warping constant
        /// </summary>
        /// <returns>The warping constant</returns>
        protected abstract override double CalculateJw();

        /// <summary>
        /// Calculate the shear center
        /// </summary>
        /// <returns>The shear center</returns>
        protected abstract override Point2d CalculateShearCenter();

        #endregion

        #region Public method

        /// <summary>
        /// Creates the shape of the section
        /// </summary>
        /// <returns>The shape</returns>
        protected override abstract Shape2d GetShape();

        /// <summary>
        /// The corners added to the thin walls or removed from them (fillets and welds between the walls, fillets at the tips of the
        /// flanges), included in the area, in the centroid and in the moments of inertia: none in the base class
        /// </summary>
        /// <returns>The corners</returns>
        private protected virtual SectionCorner[] GetCorners() => new SectionCorner[0];

        /// <summary>
        /// Calculate the centroid: the thin walls plus the corners (see <see cref="GetCorners"/>)
        /// </summary>
        /// <returns>The centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            SectionCorner[] corners = GetCorners();
            if (corners.Length == 0)
                return CalculateThinWallsCentroid();

            double xSum = 0;
            double ySum = 0;
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                xSum += _thinWalls[i].CalculateSy();
                ySum += _thinWalls[i].CalculateSx();
                area += _thinWalls[i].Area;
            }

            foreach (SectionCorner corner in corners)
            {
                xSum += corner.Sign * corner.Area * corner.X;
                ySum += corner.Sign * corner.Area * corner.Y;
                area += corner.Sign * corner.Area;
            }

            return new Point2d(xSum / area, ySum / area);
        }

        /// <summary>
        /// The centroid of the thin walls only
        /// </summary>
        /// <returns>The centroid of the thin walls</returns>
        private Point2d CalculateThinWallsCentroid()
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

        /// <summary>
        /// Calculate the torsion constant: sum of the ones of the thin walls
        /// </summary>
        /// <returns>The torsion constant</returns>
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
        /// Calculate the moment of inertia about the principal axis 1 (the maximum one) from Jxx, Jyy and Jxy
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJ11() => SectionHelper.CalculateJ11(_jxx, _jyy, _jxy);

        /// <summary>
        /// Calculate the moment of inertia about the principal axis 2 (the minimum one) from Jxx, Jyy and Jxy
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJ22() => SectionHelper.CalculateJ22(_jxx, _jyy, _jxy);

        /// <summary>
        /// The area of the thin walls
        /// </summary>
        /// <returns>The sum of the areas</returns>
        private double CalculateAreaThinWallSection()
        {
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                area += _thinWalls[i].Area;

            return area;
        }

        /// <summary>
        /// Calculate the area: sum of the areas of the thin walls plus the corners (see <see cref="GetCorners"/>)
        /// </summary>
        /// <returns>The area</returns>
        protected override double CalculateArea()
        {
            double area = CalculateAreaThinWallSection();

            foreach (SectionCorner corner in GetCorners())
                area += corner.Sign * corner.Area;

            return area;
        }

        /// <summary>
        /// Moment of inertia with respect to the X axis passing through the center of gravity
        /// of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <remarks>
        /// The moment about the origin is moved to the centroid of the thin walls and then to the one of the section, which is different when
        /// the section has corners. Before, the area of the thin walls was moved directly from the origin to the centroid of the section, as if
        /// it were the centroid of the thin walls: in a UPN 300 Jyy was 3.6% greater and Jxy was not 0
        /// </remarks>
        /// <returns>The moment of inertia</returns>
        private double CalculateJxxThinWall()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJx();

            double area = CalculateAreaThinWallSection();
            Point2d centroid = CalculateThinWallsCentroid();
            j -= area * centroid.Y * centroid.Y;
            j += area * (_centroid.Y - centroid.Y) * (_centroid.Y - centroid.Y);

            return j;
        }

        /// <summary>
        /// Calculate the moment of inertia about the X axis through the centroid: the thin walls plus the corners (own moment and transport
        /// term; see <see cref="GetCorners"/>)
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJxx()
        {
            double j = CalculateJxxThinWall();

            foreach (SectionCorner corner in GetCorners())
                j += corner.Sign * (corner.OwnJ + corner.Area * (corner.Y - _centroid.Y) * (corner.Y - _centroid.Y));

            return j;
        }

        /// <summary>
        /// Moment of inertia with respect to the Y axis passing through the center of gravity
        /// of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <remarks>The transport terms are the ones of <see cref="CalculateJxxThinWall"/></remarks>
        /// <returns>The moment of inertia</returns>
        private double CalculateJyyThinWall()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJy();

            double area = CalculateAreaThinWallSection();
            Point2d centroid = CalculateThinWallsCentroid();
            j -= area * centroid.X * centroid.X;
            j += area * (_centroid.X - centroid.X) * (_centroid.X - centroid.X);

            return j;
        }

        /// <summary>
        /// Calculate the moment of inertia about the Y axis through the centroid: the thin walls plus the corners (own moment and transport
        /// term; see <see cref="GetCorners"/>)
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJyy()
        {
            double j = CalculateJyyThinWall();

            foreach (SectionCorner corner in GetCorners())
                j += corner.Sign * (corner.OwnJ + corner.Area * (corner.X - _centroid.X) * (corner.X - _centroid.X));

            return j;
        }

        /// <summary>
        /// Product of inertia with respect to the X and Y axes passing through the center of gravity of the section: the thin walls plus the
        /// corners (see <see cref="GetCorners"/>)
        /// </summary>
        /// <remarks>The transport terms are the ones of <see cref="CalculateJxxThinWall"/></remarks>
        /// <returns>The product of inertia</returns>
        protected override double CalculateJxy()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJxy();

            double area = CalculateAreaThinWallSection();
            Point2d centroid = CalculateThinWallsCentroid();
            j -= area * centroid.X * centroid.Y;
            j += area * (_centroid.X - centroid.X) * (_centroid.Y - centroid.Y);

            foreach (SectionCorner corner in GetCorners())
                j += corner.Sign * (corner.OwnJxy + corner.Area * (corner.X - _centroid.X) * (corner.Y - _centroid.Y));

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

        /// <summary>
        /// Tell if an axis divides the area of the thin walls in two equal parts (relative tolerance 1e-9)
        /// </summary>
        /// <param name="axis">The axis</param>
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

        /// <summary>
        /// Calculate the plastic modulus respect to the axis 1
        /// </summary>
        /// <returns>The plastic modulus respect to the axis 1: when X and Y are principal the one respect to X or Y (see <see cref="Section.CalculateWpl1"/>,
        /// so the sections that compute <see cref="CalculateWplX"/> and <see cref="CalculateWplY"/> get coherent principal moduli), otherwise the one of
        /// the thin walls respect to the axis 1 (<see cref="CalculateWplAngle"/>)</returns>
        protected override double CalculateWpl1()
        {
            return PrincipalFromXY() == PrincipalAxes.Rotated ? CalculateWplAngle(AngleX1) : base.CalculateWpl1();
        }

        /// <summary>
        /// Calculate the plastic modulus respect to the axis 2
        /// </summary>
        /// <returns>The plastic modulus respect to the axis 2 (see <see cref="CalculateWpl1"/>)</returns>
        protected override double CalculateWpl2()
        {
            return PrincipalFromXY() == PrincipalAxes.Rotated ? CalculateWplAngle(AngleX1 + Math.PI * 0.5) : base.CalculateWpl2();
        }

        /// <summary>
        /// Calculate the plastic modulus respect to X
        /// </summary>
        /// <returns>The plastic modulus of the thin walls respect to X (<see cref="CalculateWplAngle"/>)</returns>
        protected override double CalculateWplX()
        {
            return CalculateWplAngle(0);
        }

        /// <summary>
        /// Calculate the plastic modulus respect to Y
        /// </summary>
        /// <returns>The plastic modulus of the thin walls respect to Y (<see cref="CalculateWplAngle"/>)</returns>
        protected override double CalculateWplY()
        {
            return CalculateWplAngle(Math.PI * 0.5);
        }

        // The elastic moduli respect to the principal axes are the ones of Section: taken from the moduli respect to X and Y when X and Y are
        // principal (before, they were abstract and every section computed them assuming that the axis 1 was X)

        /// <summary>
        /// The vertices of the perimeters of the thin walls (4 for each wall, copies)
        /// </summary>
        /// <returns>The points</returns>
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
        /// Break all the thin walls at the intersections of their middle lines with the edges (an intersection closer than 1 to an end of the
        /// middle line is ignored: the unit is the millimetre)
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

        /// <summary>
        /// Equality of the section properties and of the thin walls
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {

            return obj is ThinWallSection section &&
                   base.Equals(obj) &&
                   _thinWalls.SequenceEqual(section._thinWalls);
        }

        /// <summary>
        /// The hash code of the section and of the thin walls
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>; a null <paramref name="left"/> throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(ThinWallSection left, ThinWallSection right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(ThinWallSection left, ThinWallSection right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Serializes the section (version 2)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            int version = 2;
            info.AddValue("ThinWallSectionVersion", version);
            info.AddValue("ThinWalls", _thinWalls, typeof(ThinWall[]));
        }

        #endregion

        #region Nested classes ThinWall

        /// <summary>
        /// A rectangular thin wall, defined by length, thickness, angle and centre. With _angle = 0:
        /// <code>
        ///    ┌-----------------┐
        /// _t |                 |
        ///    └-----------------┘
        ///            _l
        /// </code>
        /// </summary>
        /// <remarks>The class is [Serializable] but does not implement <see cref="ISerializable"/>: the formatter serializes the fields and
        /// <see cref="GetObjectData(SerializationInfo, StreamingContext)"/> and the deserialization constructor are not used</remarks>
        [Serializable]
        public class ThinWall : ICloneable
        {
            #region Variables

            /// <summary>
            /// The thickness
            /// </summary>
            private readonly double _t;
            /// <summary>
            /// The length
            /// </summary>
            private readonly double _l;
            /// <summary>
            /// The angle of the wall from X (radians)
            /// </summary>
            private readonly double _angle;
            /// <summary>
            /// The centre of the wall
            /// </summary>
            private readonly Point2d _point;

            #endregion

            #region Properties

            /// <summary>
            /// The thickness of the wall
            /// </summary>
            public double T => _t;

            /// <summary>
            /// The length of the wall
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
            /// The area of the thin wall
            /// </summary>
            public double Area => CalculateArea();

            /// <summary>
            /// Defines whether the ThinWall is entirely outside or inside the concrete area.
            /// </summary>
            public bool IsInsideConcrete { get; set; }

            #endregion

            #region Protected constructor

            /// <summary>
            /// Older version of the constructor, kept for compatibility: the wall is centred at the origin
            /// </summary>
            /// <param name="length">The length</param>
            /// <param name="thickness">The thickness</param>
            /// <param name="angle">The angle from X (radians)</param>
            /// <exception cref="ArgumentException">If the length or the thickness is negative</exception>
            internal ThinWall(double length, double thickness, double angle)
                : this(length, thickness, angle, Point2d.Origin)
            {
            }

            /// <summary>
            /// The default constructor of generic ThinWall
            /// </summary>
            /// <param name="length">The length of the ThinWall</param>
            /// <param name="thickness">The thickness of the ThinWall</param>
            /// <param name="angle">The angle of the ThinWall. 0 is orizontal, Math.PI / 2.0 is vertical</param>
            /// <param name="point">Position, barycenter/centroid of rectangular.</param>
            /// <exception cref="ArgumentException">If the length or the thickness is negative or <paramref name="point"/> is null</exception>
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
            /// <param name="startPoint">The start point of the middle line</param>
            /// <param name="endPoint">The end point of the middle line</param>
            /// <param name="thickness">The thickness</param>
            /// <exception cref="ArgumentException">If <paramref name="thickness"/> is negative</exception>
            internal ThinWall(in Point2d startPoint, in Point2d endPoint, in double thickness)
            {
                _t = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;
                Vector2d vector = endPoint - startPoint;
                _l = vector.Length;
                _angle = Math.Atan2(vector.Y, vector.X);
                _point = 0.5 * (startPoint + endPoint);
            }

            /// <summary>
            /// Deserialization constructor (version 1 without <see cref="IsInsideConcrete"/>; a missing point is the origin). Not used: see the class remarks
            /// </summary>
            /// <param name="info">The serialization data</param>
            /// <param name="_">The serialization context</param>
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

            /// <summary>
            /// The perimeter of the wall (rectangle)
            /// </summary>
            /// <returns>A new polygon with the four vertices</returns>
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

            /// <summary>
            /// The end points of the middle line of the wall
            /// </summary>
            /// <returns>The two points</returns>
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
            /// Returns two thinwalls if the point is intermediate (on the middle line and farther than 1 from its ends: the unit is the millimetre),
            /// otherwise returns the thinwall itself.
            /// </summary>
            /// <param name="intermediatePoint">The point</param>
            /// <returns>The two parts, or this wall</returns>
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
            /// <returns>t l</returns>
            internal double CalculateArea()
            {
                return _t * _l;
            }

            /// <summary>
            /// Product of inertia with respect to the X and Y axes through the origin (the own product rotated plus the transport term)
            /// </summary>
            /// <returns>The product of inertia</returns>
            internal double CalculateJxy()
            {
                double Jxx = _l * Math.Pow(_t, 3) / 12.0;
                double Jyy = _t * Math.Pow(_l, 3) / 12.0;
                double Jxy = 0.0;

                return SectionHelper.CalculateJxyAlpha(Jxx, Jyy, Jxy, -_angle) + _point.X * _point.Y * Area;
            }

            /// <summary>
            /// Moment of inertia with respect to the Y-axis through the origin (the own moment plus the transport term)
            /// </summary>
            /// <returns>The moment of inertia</returns>
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
            /// Moment of inertia with respect to the X-axis through the origin (the own moment plus the transport term)
            /// </summary>
            /// <returns>The moment of inertia</returns>
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
            /// <returns>A y</returns>
            internal double CalculateSx() => Area * _point.Y;

            /// <summary>
            /// Static moment with respect to Y-axis.
            /// </summary>
            /// <returns>A x</returns>
            internal double CalculateSy() => Area * _point.X;

            /// <summary>
            /// The torsion constant of the wall: l t³ / (3 + 1.8 t / l)
            /// </summary>
            /// <returns>The torsion constant</returns>
            internal virtual double CalculateJt()
            {
                return L * Math.Pow(T, 3) / GetAlpha();
            }

            /// <summary>
            /// The warping constant of the wall (not implemented)
            /// </summary>
            /// <returns>Nothing</returns>
            /// <exception cref="NotImplementedException">Always</exception>
            internal virtual double CalculateJw()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Calculate the polar moment of inertia respect to the origin
            /// </summary>
            /// <returns>Jx + Jy</returns>
            internal virtual double CalculateJpolar()
            {
                return CalculateJx() + CalculateJy();
            }

            /// <summary>
            /// The coefficient of the torsion constant: 3 + 1.8 t / l
            /// </summary>
            /// <returns>The coefficient</returns>
            internal double GetAlpha()
            {
                return 3 + 1.8 * T / L;
            }

            /// <summary>
            /// Equality of thickness, length, angle and centre
            /// </summary>
            /// <param name="obj">The object to compare</param>
            /// <returns>True if <paramref name="obj"/> is an equal wall</returns>
            public override bool Equals(object obj)
            {
                return obj is ThinWall wall &&
                       _t == wall._t &&
                       _l == wall._l &&
                       _angle == wall._angle &&
                       _point == wall._point;
            }

            /// <summary>
            /// The hash code of thickness, length, angle and centre
            /// </summary>
            /// <returns>The hash code</returns>
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

            /// <summary>
            /// Serializes the wall (version 2). Not used by the formatter: see the class remarks
            /// </summary>
            /// <param name="info">The serialization data</param>
            /// <param name="context">The serialization context</param>
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

            /// <summary>
            /// A shallow copy of the wall
            /// </summary>
            /// <returns>The copy</returns>
            public object Clone()
            {
                return (ThinWall)MemberwiseClone();
            }

            #endregion
        }

        #endregion
    }
}
