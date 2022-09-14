using GPC.Model.Materials;

namespace GPCModelData
{
	public class SteelMaterialAISC360Data
	{
		public static SteelMaterialACI318 GradeB => new SteelMaterialACI318("Grade B", 199947.9615, 241.3165, 241.3165, 0.10, SteelMaterial.SteelTypes.Structural);
		public static SteelMaterialACI318 GradeBHardening => new SteelMaterialACI318("Grade B Hardening", 199947.9615, 241.3165, 413.6855, 0.10, SteelMaterial.SteelTypes.Structural);

		public static SteelMaterialACI318 Grade36 => new SteelMaterialACI318("Grade 36", 199947.9615, 248.2113, 248.2113, 0.10, SteelMaterial.SteelTypes.Structural);
		public static SteelMaterialACI318 Grade36Hardening => new SteelMaterialACI318("Grade 36 Hardening", 199947.9615, 248.2113, 399.896, 0.10, SteelMaterial.SteelTypes.Structural);

		public static SteelMaterialACI318 Grade50 => new SteelMaterialACI318("Grade 50", 199947.9615, 344.7378, 344.7378, 0.10, SteelMaterial.SteelTypes.Structural);
		public static SteelMaterialACI318 Grade50Hardening => new SteelMaterialACI318("Grade 50 Hardening", 199947.9615, 344.7378, 450.2276, 0.10, SteelMaterial.SteelTypes.Structural);
	}
}
