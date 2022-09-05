using GPC.Model.Materials;

namespace GPC.Model.Data.Steel
{
	public class SteelMaterialAISC360
	{
		#region Rebar

		public static SteelMaterial Grade40 => new SteelMaterial("Grade 40", 199947.9615, 275.790, 275.790, 0.10, SteelMaterial.SteelTypes.Rebar);
		public static SteelMaterial Grade40Hardening => new SteelMaterial("Grade 40 Hardening", 199947.9615, 275.790, 379.901, 0.10, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterial Grade50 => new SteelMaterial("Grade 50", 199947.9615, 344.7378, 344.7378, 0.10, SteelMaterial.SteelTypes.Rebar);
		public static SteelMaterial Grade50Hardening => new SteelMaterial("Grade 50 Hardening", 199947.9615, 344.7378, 450.2276, 0.10, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterial Grade60 => new SteelMaterial("Grade 60", 199947.9615, 413.685, 413.685, 0.10, SteelMaterial.SteelTypes.Rebar);
		public static SteelMaterial Grade60Hardening => new SteelMaterial("Grade 60 Hardening", 199947.9615, 413.685, 551.580, 0.10, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterial Grade80 => new SteelMaterial("Grade 80", 199947.9615, 551.58058, 551.58058, 0.10, SteelMaterial.SteelTypes.Rebar);
		public static SteelMaterial Grade80Hardening => new SteelMaterial("Grade 80 Hardening", 199947.9615, 551.58058, 620.5281, 0.10, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterial Grade100 => new SteelMaterial("Grade 100", 199947.9615, 689.47573, 689.47573, 0.10, SteelMaterial.SteelTypes.Rebar);
		public static SteelMaterial Grade100Hardening => new SteelMaterial("Grade 100 Hardening", 199947.9615, 689.47573, 758.423302, 0.10, SteelMaterial.SteelTypes.Rebar);

		public static SteelMaterial Grade115 => new SteelMaterial("Grade 115", 199947.9615, 792.897089, 792.897089, 0.10, SteelMaterial.SteelTypes.Rebar);
		public static SteelMaterial Grade115Hardening => new SteelMaterial("Grade 115 Hardening", 199947.9615, 792.897089, 872.186798, 0.10, SteelMaterial.SteelTypes.Rebar);

		#endregion
	}
}
