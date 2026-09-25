using GPC.Model.Standards;
using GPC.Utilities.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// A steel material (rebars, tendons, structural steel, bars, bolts) defined by the elastic modulus, the characteristic yield strength and
    /// the ultimate strength and strain, with the design stresses and strains of the supported standards (Model Code 2010, ACI 318, EN 1993-1-1,
    /// AISC 360)
    /// </summary>
    [Serializable]
    [UI(Description = "Steel", Group = "Materials", Kind = "Material")]
    public class SteelMaterial : Material
    {
        #region Public Enum        

        /// <summary>
        /// The kinds of steel
        /// </summary>
        public enum SteelTypes
        {
            /// <summary>Not defined</summary>
            Undefined,
            /// <summary>Reinforcing bars of concrete</summary>
            [Description("Rebar steel material")] Rebar,
            /// <summary>Prestressing tendons</summary>
            [Description("Tendon steel material")] Tendon,
            /// <summary>Structural steel</summary>
            [Description("Structural steel material")] Structural,
            /// <summary>Bars</summary>
            [Description("Bars steel material")] Bars,
            /// <summary>Bolts</summary>
            [Description("Bolt steel material")] Bolt,
        }

        /// <summary>
        /// The shapes of the stress-strain curve
        /// </summary>
        public enum StressStrainCurveType
        {
            /// <summary>
            /// Elastic and perfect plastic without hardening/softening.
            /// Elastic up to Fyk and then constant with Fyk value until rupture.
            /// </summary>
            ElasticPerfectPlastic = 0,
            /// <summary>
            /// Elastic and then hardening.
            /// Elastic up to Fyk and then rupture at Fu.
            /// </summary>
            ElasticHardening = 1,
            /// <summary>
            /// Generic curve: elastic up to the yield stress <see cref="Material.StressYTension"/>, then linear to the ultimate stress
            /// <see cref="Material.StressUTension"/> (hardening or softening)
            /// </summary>
            Generic = 2
        }

        #endregion

        #region Variables

        /// <summary>
        /// The characteristic yield strength
        /// </summary>
        protected double _fyk;
        /// <summary>
        /// The ultimate strength
        /// </summary>
        protected double _fu;
        /// <summary>
        /// The kind of steel
        /// </summary>
        protected SteelTypes _steelType;
        /// <summary>
        /// The shape of the stress-strain curve
        /// </summary>
        protected StressStrainCurveType _stressStrainCurveType;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic yield strength. The setter rebuilds the stress-strain tables, but then the value is taken back from
        /// <see cref="Material.StressYTension"/>, which the setter does not change (see the list of the defects found)
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
        /// Ultimate strength. The setter rebuilds the stress-strain tables, but then the value is taken back from <see cref="Material.StressUTension"/>
        /// (see <see cref="Fyk"/>)
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
        /// Strain hardening modulus: (fu - fyk) / (εu - εy); 0 if fu = fyk
        /// </summary>
        public double Et => GetEt();

        /// <summary>
        /// Type of steel
        /// </summary>
        public SteelTypes SteelType
        {
            get => _steelType;
            set => _steelType = value;
        }

        /// <summary>
        /// The shape of the stress-strain curve; the setter rebuilds the tables
        /// </summary>
        public StressStrainCurveType StressStrainCurve
        {
            get => _stressStrainCurveType;
            set
            {
                if (value != _stressStrainCurveType)
                {
                    _stressStrainCurveType = value;
                    RecalculateMechanicalProperties();
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a steel material with the tables of the given curve (read only, according to the standard)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="strainU">Ultimate strain</param>
        /// <param name="stressStrainCurveType">The shape of the stress-strain curve</param>
        /// <param name="steelType">The kind of steel</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public SteelMaterial(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticPerfectPlastic,
            SteelTypes steelType = SteelTypes.Undefined, double poisson = 0.30, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
            : this(name, elasticModulus, poisson, fyk, fu, strainU, stressStrainCurveType, steelType, density, alfaThermalExpansion)
        {

        }

        /// <summary>
        /// Creates a steel S235 (E = 210000, fyk = 235, fu = 360, εu = 0.1, elastic perfectly plastic)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="steelType">The kind of steel</param>
        public SteelMaterial(string name, SteelTypes steelType)
            : this(name, 210000, 235, 360, 0.1, StressStrainCurveType.ElasticPerfectPlastic, steelType)
        {
        }

        /// <summary>
        /// Creates a steel material from all the properties; fyk and fu are the yield and ultimate stresses in tension
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulusCompression">The elastic modulus in compression</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="strainYCompression">The strain at the yield stress in compression</param>
        /// <param name="strainUCompression">The ultimate strain in compression</param>
        /// <param name="strainYTension">The strain at the yield stress in tension</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="stressYCompression">The yield stress in compression</param>
        /// <param name="stressUCompression">The ultimate stress in compression</param>
        /// <param name="stressYTension">The yield stress in tension</param>
        /// <param name="stressUTension">The ultimate stress in tension</param>
        /// <param name="stressStrainTableCompression">The characteristic stress-strain table in compression</param>
        /// <param name="stressStrainTableTensio">The characteristic stress-strain table in tension</param>
        /// <param name="stressStrainCurveType">Not used (the curve type keeps its default value)</param>
        /// <param name="steelType">The kind of steel</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density (passed to the base constructor as thermal expansion: see the list of the defects found)</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion (passed as density)</param>
        public SteelMaterial(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio,
            StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticHardening, SteelTypes steelType = SteelTypes.Undefined,
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
        /// Protected steelMaterial constructor: the tables are built from the curve type, fyk, fu and εu
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poisson's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="strainU">The ultimate strain</param>
        /// <param name="stressStrainCurveType">The shape of the stress-strain curve</param>
        /// <param name="steelType">Type of steel</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expansion coefficient</param>
        protected SteelMaterial(string name, double elasticModulus, double poisson, double fyk,
            double fu, double strainU, StressStrainCurveType stressStrainCurveType, SteelTypes steelType, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion)
        {
            _fu = Math.Abs(fu);
            _fyk = Math.Abs(fyk);
            _strainUTension = Math.Abs(strainU);
            _strainUCompression = -Math.Abs(strainU);
            _steelType = steelType;
            _stressStrainCurveType = stressStrainCurveType;

            SetDefaultMechanicalProperties();
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Material"/>, fyk, fu, the kind of steel and (version 3) the curve type; the
        /// tables are rebuilt
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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
            if (version <= 2)
            {
                _stressStrainCurveType = StressStrainCurveType.ElasticPerfectPlastic;
            }
            else
            {
                _stressStrainCurveType = (StressStrainCurveType)info.GetInt32("StressStrainCurveType");
            }
            RecalculateMechanicalProperties();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Rebuilds the stress-strain tables (see <see cref="SetStressStrain"/>) and the yield strains; fyk and fu are taken from the yield and
        /// ultimate stresses in tension
        /// </summary>
        public virtual void RecalculateMechanicalProperties()
        {
            SetStressStrain();

            _strainYTension = _stressYTension / _elasticModulusTension;
            _strainYCompression = _stressYCompression / _elasticModulusCompression;

            _fu = _stressUTension;
            _fyk = _stressYTension;
        }

        /// <summary>
        /// Builds the tables and sets yield strains and yield and ultimate stresses from fyk, fu and E (compression: the opposite values)
        /// </summary>
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

        /// <summary>
        /// Builds the characteristic stress-strain tables from the curve type: elastic perfectly plastic (fyk up to εu), hardening (fyk to fu at εu)
        /// or generic (from the yield and ultimate stresses)
        /// </summary>
        public virtual void SetStressStrain()
        {
            switch (_stressStrainCurveType)
            {
                case StressStrainCurveType.ElasticPerfectPlastic:
                case StressStrainCurveType.ElasticHardening:
                    {
                        double fRupture;
                        if (_stressStrainCurveType == StressStrainCurveType.ElasticPerfectPlastic)
                            fRupture = _fyk;
                        else
                            fRupture = _fu;

                        _stressStrainTableCompression = new StressStrainTable(
                            new double[] { 0, -_fyk, -fRupture },
                            new double[] { 0, -_fyk / _elasticModulusCompression, _strainUCompression });
                        _stressStrainTableTension = new StressStrainTable(
                            new double[] { 0, _fyk, fRupture },
                            new double[] { 0, _fyk / _elasticModulusTension, _strainUTension });
                        return;
                    }
                case StressStrainCurveType.Generic:
                    _stressStrainTableCompression = new StressStrainTable(
                        new double[] { 0, _stressYCompression, _stressUCompression },
                        new double[] { 0, _stressYCompression / _elasticModulusCompression, _strainUCompression });
                    _stressStrainTableTension = new StressStrainTable(
                        new double[] { 0, _stressYTension, _stressUTension },
                        new double[] { 0, _stressYTension / _elasticModulusTension, _strainUTension });
                    return;
                default:
                    return;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// The strain hardening modulus
        /// </summary>
        /// <returns>(fu - fyk) / (εu - εy); 0 if fu = fyk</returns>
        protected double GetEt()
        {
            if (Math.Abs(Fu - Fyk) < GPC.Geometry.GeometryBase.GetDefaultTolerance())
                return 0.0;
            else
                return (Fu - Fyk) / (StrainUTension - StrainYTension);
        }

        #endregion

        #region Public Standard Methods

        /// <summary>
        /// The design stress for a strain, according to the standard
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010, ACI 318, EN 1993-1-1 or AISC 360)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain (tendons)</param>
        /// <returns>The design stress; 0 for the other standards</returns>
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
                case Standards.StandardAISC aisc:
                    return CalculateDesignStress(aisc, strain, epsilonP);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The design strain for a strain, according to the standard (the strain itself for the supported standards)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010, ACI 318, EN 1993-1-1 or AISC 360)</param>
        /// <param name="strain">The strain</param>
        /// <returns>The design strain; 0 for the other standards</returns>
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
                case Standards.StandardAISC aisc:
                    return CalculateDesignStrain(aisc, strain);
                default:
                    return 0;
            }
        }

        #region ModelCode2010

        /// <summary>
        /// The design yield strength: fyk / γs
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design rebar yielding stress</returns>
        public double CalculateFyd(Standards.StandardModelCode2010 standard)
        {
            return Fyk / standard.GammaS;
        }

        /// <summary>
        /// The design yield stress in tension: fy / γs (γs of prestress for the tendons)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design yield stress</returns>
        /// <exception cref="Exception">For bolts and undefined steel</exception>
        public double CalculateDesignYieldingStressTension(Standards.StandardModelCode2010 standard)
        {
            if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars || SteelType == SteelTypes.Structural)
                return StressYTension / standard.GammaS;
            else if (SteelType == SteelTypes.Tendon)
                return StressYTension / standard.GammaSPrestress;
            else
                throw new Exception();
        }

        /// <summary>
        /// The design yield stress in compression: fy / γs (γs of prestress for the tendons)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design yield stress (negative)</returns>
        /// <exception cref="Exception">For bolts and undefined steel</exception>
        public double CalculateDesignYieldingStressCompression(Standards.StandardModelCode2010 standard)
        {
            if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars || SteelType == SteelTypes.Structural)
                return StressYCompression / standard.GammaS;
            else if (SteelType == SteelTypes.Tendon)
                return StressYCompression / standard.GammaSPrestress;
            else
                throw new Exception();
        }

        /// <summary>
        /// The design yield strain in tension: design yield stress / E
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design yield strain</returns>
        public double CalculateDesignYieldingStrainTension(Standards.StandardModelCode2010 standard)
        {
            return CalculateDesignYieldingStressTension(standard) / ElasticModulusTension;
        }

        /// <summary>
        /// The design yield strain in compression: design yield stress / E
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design yield strain (negative)</returns>
        public double CalculateDesignYieldingStrainCompression(Standards.StandardModelCode2010 standard)
        {
            return CalculateDesignYieldingStressCompression(standard) / ElasticModulusCompression;
        }

        /// <summary>
        /// The design ultimate strain in tension: εu × the coefficient of the standard
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design ultimate strain</returns>
        /// <exception cref="Exception">For bolts and undefined steel</exception>
        public double CalculateDesignUltimateStrainTension(Standards.StandardModelCode2010 standard)
        {
            if (SteelType == SteelTypes.Rebar || SteelType == SteelTypes.Bars || SteelType == SteelTypes.Tendon || SteelType == SteelTypes.Structural)
                return StrainUTension * standard.SteelCoefficientStrainTension;
            else
                throw new Exception();
        }

        /// <summary>
        /// The design ultimate strain in compression: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The ultimate strain in compression</returns>
        public double CalculateDesignUltimateStrainCompression(Standards.StandardModelCode2010 standard)
        {
            return StrainUCompression;
        }

        /// <summary>
        /// The design strain: the strain itself
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <param name="strain">The strain</param>
        /// <returns><paramref name="strain"/></returns>
        public double CalculateDesignStrain(Standards.StandardModelCode2010 standardModelCode2010, double strain)
        {
            return strain;
        }

        /// <summary>
        /// The design stress: the characteristic curve in the elastic range, then the characteristic curve lowered by fyk - fyd (with the strain shifted
        /// by the same difference over E), zero beyond the ultimate strain
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design rebar stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStress(Standards.StandardModelCode2010 standard, double strain, double epsilonP = 0)
        {
            double fyd = CalculateDesignYieldingStressTension(standard);
            double strainYd = CalculateDesignYieldingStrainTension(standard);

            return CalculateDesignStressCommon(strain, epsilonP, fyd, strainYd);
        }

        /// <summary>
        /// The design stress from a characteristic stress: the stress itself in the elastic range, the stress lowered by fy - fyd beyond it, zero beyond
        /// the ultimate strain
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design stress</returns>
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

        /// <summary>
        /// The limit stress of the steel for the characteristic combination (serviceability): fyk × the coefficient of the standard
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The limit stress</returns>
        public virtual double GetServiceabilityCharacteristicStress(StandardModelCode2010 standardModelCode2010)
        {            
            return Fyk * standardModelCode2010.ServiceabilityStressSteelCoefficientForCharacteristicCombination;
        }

        /// <summary>
        /// The limit stress of the prestressing steel for the characteristic combination (serviceability): fyk × the coefficient of the standard
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The limit stress</returns>
        public virtual double GetServiceabilityCharacteristicStressPrestress(StandardModelCode2010 standardModelCode2010)
        {
            return Fyk * standardModelCode2010.ServiceabilityStressPrestressSteelCoefficientForCharacteristicCombination;
        }

        #endregion

        #region ACI318

        /// <summary>
        /// The design yield strength: fyk (ACI 318 applies the reduction factor to the resistance, not to the material)
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The design rebar yielding stress</returns>
        public double CalculateFyd(Standards.StandardACI318 standard)
        {
            return Fyk;
        }

        /// <summary>
        /// The design yield stress: fyk
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The design yield stress</returns>
        public double CalculateDesignYieldingStress(Standards.StandardACI318 standard)
        {
            return Fyk;
        }

        /// <summary>
        /// The design yield strain: fyk / E
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The design yield strain</returns>
        public double CalculateDesignYieldingStrain(Standards.StandardACI318 standard)
        {
            return CalculateDesignYieldingStress(standard) / ElasticModulusTension;
        }

        /// <summary>
        /// The design ultimate strain in tension: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The ultimate strain in tension</returns>
        public double CalculateDesignUltimateStrainTension(Standards.StandardACI318 standard)
        {
            return StrainUTension;
        }

        /// <summary>
        /// The design ultimate strain in compression: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The ultimate strain in compression</returns>
        public double CalculateDesignUltimateStrainCompression(Standards.StandardACI318 standard)
        {
            return StrainUCompression;
        }

        /// <summary>
        /// The design strain: the strain itself
        /// </summary>
        /// <param name="standardACI318">The standard (ACI 318)</param>
        /// <param name="strain">The strain</param>
        /// <returns><paramref name="strain"/></returns>
        public double CalculateDesignStrain(Standards.StandardACI318 standardACI318, double strain)
        {
            return strain;
        }

        /// <summary>
        /// The design stress: the characteristic stress at strain + prestrain
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design rebar stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStress(Standards.StandardACI318 standard, double strain, double epsilonP = 0)
        {
            return GetStress(strain + epsilonP);
        }

        /// <summary>
        /// The design stress from a characteristic stress: the stress itself
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <param name="strain">Not used</param>
        /// <param name="epsilonP">Not used</param>
        /// <returns><paramref name="stress"/></returns>
        public double CalculateDesignStress(Standards.StandardACI318 standard, double stress, double strain, double epsilonP = 0)
        {
            return stress;
        }

        /// <summary>
        /// The design stress from a characteristic stress, according to the standard
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010, ACI 318, EN 1993-1-1 or AISC 360)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design stress; 0 for the other standards</returns>
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
                case Standards.StandardAISC aisc:
                    return CalculateDesignStress(aisc, stress, strain, epsilonP);
                default:
                    return 0;
            }
        }

        #endregion

        #region Eurocode 3

        /// <summary>
        /// The design yield strength: fyk / γM0
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The design steel yielding stress</returns>
        public double CalculateFyd(Standards.StandardEN1993p11 standard)
        {
            return Fyk / standard.GammaM0;
        }

        /// <summary>
        /// The design yield stress in tension: fy / γM0 (γM2 for bolts)
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The design yield stress</returns>
        /// <exception cref="Exception">For steels other than structural and bolts</exception>
        public double CalculateDesignYieldingStressTension(Standards.StandardEN1993p11 standard)
        {
            if (SteelType == SteelTypes.Structural)
                return StressYTension / standard.GammaM0;
            else if (SteelType == SteelTypes.Bolt)
                return StressYTension / standard.GammaM2;
            else
                throw new Exception();
        }

        /// <summary>
        /// The design yield stress in compression: fy / γM0 (γM2 for bolts)
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The design yield stress (negative)</returns>
        /// <exception cref="Exception">For steels other than structural and bolts</exception>
        public double CalculateDesignYieldingStressCompression(Standards.StandardEN1993p11 standard)
        {
            if (SteelType == SteelTypes.Structural)
                return StressYCompression / standard.GammaM0;
            else if (SteelType == SteelTypes.Bolt)
                return StressYCompression / standard.GammaM2;
            else
                throw new Exception();
        }

        /// <summary>
        /// The design yield strain in tension: design yield stress / E
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The design yield strain</returns>
        public double CalculateDesignYieldingStrainTension(Standards.StandardEN1993p11 standard)
        {
            return CalculateDesignYieldingStressTension(standard) / ElasticModulusTension;
        }

        /// <summary>
        /// The design yield strain in compression: design yield stress / E
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The design yield strain (negative)</returns>
        public double CalculateDesignYieldingStrainCompression(Standards.StandardEN1993p11 standard)
        {
            return CalculateDesignYieldingStressCompression(standard) / ElasticModulusCompression;
        }

        /// <summary>
        /// The design ultimate strain in tension: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The ultimate strain in tension</returns>
        /// <exception cref="Exception">For steels other than structural and bolts</exception>
        public double CalculateDesignUltimateStrainTension(Standards.StandardEN1993p11 standard)
        {
            if (SteelType == SteelTypes.Structural)
                return StrainUTension;
            else if (SteelType == SteelTypes.Bolt)
                return StrainUTension;
            else
                throw new Exception();
        }

        /// <summary>
        /// The design ultimate strain in compression: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The ultimate strain in compression</returns>
        public double CalculateDesignUltimateStrainCompression(Standards.StandardEN1993p11 standard)
        {
            return StrainUCompression;
        }

        /// <summary>
        /// The design strain: the strain itself
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <param name="strain">The strain</param>
        /// <returns><paramref name="strain"/></returns>
        public double CalculateDesignStrain(Standards.StandardEN1993p11 standard, double strain)
        {
            return strain;
        }

        /// <summary>
        /// The design stress (as for Model Code 2010, see <see cref="CalculateDesignStress(StandardModelCode2010, double, double)"/>)
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design steel stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStress(Standards.StandardEN1993p11 standard, double strain, double epsilonP = 0)
        {
            double fyd = CalculateDesignYieldingStressTension(standard);
            double strainYd = CalculateDesignYieldingStrainTension(standard);

            return CalculateDesignStressCommon(strain, epsilonP, fyd, strainYd);
        }

        /// <summary>
        /// The design stress: the characteristic curve in the elastic range (|ε + εp| up to the design yield strain), then the characteristic curve at
        /// the strain increased by (fyk - fyd) / E and lowered by fyk - fyd, limited at the ultimate strain in tension; zero beyond it
        /// </summary>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <param name="fyd">The design yield stress</param>
        /// <param name="strainYd">The design yield strain</param>
        /// <returns>The design stress</returns>
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

        /// <summary>
        /// The design stress from a characteristic stress (as for Model Code 2010)
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design stress</returns>
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

        #region AISC 360

        /// <summary>
        /// The design yield strength: fyk (AISC 360 applies the resistance factor to the resistance)
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <returns>The design steel yielding stress</returns>
        public double CalculateFyd(Standards.StandardAISC standard)
        {
            return Fyk;
        }

        /// <summary>
        /// The design yield stress in tension: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <returns>The yield stress in tension</returns>
        public double CalculateDesignYieldingStressTension(Standards.StandardAISC standard)
        {
            return StressYTension;
        }

        /// <summary>
        /// The design yield stress in compression: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <returns>The yield stress in compression</returns>
        public double CalculateDesignYieldingStressCompression(Standards.StandardAISC standard)
        {
            return StressYCompression;
        }

        /// <summary>
        /// The design yield strain in tension: yield stress / E
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <returns>The yield strain</returns>
        public double CalculateDesignYieldingStrainTension(Standards.StandardAISC standard)
        {
            return CalculateDesignYieldingStressTension(standard) / ElasticModulusTension;
        }

        /// <summary>
        /// The design yield strain in compression: yield stress / E
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <returns>The yield strain (negative)</returns>
        public double CalculateDesignYieldingStrainCompression(Standards.StandardAISC standard)
        {
            return CalculateDesignYieldingStressCompression(standard) / ElasticModulusCompression;
        }

        /// <summary>
        /// The design ultimate strain in tension: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <returns>The ultimate strain in tension</returns>
        public double CalculateDesignUltimateStrainTension(Standards.StandardAISC standard)
        {
            return StrainUTension;
        }

        /// <summary>
        /// The design ultimate strain in compression: the characteristic one
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <returns>The ultimate strain in compression</returns>
        public double CalculateDesignUltimateStrainCompression(Standards.StandardAISC standard)
        {
            return StrainUCompression;
        }

        /// <summary>
        /// The design strain: the strain itself
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <param name="strain">The strain</param>
        /// <returns><paramref name="strain"/></returns>
        public double CalculateDesignStrain(Standards.StandardAISC standard, double strain)
        {
            return strain;
        }

        /// <summary>
        /// The design stress (as for Model Code 2010, with fyd = fy: the characteristic curve)
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design steel stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStress(Standards.StandardAISC standard, double strain, double epsilonP = 0)
        {
            double fyd = CalculateDesignYieldingStressTension(standard);
            double strainYd = CalculateDesignYieldingStrainTension(standard);

            return CalculateDesignStressCommon(strain, epsilonP, fyd, strainYd);
        }

        /// <summary>
        /// The design stress from a characteristic stress (as for Model Code 2010, with fyd = fy)
        /// </summary>
        /// <param name="standard">The standard (AISC 360)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design stress</returns>
        public double CalculateDesignStress(Standards.StandardAISC standard, double stress, double strain, double epsilonP = 0)
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

        /// <summary>
        /// Serializes the data of <see cref="Material"/>, fyk, fu, the kind of steel and the curve type (version 3)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 3;

            info.AddValue("SteelMaterialVersion", version);

            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
            info.AddValue("SteelType", _steelType);
            info.AddValue("StressStrainCurveType", _stressStrainCurveType);
        }

        /// <summary>
        /// Equality of the data of <see cref="Material"/>, fyk, fu and kind of steel
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal steel material</returns>
        public override bool Equals(object obj)
        {
            return obj is SteelMaterial material &&
                   base.Equals(obj) &&
                   _fyk == material._fyk &&
                   _fu == material._fu &&
                   _steelType == material._steelType;
        }

        /// <summary>
        /// The hash code of the data of <see cref="Material"/>, fyk, fu and kind of steel
        /// </summary>
        /// <returns>The hash code</returns>
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
