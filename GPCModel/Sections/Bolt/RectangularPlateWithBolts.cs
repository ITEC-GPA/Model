using GPC.Geometry;
using GPC.Model.Fem.FiniteElements;
using GPC.Model.Materials;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
    /// Generic plate with holes and bolts.
    /// </summary>
    public class RectangularPlateWithBolts : PlateWithBolts
    {
        #region Variables

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
        /// minimum spacing in X direction.
        /// Use double.PositiveInfinity if there are no steps.
        /// </summary>
        private double _p_x;

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
        /// minimum spacing in Y direction.
        /// Use double.PositiveInfinity if there are no steps.
        /// </summary>
        private double _p_y;

        #endregion

        #region Properties

        public double P_x => _p_x;

        public double P_y => _p_y;

        public RectangularBoltGrid RectangularBoltGrid => (RectangularBoltGrid)_boltGrid;

		#endregion

		#region Constructor

		/// <summary>
		/// Define a rectangular plate with a rectangular bolt grid inside.
		/// </summary>
		/// <param name="plateB">Base of rectangular plate, start from (0, 0) up to (plateB, 0).</param>
		/// <param name="plateH">Height of rectangular plate, start from (0, 0) up to (0, plateH).</param>
		/// <param name="plateMaterial"></param>
		/// <param name="plateThickness">Plate plate thickness.</param>
		/// <param name="boltsStepX">Steps in X.</param>
		/// <param name="boltsStepY">Steps in Y.</param>
		/// <param name="boltsDiameter"></param>
		/// <param name="boltsMaterial"></param>
		/// <param name="boltsOrigin">Starting point, bottom right corner.</param>
		public RectangularPlateWithBolts(in double plateB, in double plateH, in SteelMaterial plateMaterial, in double plateThickness,
            in IEnumerable<double> boltsStepX, in IEnumerable<double> boltsStepY, in double boltsDiameter, in SteelMaterial boltsMaterial,
            in Point2d boltsOrigin)
            : base(
                new Polygon2d(
                    new Point2d[]
                    {
                        new Point2d(0, 0),
                        new Point2d(plateB, 0),
                        new Point2d(plateB, plateH),
                        new Point2d(0, plateH)
                    }),
                plateMaterial,
                new RectangularBoltGrid(boltsStepX, boltsStepY, boltsDiameter, boltsMaterial, boltsOrigin),
                plateThickness
                )
        {
            // No check is made on whether the bolts are inside or outside the plate.
 

            if (boltsStepX.Count() > 0)
                _p_x = boltsStepX.Min();
            else
                _p_x = double.PositiveInfinity;

            if (boltsStepY.Count() > 0)
                _p_y = boltsStepY.Min();
            else
                _p_y = double.PositiveInfinity;
        }

        #endregion

        #region Comparers

        public override bool Equals(object obj)
        {
            return Equals(obj as RectangularPlateWithBolts);
        }

        public bool Equals(RectangularPlateWithBolts other)
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
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<BoltGrid>.Default.GetHashCode(_boltGrid);
                hashCode = hashCode * -17 + _thickness.GetHashCode();
                return hashCode;
            }
        }

		public override double CalculateE1(int boltId, ResultBeamForces resultBeamForces)
		{
			return base.CalculateE1(boltId, resultBeamForces);
		}

		public override double CalculateE2(int boltId, ResultBeamForces resultBeamForces)
		{
			return base.CalculateE2(boltId, resultBeamForces);
		}

		public override double CalculateP1(int boltId, ResultBeamForces resultBeamForces)
		{
			return base.CalculateP1(boltId, resultBeamForces);
		}

		public override double CalculateP2(int boltId, ResultBeamForces resultBeamForces)
		{
			return base.CalculateP2(boltId, resultBeamForces);
		}

		/// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
		/// minimum distance from the right plate edge.
		/// </summary>
		/// <returns></returns>
		public double CalculateExRight()
		{
			return Get2dBoundingBox().Max.X - _boltGrid.Bolts.Max(bd => bd.Position.X);
		}

		/// <summary>
		/// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
		/// minimum distance from the right plate edge.
		/// </summary>
		/// <returns></returns>
		public double CalculateExLeft()
		{
			return _boltGrid.Bolts.Min(bd => bd.Position.X);
		}

		/// <summary>
		/// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
		/// minimum distance from the top plate edge.
		/// </summary>
		/// <returns></returns>
		public double CalculateEyTop()
		{
			return Get2dBoundingBox().Max.Y - _boltGrid.Bolts.Max(bd => bd.Position.Y);
		}

		/// <summary>
		/// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
		/// minimum distance from the bottom plate edge.
		/// </summary>
		/// <returns></returns>
		public double CalculateEyBottom()
		{
			return _boltGrid.Bolts.Min(bd => bd.Position.Y);
		}

		public override double CalculateE1Min(ResultBeamForces resultBeamForces)
		{
			return base.CalculateE1Min(resultBeamForces);
		}

		public override double CalculateE2Min(ResultBeamForces resultBeamForces)
		{
			return base.CalculateE2Min(resultBeamForces);
		}

		public override double CalculateP1Min(ResultBeamForces resultBeamForces)
		{
			return base.CalculateP1Min(resultBeamForces);
		}

		public override double CalculateP2Min(ResultBeamForces resultBeamForces)
		{
			return base.CalculateP2Min(resultBeamForces);
		}

		public static bool operator ==(RectangularPlateWithBolts left, RectangularPlateWithBolts right)
        {
            if (left is null)
                return right is null;

            return EqualityComparer<RectangularPlateWithBolts>.Default.Equals(left, right);
        }

        public static bool operator !=(RectangularPlateWithBolts left, RectangularPlateWithBolts right)
        {
            return !(left == right);
        }

        #endregion
    }
}
