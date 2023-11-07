using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class SteelMaterialAISC360 : SteelMaterial
	{
		#region Constructor

		public SteelMaterialAISC360(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticPerfectPlastic,
			SteelTypes steelType = SteelTypes.Undefined, double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05)
			: base(name, elasticModulus, fyk, fu, strainU, stressStrainCurveType, steelType, poisson, density, alfaThermalExpansion)
		{
        }

        public SteelMaterialAISC360(string name)
            : base(name, SteelTypes.Structural)
        {
        }

        public SteelMaterialAISC360(string name, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression,
			double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticHardening, SteelTypes steelType = SteelTypes.Undefined,
			double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05)
			: base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression,
				  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension,
				  stressStrainTableCompression, stressStrainTableTensio, stressStrainCurveType, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		protected SteelMaterialAISC360(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		protected SteelMaterialAISC360(string name, double elasticModulus, double poisson, double fyk, double fu, double strainU, StressStrainCurveType stressStrainCurveType,
			SteelTypes steelType, double density, double alfaThermalExpansion)
			: base(name, elasticModulus, poisson, fyk, fu, strainU, stressStrainCurveType, steelType, density, alfaThermalExpansion)
		{
		}

		#endregion

		#region Public Methods Override

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public override string ToString()
		{
			return base.ToString();
		}

		#endregion
	}
}
