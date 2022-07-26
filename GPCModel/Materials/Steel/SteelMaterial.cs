using GPC.Model.Fem.Materials;
using GPC.Utilities.Attributes;
using GPC.Utilities.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Steel", Group = "Materials", Kind = "Material")]
    public class SteelMaterial : Material
    {
		#region Properties

		#region Structural 

		/// <summary>
		/// Default Steel S235 according to EN1993 for hot rolled structural steel
		/// </summary>
		public static SteelMaterial S235 => new SteelMaterial("S235", 210000, 235, 360, 0.15, SteelTypes.Structural);

        /// <summary>
        /// Default Steel S275 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterial S275 => new SteelMaterial("S275", 210000, 275, 430, 0.15, SteelTypes.Structural);

        /// <summary>
        /// Default Steel S355 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterial S355 => new SteelMaterial("S355", 210000, 355, 490, 0.15, SteelTypes.Structural);

        /// <summary>
        /// Default Steel S420 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterial S420 => new SteelMaterial("S420", 210000, 420, 520, 0.15, SteelTypes.Structural);

        /// <summary>
        /// Default Steel S450 according to EN1993 for hot rolled structural steel
        /// </summary>
        public static SteelMaterial S450 => new SteelMaterial("S450", 210000, 440, 550, 0.15, SteelTypes.Structural);

		#endregion

		#region Rebar

		public static SteelMaterial B450A => new SteelMaterial("B450A", 200000, 450, 450, 0.03, SteelTypes.Rebar);
        public static SteelMaterial B450AHardening => new SteelMaterial("B450A Hardening", 200000, 450, 540, 0.03, SteelTypes.Rebar);

        public static SteelMaterial B450C => new SteelMaterial("B450C", 200000, 450, 450, 0.075, SteelTypes.Rebar);
        public static SteelMaterial B450CHardening => new SteelMaterial("B450C Hardening", 200000, 450, 540, 0.075, SteelTypes.Rebar);

        public static SteelMaterial B500A => new SteelMaterial("B500A", 200000, 500, 500, 0.03, SteelTypes.Rebar);
        public static SteelMaterial B500AHardening => new SteelMaterial("B500A Hardening", 200000, 500, 525, 0.03, SteelTypes.Rebar);

        public static SteelMaterial B500B => new SteelMaterial("B500B", 200000, 500, 500, 0.05, SteelTypes.Rebar);
        public static SteelMaterial B500BHardening => new SteelMaterial("B500B Hardening", 200000, 500, 550, 0.05, SteelTypes.Rebar);

        public static SteelMaterial B500C => new SteelMaterial("B500C", 200000, 500, 500, 0.075, SteelTypes.Rebar);
        public static SteelMaterial B500CHardening => new SteelMaterial("B500C Hardening", 200000, 500, 575, 0.075, SteelTypes.Rebar);

        public static SteelMaterial Grade40 => new SteelMaterial("Grade 40", 199947.9615, 275.790, 275.790, 0.10, SteelTypes.Rebar);
        public static SteelMaterial Grade40Hardening => new SteelMaterial("Grade 40 Hardening", 199947.9615, 275.790, 379.901, 0.10, SteelTypes.Rebar);

        public static SteelMaterial Grade50 => new SteelMaterial("Grade 50", 199947.9615, 344.7378, 344.7378, 0.10, SteelTypes.Rebar);
        public static SteelMaterial Grade50Hardening => new SteelMaterial("Grade 50 Hardening", 199947.9615, 344.7378, 450.2276, 0.10, SteelTypes.Rebar);

        public static SteelMaterial Grade60 => new SteelMaterial("Grade 60", 199947.9615, 413.685, 413.685, 0.10, SteelTypes.Rebar);
        public static SteelMaterial Grade60Hardening => new SteelMaterial("Grade 60 Hardening", 199947.9615, 413.685, 551.580, 0.10, SteelTypes.Rebar);

        public static SteelMaterial Grade80 => new SteelMaterial("Grade 80", 199947.9615, 551.58058, 551.58058, 0.10, SteelTypes.Rebar);
        public static SteelMaterial Grade80Hardening => new SteelMaterial("Grade 80 Hardening", 199947.9615, 551.58058, 620.5281, 0.10, SteelTypes.Rebar);

        public static SteelMaterial Grade100 => new SteelMaterial("Grade 100", 199947.9615, 689.47573, 689.47573, 0.10, SteelTypes.Rebar);
        public static SteelMaterial Grade100Hardening => new SteelMaterial("Grade 100 Hardening", 199947.9615, 689.47573, 758.423302, 0.10, SteelTypes.Rebar);

        public static SteelMaterial Grade115 => new SteelMaterial("Grade 115", 199947.9615, 792.897089, 792.897089, 0.10, SteelTypes.Rebar);
        public static SteelMaterial Grade115Hardening => new SteelMaterial("Grade 115 Hardening", 199947.9615, 792.897089, 872.186798, 0.10, SteelTypes.Rebar);

		#endregion

		#region Bars

		public static SteelMaterial Y1030C => new SteelMaterial("Y1030", 205000, 1030, 1030, 0.04, SteelTypes.Bars);
        public static SteelMaterial Y1030CHardening => new SteelMaterial("Y1030 Hardening", 205000, 1030, 1180, 0.04, SteelTypes.Bars);

        public static SteelMaterial Y1050C => new SteelMaterial("Y1050", 205000, 1050, 1050, 0.04, SteelTypes.Bars);
        public static SteelMaterial Y1050CHardening => new SteelMaterial("Y1050 Hardening", 205000, 1050, 1240, 0.04, SteelTypes.Bars);

        public static SteelMaterial Y1100C => new SteelMaterial("Y1100", 205000, 1100, 1100, 0.04, SteelTypes.Bars);
        public static SteelMaterial Y1100CHardening => new SteelMaterial("Y1100 Hardening", 205000, 1100, 1280, 0.04, SteelTypes.Bars);

        public static SteelMaterial Y1230C => new SteelMaterial("Y1230", 205000, 1230, 1230, 0.04, SteelTypes.Bars);
        public static SteelMaterial Y1230CHardening => new SteelMaterial("Y1230 Hardening", 205000, 1230, 1420, 0.04, SteelTypes.Bars);

        #endregion

        #region Tendon

        public static SteelMaterial Y1570C => new SteelMaterial("Y1570", 195000, 1420, 1420, 0.035, SteelTypes.Tendon);
        public static SteelMaterial Y1570CHardening => new SteelMaterial("Y1570 Hardening", 195000, 1420, 1570, 0.035, SteelTypes.Tendon);

        public static SteelMaterial Y1620C => new SteelMaterial("Y1620", 195000, 1420, 1420, 0.035, SteelTypes.Tendon);
        public static SteelMaterial Y1620CHardening => new SteelMaterial("Y1620 Hardening", 195000, 1420, 1620, 0.035, SteelTypes.Tendon);

        public static SteelMaterial Y1670C => new SteelMaterial("Y1670", 195000, 1480, 1480, 0.035, SteelTypes.Tendon);
        public static SteelMaterial Y1670CHardening => new SteelMaterial("Y1670 Hardening", 195000, 1480, 1670, 0.035, SteelTypes.Tendon);

        public static SteelMaterial Y1770C => new SteelMaterial("Y1770", 195000, 1560, 1560, 0.035, SteelTypes.Tendon);
        public static SteelMaterial Y1770CHardening => new SteelMaterial("Y1770 Hardening", 195000, 1560, 1770, 0.035, SteelTypes.Tendon);

        public static SteelMaterial Y1860C => new SteelMaterial("Y1860", 195000, 1640, 1640, 0.035, SteelTypes.Tendon);
        public static SteelMaterial Y1860CHardening => new SteelMaterial("Y1860 Hardening", 195000, 1640, 1860, 0.035, SteelTypes.Tendon);

        public static SteelMaterial Y1960C => new SteelMaterial("Y1960", 195000, 1740, 1740, 0.035, SteelTypes.Tendon);
        public static SteelMaterial Y1960CHardening => new SteelMaterial("Y1960 Hardening", 195000, 1740, 1960, 0.035, SteelTypes.Tendon);

        public static SteelMaterial Y2060C => new SteelMaterial("Y2060C", 195000, 1820, 1820, 0.035, SteelTypes.Tendon);
        public static SteelMaterial Y2060CHardening => new SteelMaterial("Y2060C Hardening", 195000, 1820, 2060, 0.035, SteelTypes.Tendon);

        #endregion

        #endregion

        #region Public Enum        

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum SteelTypes
        {
            Undefined,

            [Description("Rebar steel material")]
            Rebar,

            [Description("Tendon steel material")]
            Tendon,

            [Description("Structural steel material")]
            Structural,

            [Description("Bars steel material")]
            Bars,
        }

        #endregion

        #region Variables

        protected double _fyk;
        protected double _fu;
        protected double _strainU;
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
        /// Yielding strain
        /// </summary>
        public double StrainY => _fyk / _elasticModulusCompression;

        /// <summary>
        /// Ultimate strain
        /// </summary>
        public double StrainU => _strainU;

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
            _fu = fu <= 0 ? throw new ArgumentException($"{nameof(fu)} cannot be zero or lower") : fu;
            _fyk = fyk <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : fyk;
            _strainU = strainU <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : strainU;
            _steelType = steelType;

            SetMechanicalProperties();
        }

        protected SteelMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fu = info.GetDouble("Fu");
            _fyk = info.GetDouble("Fyk");
            _strainU = info.GetDouble("EpsilonU");
            _steelType = (SteelTypes)info.GetInt32("SteelType");
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("EpsilonU", _strainU);
            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
            info.AddValue("SteelType", _steelType);
        }

        public void RecalculateMechanicalProperties()
		{
            SetMechanicalProperties();
		}

        #endregion 

        #region Protected Methods

        protected double GetEt()
		{
            if (Math.Abs(Fu - Fyk) < Geometry.GeometryBase.GetDefaultTolerance())
                return 0.0;
            else
                return (Fu - Fyk) / (StrainU - StrainY);
		}

        protected void SetMechanicalProperties()
		{
            _stressStrainTableCompression = new StressStrainTable(
                new double[] { 0, -_fyk, -_fu },
                new double[] { 0, -_fyk / _elasticModulusCompression, -_strainU });
            _stressStrainTableTension = new StressStrainTable(
                new double[] { 0, _fyk, _fu },
                new double[] { 0, _fyk / _elasticModulusTension, _strainU });
        }

        #endregion

        #region Public Standard Methods

        #region ModelCode2010

        /// <returns>The design rebar yielding stress</returns>
        public double CalculateFyd(Standards.StandardModelCode2010 standard)
        {
            return Fyk / standard.GammaS;
        }

        public double CalculateUltimateDesignStrainRebar(Standards.StandardModelCode2010 standard)
        {
            if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Tendon || SteelType == SteelTypes.Bars)
                return StrainU * standard.SteelCoefficientStrainTension;
            else
                throw new Exception();
        }

        public double CalculateDesignYieldingStressRebar(Standards.StandardModelCode2010 standard)
        {
            if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars)
                return Fyk / standard.GammaS;
            else if (SteelType == SteelTypes.Tendon)
                return Fyk / standard.GammaSPrestress;
            else
                throw new Exception();
        }

        public double CalculateDesignYieldingStrainRebar(Standards.StandardModelCode2010 standard)
        {
            return CalculateDesignYieldingStressRebar(standard) / ElasticModulusCompression;   
        }

        public double CalculateDesignUltimateStrainRebar(Standards.StandardModelCode2010 standard)
        {
            if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars || SteelType == SteelTypes.Tendon)
                return StrainU * standard.SteelCoefficientStrainTension;
            else
                throw new Exception();
        }

        /// <returns>The design rebar stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStressRebar(Standards.StandardModelCode2010 standard, double strain, double epsilonP = 0)
        {
            double fyd = CalculateDesignYieldingStressRebar(standard);
            double strainYd = CalculateDesignYieldingStrainRebar(standard);

            if (Math.Abs(strain + epsilonP) <= strainYd)
                return GetStress(strain + epsilonP);

            else
            {
                double deltaStress = Fyk - fyd;
                double deltaStrain = deltaStress / ElasticModulusCompression;

                return GetStress(strain + Math.Sign(strain) * deltaStrain + epsilonP) - Math.Sign(strain) * deltaStress;
            }
        }

		#endregion

		#region ACI318

		/// <returns>The design rebar yielding stress</returns>
		public double CalculateFyd(Standards.StandardACI318 standard)
        {
            return Fyk;
        }

        public double CalculateUltimateDesignStrainRebar(Standards.StandardACI318 standard)
        {
            return StrainU;
        }

        public double CalculateDesignYieldingStress(Standards.StandardACI318 standard)
        {
            return Fyk;
        }

        public double CalculateDesignYieldingStrainRebar(Standards.StandardACI318 standard)
        {
            return CalculateDesignYieldingStress(standard) / ElasticModulusCompression;
        }

        public double CalculateDesignUltimateStrainRebar(Standards.StandardACI318 standard)
        {
            return StrainU;
        }

        /// <returns>The design rebar stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStressRebar(Standards.StandardACI318 standard, double strain, double epsilonP = 0)
        {
            return GetStress(strain + epsilonP);
        }

        #endregion

        public override bool Equals(object obj)
        {
            return obj is SteelMaterial material &&
                   base.Equals(obj) &&
                   _fyk == material._fyk &&
                   _fu == material._fu &&
                   _strainU == material._strainU &&
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
				hashCode = hashCode * -17 + _strainU.GetHashCode();
				hashCode = hashCode * -17 + _steelType.GetHashCode();
				return hashCode; 
			}
        }

        #endregion
    }
}
