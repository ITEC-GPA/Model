using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
using MathNet.Numerics;
using System;
using System.Collections.Generic;

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

		public virtual double CalculateE1(int boltId, double angle)
		{
			BoltGrid.BoltPosition boltPosition = _boltGrid.Bolts.GetById(boltId);
            Line2d line = new Line2d(boltPosition.Position, new Point2d(boltPosition.Position.X + Math.Cos(angle), boltPosition.Position.Y + Math.Sin(angle)));

			Line2d[] edges = Shape.Fill2d.Explode();
            List<Point2d> points = new List<Point2d>();

            for (int i = 0; i < edges.Length; i++)
            {
                if (line.GetIntersectionWithInfiniteLine(edges[i], out Point2d intersection))
                {
                    points.Add(intersection);
                }
            }
            
            if (Shape.HasHoles)
            {
                for(int i = 0; i < Shape.Holes2d.Length; i++)
                {
					Line2d[] edgesHole = Shape.Holes2d[i].Explode();
					for (int j = 0; j < edgesHole.Length; j++)
					{
						if (line.GetIntersectionWithInfiniteLine(edgesHole[j], out Point2d intersection))
						{
							points.Add(intersection);
						}
					}
				}
            }

            double distance = double.MaxValue;

            if(points.Count > 0)
            {
                Vector2d v1 = line.ToVector();

                for(int i = 0; i < points.Count; i++)
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

		public virtual double CalculateE2(int boltId, double angle)
		{
			return 0;
		}

		public virtual double CalculateP1(int boltId, double angle)
		{
			return 0;
		}

		public virtual double CalculateP2(int boltId, double angle)
		{
			return 0;
		}

		public virtual double CalculateE1Min(ResultBeamForces resultBeamForces)
		{
			return 0;
		}

		public virtual double CalculateE2Min(ResultBeamForces resultBeamForces)
		{
			return 0;
		}

		public virtual double CalculateP1Min(ResultBeamForces resultBeamForces)
		{
			return 0;
		}

		public virtual double CalculateP2Min(ResultBeamForces resultBeamForces)
		{
			return 0;
		}

        public virtual double CalculateAngle(ResultBeamForces resultBeamForces)
        {
            return Math.Atan2(resultBeamForces.V2, resultBeamForces.V1);
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
