using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class SteelMaterialEN1993 : SteelMaterial
	{
		#region Constructor

		public SteelMaterialEN1993(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1, 
			SteelTypes steelType = SteelTypes.Structural, double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05) 
			: base(name, elasticModulus, fyk, fu, strainU, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		public SteelMaterialEN1993(string name)
			: base(name, SteelTypes.Structural)
		{
		}

        public SteelMaterialEN1993(string name, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression, 
			double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, SteelTypes steelType = SteelTypes.Structural, 
			double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05)
			: base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression, 
				  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension, 
				  stressStrainTableCompression, stressStrainTableTensio, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		protected SteelMaterialEN1993(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		protected SteelMaterialEN1993(string name, double elasticModulus, double poisson, double fyk, double fu, double strainU,
			SteelTypes steelType, double density, double alfaThermalExpansion) 
			: base(name, elasticModulus, poisson, fyk, fu, strainU, steelType, density, alfaThermalExpansion)
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
