using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections.Bolt
{
    public class PlateWithBolts : ShapeEx, IEquatable<PlateWithBolts>
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
            set => _thickness = value > 0.01 ? _thickness : 0.01;
        }

        public SteelMaterial PlateMaterial => (SteelMaterial)Material;

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

        #endregion

        #region Virtual Methods

        public virtual double CalculateE1(int boltId, ResultBeamForces resultBeamForces)
        {
            return CalculateE1(boltId, CalculateAngle(resultBeamForces));
        }

        public virtual double CalculateE2(int boltId, ResultBeamForces resultBeamForces)
        {
            return CalculateE2(boltId, CalculateAngle(resultBeamForces));
        }

        public virtual double CalculateP1(int boltId, ResultBeamForces resultBeamForces)
        {
            return CalculateP1(boltId, CalculateAngle(resultBeamForces));
        }

        public virtual double CalculateP2(int boltId, ResultBeamForces resultBeamForces)
        {
            return CalculateP2(boltId, CalculateAngle(resultBeamForces));
        }

        public virtual double CalculateE1(int boltId, double forceDirectionAngle)
        {
            return CalculateClosestEdgePoint(boltId, forceDirectionAngle);
        }

        public virtual double CalculateE2(int boltId, double forceDirectionAngle)
        {
            return Math.Min(CalculateClosestEdgePoint(boltId, forceDirectionAngle + Math.PI / 2.0), CalculateClosestEdgePoint(boltId, forceDirectionAngle - Math.PI / 2.0));
        }

        public virtual double CalculateP1(int boltId, double forceDirectionAngle)
        {
            return CalculateClosestBolt(boltId, forceDirectionAngle);
        }

        public virtual double CalculateP2(int boltId, double forceDirectionAngle)
        {
            return Math.Min(CalculateClosestBolt(boltId, forceDirectionAngle + Math.PI / 2.0), CalculateClosestBolt(boltId, forceDirectionAngle - Math.PI / 2.0)); ;
        }

        public virtual double CalculateE1Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateE1(_boltGrid.Bolts.ElementAt(i).Id, angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public virtual double CalculateE2Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateE2(_boltGrid.Bolts.ElementAt(i).Id, angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public virtual double CalculateP1Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateP1(_boltGrid.Bolts.ElementAt(i).Id, angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public virtual double CalculateP2Min(ResultBeamForces resultBeamForces)
        {
            double distance = double.MaxValue;

            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                var angle = CalculateAngle(resultBeamForces);
                double distanceBuffer = CalculateP2(_boltGrid.Bolts.ElementAt(i).Id, angle);

                if (distanceBuffer < distance)
                    distance = distanceBuffer;
            }

            return distance;
        }

        public virtual double CalculateAngle(ResultBeamForces resultBeamForces)
        {
            return Math.Atan2(resultBeamForces.V2, resultBeamForces.V1);
        }

        private double CalculateClosestEdgePoint(int boltId, double angle)
        {
            BoltGrid.BoltPosition boltPosition = _boltGrid.Bolts.GetById(boltId);
            Line2d line = new Line2d(boltPosition.Position, new Point2d(boltPosition.Position.X + Math.Cos(angle), boltPosition.Position.Y + Math.Sin(angle)));

            Line2d[] edges = Shape.Fill2d.Explode();
            List<Point2d> points = new List<Point2d>();

            for (int i = 0; i < edges.Length; i++)
            {
                if (line.GetIntersectionWithInfiniteLine(edges[i], out Point2d intersection))
                {
                    if (edges[i].IsPointOnLine(intersection))
                        points.Add(intersection);
                }
            }

            if (Shape.HasHoles)
            {
                for (int i = 0; i < Shape.Holes2d.Length; i++)
                {
                    Line2d[] edgesHole = Shape.Holes2d[i].Explode();
                    for (int j = 0; j < edgesHole.Length; j++)
                    {
                        if (line.GetIntersectionWithInfiniteLine(edgesHole[j], out Point2d intersection))
                        {
                            if (edges[i].IsPointOnLine(intersection))
                                points.Add(intersection);
                        }
                    }
                }
            }

            double distance = double.MaxValue;

            if (points.Count > 0)
            {
                Vector2d v1 = line.ToVector();

                for (int i = 0; i < points.Count; i++)
                {
                    double distanceBuffer = boltPosition.Position.DistanceTo(points[i]);
                    if (distanceBuffer < distance)
                    {
                        Vector2d v2 = new Line2d(boltPosition.Position, points[i]).ToVector();

                        if (v1.DotProduct(v2) > 0)
                        {
                            distance = distanceBuffer;
                        }
                    }
                }
            }

            return distance;
        }

        private double CalculateClosestBolt(int boltId, double angle)
        {
            BoltGrid.BoltPosition boltPosition = _boltGrid.Bolts.GetById(boltId);
            Vector2d v1 = new Vector2d(Math.Cos(angle), Math.Sin(angle));

            var otherBolts = new List<BoltGrid.BoltPosition>();
            for (int i = 0; i < _boltGrid.Bolts.Count; i++)
            {
                if (_boltGrid.Bolts.ElementAt(i).Id != boltId)
                {
                    otherBolts.Add(_boltGrid.Bolts.ElementAt(i));
                }
            }

            double distance = double.MaxValue;

            if (otherBolts.Count > 0)
            {
                for (int i = 0; i < otherBolts.Count; i++)
                {
                    double distanceBuffer = boltPosition.Position.DistanceTo(otherBolts[i].Position);
                    if (distanceBuffer < distance)
                    {
                        Vector2d v2 = new Line2d(boltPosition.Position, otherBolts[i].Position).ToVector();
                        v2.Unitize();

                        if (v1.DotProduct(v2) > Math.Cos(Math.PI * 0.25))
                        {
                            distance = distanceBuffer;
                        }
                    }
                }
            }

            return distance;
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
