using GPC.Model.Materials;

namespace GPC.Model.Data.Concrete
{
    /// <summary>
    /// Predefined concrete classes of EN 1992-1-1: each property returns a new instance
    /// </summary>
    public class ConcreteMaterialEN1992Data
    {
        #region Static Properties

        /// <summary>
        /// The material "C20/25" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C20_25 => new ConcreteMaterialEN1992("C20/25", 20, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C25/30" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C25_30 => new ConcreteMaterialEN1992("C25/30", 25, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C28/35" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C28_35 => new ConcreteMaterialEN1992("C28/35", 28, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C30/37" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C30_37 => new ConcreteMaterialEN1992("C30/37", 30, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C32/40" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C32_40 => new ConcreteMaterialEN1992("C32/40", 32, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C35/45" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C35_45 => new ConcreteMaterialEN1992("C35/45", 35, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C40/50" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C40_50 => new ConcreteMaterialEN1992("C40/50", 40, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C45/55" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C45_55 => new ConcreteMaterialEN1992("C45/55", 45, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C50/60" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C50_60 => new ConcreteMaterialEN1992("C50/60", 50, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C55/67" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C55_67 => new ConcreteMaterialEN1992("C55/67", 55, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C60/75" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C60_75 => new ConcreteMaterialEN1992("C60/75", 60, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C70/85" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C70_85 => new ConcreteMaterialEN1992("C70/85", 70, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C80/90" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C80_95 => new ConcreteMaterialEN1992("C80/90", 80, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "C90/105" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialEN1992 C90_105 => new ConcreteMaterialEN1992("C90/105", 90, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        #endregion
    }
}
