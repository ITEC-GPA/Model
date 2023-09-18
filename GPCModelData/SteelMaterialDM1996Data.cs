using GPC.Model.Materials;

namespace GPC.ModelData
{
    public class SteelMaterialDM1996Data
    {
        #region Rebar

        public static SteelMaterialDM1996 FeB22k => new SteelMaterialDM1996("FeB22k", 206000, 215, 335, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialDM1996 FeB32k => new SteelMaterialDM1996("FeB32k", 206000, 315, 490, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialDM1996 FeB38k => new SteelMaterialDM1996("FeB38k", 206000, 375, 450, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialDM1996 FeB44k => new SteelMaterialDM1996("FeB44k", 206000, 430, 540, 0.01, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        #endregion
    }
}
