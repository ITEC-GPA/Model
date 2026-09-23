using GPC.Model.Materials;

namespace GPC.Model.Data.Concrete
{
	public class ConcreteMaterialACI318Data
	{
        #region Static Properties

        public static ConcreteMaterialACI318 Fc3000 => new ConcreteMaterialACI318("fc' 3000", 20.6843, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialACI318 Fc3500 => new ConcreteMaterialACI318("fc' 3500", 24.13159, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialACI318 Fc4000 => new ConcreteMaterialACI318("fc' 4000", 27.579, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		public static ConcreteMaterialACI318 Fc4500 => new ConcreteMaterialACI318("fc' 4500", 31.0263, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		public static ConcreteMaterialACI318 Fc5000 => new ConcreteMaterialACI318("fc' 5000", 34.4738, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		public static ConcreteMaterialACI318 Fc5500 => new ConcreteMaterialACI318("fc' 5500", 37.92108, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		public static ConcreteMaterialACI318 Fc6000 => new ConcreteMaterialACI318("fc' 6000", 41.3685, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialACI318 Fc7000 => new ConcreteMaterialACI318("fc' 7000", 48.2632, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialACI318 Fc8000 => new ConcreteMaterialACI318("fc' 8000", 55.1579, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialACI318 Fc9000 => new ConcreteMaterialACI318("fc' 9000", 62.05267, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialACI318 Fc10000 => new ConcreteMaterialACI318("fc' 10000", 68.9474, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		#endregion
	}
}
