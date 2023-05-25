using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Bolt
{
    [Serializable]
    public class PlateWithBolts : ShapeEx, IEquatable<PlateWithBolts>, ISerializable
    {
        #region Variables

        protected BoltGrid _boltGrid;
        protected double _thickness;

        #endregion

        #region Properties

        public BoltGrid BoltGrid => _boltGrid;

        public double Thickness
        {
            get => _thickness;
            set => _thickness = value > 0.01 ? value : 0.01;
        }

        public SteelMaterial PlateMaterial
        {
            get => (SteelMaterial)_material;
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
        public PlateWithBolts(in Polygon2d plateShape, in SteelMaterial plateMaterial, in BoltGrid boltGrid, in double plateThickness)
            : base(plateShape, plateMaterial)
        {
            _boltGrid = boltGrid;
            _thickness = plateThickness;
        }

        public PlateWithBolts(in Polygon2d plateShape, in SteelMaterial plateMaterial)
            : this(plateShape, plateMaterial, new BoltGrid(), 10.0)
        {
        }

        public PlateWithBolts(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            double version = info.GetInt32("Version");

            _boltGrid = (BoltGrid)info.GetValue("BoltGrid", typeof(BoltGrid));
            _thickness = info.GetDouble("Thickness");
        }

        #endregion

        #region Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            int version = 1;
            info.AddValue("Version", version);

            info.AddValue("BoltGrid", _boltGrid, typeof(BoltGrid));
            info.AddValue("Thickness", _thickness);
        }

        public double CalculateE1(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateE1(bolt, CalculateAngle(resultBeamForces));
        }

        public double CalculateE2(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateE2(bolt, CalculateAngle(resultBeamForces));
        }

        public double CalculateP1(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateP1(bolt, CalculateAngle(resultBeamForces));
        }

        public double CalculateP2(BoltPosition bolt, ResultBeamForces resultBeamForces)
        {
            return CalculateP2(bolt, CalculateAngle(resultBeamForces));
        }

        public double CalculateE1(BoltPosition bolt, double forceDirectionAngle)
        {
            return CalculateClosestEdgePoint(bolt, forceDirectionAngle);
        }

        public double CalculateE2(BoltPosition bolt, double forceDirectionAngle)
        {
            return Math.Min(CalculateClosestEdgePoint(bolt, forceDirectionAngle + Math.PI / 2.0), CalculateClosestEdgePoint(bolt, forceDirectionAngle - Math.PI / 2.0));
        }

        public double CalculateP1(BoltPosition bolt, double forceDirectionAngle)
        {
            return CalculateClosestBolt(bolt, forceDirectionAngle);
        }

        public double CalculateP2(BoltPosition bolt, double forceDirectionAngle)
        {
            return Math.Min(CalculateClosestBolt(bolt, forceDirectionAngle + Math.PI / 2.0), CalculateClosestBolt(bolt, forceDirectionAngle - Math.PI / 2.0)); ;
        }

        public double CalculateE1Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateE1(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public double CalculateE2Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateE2(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public double CalculateP1Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateP1(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public double CalculateP2Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateP2(_boltGrid.Bolts.ElementAt(i), angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public virtual double CalculateAngle(ResultBeamForces resultBeamForces)
        {
            return Math.Atan2(resultBeamForces.V2, resultBeamForces.V1);
        }

        /// <summary>
        /// Utility.
        /// </summary>
        /// <returns></returns>
        private List<Line2d> GetEdges()
        {
            var edges = Shape.Fill2d.Explode().ToList();
            if (Shape.HasHoles)
                for (int i = 0; i < Shape.Holes2d.Length; i++)
                    edges.AddRange(Shape.Holes2d[i].Explode());

            return edges;
        }

        /// <summary>
        /// Given a bolt find the minimum distance from the edge in a specific direction.
        /// Works for normal and slotted holes.
        /// </summary>
        /// <param name="bolt"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double CalculateClosestEdgePoint(BoltPosition boltPosition, double angle)
        {
            double distance = double.MaxValue;

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
                List<Point2d> points = new List<Point2d>();
                for (int j = 0; j < edges.Count; j++)
                {
                    if (line.GetIntersectionWithInfiniteLine(edges[j], out Point2d intersection))
                    {
                        if (edges[j].IsPointOnLine(intersection))
                            points.Add(intersection);
                    }
                }

                // Selects points by direction.
                if (points.Count > 0)
                {
                    Vector2d v1 = line.ToVector();

                    for (int j = 0; j < points.Count; j++)
                    {
                        double distanceBuffer = center.DistanceTo(points[j]);
                        if (distanceBuffer < distance)
                        {
                            Vector2d v2 = new Line2d(center, points[j]).ToVector();

                            if (v1.DotProduct(v2) > 0)
                            {
                                distance = distanceBuffer;
                            }
                        }
                    }
                }
            }

            return distance;
        }

        /// <summary>
        /// Returns the list of all bolts other than a specific id.
        /// </summary>
        /// <param name="boltPosition"></param>
        /// <returns></returns>
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
        /// Calculates the minimum distance of holes around in a specific direction.
        /// Works for normal and slotted holes.
        /// If it finds no bolts it returns double.MaxValue.
        /// </summary>
        /// <param name="boltPosition"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double CalculateClosestBolt(BoltPosition boltPosition, double angle, double tolerance = GeometryBase.AngularTolerance)
        {
            double distance = double.MaxValue;
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
                            }
                        }
                    }
                }
            }

            return distance;
        }

        /// <summary>
        /// Given a bolt find the minimum distance from another bolt in all directions.
        /// Works for normal and slotted holes.
        /// </summary>
        /// <param name="boltPosition"></param>
        /// <returns></returns>
        public double CalculateClosestBolt(BoltPosition boltPosition)
        {
            double minDist = double.MaxValue;
            double iDist = double.MaxValue;

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
        /// <param name="boltPosition"></param>
        /// <returns>Point from center to point of minimum distance.</returns>
        public Line2d CalculateClosestEdgePoint(BoltPosition boltPosition)
        {
            double minDist = double.MaxValue;
            Line2d minLine = null;

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
                    }
                }
            }

            return minLine;
        }

        /// <summary>
        /// Calculate if bolt is of type outer or inner.
        /// To say whether it is outer is enough if it is on one side, but if it is not for any side then it is inner.
        /// </summary>
        /// <param name="bolt"></param>
        /// <returns></returns>
        public bool IsOuuter(BoltPosition boltPosition)
        {
            // Find point on edges with minimum distance.
            var minDistLine = CalculateClosestEdgePoint(boltPosition);
            var minDist = minDistLine.Length;

            // *** First attempt with minimum point.
            var minDistVector = minDistLine.ToVector();
            double minDistDirection = Math.Atan2(minDistVector.Y, minDistVector.X);
            var nearestBoltDistance = CalculateClosestBolt(boltPosition, minDistDirection);
            if (nearestBoltDistance > minDist)
                return true;

            // *** Second attempt with all edge orthogonal directions.
            // Edge list. For each edge add the orthogonal directions in degrees to reduce them in number.
            HashSet<int> angles = new HashSet<int>();
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

        #endregion

        #region Comparers

        public override bool Equals(object obj)
        {
            return Equals(obj as PlateWithBolts);
        }

        public bool Equals(PlateWithBolts other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   EqualityComparer<BoltGrid>.Default.Equals(_boltGrid, other._boltGrid) &&
                   _thickness == other._thickness;
        }

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

        public static bool operator ==(PlateWithBolts left, PlateWithBolts right)
        {
            if (left is null)
                return right is null;

            return EqualityComparer<PlateWithBolts>.Default.Equals(left, right);
        }

        public static bool operator !=(PlateWithBolts left, PlateWithBolts right)
        {
            return !(left == right);
        }

        #endregion
    }
}
