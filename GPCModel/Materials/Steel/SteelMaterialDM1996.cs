using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Materials in DM 96:
    /// "DECRETO MINISTERIALE 9 gennaio 1996. Norme tecniche per il calcolo, l’esecuzione ed il collaudo delle strutture in cemento armato, normale e precompresso e per le strutture metalliche.
    /// </summary>
    [Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class SteelMaterialDM1996 : SteelMaterial
	{
		#region Constructor

		public SteelMaterialDM1996(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1,
			SteelTypes steelType = SteelTypes.Undefined, double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05)
			: base(name, elasticModulus, fyk, fu, strainU, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		public SteelMaterialDM1996(string name, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression,
			double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, SteelTypes steelType = SteelTypes.Undefined,
			double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05)
			: base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression,
				  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension,
				  stressStrainTableCompression, stressStrainTableTensio, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		protected SteelMaterialDM1996(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		protected SteelMaterialDM1996(string name, double elasticModulus, double poisson, double fyk, double fu, double strainU,
			SteelTypes steelType, double density, double alfaThermalExpansion)
			: base(name, elasticModulus, poisson, fyk, fu, strainU, steelType, density, alfaThermalExpansion)
		{
		}

		#endregion
	}
}
