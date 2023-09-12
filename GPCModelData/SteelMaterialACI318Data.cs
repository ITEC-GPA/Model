using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	public class SteelMaterialACI318Data
	{
		#region Rebar

		public static SteelMaterialACI318 Grade40 => new SteelMaterialACI318("Grade 40", 199947.9615, 275.790, 379.901, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterialACI318 Grade50 => new SteelMaterialACI318("Grade 50", 199947.9615, 344.7378, 450.2276, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterialACI318 Grade60 => new SteelMaterialACI318("Grade 60", 199947.9615, 413.685, 551.580, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialACI318 Grade70 => new SteelMaterialACI318("Grade 70", 199947.9615, 482.63301, 643.51068, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialACI318 Grade75 => new SteelMaterialACI318("Grade 75", 199947.9615, 517.1068, 689.4758, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterialACI318 Grade80 => new SteelMaterialACI318("Grade 80", 199947.9615, 551.58058, 620.5281, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialACI318 Grade90 => new SteelMaterialACI318("Grade 90", 199947.9615, 620.52816, 690.337548, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

        public static SteelMaterialACI318 Grade100 => new SteelMaterialACI318("Grade 100", 199947.9615, 689.47573, 758.423302, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterialACI318 Grade115 => new SteelMaterialACI318("Grade 115", 199947.9615, 792.897089, 872.186798, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);

		#endregion

		#region Tendon

		public static SteelMaterialACI318 Grade250 => new SteelMaterialACI318("Grade 250", 196500.6, 1493.4046, 1723.6895, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

		public static SteelMaterialACI318 Grade270 => new SteelMaterialACI318("Grade 270", 196500.6, 1689.9052, 1861.5846, 0.10, SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Tendon);

		#endregion
	}
}
