using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Standards;
using System;
using System.Collections.Generic;
using System.Text;

namespace GPC.Model.Sections.Bolt
{
    public class BoltSection : SectionCircular
    {
        #region Public Property
        public static readonly Dictionary<decimal, double> ThreadedAreas = new Dictionary<decimal, double>()
        {
            { 8, 36.6},
            {10, 58.0},
            {12, 84.3},
            {14, 115},
            {16, 157},
            {18, 192},
            {20, 245},
            {22, 303},
            {24, 353},
            {27, 459},
            {30, 561},
            {33, 694},
            {36, 817},
            {39, 976},
            {42, 1120},
            {45, 1310},
            {48, 1470},
            {52, 1760},
            {56, 2030},
            {60, 2360},
            {64, 2680},
            {68, 3060}
        };
        #endregion

        #region Public Constructors

        public BoltSection(double diameter, SteelMaterial material, string name = "") : base(diameter, material, name)
        { }

        public BoltSection() :
            this(12, new SteelMaterial("10.9", 200000, 700, 1000, 0.3, SteelMaterial.SteelTypes.Structural), "Ø12 - 10.9")
        { }

        #endregion

        #region Public Methods

        protected decimal CalculateDiameterDecimal()
        {
            var DiaDec = Convert.ToDecimal(Diameter);
            return Math.Round(DiaDec, 2);
        }

        /// <summary>
        /// BoltSection net area.
        /// Info in "\\studio\Software_Development\01 Theory\08 BoltSection\Net area bolt.docx".
        /// </summary>
        /// <returns>Net area.</returns>
        public double CalculateAreaEff()
        {
            var DiaDec = CalculateDiameterDecimal();
            // First check if it is a standard diameter, if it is true returns from tabled values.
            if (ThreadedAreas.ContainsKey(DiaDec))
                return ThreadedAreas[DiaDec];
            else
            {
                // Then, if not present use an approssimation.
                // This should be a rare, if not nonexistent case, however, I define it to prevent null values.
                double coeff_F = 0.0295 * Math.Log(Diameter) + 0.7925;
                double d_F = coeff_F * Diameter;
                return d_F * d_F * 0.25 * Math.PI;
            }
        }

        /// <summary>
        /// Calculation of the average diameter of the bolt head.
        /// This is an approximation, derived the coefficient 1.61 as the average of various bolt heads obtained from the standards.
        /// Called d_M in Eurocode EN 1993-1-8: the mean of the across points and across flats dimensions of the bolt head or the nut,
        /// whichever is  smaller.
        /// Called also d_w in Eurocode EN 1993-1-8:is the diameter of the washer, or the width across points of 
        /// the bolt head or nut, as relevant.
        /// If this value is not provided by the user then this value can be used as the default.
        /// Info in "\\studio\Software_Development\01 Theory\08 BoltSection\Net area bolt.docx".
        /// </summary>
        /// <returns>Average diameter of the bolt head.</returns>
        public double CalculateMeanDiameterBoltHead() => 1.61 * Diameter;

        /// <summary>
        /// Calculate fourth of d_w.
        /// Used in Eurocode EN 1993-1-8, called e_w.
        /// Info in "\\studio\Software_Development\01 Theory\08 BoltSection\Net area bolt.docx".
        /// </summary>
        /// <returns>Fourth of d_w.</returns>
        public double CalculateFourthOfDw() => CalculateMeanDiameterBoltHead() * 0.25;

        /// <summary>
        /// Calculates shear resistance for one bolt, EC3.
        /// </summary>
        /// <param name="Std"></param>
        /// <param name="ThreadedArea">True to use threaded or net area.</param>
        /// <returns>F_V_Rd</returns>
        /// <exception cref="InvalidCastException"></exception>
        public double CalculateEN1993ResistanceShear(in StandardEN1993p11 Std, in bool ThreadedArea)
        {
            var SteMat = Material as SteelMaterial ?? throw new InvalidCastException("Inconsistent type of material.");
            var ResArea = ThreadedArea ? CalculateAreaEff() : Area;
            // F_V_Rd = αv * fu * As / γM2;
            return /* SteMat.Alpha_v * */ 0.5 * SteMat.Fu * ResArea / Std.GammaM2;
        }

        public bool Equals(BoltSection other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public override bool Equals(object obj) => Equals(obj as BoltSection);

        public override int GetHashCode()
        {
            return base.GetHashCode() * 103;
        }

        #endregion

        #region Operators overrides

        public static bool operator ==(BoltSection left, BoltSection right) => left.Equals(right);

        public static bool operator !=(BoltSection left, BoltSection right) => !(left == right);

        #endregion

    }
}
