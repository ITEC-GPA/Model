using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    /// <summary>
    /// A steel section: shape, material and type (rolled or welded, hot finished or cold formed)
    /// </summary>
    public interface ISteelSection : ISectionShape
    {
        /// <summary>
        /// The steel
        /// </summary>
        SteelMaterial SteelMaterial { get; }

        /// <summary>
        /// Rolled or welded
        /// </summary>
        Section.SectionTypes SectionType { get; }

        /// <summary>
        /// Hot finished or cold formed
        /// </summary>
        Section.FormedTypes FormedType { get; }

        /// <summary>
        /// True if rolled
        /// </summary>
        bool IsRolled { get; }

        /// <summary>
        /// True if welded
        /// </summary>
        bool IsWelded { get; }

        /// <summary>
        /// The shape of the section
        /// </summary>
        ISectionShape SectionShape { get; }

        /// <summary>
        /// The minimum elastic normal stress: N / A ± M1 / Wel1 ± M2 / Wel2 with the minimum moduli
        /// </summary>
        /// <param name="N">The axial force (positive: tension)</param>
        /// <param name="M1">The bending moment about the axis 1</param>
        /// <param name="M2">The bending moment about the axis 2</param>
        /// <returns>The minimum stress</returns>
        double GetMinSigma(double N, double M1, double M2);

        /// <summary>
        /// The maximum elastic normal stress: N / A ± M1 / Wel1 ± M2 / Wel2 with the minimum moduli
        /// </summary>
        /// <param name="N">The axial force (positive: tension)</param>
        /// <param name="M1">The bending moment about the axis 1</param>
        /// <param name="M2">The bending moment about the axis 2</param>
        /// <returns>The maximum stress</returns>
        double GetMaxSigma(double N, double M1, double M2);
    }
}
