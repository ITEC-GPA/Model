using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
    /// A steel plate (shape with holes and material) with a grid of bolts: distances of the bolts from the edges and between them (e1, e2, p1, p2 of EN 1993-1-8) and distribution of the forces on the bolts
    /// </summary>
    [Serializable]
    public class PlateWithBolts : ShapeEx, IEquatable<PlateWithBolts>, ISerializable
    {
        #region Constant

        /// <summary>
        /// The value of a spacing when there is no bolt or edge in the direction
        /// </summary>
        public const double SPACINGMAXVALUE = double.MaxValue;

        #endregion

        #region Variables

        /// <summary>
        /// The bolts
        /// </summary>
        protected BoltGrid _boltGrid;
        /// <summary>
        /// The thickness of the plate
        /// </summary>
        protected double _thickness;

        #endregion

        #region Properties

        /// <summary>
        /// The bolts
        /// </summary>
        public BoltGrid BoltGrid => _boltGrid;

        /// <summary>
        /// The thickness of the plate (minimum 0.01)
        /// </summary>
        public double Thickness
        {
            get => _thickness;
            set => _thickness = value > 0.01 ? value : 0.01;
        }

        /// <summary>
        /// The material of the plate
        /// </summary>
        public Material PlateMaterial
        {
            get => _material;
            set => _material = value;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Define a generic plate with a generic bolt grid inside.
        /// </summary>
        /// <param name="plateShape">Plate shape.</param>
        /// <param name="plateMaterial">Plate material.</param>
        /// <param name="boltGrid">Bolt grid.</param>
        /// <param name="plateThickness">Plate plate thickness.</param>
        /// <param name="holes">Holes.</param>
        public PlateWithBolts(in Polygon2d plateShape, in Material plateMaterial, in BoltGrid boltGrid, in double plateThickness, Polygon2d[] holes = null)
            : base(plateShape, plateMaterial, holes)
        {
            _boltGrid = boltGrid;
            _thickness = plateThickness;
        }

        /// <summary>
        /// Define a generic plate without bolts and thickness 10
        /// </summary>
        /// <param name="plateShape">Plate shape.</param>
        /// <param name="plateMaterial">Plate material.</param>
        public PlateWithBolts(in Polygon2d plateShape, in Material plateMaterial)
            : this(plateShape, plateMaterial, new BoltGrid(), 10.0)
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public PlateWithBolts(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            double version = info.GetInt32("Version");

            _boltGrid = (BoltGrid)info.GetValue("BoltGrid", typeof(BoltGrid));
            _thickness = info.GetDouble("Thickness");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes the plate
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            int version = 1;
            info.AddValue("Version", version);

            info.AddValue("BoltGrid", _boltGrid, typeof(BoltGrid));
            info.AddValue("Thickness", _thickness);
        }

        /// <summary>
        /// The end distance e1 of a bolt in the direction of the force (see <see cref="CalculateE1(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The distance</returns>
        public double CalculateE1(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateE1(bolt, CalculateAngle(resultBeamForces));
        }

        /// <summary>
        /// The edge distance e2 of a bolt perpendicular to the force (see <see cref="CalculateE2(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The distance</returns>
        public double CalculateE2(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateE2(bolt, CalculateAngle(resultBeamForces));
        }

        /// <summary>
        /// The line to the closest bolt in the direction of the force (see <see cref="CalculateP1Line(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The line; null if there is no bolt</returns>
        public Line2d CalculateP1Line(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateP1Line(bolt, CalculateAngle(resultBeamForces));
        }

        /// <summary>
        /// The line to the closest bolt perpendicular to the force (see <see cref="CalculateP2Line(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The line; null if there is no bolt</returns>
        public Line2d CalculateP2Line(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateP2Line(bolt, CalculateAngle(resultBeamForces));
        }

        /// <summary>
        /// The end distance e1: from the centers of the hole to the edge in the direction of the force (a null line, no edge in the direction,
        /// throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="forceDirectionAngle">The direction of the force from X (radians)</param>
        /// <returns>The distance</returns>
        public double CalculateE1(BoltPosition bolt, double forceDirectionAngle)
        {
            return CalculateClosestEdgePoint(bolt, forceDirectionAngle, out Line2d _).Length;
        }

        /// <summary>
        /// The edge distance e2: the minimum distance from the centers of the hole to the edge in the two directions perpendicular to the force
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="forceDirectionAngle">The direction of the force from X (radians)</param>
        /// <returns>The distance</returns>
        public double CalculateE2(BoltPosition bolt, double forceDirectionAngle)
        {
            return Math.Min(CalculateClosestEdgePoint(bolt, forceDirectionAngle + Math.PI / 2.0, out Line2d _).Length,
                CalculateClosestEdgePoint(bolt, forceDirectionAngle - Math.PI / 2.0, out Line2d _).Length);
        }

        /// <summary>
        /// The line to the closest bolt in the direction of the force (within ±45°)
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="forceDirectionAngle">The direction of the force from X (radians)</param>
        /// <returns>The line; null if there is no bolt</returns>
        public Line2d CalculateP1Line(BoltPosition bolt, double forceDirectionAngle)
        {
            return CalculateClosestBolt(bolt, forceDirectionAngle);
        }

        /// <summary>
        /// The spacing p1: the distance from the closest bolt in the direction of the force
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="forceDirectionAngle">The direction of the force from X (radians)</param>
        /// <returns>The distance; <see cref="SPACINGMAXVALUE"/> if there is no bolt</returns>
        public double CalculateP1(BoltPosition bolt, double forceDirectionAngle)
        {
            return CalculateP1Line(bolt, forceDirectionAngle)?.Length ?? SPACINGMAXVALUE;
        }

        /// <summary>
        /// The line to the closest bolt in the two directions perpendicular to the force
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="forceDirectionAngle">The direction of the force from X (radians)</param>
        /// <returns>The line; null if there is no bolt</returns>
        public Line2d CalculateP2Line(BoltPosition bolt, double forceDirectionAngle)
        {
            var d1Line = CalculateClosestBolt(bolt, forceDirectionAngle + Math.PI / 2.0);
            var d2Line = CalculateClosestBolt(bolt, forceDirectionAngle - Math.PI / 2.0);
            double d1 = d1Line?.Length ?? SPACINGMAXVALUE;
            double d2 = d2Line?.Length ?? SPACINGMAXVALUE;

            if (d1 < d2)
                return d1Line;
            else
                return d2Line;
        }

        /// <summary>
        /// The spacing p2: the distance from the closest bolt perpendicular to the force
        /// </summary>
        /// <param name="bolt">The bolt</param>
        /// <param name="forceDirectionAngle">The direction of the force from X (radians)</param>
        /// <returns>The distance; <see cref="SPACINGMAXVALUE"/> if there is no bolt</returns>
        public double CalculateP2(BoltPosition bolt, double forceDirectionAngle)
        {
            return CalculateP2Line(bolt, forceDirectionAngle)?.Length ?? SPACINGMAXVALUE;
        }

        /// <summary>
        /// The minimum e1 of all the bolts (see <see cref="CalculateE1(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The minimum distance; <see cref="SPACINGMAXVALUE"/> without bolts</returns>
        public double CalculateE1Min(ResultBeamForces resultBeamForces)
        {
            double distance = SPACINGMAXVALUE;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateE1(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        /// <summary>
        /// The minimum e2 of all the bolts (see <see cref="CalculateE2(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The minimum distance; <see cref="SPACINGMAXVALUE"/> without bolts</returns>
        public double CalculateE2Min(ResultBeamForces resultBeamForces)
        {
            double distance = SPACINGMAXVALUE;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateE2(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        /// <summary>
        /// The minimum p1 of all the bolts (see <see cref="CalculateP1(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The minimum distance; <see cref="SPACINGMAXVALUE"/> without bolts</returns>
        public double CalculateP1Min(ResultBeamForces resultBeamForces)
        {
            double distance = SPACINGMAXVALUE;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateP1(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        /// <summary>
        /// The minimum p2 of all the bolts (see <see cref="CalculateP2(BoltPosition, double)"/>)
        /// </summary>
        /// <param name="resultBeamForces">The forces: the direction of the shear (V1, V2) is used</param>
        /// <returns>The minimum distance; <see cref="SPACINGMAXVALUE"/> without bolts</returns>
        public double CalculateP2Min(ResultBeamForces resultBeamForces)
        {
            double distance = SPACINGMAXVALUE;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateP2(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        /// <summary>
        /// The direction of the shear force: atan2(V2, V1)
        /// </summary>
        /// <param name="resultBeamForces">The forces</param>
        /// <returns>The angle from X (radians)</returns>
        public virtual double CalculateAngle(ResultBeamForces resultBeamForces)
        {
            return Math.Atan2(resultBeamForces.V2, resultBeamForces.V1);
        }

        /// <summary>
        /// The sides of the outer polygon and of the holes
        /// </summary>
        /// <returns>The sides</returns>
        private List<Line2d> GetEdges()
        {
            var edges = Shape.Fill2d.Explode().ToList();
            if (Shape.HasHoles)
                for (int i = 0; i < Shape.Holes2d.Length; i++)
                    edges.AddRange(Shape.Holes2d[i].Explode());

            return edges;
        }

        /// <summary>
        /// The sides of the outer polygon (counterclockwise: positive area) and of the holes (clockwise: negative area)
        /// </summary>
        /// <returns>The oriented sides</returns>
        private List<Line2d> GetEdgesBoundaryOriented()
        {
            List<Line2d> edges;
            if (Shape.Fill2d.GetSignedArea() > 0)
                edges = Shape.Fill2d.Explode().ToList();
            else
            {
                Shape.Fill2d.Reverse();
                edges = Shape.Fill2d.Explode().ToList();
                Shape.Fill2d.Reverse();
            }
            if (Shape.HasHoles)
                for (int i = 0; i < Shape.Holes2d.Length; i++)
                {
                    var shapeI = Shape.Holes2d[i];
                    if (shapeI.GetSignedArea() < 0)
                        edges.AddRange(shapeI.Explode());
                    else
                    {
                        shapeI.Reverse();
                        edges.AddRange(shapeI.Explode());
                        shapeI.Reverse();
                    }
                }

            return edges;
        }

        /// <summary>
        /// Given a bolt find the minimum distance from the edge in a specific direction.
        /// Works for normal and slotted holes.
        /// </summary>
        /// <param name="boltPosition">The bolt</param>
        /// <param name="angle">The direction from X (radians)</param>
        /// <param name="minEdge">The edge of the minimum distance (null if none)</param>
        /// <returns>Line from center to point of minimum distance; null if no edge is found in the direction.</returns>
        private Line2d CalculateClosestEdgePoint(BoltPosition boltPosition, double angle, out Line2d minEdge)
        {
            double distance = SPACINGMAXVALUE;
            Line2d minLine = null;
            minEdge = null;

            // Center list.
            Point2d[] centers = boltPosition.CalculateCenters();

            // Edge list.
            var edges = GetEdges();

            // Find minimum distance.
            for (int i = 0; i < centers.Length; i++)
            {
                Point2d center = centers[i];
                Line2d line = new Line2d(center, new Point2d(center.X + Math.Cos(angle), center.Y + Math.Sin(angle)));

                // Get intersections with all edges.
                List<(Point2d, Line2d)> points = new List<(Point2d, Line2d)>();
                for (int j = 0; j < edges.Count; j++)
                {
                    if (line.GetIntersectionWithInfiniteLine(edges[j], out Point2d intersection))
                    {
                        if (edges[j].IsPointOnLine(intersection))
                            points.Add((intersection, edges[j]));
                    }
                }

                // Selects points by direction.
                if (points.Count > 0)
                {
                    Vector2d v1 = line.ToVector();

                    for (int j = 0; j < points.Count; j++)
                    {
                        double distanceBuffer = center.DistanceTo(points[j].Item1);
                        if (distanceBuffer < distance)
                        {
                            Vector2d v2 = new Line2d(center, points[j].Item1).ToVector();

                            if (v1.DotProduct(v2) > 0)
                            {
                                distance = distanceBuffer;
                                minLine = new Line2d(center, points[j].Item1);
                                minEdge = points[j].Item2;
                            }
                        }
                    }
                }
            }

            return minLine;
        }

        /// <summary>
        /// Returns the list of all bolts other than a specific one.
        /// </summary>
        /// <param name="boltPosition">The bolt to exclude</param>
        /// <returns>The other bolts</returns>
        private List<BoltPosition> GetOtherBolts(BoltPosition boltPosition)
        {
            var otherBolts = new List<BoltPosition>();
            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                if (_boltGrid.Bolts.ElementAt(i) != boltPosition)
                {
                    otherBolts.Add(_boltGrid.Bolts.ElementAt(i));
                }
            }
            return otherBolts;
        }

        /// <summary>
        /// Calculates the minimum distance of holes around in a specific direction (the other centers within ±(45° + tolerance) from it).
        /// Works for normal and slotted holes.
        /// </summary>
        /// <param name="boltPosition">The bolt</param>
        /// <param name="angle">The direction from X (radians)</param>
        /// <param name="tolerance">The angular tolerance</param>
        /// <returns>The line to the closest center; null if it finds no bolts.</returns>
        private Line2d CalculateClosestBolt(BoltPosition boltPosition, double angle, double tolerance = AngularTolerance)
        {
            double distance = SPACINGMAXVALUE;
            Line2d minDistanceLine = null;
            Vector2d v1 = new Vector2d(Math.Cos(angle), Math.Sin(angle));

            // Center list.
            Point2d[] centers = boltPosition.CalculateCenters();

            // Other bolts.
            var otherBolts = GetOtherBolts(boltPosition);
            var otherBoltsCenters = otherBolts.Select(ob => ob.CalculateCenters()).ToArray();

            // Find minimum distance.
            for (int i = 0; i < centers.Length; i++)
            {
                var center = centers[i];

                for (int j = 0; j < otherBoltsCenters.Length; j++)
                {
                    var otherBoltCenters = otherBoltsCenters[j];

                    for (int k = 0; k < otherBoltCenters.Length; k++)
                    {
                        var centerOther = otherBoltCenters[k];
                        double distanceBuffer = center.DistanceTo(centerOther);

                        if (distanceBuffer < distance)
                        {
                            Vector2d v2 = new Line2d(center, centerOther).ToVector();
                            v2.Unitize();

                            if (v1.DotProduct(v2) > Math.Cos(Math.PI * 0.25 + tolerance))
                            {
                                distance = distanceBuffer;
                                minDistanceLine = new Line2d(center, centerOther);
                            }
                        }
                    }
                }
            }

            return minDistanceLine;
        }

        /// <summary>
        /// Given a bolt find the minimum distance from another bolt in all directions.
        /// Works for normal and slotted holes.
        /// </summary>
        /// <param name="boltPosition">The bolt</param>
        /// <returns>The minimum distance between the centers; <see cref="SPACINGMAXVALUE"/> if there are no other bolts</returns>
        public double CalculateClosestBolt(BoltPosition boltPosition)
        {
            double minDist = SPACINGMAXVALUE;
            double iDist = SPACINGMAXVALUE;

            // Center list.
            Point2d[] centers = boltPosition.CalculateCenters();

            // Other bolts.
            var otherBolts = GetOtherBolts(boltPosition);
            var otherBoltsCenters = otherBolts.Select(ob => ob.CalculateCenters()).ToArray();

            // Find minimum distance.
            for (int i = 0; i < centers.Length; i++)
            {
                var center = centers[i];

                for (int j = 0; j < otherBoltsCenters.Length; j++)
                {
                    var otherBoltCenters = otherBoltsCenters[j];

                    for (int k = 0; k < otherBoltCenters.Length; k++)
                    {
                        iDist = center.DistanceTo(otherBoltCenters[k]);
                        if (iDist < minDist)
                            minDist = iDist;
                    }
                }
            }

            return minDist;
        }

        /// <summary>
        /// Adds a bolt inside the plate (see <see cref="BoltGrid.AddBolt(double, double, double, SteelMaterial, Hole)"/>)
        /// </summary>
        /// <param name="posX">The X of the position</param>
        /// <param name="posY">The Y of the position</param>
        /// <param name="diameter">The diameter of the bolt</param>
        /// <param name="mat">The material of the bolt</param>
        /// <param name="hole">The hole (null: circular with the diameter of the bolt + 1)</param>
        /// <returns>The new bolt position; null if the position is outside the plate or the bolt overlaps another one</returns>
        public BoltPosition AddBolt(double posX, double posY, double diameter, SteelMaterial mat, Hole hole = null)
        {
            // Check it is inside.
            if (!IsPointInside(new Point2d(posX, posY)))
                return null;

            return _boltGrid.AddBolt(posX, posY, diameter, mat, hole);
        }

        /// <summary>
        /// Given a bolt find the minimum distance from the edge in all directions.
        /// Works for normal and slotted holes.
        /// </summary>
        /// <param name="boltPosition">The bolt</param>
        /// <param name="minEdge">The closest edge</param>
        /// <returns>Line from center to point of minimum distance.</returns>
        public Line2d CalculateClosestEdgePoint(BoltPosition boltPosition, out Line2d minEdge)
        {
            double minDist = SPACINGMAXVALUE;
            Line2d minLine = null;
            minEdge = null;

            // Center list.
            Point2d[] centers = boltPosition.CalculateCenters();

            // Edge list.
            var edges = GetEdges();

            // Find minimum distance.
            for (int i = 0; i < centers.Length; i++)
            {
                Point2d center = centers[i];
                for (int j = 0; j < edges.Count; j++)
                {
                    Line2d edge = edges[j];
                    Point2d iDistPoint = edge.PointDistanceTo(center);
                    double iDist = iDistPoint.DistanceTo(center);
                    if (iDist < minDist)
                    {
                        minDist = iDist;
                        minLine = new Line2d(center, iDistPoint);
                        minEdge = edge;
                    }
                }
            }

            return minLine;
        }

        /// <summary>
        /// This method comes from the need to check even the farthest edge with the maximum value.
        /// Chosen to check the minimum point between all sides, and of these take the maximum value that
        /// has no bolts in the middle (only the directions orthogonal to the edges, see <see cref="CalculateSignificantAngles"/>).
        /// </summary>
        /// <param name="boltPosition">The bolt</param>
        /// <param name="minEdge">The edge of the returned distance</param>
        /// <returns>Line from center to the edge; null if none is found</returns>
        public Line2d CalculateFurtherMinimumEdgePoint(BoltPosition boltPosition, out Line2d minEdge)
        {
            double maxDist = double.MinValue;
            Line2d maxLine = null;
            minEdge = null;
            // Orthogonality error, given by the use of angles with integers in sexagesimal degrees.
            // Error given of orthogonality between 0° and 89° --> Scalar product of versors equal to dotproduct(versor(0°), versor(89°))=0.017452406.
            double orthogonalityError = 0.017452406;

            // Directions.
            var angles = CalculateSignificantAngles();

            // For each angle, calculate the edge distance and check if there are no closer bolts and if it is ortogonal.
            foreach (var angle in angles)
            {
                double angleRad = angle * Math.PI / 180.0;
                var e1Line = CalculateClosestEdgePoint(boltPosition, angleRad, out Line2d edge);

                if (e1Line != null && edge != null)
                {
                    // Check orthogonality first.
                    double e1 = e1Line.Length;
                    var edgeVector = edge.ToVector();
                    edgeVector.Unitize();
                    var angleVector = new Vector2d(Math.Cos(angleRad), Math.Sin(angleRad));

                    if (Math.Abs(angleVector * edgeVector) < orthogonalityError)
                    {
                        // Check for bolts.
                        double p1 = CalculateP1(boltPosition, angleRad);
                        if (p1 > e1 && e1 > maxDist)
                        {
                            maxDist = e1;
                            maxLine = e1Line;
                            minEdge = edge;
                        }
                    }
                }
            }
            return maxLine;
        }

        /// <summary>
        /// Calculate if bolt is of type outer or inner.
        /// To say whether it is outer is enough if it is on one side, but if it is not for any side then it is inner.
        /// </summary>
        /// <param name="boltPosition">The bolt</param>
        /// <returns>True if the bolt is outer (no other bolt between it and an edge)</returns>
        public bool IsOuuter(BoltPosition boltPosition)
        {
            // Find point on edges with minimum distance.
            var minDistLine = CalculateClosestEdgePoint(boltPosition, out Line2d _);
            var minDist = minDistLine.Length;

            // *** First attempt with minimum point.
            var minDistVector = minDistLine.ToVector();
            double minDistDirection = Math.Atan2(minDistVector.Y, minDistVector.X);
            var nearestBoltDistance = CalculateClosestBolt(boltPosition, minDistDirection)?.Length ?? SPACINGMAXVALUE;
            if (nearestBoltDistance > minDist)
                return true;

            // *** Second attempt with all edge orthogonal directions.
            var angles = CalculateSignificantAngles();
            // For each angle, calculate the edge distance and check if there are no closer bolts.
            foreach (var angle in angles)
            {
                double angleRad = ((double)angle) * Math.PI / 180.0;
                double e1 = CalculateE1(boltPosition, angleRad);
                double p1 = CalculateP1(boltPosition, angleRad);
                if (p1 > e1)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Edge list. For each edge add the orthogonal directions in degrees (integers, 0 to 359) to reduce them in number.
        /// </summary>
        /// <returns>The directions in degrees</returns>
        private HashSet<int> CalculateSignificantAngles()
        {
            var angles = new HashSet<int>();
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                var edgeVector = edge.ToVector();
                int edgeVectorAngle = (int)(Math.Atan2(edgeVector.Y, edgeVector.X) * 180.0 / Math.PI);
                if (edgeVectorAngle < 0)
                    edgeVectorAngle += 360; // --> Angle from 0 to 360.
                if (edgeVectorAngle == 360)
                    edgeVectorAngle = 0; // --> Angle from 0 to 359, because 360 == 0.

                int angle1 = edgeVectorAngle + 90;
                if (angle1 >= 360)
                    angle1 -= 360; // --> Angle from 0 to 359.

                angles.Add(angle1);

                int angle2 = edgeVectorAngle - 90;
                if (angle2 < 0)
                    angle2 += 360; // --> Angle from 0 to 359.

                angles.Add(angle2);
            }

            return angles;
        }

        /// <summary>
        /// Calculation of partial concrete-steel sections.
        /// In this case, concrete is under the plate and works by contact.
        /// Material under the plate could be also steel that works by contact, for clarity I call this material in general concrete.
        /// The method used here considers a behavior:
        /// - conservation of plane section.
        /// - resistance of concrete in compression only.
        /// - homogenization of the section by a parameter n (quite similar to that used in the method of allowable stresses): homogenization coefficient given by the ratio Es / Ec = n.
        ///
        /// The initial shapes of the concrete and steel:
        /// - concrete, polygonal shapes.
        /// - steel, always formed by a series of circles. Assumes same material for all bolts.
        /// The neutral axis is found iteratively; the loop condition is "not converged OR less than 30 iterations": at least 30 iterations, and
        /// it does not end while the neutral axis moves more than 0.1.
        /// </summary>
        /// <param name="Soll">Stress on the entire section.</param>
        /// <param name="concreteElasticModulus">Elastic modulus of the material under the plate subject to contact partialization.</param>
        /// <param name="boltForces">Update axial N values with redistribuited tension.</param>
        /// <param name="concreteMinStress">Minimum stress in concrete area.</param>
        /// <exception cref="InvalidOperationException">If the plate has no bolts (and |N| is at least 1)</exception>
        public void CalculateTensionForcesElastic(in ResultBeamForces Soll, in double concreteElasticModulus, Dictionary<BoltPosition, ResultBeamForces> boltForces, out double concreteMinStress)
        {
            // Special case, there is no axial force.
            concreteMinStress = 0;
            if (Math.Abs(Soll.N) < 1)
                return;

            // Bounding box and first area.
            var bounding = Shape.Get2dBoundingBox();
            var diagonal = bounding.Min.DistanceTo(bounding.Max);
            double A_c_0 = Shape.GetArea(); // Save first iteration area.

            // Move sollecitation to global system.
            var globalCS = new CoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis);
            var globalSoll = Soll.ToCoordinateSystemWithEccentricity(globalCS);

            // Load eccentricity.
            var eccentricity = new Point2d
            {
                X = -globalSoll.M2 / globalSoll.N,
                Y = globalSoll.M1 / globalSoll.N
            };

            // Bolts inertia
            var A_s = BoltGrid.CalculateArea();
            var G_s = BoltGrid.CalculateBarycenter();
            BoltGrid.CalculateInertiaMoment(out double I_X_s, out double I_Y_s, out double I_XY_s);

            // Lines for boundaries. Positive areas are counterclockwise, hole areas are clockwise.
            var edges = GetEdgesBoundaryOriented();
            int nIteractions = 1;
            double relativeError = 0.1; // Distance error for neutral axis points.
            double relativeErrorInertia = 0.000001;
            double absoluteErrorEccentricity = 0.1; // Used to identify centrated load over centroid.
            bool barycentricLoad = false; // Is load centrated over centroid?

            // Variables for concrete integration.
            double A_c = 0.0, S_X_c = 0.0, S_Y_c = 0.0, I_X_c = 0.0, I_Y_c = 0.0, I_XY_c = 0.0;
            var G_c = new Point2d();
            // Integrate area and intertia.
            foreach (var e in edges)
                e.IntegrateOnBoundary(ref A_c, ref S_X_c, ref S_Y_c, ref I_X_c, ref I_Y_c, ref I_XY_c);
            if (A_c != 0.0)
                G_c = new Point2d(S_Y_c / A_c, S_X_c / A_c);

            // Variables for homogenized section.
            // Assumes same material for all bolts.
            double homogCoeff = BoltGrid.Bolts.First().BoltDef.BoltMaterial.E / concreteElasticModulus;
            double A_h = 0.0, I_X_h, I_Y_h, I_XY_h;
            Point2d G_h;
            double I_X_h_g, I_Y_h_g, I_XY_h_g; // In centroid.
            double I_X_h_0, I_Y_h_0, r_X_h_0, r_Y_h_0; // In principal system.
            Point2d eccentricity_0;
            double alpha_h;

            // Intermediate results.
            double sqrt_d_a_plus_b = 0.0;
            var neutralAxis = new Line2d(Point2d.Origin, Point2d.Origin + Vector2d.XAxis);
            bool PointQIsAbove = false;
            Line2d previuosNeutralAxis;

            do
            {
                previuosNeutralAxis = new Line2d(new Point2d(neutralAxis.Start.X, neutralAxis.Start.Y), new Point2d(neutralAxis.End.X, neutralAxis.End.Y));

                // Homogenized section properties.
                A_h = A_c + homogCoeff * A_s;
                I_X_h = I_X_c + homogCoeff * I_X_s;
                I_Y_h = I_Y_c + homogCoeff * I_Y_s;
                I_XY_h = I_XY_c + homogCoeff * I_XY_s;
                G_h = (A_c * G_c + homogCoeff * A_s * G_s) / A_h;

                // Homogenized section properties traslated to centroid.
                I_X_h_g = I_X_h - A_h * Math.Pow(G_h.Y, 2.0);
                I_Y_h_g = I_Y_h - A_h * Math.Pow(G_h.X, 2.0);
                I_XY_h_g = I_XY_h - A_h * (G_h.X * G_h.Y);

                alpha_h = 0.0;

                if ((Math.Abs(I_X_h_g - I_Y_h_g) / (I_X_h_g + I_Y_h_g)) > relativeErrorInertia) // I_X_h_g != I_Y_h_g
                    alpha_h = Math.Atan((2.0 * I_XY_h_g) / (I_Y_h_g - I_X_h_g)) / 2.0;

                // Homogenized section properties in principal system.
                double sin_ = Math.Sin(alpha_h);
                double sin_2 = sin_ * sin_;
                double cos_ = Math.Cos(alpha_h);
                double cos_2 = cos_ * cos_;
                double sin_cos_ = sin_ * cos_;
                I_X_h_0 = I_X_h_g * cos_2 - 2.0 * I_XY_h_g * sin_cos_ + I_Y_h_g * sin_2;
#if DEBUG
                double Ixy0 = (I_X_h_g - I_Y_h_g) * sin_cos_ + I_XY_h_g * (cos_2 - sin_2); // Only for test, must be Ixy0 == 0.
#endif
                I_Y_h_0 = I_X_h_g * sin_2 + 2.0 * I_XY_h_g * sin_cos_ + I_Y_h_g * cos_2;

                r_Y_h_0 = Math.Sqrt(I_Y_h_0 / A_h);
                r_X_h_0 = Math.Sqrt(I_X_h_0 / A_h);

                // Eccentricity of the load with respect to the inertia ucs.
                eccentricity_0 = eccentricity - G_h;
                eccentricity_0.Rotate(Point2d.Origin, -alpha_h);

                if ((Math.Abs(eccentricity_0.X) < absoluteErrorEccentricity) && (Math.Abs(eccentricity_0.Y) < absoluteErrorEccentricity))
                {
                    barycentricLoad = true;
                    break;
                }

                // Find neutral axis.
                // Rotation in principal inertia system.
                double d_a = eccentricity_0.X / Math.Pow(r_Y_h_0, 2.0);
                double d_b = eccentricity_0.Y / Math.Pow(r_X_h_0, 2.0);

                double d_a_plus_b = d_a * d_a + d_b * d_b;
                sqrt_d_a_plus_b = Math.Sqrt(d_a_plus_b);

                // Point on neutral axis.
                var P_0 = new Point2d(-d_a / d_a_plus_b, -d_b / d_a_plus_b);
                // Other two points on neutral axis.
                var P1_0 = new Point2d(P_0.X - diagonal * d_b / sqrt_d_a_plus_b, P_0.Y + diagonal * d_a / sqrt_d_a_plus_b);
                var P2_0 = new Point2d(P_0.X + diagonal * d_b / sqrt_d_a_plus_b, P_0.Y - diagonal * d_a / sqrt_d_a_plus_b);

                double diagonalWithSign;
                if (globalSoll.N < 0)
                    diagonalWithSign = diagonal;
                else
                    diagonalWithSign = -diagonal;
                // Point in compression side, referred to principal inertia system.
                var Q_0 = new Point2d(P_0.X + diagonalWithSign * d_a / sqrt_d_a_plus_b, P_0.Y + diagonalWithSign * d_b / sqrt_d_a_plus_b);

                // Traslate and rotate point form principal inertia to global system.
                var P1 = P1_0;
                P1.Rotate(Point2d.Origin, alpha_h);
                P1 += G_h;
                var P2 = P2_0;
                P2.Rotate(Point2d.Origin, alpha_h);
                P2 += G_h;
                var Q = Q_0;
                Q.Rotate(Point2d.Origin, alpha_h);
                Q += G_h;

                // Define new neutral axis.
                neutralAxis = new Line2d(P1, P2);
#if DEBUG
                Debug.WriteLine($"{P1.X},{P1.Y}");
                Debug.WriteLine($"{P2.X},{P2.Y}");
                Debug.WriteLine($"{P1.X},{P1.Y}");
#endif
                // Evaluate whether point Q is above or below with respect to the line of intersection.
                if (neutralAxis.OrientedDistFromSegment2D(Q) >= 0.0)
                    PointQIsAbove = true;
                else
                    PointQIsAbove = false;

                // Redefine concrete area.
                {
                    // Find the intersections, and filter the segments according to their position relative to the line of intersection.
                    // Always use the original plate points for this part.
                    var edges_original = GetEdgesBoundaryOriented();
                    edges.Clear();

                    Point2d previousPoint = null;
                    var paths = new List<List<Line2d>>();

                    foreach (var e_original in edges_original)
                    {
                        // To reconstruct the paths, it is necessary to know when going from one perimeter path to another,
                        // the holes are each a different path.
                        // isNewPath identifies the path change.
                        bool isNewPath = previousPoint is null || previousPoint != e_original.Start;
                        if (isNewPath)
                            paths.Add(new List<Line2d>());
                        bool startIsSameSide = neutralAxis.OrientedDistFromSegment2D(e_original.Start) >= 0.0 == PointQIsAbove;
                        bool endIsSameSide = neutralAxis.OrientedDistFromSegment2D(e_original.End) >= 0.0 == PointQIsAbove;

                        if (startIsSameSide && endIsSameSide)
                        {
                            paths.Last().Add(e_original);
                        }
                        else if (startIsSameSide || endIsSameSide)
                        {
                            if (neutralAxis.GetIntersectionWithInfiniteLine(e_original, out Point2d intersection))
                            {
                                if (startIsSameSide)
                                    paths.Last().Add(new Line2d(e_original.Start, intersection));
                                else
                                    paths.Last().Add(new Line2d(intersection, e_original.End));
                            }
                        }
                        previousPoint = e_original.End;
                    }
                    // Reconstruct the paths with missing edges.
                    foreach (var path in paths)
                    {
                        var pathCount = path.Count;
                        if (pathCount > 1)
                        {
                            for (int i = 0; i < pathCount; i++)
                            {
                                var prevPoint = path[i].End;
                                var nextPoint = path[(i + 1) % pathCount].Start;
                                // If the end point in the previous segment is different from the start point of the next segment, it means there is a break in the boundary and must be reconstructed by inserting a new line.
                                if (prevPoint != nextPoint)
                                    edges.Add(new Line2d(prevPoint, nextPoint));
                            }
                            // Copy remaining segments.
                            foreach (var line in path)
                                edges.Add(line);
                        }
                    }
                }

                // Concrete area, centroid and inertia.
                {
                    A_c = 0.0;
                    S_X_c = 0.0;
                    S_Y_c = 0.0;
                    G_c = Point2d.Origin;
                    I_X_c = 0.0;
                    I_Y_c = 0.0;
                    I_XY_c = 0.0;

                    // Integrate area and intertia.
                    foreach (var e in edges)
                        e.IntegrateOnBoundary(ref A_c, ref S_X_c, ref S_Y_c, ref I_X_c, ref I_Y_c, ref I_XY_c);
                    // Remove holes from concrete area.
                    foreach (var b in BoltGrid.Bolts)
                    {
                        if (neutralAxis.OrientedDistFromSegment2D(b.Position) >= 0.0 == PointQIsAbove)
                        {
                            double boltArea = b.BoltDef.Area;
                            A_c -= boltArea;
                            S_X_c -= boltArea * b.Position.Y;
                            S_Y_c -= boltArea * b.Position.X;
                            I_X_c -= boltArea * b.Position.Y * b.Position.Y + b.BoltDef.J11;
                            I_Y_c -= boltArea * b.Position.X * b.Position.X + b.BoltDef.J22;
                            I_XY_c -= boltArea * b.Position.X * b.Position.Y;
                        }
                    }

                    if (A_c != 0.0)
                        G_c = new Point2d(S_Y_c / A_c, S_X_c / A_c);
                }

            } while (
                (previuosNeutralAxis.Start.DistanceTo(neutralAxis.Start) > relativeError || previuosNeutralAxis.End.DistanceTo(neutralAxis.End) > relativeError)
                || nIteractions++ < 30);

            concreteMinStress = 0.0;

            // Stress recovery.
            if (barycentricLoad == true)
            {
                // Calculates the stresses in the case of centered loading.
                double sigmaConcrete;
                double sigmaSteel;

                if (globalSoll.N > 0.0) // Tension.
                {
                    sigmaConcrete = 0.0;
                    sigmaSteel = globalSoll.N / A_s;
                }
                else // Compression.
                {
                    sigmaConcrete = globalSoll.N / A_h;
                    sigmaSteel = homogCoeff * globalSoll.N / A_h;
                }
                concreteMinStress = sigmaConcrete;

                foreach (var b in BoltGrid.Bolts)
                {
                    var forceSteel = sigmaSteel * b.BoltDef.Area;
                    if (boltForces.TryGetValue(b, out var rbf))
                        rbf.N = forceSteel;
                    else
                        boltForces[b] = new ResultBeamForces(forceSteel, 0, 0, 0, 0, 0, new CoordinateSystem(new Point3d(b.Position), Vector3d.XAxis, Vector3d.YAxis), Soll.Id);
                }
            }
            else
            {
                // Calculates the stresses in general case.
                double dC = sqrt_d_a_plus_b * globalSoll.N / A_h;

                // Minimum stress in concrete.
                foreach (var e in edges)
                {
                    double distStart = neutralAxis.OrientedDistFromSegment2D(e.Start);
                    double distEnd = neutralAxis.OrientedDistFromSegment2D(e.End);

                    if ((dC * distStart) < concreteMinStress)
                        concreteMinStress = dC * distStart;

                    if ((dC * distEnd) < concreteMinStress)
                        concreteMinStress = dC * distEnd;
                }

                // All tension forces in bolts.
                foreach (var b in BoltGrid.Bolts)
                {
                    double dist = neutralAxis.OrientedDistFromSegment2D(b.Position);
                    double stress = homogCoeff * dC * dist * b.BoltDef.Area;
                    if (boltForces.TryGetValue(b, out var rbf))
                        rbf.N = stress;
                    else
                        boltForces[b] = new ResultBeamForces(stress, 0, 0, 0, 0, 0, new CoordinateSystem(new Point3d(b.Position), Vector3d.XAxis, Vector3d.YAxis), Soll.Id);
                }
            }
        }

        #endregion

        #region Comparers

        /// <summary>
        /// Equality with another plate (see <see cref="Equals(PlateWithBolts)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal plate</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as PlateWithBolts);
        }
        /// <summary>
        /// Equality of the shape and material, of the bolt grid and of the thickness
        /// </summary>
        /// <param name="other">The plate to compare</param>
        /// <returns>True if the plates are equal</returns>
        public bool Equals(PlateWithBolts other)
        {
            return !(other is null) &&
            base.Equals(other) &&
            EqualityComparer<BoltGrid>.Default.Equals(_boltGrid, other._boltGrid) &&
            _thickness == other._thickness;
        }

        /// <summary>
        /// The hash code of the shape, of the bolt grid and of the thickness
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<BoltGrid>.Default.GetHashCode(_boltGrid);
                hashCode = hashCode * -17 + _thickness.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(PlateWithBolts)"/>)
        /// </summary>
        /// <param name="left">The first plate</param>
        /// <param name="right">The second plate</param>
        /// <returns>True if the plates are equal</returns>
        public static bool operator ==(PlateWithBolts left, PlateWithBolts right)
        {
            if (left is null)
                return right is null;

            return EqualityComparer<PlateWithBolts>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(PlateWithBolts)"/>)
        /// </summary>
        /// <param name="left">The first plate</param>
        /// <param name="right">The second plate</param>
        /// <returns>True if the plates are different</returns>
        public static bool operator !=(PlateWithBolts left, PlateWithBolts right)
        {
            return !(left == right);
        }

        #endregion
    }
}
