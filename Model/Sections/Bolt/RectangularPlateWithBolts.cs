using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
	/// Obsolete: it is no longer used, instead the generic PlateWithBolts is used.
    /// Maintained only for testing.
    /// Generic plate with holes and bolts.
    /// </summary>
    public class RectangularPlateWithBolts : PlateWithBolts
    {
        #region Properties

        /// <summary>
        /// The rectangular bolt grid
        /// </summary>
        public RectangularBoltGrid RectangularBoltGrid => (RectangularBoltGrid)_boltGrid;

        #endregion

        #region Constructor

        /// <summary>
        /// Define a rectangular plate with a rectangular bolt grid inside.
        /// </summary>
        /// <param name="plateB">Base of rectangular plate, start from (0, 0) up to (plateB, 0).</param>
        /// <param name="plateH">Height of rectangular plate, start from (0, 0) up to (0, plateH).</param>
        /// <param name="plateMaterial">The material of the plate</param>
        /// <param name="plateThickness">Plate plate thickness.</param>
        /// <param name="boltsStepX">Steps in X.</param>
        /// <param name="boltsStepY">Steps in Y.</param>
        /// <param name="boltsDiameter">The diameter of the bolts</param>
        /// <param name="boltsMaterial">The material of the bolts</param>
        /// <param name="boltsOrigin">Starting point, bottom left corner.</param>
        /// <exception cref="Exception">If a bolt is outside the plate</exception>
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
            for (int i = 0; i < RectangularBoltGrid.Bolts.Count; i++)
            {
                if (!Shape.IsPointInside(RectangularBoltGrid.Bolts.ElementAt(i).Position))
                    throw new Exception("Bolt must be internal");
            }
        }

        /// <summary>
        /// Define a rectangular plate with a rectangular bolt grid inside, from the distances of the bolts from the sides
        /// </summary>
        /// <param name="paddingLeft">The distance of the bolts from the left side</param>
        /// <param name="paddingBottom">The distance of the bolts from the bottom side</param>
        /// <param name="paddingRight">The distance of the bolts from the right side</param>
        /// <param name="paddingTop">The distance of the bolts from the top side</param>
        /// <param name="plateMaterial">The material of the plate</param>
        /// <param name="plateThickness">The thickness of the plate</param>
        /// <param name="boltsStepX">Steps in X.</param>
        /// <param name="boltsStepY">Steps in Y.</param>
        /// <param name="boltsDiameter">The diameter of the bolts</param>
        /// <param name="boltsMaterial">The material of the bolts</param>
        /// <param name="boltsOrigin">Starting point of the grid, bottom left corner.</param>
        /// <exception cref="Exception">If a bolt is outside the plate</exception>
        public RectangularPlateWithBolts(in double paddingLeft, in double paddingBottom, in double paddingRight, in double paddingTop,
            in SteelMaterial plateMaterial, in double plateThickness,
            in IEnumerable<double> boltsStepX, in IEnumerable<double> boltsStepY, in double boltsDiameter, in SteelMaterial boltsMaterial,
            in Point2d boltsOrigin)
            : this(paddingLeft + boltsStepX.Sum() + paddingRight, paddingBottom + boltsStepY.Sum() + paddingTop,
                plateMaterial, plateThickness, boltsStepX, boltsStepY, boltsDiameter, boltsMaterial, boltsOrigin)
        { }

        #endregion

        #region Comparers

        /// <summary>
        /// Equality with another plate (see <see cref="Equals(RectangularPlateWithBolts)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal plate</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as RectangularPlateWithBolts);
        }

        /// <summary>
        /// Equality of the plate, of the bolt grid and of the thickness
        /// </summary>
        /// <param name="other">The plate to compare</param>
        /// <returns>True if the plates are equal</returns>
        public bool Equals(RectangularPlateWithBolts other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   EqualityComparer<BoltGrid>.Default.Equals(_boltGrid, other._boltGrid) &&
                   _thickness == other._thickness;
        }

        /// <summary>
        /// The hash code of the plate, of the bolt grid and of the thickness
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(RectangularPlateWithBolts)"/>)
        /// </summary>
        /// <param name="left">The first plate</param>
        /// <param name="right">The second plate</param>
        /// <returns>True if the plates are equal</returns>
        public static bool operator ==(RectangularPlateWithBolts left, RectangularPlateWithBolts right)
        {
            if (left is null)
                return right is null;

            return EqualityComparer<RectangularPlateWithBolts>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(RectangularPlateWithBolts)"/>)
        /// </summary>
        /// <param name="left">The first plate</param>
        /// <param name="right">The second plate</param>
        /// <returns>True if the plates are different</returns>
        public static bool operator !=(RectangularPlateWithBolts left, RectangularPlateWithBolts right)
        {
            return !(left == right);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid), minimum distance from the right plate edge.
        /// </summary>
        /// <returns>The distance</returns>
        public double CalculateExRight()
        {
            return Get2dBoundingBox().Max.X - _boltGrid.Bolts.Max(bd => bd.Position.X);
        }

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid), minimum distance from the left plate edge (x = 0).
        /// </summary>
        /// <returns>The distance</returns>
        public double CalculateExLeft()
        {
            return _boltGrid.Bolts.Min(bd => bd.Position.X);
        }

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid), minimum distance from the top plate edge.
        /// </summary>
        /// <returns>The distance</returns>
        public double CalculateEyTop()
        {
            return Get2dBoundingBox().Max.Y - _boltGrid.Bolts.Max(bd => bd.Position.Y);
        }

        /// <summary>
        /// Overall parameter of bolt grid (useful for simple configuration such as rectangular grid), minimum distance from the bottom plate edge (y = 0).
        /// </summary>
        /// <returns>The distance</returns>
        public double CalculateEyBottom()
        {
            return _boltGrid.Bolts.Min(bd => bd.Position.Y);
        }

        #endregion
    }
}
