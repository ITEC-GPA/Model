using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
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
        #region Properties

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
            for (int i = 0; i < RectangularBoltGrid.Bolts.Count; i++)
            {
                if (!Shape.IsPointInside(RectangularBoltGrid.Bolts.ElementAt(i).Position))
                    throw new Exception("Bolt must be internal");
            }
        }

        public RectangularPlateWithBolts(in double paddingLeft, in double paddingBottom, in double paddingRight, in double paddingTop,
            in SteelMaterial plateMaterial, in double plateThickness,
            in IEnumerable<double> boltsStepX, in IEnumerable<double> boltsStepY, in double boltsDiameter, in SteelMaterial boltsMaterial,
            in Point2d boltsOrigin)
            : this(paddingLeft + boltsStepX.Sum() + paddingRight, paddingBottom + boltsStepY.Sum() + paddingTop,
                plateMaterial, plateThickness, boltsStepX, boltsStepY, boltsDiameter, boltsMaterial, boltsOrigin)
        { }

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

        #region Public Methods

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

        #endregion
    }
}
