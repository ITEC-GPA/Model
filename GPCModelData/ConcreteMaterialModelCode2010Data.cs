using GPC.Model.Materials;

namespace GPC.Model.Data.Concrete
{
	public class ConcreteMaterialModelCode2010Data
	{
		#region Static Properties

		public static ConcreteMaterialModelCode2010 C20_25 => new ConcreteMaterialModelCode2010("C20/25", 20, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C25_30 => new ConcreteMaterialModelCode2010("C25/30", 25, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C28_35 => new ConcreteMaterialModelCode2010("C28/35", 28, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C30_37 => new ConcreteMaterialModelCode2010("C30/37", 30, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C32_40 => new ConcreteMaterialModelCode2010("C32/40", 32, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C35_45 => new ConcreteMaterialModelCode2010("C35/45", 35, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C40_50 => new ConcreteMaterialModelCode2010("C40/50", 40, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C45_55 => new ConcreteMaterialModelCode2010("C45/55", 45, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C50_60 => new ConcreteMaterialModelCode2010("C50/60", 50, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C55_67 => new ConcreteMaterialModelCode2010("C55/67", 55, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C60_75 => new ConcreteMaterialModelCode2010("C60/75", 60, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C70_85 => new ConcreteMaterialModelCode2010("C70/85", 70, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C80_95 => new ConcreteMaterialModelCode2010("C80/90", 80, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C90_105 => new ConcreteMaterialModelCode2010("C90/105", 90, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteMaterial.ConcreteTypes.Concrete);

		public static ConcreteMaterialModelCode2010 C25_30_1_0_C => new ConcreteMaterialModelCode2010("C25/30 1.0C", 25,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.45, 0.3, 0.45 / C25_30.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C25_30_1_5_C => new ConcreteMaterialModelCode2010("C25/30 1.5C", 25,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.68, 0.45, 0.68 / C25_30.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C25_30_2_0_C => new ConcreteMaterialModelCode2010("C25/30 2.0C", 25,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.90, 0.60, 0.90 / C25_30.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C25_30_2_0_D => new ConcreteMaterialModelCode2010("C25/30 2.0C", 25,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.90, 0.80, 0.90 / C25_30.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C30_37_1_5_C => new ConcreteMaterialModelCode2010("C30/37 1.5C", 30,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.68, 0.45, 0.68 / C30_37.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C30_37_2_0_B => new ConcreteMaterialModelCode2010("C30/37 2.0B", 30,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.90, 0.40, 0.90 / C30_37.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C30_37_2_0_D => new ConcreteMaterialModelCode2010("C30/37 2.0D", 30,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.90, 0.80, 0.90 / C30_37.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C35_45_1_5_C => new ConcreteMaterialModelCode2010("C35/45 1.5C", 35,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.68, 0.45, 0.68 / C45_55.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C35_45_2_0_D => new ConcreteMaterialModelCode2010("C35/45 2.0D", 35,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.90, 0.80, 0.90 / C45_55.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C35_45_3_5_D => new ConcreteMaterialModelCode2010("C35/45 3.5D", 35,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.58, 1.40, 1.58 / C45_55.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C45_55_1_0_B => new ConcreteMaterialModelCode2010("C45/55 1.0B", 45,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.45, 0.2, 0.45 / C45_55.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C45_55_1_5_C => new ConcreteMaterialModelCode2010("C45/55 1.5C", 45,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.68, 0.45, 0.68 / C45_55.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C45_55_2_0_D => new ConcreteMaterialModelCode2010("C45/55 2.0D", 45,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.90, 0.80, 0.90 / C45_55.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C45_55_3_5_D => new ConcreteMaterialModelCode2010("C45/55 3.5D", 45,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.58, 1.40, 1.58 / C45_55.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C55_65_2_5_C => new ConcreteMaterialModelCode2010("C55/65 2.5C", 55,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.13, 0.75, 1.13 / C70_85.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C55_65_3_5_C => new ConcreteMaterialModelCode2010("C55/65 3.5C", 55,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.58, 1.05, 1.58 / C70_85.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C70_85_2_5_C => new ConcreteMaterialModelCode2010("C70/85 2.5C", 70,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.13, 0.75, 1.13 / C70_85.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C70_85_3_5_C => new ConcreteMaterialModelCode2010("C70/85 3.5C", 70,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.58, 1.05, 1.58 / C70_85.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C70_85_4_5_C => new ConcreteMaterialModelCode2010("C70/85 4.5D", 70,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 2.03, 1.80, 2.03 / C70_85.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);










		
		public static ConcreteMaterialModelCode2010 C25_30_5 => new ConcreteMaterialModelCode2010("C25/30 5 kg/m³", 25,
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.4905, 0.302, 0.4905 / C25_30.ElasticModulusCompression * 5, 0.02, 
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C25_30_10 => new ConcreteMaterialModelCode2010("C25/30 10 kg/m³", 25, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.6975, 0.505, 0.6975 / C25_30.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C25_30_17 => new ConcreteMaterialModelCode2010("C25/30 17 kg/m³", 25, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.0845, 0.843, 1.0845 / C25_30.ElasticModulusCompression * 5, 0.02, 
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C30_37_5 => new ConcreteMaterialModelCode2010("C30/37 5 kg/m³", 30, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.414, 0.256, 0.414 / C30_37.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C30_37_10 => new ConcreteMaterialModelCode2010("C30/37 10 kg/m³", 30, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.6975, 0.59, 0.6975 / C30_37.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C30_37_15 => new ConcreteMaterialModelCode2010("C30/37 15 kg/m³", 30, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.963, 0.852, 0.963 / C30_37.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C30_37_25 => new ConcreteMaterialModelCode2010("C30/37 25 kg/m³", 30, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.44, 1.23, 1.44 / C30_37.ElasticModulusCompression * 5, 0.02,
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C45_55_5 => new ConcreteMaterialModelCode2010("C45/55 5 kg/m³", 45, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.4995, 0.263, 0.4995 / C45_55.ElasticModulusCompression * 5, 0.02, 
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C45_55_10 => new ConcreteMaterialModelCode2010("C45/55 10 kg/m³", 45, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.828, 0.622, 0.828 / C45_55.ElasticModulusCompression * 5, 0.02, 
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C45_55_15 => new ConcreteMaterialModelCode2010("C45/55 15 kg/m³", 45, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.972, 0.898, 0.972 / C45_55.ElasticModulusCompression * 5, 0.02, 
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C70_85_5 => new ConcreteMaterialModelCode2010("C70/85 5 kg/m³", 70, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 0.4275, 0.205, 0.4275 / C70_85.ElasticModulusCompression * 5, 0.02, 
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		public static ConcreteMaterialModelCode2010 C70_85_15 => new ConcreteMaterialModelCode2010("C70/85 15 kg/m³", 70, 
			ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle, 1.188, 0.902, 1.188 / C70_85.ElasticModulusCompression * 5, 0.02, 
			ConcreteMaterial.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);

		#endregion
	}
}
