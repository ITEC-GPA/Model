using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
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
        /// minimum distance from the right plate edge.
        /// </summary>
        private double _e_x_left;

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
        /// minimum distance from the right plate edge.
        /// </summary>
        private double _e_x_right;

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
        /// minimum distance from the bottom plate edge.
        /// </summary>
        private double _e_y_bottom;

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid),
        /// minimum distance from the top plate edge.
        /// </summary>
        private double _e_y_top;

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

        public double E_x_left => _e_x_left;

        public double E_x_right => _e_x_right;

        public double E_y_bottom => _e_y_bottom;

        public double E_y_top => _e_y_top;

        public double P_x => _p_x;

        public double P_y => _p_y;

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
                new BoltGrid(boltsStepX, boltsStepY, boltsDiameter, boltsMaterial, boltsOrigin),
                plateThickness
                )
        {
            // No check is made on whether the bolts are inside or outside the plate.
            double distanceTolerance = 0.01;

            _e_x_left = _boltGrid.Bolts.Min(bd => bd.Position.X);
            _e_x_left = Math.Max(_e_x_left, distanceTolerance); // It cannot be negative.

            _e_x_right = plateB - _boltGrid.Bolts.Max(bd => bd.Position.X);
            _e_x_right = Math.Max(_e_x_right, distanceTolerance); // It cannot be negative.

            _e_y_bottom = _boltGrid.Bolts.Min(bd => bd.Position.Y);
            _e_y_bottom = Math.Max(_e_y_bottom, distanceTolerance); // It cannot be negative.

            _e_y_top = plateH - _boltGrid.Bolts.Max(bd => bd.Position.Y);
            _e_y_top = Math.Max(_e_y_top, distanceTolerance); // It cannot be negative.

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
            int hashCode = -799835886;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<BoltGrid>.Default.GetHashCode(_boltGrid);
            hashCode = hashCode * -1521134295 + _thickness.GetHashCode();
            return hashCode;
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
