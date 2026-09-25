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

        #endregion
    }
}
