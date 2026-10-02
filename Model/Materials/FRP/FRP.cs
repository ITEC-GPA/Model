using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// A fiber reinforced polymer (FRP): tension only (the compression table is zero), defined by E, fyk, fu and εu, with the design stresses of
    /// Model Code 2010, ACI 318 and EN 1993-1-1 computed as for <see cref="SteelMaterial"/>
    /// </summary>
    [Serializable]
    [UI(Description = "Steel", Group = "Materials", Kind = "Material")]
    public class FRP : Material
    {
        #region Public Enum        

        /// <summary>
        /// The shapes of the tension curve
        /// </summary>
        public enum StressStrainCurveType
        {
            /// <summary>Not defined (the tables are not rebuilt)</summary>
            Undefined = 0,
            /// <summary>Elastic up to fyk, then constant fyk up to εu (elastic perfectly plastic)</summary>
            Linear = 1,
            /// <summary>Elastic up to fyk, then linear to fu at εu</summary>
            Bilinear = 2,
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
        /// The shape of the tension curve
        /// </summary>
        protected StressStrainCurveType _stressStrainCurveType;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic yield strength (the setter has the defect of <see cref="SteelMaterial.Fyk"/>: the value is taken back from the stresses)
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
        /// Ultimate strength (the setter has the defect of <see cref="SteelMaterial.Fyk"/>)
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
        /// The shape of the tension curve; the setter rebuilds tables and stresses from fyk and fu
        /// </summary>
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

        /// <summary>
        /// Creates an FRP (read only, according to the standard)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">FRP elastic modulus</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="strainU">Ultimate strain</param>
        /// <param name="stressStrainCurveType">The shape of the tension curve</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density (the default is the one of the steel)</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public FRP(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.Linear,
            double poisson = 0.30, double density = SteelDensity, double alfaThermalExpansion = 12 * 1e-6)
            : this(name, elasticModulus, poisson, fyk, fu, strainU, stressStrainCurveType, density, alfaThermalExpansion)
        {

        }

        /// <summary>
        /// Creates an FRP with the values of a steel S235 (E = 210000, fyk = 235, fu = 360, εu = 0.1)
        /// </summary>
        /// <param name="name">The name</param>
        public FRP(string name)
            : this(name, 210000, 235, 360, 0.1, StressStrainCurveType.Linear)
        {
        }

        /// <summary>
        /// Creates an FRP from all the properties; fyk and fu are the yield and ultimate stresses in tension
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
        /// <param name="stressStrainTableCompression">The table in compression</param>
        /// <param name="stressStrainTableTensio">The table in tension</param>
        /// <param name="stressStrainCurveType">Not used</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density, t/mm³</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public FRP(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio,
            StressStrainCurveType stressStrainCurveType = StressStrainCurveType.Bilinear,
             double poisson = 0.30, double density = SteelDensity, double alfaThermalExpansion = 12 * 1e-6)
            : base(name, elasticModulusCompression, elasticModulusTension,
            strainYCompression, strainUCompression, strainYTension, strainUTension,
            stressYCompression, stressUCompression, stressYTension, stressUTension,
            stressStrainTableCompression, stressStrainTableTensio, poisson, alfaThermalExpansion, density)
        {
            _fyk = stressYTension;
            _fu = stressUTension;
        }

        /// <summary>
        /// Protected constructor: the tables are built from the curve type, fyk, fu and εu
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Elastic modulus</param>
        /// <param name="poisson">Poisson's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="strainU">The ultimate strain</param>
        /// <param name="stressStrainCurveType">The shape of the tension curve</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expansion coefficient</param>
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

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Material"/>, fu, fyk and the curve type; the tables are rebuilt
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Rebuilds the tables and the yield strains; fyk and fu are taken from the yield and ultimate stresses in tension
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
        /// Builds the tables and sets yield strains and yield and ultimate stresses from fyk, fu and E
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
        /// Builds the characteristic tables of the curve type (compression: zero)
        /// </summary>
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
        /// <param name="standard">The standard (Model Code 2010, ACI 318 or EN 1993-1-1)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
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
                default:
                    return 0;
            }
        }

        /// <summary>
        /// The design strain for a strain (the strain itself for the supported standards)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010, ACI 318 or EN 1993-1-1)</param>
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
                default:
                    return 0;
            }
        }

        #region ModelCode2010

        /// <summary>
        /// The design yield strength: fyk / γs
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design yielding stress</returns>
        public double CalculateFyd(Standards.StandardModelCode2010 standard)
        {
            return Fyk / standard.GammaS;
        }

        /// <summary>
        /// The design yield stress in tension: fy / γs
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design yield stress</returns>
        public double CalculateDesignYieldingStressTension(Standards.StandardModelCode2010 standard)
        {
            return StressYTension / standard.GammaS;
        }

        /// <summary>
        /// The design yield stress in compression: fy / γs
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design yield stress (negative)</returns>
        public double CalculateDesignYieldingStressCompression(Standards.StandardModelCode2010 standard)
        {
            return StressYCompression / standard.GammaS;
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
        /// The design ultimate strain: εu × the coefficient of the standard
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design ultimate strain</returns>
        public double CalculateDesignUltimateStrain(Standards.StandardModelCode2010 standard)
        {
            return StrainUTension * standard.SteelCoefficientStrainTension;
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
        /// The design stress (see <see cref="SteelMaterial.CalculateDesignStress(Standards.StandardModelCode2010, double, double)"/>)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStress(Standards.StandardModelCode2010 standard, double strain, double epsilonP = 0)
        {
            double fyd = CalculateDesignYieldingStressTension(standard);
            double strainYd = CalculateDesignYieldingStrainTension(standard);

            return CalculateDesignStressCommon(strain, epsilonP, fyd, strainYd);
        }

        /// <summary>
        /// The design stress from a characteristic stress: the stress itself in the elastic range, lowered by fy - fyd beyond it, zero beyond εu
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

        #endregion

        #region ACI318

        /// <summary>
        /// The design yield strength: fyk
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The design yielding stress</returns>
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
        /// The design ultimate strain: εu
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The ultimate strain</returns>
        public double CalculateDesignUltimateStrain(Standards.StandardACI318 standard)
        {
            return StrainUTension;
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
        /// <returns>The design stress related to <paramref name="strain"/></returns>
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
        /// <param name="standard">The standard (Model Code 2010, ACI 318 or EN 1993-1-1)</param>
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
        /// <returns>The design yielding stress</returns>
        public double CalculateFyd(Standards.StandardEN1993p11 standard)
        {
            return Fyk / standard.GammaM0;
        }

        /// <summary>
        /// The design yield stress in tension: fy / γM0
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The design yield stress</returns>
        public double CalculateDesignYieldingStressTension(Standards.StandardEN1993p11 standard)
        {
            return StressYTension / standard.GammaM0;
        }

        /// <summary>
        /// The design yield stress in compression: fy / γM0
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The design yield stress (negative)</returns>
        public double CalculateDesignYieldingStressCompression(Standards.StandardEN1993p11 standard)
        {
            return StressYCompression / standard.GammaM0;
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
        /// The design ultimate strain: εu
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <returns>The ultimate strain</returns>
        public double CalculateDesignUltimateStrain(Standards.StandardEN1993p11 standard)
        {
            return StrainUTension;
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
        /// The design stress (as for Model Code 2010)
        /// </summary>
        /// <param name="standard">The standard (EN 1993-1-1)</param>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="epsilonP">The prestrain</param>
        /// <returns>The design stress related to <paramref name="strain"/></returns>
        public double CalculateDesignStress(Standards.StandardEN1993p11 standard, double strain, double epsilonP = 0)
        {
            double fyd = CalculateDesignYieldingStressTension(standard);
            double strainYd = CalculateDesignYieldingStrainTension(standard);

            return CalculateDesignStressCommon(strain, epsilonP, fyd, strainYd);
        }

        /// <summary>
        /// The design stress: the characteristic curve in the elastic range, then the curve at the strain increased by (fyk - fyd) / E and lowered
        /// by fyk - fyd, limited at εu; zero beyond εu
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

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Serializes the data of <see cref="Material"/>, fyk, fu and the curve type (version 1)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 1;

            info.AddValue("FRPMaterialVersion", version);

            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
            info.AddValue("StressStrainCurveType", _stressStrainCurveType);
        }

        /// <summary>
        /// Equality of the data of <see cref="Material"/>, fyk and fu
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal FRP</returns>
        public override bool Equals(object obj)
        {
            return obj is FRP material &&
                   base.Equals(obj) &&
                   _fyk == material._fyk &&
                   _fu == material._fu;
        }

        /// <summary>
        /// The hash code of the data of <see cref="Material"/>, fyk and fu
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
                return hashCode;
            }
        }

        #endregion
    }
}
