using GPC.Model.Fem.Materials;
using GPC.Utilities.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class SteelMaterial : Material
	{
		#region Public Enum        

		public enum SteelTypes
		{
			Undefined,
			[Description("Rebar steel material")] Rebar,
			[Description("Tendon steel material")] Tendon,
			[Description("Structural steel material")] Structural,
			[Description("Bars steel material")] Bars,
            [Description("Bolt steel material")] Bolt,
        }

		#endregion

		#region Variables

		protected double _fyk;
		protected double _fu;
		protected SteelTypes _steelType;

		#endregion

		#region Properties

		/// <summary>
		/// Characteristic yield strength
		/// </summary>
		public double Fyk => _fyk;

		/// <summary>
		/// Ultimate strength
		/// </summary>
		public double Fu => _fu;

		/// <summary>
		/// Strain hardening modulus
		/// </summary>
		public double Et => GetEt();

		/// <summary>
		/// Type of steel
		/// </summary>
		public SteelTypes SteelType => _steelType;

		#endregion

		#region Constructor

		/// <param name="name"></param>
		/// <param name="elasticModulus">Steel elastic modulus</param>
		/// <param name="fyk">Yielding stress</param>
		/// <param name="fu">Ultimate stress</param>
		/// <param name="strainU">Ultimate strain</param>
		/// <param name="steelType"></param>
		/// <param name="poisson"></param>
		/// <param name="density"></param>
		/// <param name="alfaThermalExpansion"></param>
		public SteelMaterial(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1, SteelTypes steelType = SteelTypes.Undefined,
			 double poisson = 0.30, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
			: this(name, elasticModulus, poisson, fyk, fu, strainU, steelType, density, alfaThermalExpansion)
		{

		}

		public SteelMaterial(string name, double elasticModulusCompression, double elasticModulusTension,
			double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
			double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, SteelTypes steelType = SteelTypes.Undefined,
			 double poisson = 0.30, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
			: base(name, elasticModulusCompression, elasticModulusTension,
			strainYCompression, strainUCompression, strainYTension, strainUTension,
			stressYCompression, stressUCompression, stressYTension, stressUTension,
			stressStrainTableCompression, stressStrainTableTensio, poisson, density, alfaThermalExpansion)
		{
			_steelType = steelType;
			_fyk = stressYTension;
			_fu = stressUTension;
		}

		/// <summary>
		/// Protected steelMaterial constructor 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="elasticModulus">Steel elastic modulus</param>
		/// <param name="poisson">Poissoins's Ratio</param>
		/// <param name="fyk">Yielding stress</param>
		/// <param name="fu">Ultimate stress</param>
		/// <param name="strainU">The ultimate strain</param>
		/// <param name="steelType">Type of steel</param>
		/// <param name="density">Density of material</param>
		/// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
		protected SteelMaterial(string name, double elasticModulus, double poisson, double fyk,
			double fu, double strainU, SteelTypes steelType, double density, double alfaThermalExpansion)
			: base(name, elasticModulus, poisson, density, alfaThermalExpansion)
		{
			_fu = Math.Abs(fu);
			_fyk = Math.Abs(fyk);
			_strainUTension = Math.Abs(strainU);
			_strainUCompression = -Math.Abs(strainU);
			_steelType = steelType;

			SetDefaultMechanicalProperties();
		}

		protected SteelMaterial(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			int version;
			try
			{
				version = info.GetInt32("SteelMaterialVersion");
			}
			catch (Exception)
			{
				version = 1;
			}

			_fu = info.GetDouble("Fu");
			_fyk = info.GetDouble("Fyk");
			_steelType = (SteelTypes)info.GetInt32("SteelType");

			if (version == 1)
			{
				_elasticModulusTension = info.GetDouble("ElasticModulus");
				_strainUTension = info.GetDouble("EpsilonU");
				_strainUCompression = -info.GetDouble("EpsilonU");

				SetDefaultMechanicalProperties();
			}
			else if (version >= 2)
			{

			}
		}

		#endregion

		#region Public Methods

		public override IsotropicFemMaterial GetIsotropicFemMaterial()
		{
			return new IsotropicFemMaterial(ElasticModulusCompression, Ni, AlfaThermalExpansion, Density);
		}

		public override OrthotropicFemMaterial GetOrthotropicFemMaterial()
		{
			return new OrthotropicFemMaterial(ElasticModulusCompression, ElasticModulusCompression, ElasticModulusCompression, Ni, Ni, Ni, GetShearModule(), GetShearModule(), GetShearModule(), AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
		}

		public virtual void RecalculateMechanicalProperties()
		{
			_stressStrainTableCompression = new StressStrainTable(
				new double[] { 0, _stressYCompression, _stressUCompression },
				new double[] { 0, _stressYCompression / _elasticModulusCompression, _strainUCompression });
			_stressStrainTableTension = new StressStrainTable(
				new double[] { 0, _stressYTension, _stressUTension },
				new double[] { 0, _stressYTension / _elasticModulusTension, _strainUTension });

			_strainYTension = _stressYTension / _elasticModulusTension;
			_strainYCompression = _stressYCompression / _elasticModulusCompression;

			_fu = _stressUTension;
			_fyk = _stressYTension;
		}

		public virtual void SetDefaultMechanicalProperties()
		{
			_stressStrainTableCompression = new StressStrainTable(
				new double[] { 0, -_fyk, -_fu },
				new double[] { 0, -_fyk / _elasticModulusCompression, _strainUCompression });
			_stressStrainTableTension = new StressStrainTable(
				new double[] { 0, _fyk, _fu },
				new double[] { 0, _fyk / _elasticModulusTension, _strainUTension });

			_strainYTension = _fyk / _elasticModulusTension;
			_strainYCompression = -_fyk / _elasticModulusCompression;

			_stressUCompression = -_fu;
			_stressUTension = _fu;
			_stressYCompression = -_fyk;
			_stressYTension = _fyk;
		}

		#endregion

		#region Protected Methods

		protected double GetEt()
		{
			if (Math.Abs(Fu - Fyk) < Geometry.GeometryBase.GetDefaultTolerance())
				return 0.0;
			else
				return (Fu - Fyk) / (StrainUTension - StrainYTension);
		}

		#endregion

		#region Public Standard Methods

		public double CalculateDesignStress(Standards.Standard standard, double strain, double epsilonP = 0)
		{
			switch (standard)
			{
				case Standards.StandardModelCode2010 mc:
					return CalculateDesignStress(mc, strain, epsilonP);
				case Standards.StandardACI318 aci:
					return CalculateDesignStress(aci, strain, epsilonP);
				default:
					return 0;
			}
		}

		public double CalculateDesignStrain(Standards.Standard standard, double strain)
		{
			switch (standard)
			{
				case Standards.StandardModelCode2010 mc:
					return CalculateDesignStrain(mc, strain);
				case Standards.StandardACI318 aci:
					return CalculateDesignStrain(aci, strain);
				default:
					return 0;
			}
		}

		#region ModelCode2010

		/// <returns>The design rebar yielding stress</returns>
		public double CalculateFyd(Standards.StandardModelCode2010 standard)
		{
			return Fyk / standard.GammaS;
		}

		public double CalculateDesignYieldingStressTension(Standards.StandardModelCode2010 standard)
		{
			if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars)
				return StressYTension / standard.GammaS;
			else if (SteelType == SteelTypes.Tendon)
				return StressYTension / standard.GammaSPrestress;
			else
				throw new Exception();
		}

		public double CalculateDesignYieldingStressCompression(Standards.StandardModelCode2010 standard)
		{
			if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars)
				return StressYCompression / standard.GammaS;
			else if (SteelType == SteelTypes.Tendon)
				return StressYCompression / standard.GammaSPrestress;
			else
				throw new Exception();
		}

		public double CalculateDesignYieldingStrainTension(Standards.StandardModelCode2010 standard)
		{
			return CalculateDesignYieldingStressTension(standard) / ElasticModulusTension;
		}

		public double CalculateDesignYieldingStrainCompression(Standards.StandardModelCode2010 standard)
		{
			return CalculateDesignYieldingStressCompression(standard) / ElasticModulusCompression;
		}

		public double CalculateDesignUltimateStrain(Standards.StandardModelCode2010 standard)
		{
			if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars || SteelType == SteelTypes.Tendon)
				return StrainUTension * standard.SteelCoefficientStrainTension;
			else
				throw new Exception();
		}

		public double CalculateDesignStrain(Standards.StandardModelCode2010 standardModelCode2010, double strain)
		{
			return strain;
		}

		/// <returns>The design rebar stress related to <paramref name="strain"/></returns>
		public double CalculateDesignStress(Standards.StandardModelCode2010 standard, double strain, double epsilonP = 0)
		{
			double fyd = CalculateDesignYieldingStressTension(standard);
			double strainYd = CalculateDesignYieldingStrainTension(standard);

			if (Math.Abs(strain + epsilonP) <= strainYd)
				return GetStress(strain + epsilonP);

			else
			{
				double deltaStress = Fyk - fyd;
				double deltaStrain = deltaStress / ElasticModulusTension;

				double stressCalc = strain + Math.Sign(strain) * deltaStrain + epsilonP;
				double designUltimateStrain = StrainUTension;

				if (Math.Abs(stressCalc) > designUltimateStrain && Math.Abs(strain) <= designUltimateStrain)
					stressCalc = Math.Sign(stressCalc) * Math.Abs(designUltimateStrain);
				else if (Math.Abs(strain) > designUltimateStrain)
					return 0;

				if (GetStress(stressCalc) != 0)
					return GetStress(stressCalc) - Math.Sign(strain) * deltaStress;
				else
					return GetStress(stressCalc);
			}
		}

		public double CalculateDesignStress(Standards.StandardModelCode2010 standard, double stress, double strain, double epsilonP = 0)
		{
			if (strain >= 0)
			{
				double strainYd = CalculateDesignYieldingStrainTension(standard);

				if (Math.Abs(strain + epsilonP) <= strainYd)
					return stress;

				else
				{
					double fyd = CalculateDesignYieldingStressTension(standard);
					double deltaStress = StressYTension - fyd;

					if (strain > StrainUTension)
						return 0;

					return stress - deltaStress;
				}
			}
			else
			{
				double strainYd = CalculateDesignYieldingStrainCompression(standard);

				if (Math.Abs(strain + epsilonP) <= strainYd)
					return stress;

				else
				{
					double fyd = CalculateDesignYieldingStressCompression(standard);
					double deltaStress = StressYCompression - fyd;

					if (strain < StrainUCompression)
						return 0;

					return stress - deltaStress;
				}
			}
		}

		#endregion

		#region ACI318

		/// <returns>The design rebar yielding stress</returns>
		public double CalculateFyd(Standards.StandardACI318 standard)
		{
			return Fyk;
		}

		public double CalculateDesignYieldingStress(Standards.StandardACI318 standard)
		{
			return Fyk;
		}

		public double CalculateDesignYieldingStrain(Standards.StandardACI318 standard)
		{
			return CalculateDesignYieldingStress(standard) / ElasticModulusTension;
		}

		public double CalculateDesignUltimateStrain(Standards.StandardACI318 standard)
		{
			return StrainUTension;
		}

		public double CalculateDesignStrain(Standards.StandardACI318 standardACI318, double strain)
		{
			return strain;
		}

		/// <returns>The design rebar stress related to <paramref name="strain"/></returns>
		public double CalculateDesignStress(Standards.StandardACI318 standard, double strain, double epsilonP = 0)
		{
			return GetStress(strain + epsilonP);
		}

		public double CalculateDesignStress(Standards.StandardACI318 standard, double stress, double strain, double epsilonP = 0)
		{
			return stress;
		}

		public double CalculateDesignStress(Standards.Standard standard, double stress, double strain, double epsilonP = 0)
		{
			switch (standard)
			{
				case Standards.StandardModelCode2010 mc:
					return CalculateDesignStress(mc, stress, strain, epsilonP);
				case Standards.StandardACI318 aci:
					return CalculateDesignStress(aci, stress, strain, epsilonP);
				default:
					return 0;
			}
		}

		#endregion

		#endregion

		#region Public Methods Override

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			double version = 2;

			info.AddValue("SteelMaterialVersion", version);

			info.AddValue("Fyk", _fyk);
			info.AddValue("Fu", _fu);
			info.AddValue("SteelType", _steelType);
		}

		public override bool Equals(object obj)
		{
			return obj is SteelMaterial material &&
				   base.Equals(obj) &&
				   _fyk == material._fyk &&
				   _fu == material._fu &&
				   _steelType == material._steelType;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = -17;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _fyk.GetHashCode();
				hashCode = hashCode * -17 + _fu.GetHashCode();
				hashCode = hashCode * -17 + _steelType.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}
}
