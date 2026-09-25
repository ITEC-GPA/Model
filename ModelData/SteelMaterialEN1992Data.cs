using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	/// <summary>
	/// Predefined reinforcing and prestressing steels of EN 1992-1-1: each property returns a new instance
	/// </summary>
	public class SteelMaterialEN1992Data
	{
        #region Rebar

        /// <summary>
        /// The material "B450A" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 B450A => new SteelMaterialEN1992("B450A", 200000, 450, 540, 0.03, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// The material "B450C" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 B450C => new SteelMaterialEN1992("B450C", 200000, 450, 540, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// The material "B500A" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 B500A => new SteelMaterialEN1992("B500A", 200000, 500, 525, 0.03, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// The material "B500B" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 B500B => new SteelMaterialEN1992("B500B", 200000, 500, 550, 0.05, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        /// <summary>
        /// The material "B500C" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 B500C => new SteelMaterialEN1992("B500C", 200000, 500, 575, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        #endregion

        #region Bars

        /// <summary>
        /// The material "Y1180" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1180C => new SteelMaterialEN1992("Y1180", 205000, 1030, 1180, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Bars);

        /// <summary>
        /// The material "Y1240" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1240C => new SteelMaterialEN1992("Y1240", 205000, 1050, 1240, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Bars);

        /// <summary>
        /// The material "Y1280" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1280C => new SteelMaterialEN1992("Y1280", 205000, 1100, 1280, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Bars);

        /// <summary>
        /// The material "Y1370" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1370C => new SteelMaterialEN1992("Y1370", 205000, 1200, 1370, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Bars);

        /// <summary>
        /// The material "Y1420" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1420C => new SteelMaterialEN1992("Y1420", 205000, 1230, 1420, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Bars);

        /// <summary>
        /// The material "Y1470" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1470C => new SteelMaterialEN1992("Y1470", 205000, 1290, 1470, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Bars);

        #endregion

        #region Tendon

        /// <summary>
        /// The material "Y1570" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1570C => new SteelMaterialEN1992("Y1570", 195000, 1360, 1570, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y1620" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1620C => new SteelMaterialEN1992("Y1620", 195000, 1420, 1620, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y1670" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1670C => new SteelMaterialEN1992("Y1670", 195000, 1480, 1670, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y1770" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1770C => new SteelMaterialEN1992("Y1770", 195000, 1560, 1770, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y1860" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1860C => new SteelMaterialEN1992("Y1860", 195000, 1640, 1860, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y1960" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y1960C => new SteelMaterialEN1992("Y1960", 195000, 1740, 1960, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y2060" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y2060C => new SteelMaterialEN1992("Y2060", 195000, 1820, 2060, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y2160" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y2160C => new SteelMaterialEN1992("Y2160", 195000, 1900, 2160, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y2260" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y2260C => new SteelMaterialEN1992("Y2260", 195000, 1980, 2260, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        /// <summary>
        /// The material "Y2360" (a new instance at each access)
        /// </summary>
        public static SteelMaterialEN1992 Y2360C => new SteelMaterialEN1992("Y2360", 195000, 2070, 2360, 0.075, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

        #endregion
    }
}
