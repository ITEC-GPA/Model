using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class BoltMaterialEN1993 : SteelMaterial
	{
		#region Constructor

		public BoltMaterialEN1993(string name, double elasticModulus, double fyb, double fub, double strainU = 0.1, 
			SteelTypes steelType = SteelTypes.Bolt, double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05) 
			: base(name, elasticModulus, fyb, fub, strainU, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		public BoltMaterialEN1993(string name, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression, 
			double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, SteelTypes steelType = SteelTypes.Bolt, 
			double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05)
			: base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression, 
				  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension, 
				  stressStrainTableCompression, stressStrainTableTensio, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		protected BoltMaterialEN1993(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		protected BoltMaterialEN1993(string name, double elasticModulus, double poisson, double fyb, double fub, double strainU,
			SteelTypes steelType, double density, double alfaThermalExpansion) 
			: base(name, elasticModulus, poisson, fyb, fub, strainU, steelType, density, alfaThermalExpansion)
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
