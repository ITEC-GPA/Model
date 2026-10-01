using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
    /// <summary>
    /// Predefined steels of the Italian D.M. 1996: each property returns a new instance
    /// </summary>
    public class SteelMaterialDM1996Data
    {
        #region Rebar

        /// <summary>
        /// The material "FeB22k" (a new instance at each access)
        /// </summary>
        public static SteelMaterialDM1996 FeB22k => new SteelMaterialDM1996("FeB22k", 206000, 215, 335, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// The material "FeB32k" (a new instance at each access)
        /// </summary>
        public static SteelMaterialDM1996 FeB32k => new SteelMaterialDM1996("FeB32k", 206000, 315, 490, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// The material "FeB38k" (a new instance at each access)
        /// </summary>
        public static SteelMaterialDM1996 FeB38k => new SteelMaterialDM1996("FeB38k", 206000, 375, 450, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// The material "FeB44k" (a new instance at each access)
        /// </summary>
        public static SteelMaterialDM1996 FeB44k => new SteelMaterialDM1996("FeB44k", 206000, 430, 540, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// Surface and minimum elongation A5 of the rebars of D.M. 9 January 1996, part I, section I: table 1-I (smooth bars FeB22k 24%, FeB32k 23%)
        /// and table 2-I (ribbed bars FeB38k 14%, FeB44k 12%). Nominal historical minima, transferred from ANTHEA (RebarMaterial.Catalog): they
        /// document existing structures (bond of smooth bars, ductility) and never become constitutive strains.
        /// </summary>
        public static IReadOnlyList<HistoricRebar> HistoricRebars { get; } = new[]
        {
            new HistoricRebar("FeB22k", true, 24), new HistoricRebar("FeB32k", true, 23), new HistoricRebar("FeB38k", false, 14), new HistoricRebar("FeB44k", false, 12)
        };

        /// <summary>The historical data of a rebar of D.M. 1996 by name (case sensitive).</summary>
        public static HistoricRebar Historic(string name) => HistoricRebars.FirstOrDefault(r => r.Name == name) ?? throw new ArgumentException("Not a rebar of D.M. 1996: " + name, nameof(name));

        #endregion
    }

    /// <summary>Surface and minimum percentage elongation A5 of a historical rebar.</summary>
    public sealed class HistoricRebar
    {
        public string Name { get; }
        /// <summary>Smooth (plain) bar; false = ribbed (high bond).</summary>
        public bool Smooth { get; }
        /// <summary>Minimum percentage elongation after fracture A5, %.</summary>
        public double MinimumElongation { get; }
        internal HistoricRebar(string name, bool smooth, double minimumElongation) { Name = name; Smooth = smooth; MinimumElongation = minimumElongation; }
    }
}
