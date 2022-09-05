using GPC.Model.Materials;

namespace GPC.Model.Data.Concrete
{
	public class ConcreteMaterialACI318Data
	{
        #region Static Properties

        public static ConcreteMaterialACI318 Fc3000 => new ConcreteMaterialACI318("fc' 3000 psi", 20.6843, ConcreteMaterial.CompressionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialACI318 Fc4000 => new ConcreteMaterialACI318("fc' 4000 psi", 27.579, ConcreteMaterial.CompressionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialACI318 Fc5000 => new ConcreteMaterialACI318("fc' 5000 psi", 34.4738, ConcreteMaterial.CompressionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialACI318 Fc6000 => new ConcreteMaterialACI318("fc' 6000 psi", 41.3685, ConcreteMaterial.CompressionStressStrainDiagrams.Bilinear);

        #endregion
    }
}
