using GPC.Model.Materials;

namespace GPC.Model.Data.Concrete
{
	/// <summary>
	/// Predefined concretes of ACI 318 (f'c in psi): each property returns a new instance
	/// </summary>
	public class ConcreteMaterialACI318Data
	{
        #region Static Properties

        /// <summary>
        /// The material "fc' 3000" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialACI318 Fc3000 => new ConcreteMaterialACI318("fc' 3000", 20.6843, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "fc' 3500" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialACI318 Fc3500 => new ConcreteMaterialACI318("fc' 3500", 24.13159, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "fc' 4000" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialACI318 Fc4000 => new ConcreteMaterialACI318("fc' 4000", 27.579, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		/// <summary>
		/// The material "fc' 4500" (a new instance at each access)
		/// </summary>
		public static ConcreteMaterialACI318 Fc4500 => new ConcreteMaterialACI318("fc' 4500", 31.0263, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		/// <summary>
		/// The material "fc' 5000" (a new instance at each access)
		/// </summary>
		public static ConcreteMaterialACI318 Fc5000 => new ConcreteMaterialACI318("fc' 5000", 34.4738, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		/// <summary>
		/// The material "fc' 5500" (a new instance at each access)
		/// </summary>
		public static ConcreteMaterialACI318 Fc5500 => new ConcreteMaterialACI318("fc' 5500", 37.92108, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		/// <summary>
		/// The material "fc' 6000" (a new instance at each access)
		/// </summary>
		public static ConcreteMaterialACI318 Fc6000 => new ConcreteMaterialACI318("fc' 6000", 41.3685, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "fc' 7000" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialACI318 Fc7000 => new ConcreteMaterialACI318("fc' 7000", 48.2632, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "fc' 8000" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialACI318 Fc8000 => new ConcreteMaterialACI318("fc' 8000", 55.1579, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "fc' 9000" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialACI318 Fc9000 => new ConcreteMaterialACI318("fc' 9000", 62.05267, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

        /// <summary>
        /// The material "fc' 10000" (a new instance at each access)
        /// </summary>
        public static ConcreteMaterialACI318 Fc10000 => new ConcreteMaterialACI318("fc' 10000", 68.9474, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

		#endregion
	}
}
