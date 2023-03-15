using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;

namespace GPC.Model.Sections.Bolt
{
    public abstract class PlateWithBolts : ShapeEx, IEquatable<PlateWithBolts>
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

        public SteelMaterialEN1993 PlateMaterial => (SteelMaterialEN1993)Material;

        #endregion

        #region Constructor

        /// <summary>
        /// Define a generic plate with a generic bolt grid inside.
        /// </summary>
        /// <param name="plateShape">Plate shape.</param>
        /// <param name="plateMaterial">Plate material.</param>
        /// <param name="boltGrid">Bolt grid.</param>
        /// <param name="plateThickness">Plate plate thickness.</param>
        public PlateWithBolts(in Polygon2d plateShape, in SteelMaterial plateMaterial, in BoltGrid boltGrid, in double plateThickness) :
            base(plateShape, plateMaterial)
        {
            _boltGrid = boltGrid;
            Thickness = plateThickness;
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
            int hashCode = 301669611;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<BoltGrid>.Default.GetHashCode(_boltGrid);
            hashCode = hashCode * -1521134295 + _thickness.GetHashCode();
            return hashCode;
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
