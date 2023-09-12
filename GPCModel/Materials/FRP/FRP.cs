using GPC.Model.Fem.Materials;
using GPC.Utilities.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Steel", Group = "Materials", Kind = "Material")]
    public class FRP : Material
    {
        #region Public Enum        

        public enum StressStrainCurveType
        {
            Undefined = 0,
            Linear = 1,
            Bilinear = 2,
        }

        #endregion

        #region Variables

        protected double _fyk;
        protected double _fu;
        protected StressStrainCurveType _stressStrainCurveType;

		#endregion

		#region Properties

		/// <summary>
		/// Characteristic yield strength
		/// </summary>
		public double Fyk
        {
            get => _fyk;
            set
            {
                if (_fyk != value)
                {
                    _fyk = value;
                    RecalculateMechanicalProperties();
                }
            }
        }

        /// <summary>
        /// Ultimate strength
        /// </summary>
        public double Fu
        {
            get => _fu;
            set
            {
                if (_fu != value)
                {
                    _fu = value;
                    RecalculateMechanicalProperties();
                }
            }
        }

        /// <summary>
        /// Strain hardening modulus
        /// </summary>
        public double Et => GetEt();

        public StressStrainCurveType StressStrainCurve
        {
            get => _stressStrainCurveType;
            set
            {
                if (value != _stressStrainCurveType)
                {
                    _stressStrainCurveType = value;
					SetDefaultMechanicalProperties();
                }
            }
        }

		#endregion

		#region Constructor

		/// <param name="name"></param>
		/// <param name="elasticModulus">FRP elastic modulus</param>
		/// <param name="fyk">Yielding stress</param>
		/// <param name="fu">Ultimate stress</param>
		/// <param name="strainU">Ultimate strain</param>
		/// <param name="steelType"></param>
		/// <param name="poisson"></param>
		/// <param name="density"></param>
		/// <param name="alfaThermalExpansion"></param>
		public FRP(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.Linear,
            double poisson = 0.30, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
            : this(name, elasticModulus, poisson, fyk, fu, strainU, stressStrainCurveType, density, alfaThermalExpansion)
        {

        }

        public FRP(string name)
            : this(name, 210000, 235, 360, 0.1, StressStrainCurveType.Linear)
        {
        }

        public FRP(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, 
            StressStrainCurveType stressStrainCurveType = StressStrainCurveType.Bilinear, 
             double poisson = 0.30, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
            : base(name, elasticModulusCompression, elasticModulusTension,
            strainYCompression, strainUCompression, strainYTension, strainUTension,
            stressYCompression, stressUCompression, stressYTension, stressUTension,
            stressStrainTableCompression, stressStrainTableTensio, poisson, density, alfaThermalExpansion)
        {
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
        protected FRP(string name, double elasticModulus, double poisson, double fyk,
            double fu, double strainU, StressStrainCurveType stressStrainCurveType, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion)
        {
            _fu = Math.Abs(fu);
            _fyk = Math.Abs(fyk);
            _strainUTension = Math.Abs(strainU);
            _strainUCompression = -Math.Abs(strainU);
            _stressStrainCurveType = stressStrainCurveType;

            SetDefaultMechanicalProperties();
        }

        protected FRP(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("FRPMaterialVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version >= 1)
            {
                _fu = info.GetDouble("Fu");
                _fyk = info.GetDouble("Fyk");
                _stressStrainCurveType = (StressStrainCurveType)info.GetInt32("StressStrainCurveType");
                RecalculateMechanicalProperties();
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
			SetStressStrain();

			_strainYTension = _stressYTension / _elasticModulusTension;
            _strainYCompression = _stressYCompression / _elasticModulusCompression;

            _fu = _stressUTension;
            _fyk = _stressYTension;
        }

        public virtual void SetDefaultMechanicalProperties()
        {
            SetStressStrain();

            _strainYTension = _fyk / _elasticModulusTension;
            _strainYCompression = -_fyk / _elasticModulusCompression;

            _stressUCompression = -_fu;
            _stressUTension = _fu;
            _stressYCompression = -_fyk;
            _stressYTension = _fyk;
        }

        public virtual void SetStressStrain()
        {
            switch (_stressStrainCurveType)
            {
                case StressStrainCurveType.Linear:
					{
						_stressStrainTableCompression = new StressStrainTable(
							new double[] { 0, 0 },
							new double[] { 0, 0 });
						_stressStrainTableTension = new StressStrainTable(
							new double[] { 0, _fyk, _fyk },
							new double[] { 0, _fyk / _elasticModulusTension, _strainUTension });
						return;
					}
				case StressStrainCurveType.Bilinear:
                    {
                        _stressStrainTableCompression = new StressStrainTable(
							new double[] { 0, 0 },
							new double[] { 0, 0 });
						_stressStrainTableTension = new StressStrainTable(
                            new double[] { 0, _fyk, _fu },
                            new double[] { 0, _fyk / _elasticModulusTension, _strainUTension });
                        return;
                    }
                default:
                    return;
            }
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
                case Standards.StandardEN1993p11 ec3:
                    return CalculateDesignStress(ec3, strain, epsilonP);
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
                case Standards.StandardEN1993p11 ec3:
                    return CalculateDesignStrain(ec3, strain);
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
            return StressYTension / standard.GammaS;
        }

        public double CalculateDesignYieldingStressCompression(Standards.StandardModelCode2010 standard)
        {
            return StressYCompression / standard.GammaS;
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
            return StrainUTension * standard.SteelCoefficientStrainTension;
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

            return CalculateDesignStressCommon(strain, epsilonP, fyd, strainYd);
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
                case Standards.StandardEN1993p11 ec3:
                    return CalculateDesignStress(ec3, stress, strain, epsilonP);
                default:
                    return 0;
            }
        }

        #endregion

        #region Eurocode 3

        /// <returns>The design steel yielding stress</returns>
        public double CalculateFyd(Standards.StandardEN1993p11 standard)
        {
            return Fyk / standard.GammaM0;
        }

        public double CalculateDesignYieldingStressTension(Standards.StandardEN1993p11 standard)
        {
            return StressYTension / standard.GammaM0;
        }

        public double CalculateDesignYieldingStressCompression(Standards.StandardEN1993p11 standard)
        {
            return StressYCompression / standard.GammaM0;
        }

        public double CalculateDesignYieldingStrainTension(Standards.StandardEN1993p11 standard)
        {
            return CalculateDesignYieldingStressTension(standard) / ElasticModulusTension;
        }

        public double CalculateDesignYieldingStrainCompression(Standards.StandardEN1993p11 standard)
        {
            return CalculateDesignYieldingStressCompression(standard) / ElasticModulusCompression;
        }

        public double CalculateDesignUltimateStrain(Standards.StandardEN1993p11 standard)
        {
            return StrainUTension;
        }

        public double CalculateDesignStrain(Standards.StandardEN1993p11 standard, double strain)
        {
            return strain;
        }

        /// <returns>The design steel stress related to <paramref name="strain"/></returns>
        /// Copied from same method for StandardModelCode2010.
        public double CalculateDesignStress(Standards.StandardEN1993p11 standard, double strain, double epsilonP = 0)
        {
            double fyd = CalculateDesignYieldingStressTension(standard);
            double strainYd = CalculateDesignYieldingStrainTension(standard);

            return CalculateDesignStressCommon(strain, epsilonP, fyd, strainYd);
        }

        private double CalculateDesignStressCommon(double strain, double epsilonP, double fyd, double strainYd)
        {
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

        /// Copied from same method for StandardModelCode2010.
        public double CalculateDesignStress(Standards.StandardEN1993p11 standard, double stress, double strain, double epsilonP = 0)
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

        #endregion

        #region Public Methods Override

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 1;

            info.AddValue("FRPMaterialVersion", version);

            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
            info.AddValue("StressStrainCurveType", _stressStrainCurveType);
		}

        public override bool Equals(object obj)
        {
            return obj is FRP material &&
                   base.Equals(obj) &&
                   _fyk == material._fyk &&
                   _fu == material._fu;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _fyk.GetHashCode();
                hashCode = hashCode * -17 + _fu.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
