using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	public class SteelMaterialEN1993Data
	{
        #region Structural 

        /// <summary>
        /// Default Steel S235 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterialEN1993 S235 => new SteelMaterialEN1993("S235", 210000, 235, 360, 0.15, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// Default Steel S275 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterialEN1993 S275 => new SteelMaterialEN1993("S275", 210000, 275, 430, 0.15, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// Default Steel S355 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterialEN1993 S355 => new SteelMaterialEN1993("S355", 210000, 355, 490, 0.15, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// Default Steel S420 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterialEN1993 S420 => new SteelMaterialEN1993("S420", 210000, 420, 520, 0.15, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Structural);

        /// <summary>
        /// Default Steel S450 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterialEN1993 S450 => new SteelMaterialEN1993("S450", 210000, 440, 550, 0.15, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Structural);

        #endregion
    }
}
