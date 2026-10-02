using GPC.Model.Standards;
using GPC.Utilities.Maths;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Base of the European concretes (Model Code 2010, EN 1992-1-1): the properties are derived from fck with the formulas of fib Model Code 2010
    /// and EN 1992-1-1 (fcm, fctm, Ecm, strains of the diagrams). fck and the stresses in compression are negative
    /// </summary>
    [Serializable]
    public abstract class ConcreteMaterialEuropeanCommon : ConcreteMaterial, ISerializable
    {
        #region Variables 

        /// <summary>
        /// The characteristic compressive strength (negative)
        /// </summary>
        protected double _fck;
        /// <summary>
        /// The characteristic tensile strength (the peak of the tension diagram)
        /// </summary>
        protected double _fctk;
        /// <summary>
        /// The ultimate (residual) tensile strength
        /// </summary>
        protected double _fctu;

        /// <summary>
        /// The class of cement
        /// </summary>
        protected CementTypes _cementType;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic compressive cylinder strength of concrete at 28 days (negative); the setter recalculates the mechanical properties
        /// </summary>
        public double Fck
        {
            get => _fck;
            set
            {
                if (_fck != value)
                {
                    _fck = value;
                    RecalculateMechanicalProperties();
                }
            }
        }

        /// <summary>
        /// Characteristic tensile strength of concrete: the peak of the tension diagram (fctk,0.05 for the plain concrete)
        /// </summary>
        public double Fctk { get => _fctk; set => _fctk = value; }

        /// <summary>
        /// Ultimate (residual) tensile strength: the last stress of the tension diagram
        /// </summary>
        public double Fctu { get => _fctu; set => _fctu = value; }

        /// <summary>
        /// Type of cement
        /// </summary>
        public CementTypes CementType { get => _cementType; set => _cementType = value; }

        /// <summary>
        /// Mean compressive strength at 28 days
        /// </summary>
        public double Fcm => GetFcm();

        /// <summary>
        /// Mean tensile strength: 0.3 fck^(2/3) up to C50, 2.12 ln(1 + fcm / 10) beyond
        /// </summary>
        public double Fctm => GetFctm();

        /// <summary>
        /// Characteristic tensile strength, 95% fractile: 1.3 fctm
        /// </summary>
        public double Fctk95 => GetFctk95();

        /// <summary>
        /// Characteristic tensile strength, 5% fractile: 0.7 fctm
        /// </summary>
        public double Fctk05 => GetFctk05();

        /// <summary>
        /// Secant modulus of elasticity of concrete: 22000 (fcm / 10)^0.3 [MPa]
        /// </summary>
        public double Ecm => GetEcm();

        /// <summary>
        /// Strain in the concrete for the pure compression case (for the stress block, the one of the parabola-rectangle)
        /// </summary>
        public double StrainYPureCompression => GetStrainYPureCompression(CompressionStressStrainDiagram);

        /// <summary>
        /// Tangent modulus of elasticity: 1.05 Ecm
        /// </summary>
        public double Ec => 1.05 * ElasticModulusCompression;

        /// <summary>
        /// Characteristic compressive cubic strength of concrete at 28 days (table of the strength classes; 1 / 0.83 fck for the other values), with
        /// the sign of fck (negative, as <see cref="Fcm"/>)
        /// </summary>
        public double Rck => GetFckCube(Fck);

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a plain concrete from fck (tension: linear up to fctk,0.05)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="fck">The characteristic compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion (the default 1E-6 is ten times smaller than the 1E-5 of EN 1992-1-1 3.1.3)</param>
        /// <param name="cementType">The class of cement</param>
        public ConcreteMaterialEuropeanCommon(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams, ConcreteTypes concreteType,
            double poisson = 0.2, double density = ConcreteDensity, double alfaThermalExpansion = 10e-6, CementTypes cementType = CementTypes.ClassN)
            : base(name, poisson, density, alfaThermalExpansion)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = TensionStressStrainDiagrams.Linear;
            _concreteType = concreteType;

            SetMechanicalProperties(-Math.Abs(fck), 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

            SetStressProperties();

            _cementType = cementType;
        }

        /// <summary>
        /// Creates a fiber reinforced concrete from fck and the residual tensile strengths
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="fck">The characteristic compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="ffts">The serviceability residual strength (the peak of the tension diagram)</param>
        /// <param name="fFtu">The ultimate residual strength</param>
        /// <param name="strainYTension">The strain at the peak (0: ffts / E)</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="tensionStressStrainDiagrams">The diagram in tension</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="cementType">The class of cement</param>
        public ConcreteMaterialEuropeanCommon(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, TensionStressStrainDiagrams tensionStressStrainDiagrams, ConcreteTypes concreteType,
            double poisson = 0.2, double density = ConcreteDensity, double alfaThermalExpansion = 10e-6, CementTypes cementType = CementTypes.ClassN)
            : base(name, poisson, density, alfaThermalExpansion)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = tensionStressStrainDiagrams;
            _concreteType = concreteType;

            SetMechanicalProperties(-Math.Abs(fck), Math.Abs(ffts), Math.Abs(fFtu), Math.Abs(strainYTension), Math.Abs(strainUTension),
                _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

            SetStressProperties();

            _cementType = cementType;
        }

        /// <summary>
        /// Creates a concrete from generic stress-strain tables (fck: the minimum stress of the compression table)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="strainYTension">The strain at the tensile strength</param>
        /// <param name="strainYCompression">The strain at the peak compression (0: the strain of the minimum stress)</param>
        /// <param name="stressStrainTableCompression">The table in compression</param>
        /// <param name="stressStrainTableTension">The table in tension</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="cementType">The class of cement</param>
        public ConcreteMaterialEuropeanCommon(string name, double strainYTension, double strainYCompression,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson = 0.2, double density = ConcreteDensity, double alfaThermalExpansion = 10e-6,
            CementTypes cementType = CementTypes.ClassN)
            : base(name, stressStrainTableCompression, stressStrainTableTension, stressStrainTableCompression.GetElasticModulus(),
                  stressStrainTableTension.GetElasticModulus(), poisson, density, alfaThermalExpansion)
        {
            _compressionStressStrainDiagrams = CompressionStressStrainDiagrams.Generic;
            _tensionStressStrainDiagrams = TensionStressStrainDiagrams.Generic;
            _concreteType = concreteType;

            _stressStrainTableCompression = stressStrainTableCompression;
            _stressStrainTableTension = stressStrainTableTension;

            SetMechanicalProperties(stressStrainTableCompression.GetMinimumStress(), stressStrainTableTension.GetStress(strainYTension),
                stressStrainTableTension.GetLastStress(), strainYTension, stressStrainTableTension.GetLastStrain(),
                _compressionStressStrainDiagrams, _tensionStressStrainDiagrams, strainYCompression);

            SetStressProperties();

            _cementType = cementType;
        }

        /// <summary>
        /// Creates a concrete from all the properties
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulusCompression">The elastic modulus in compression</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="strainYCompression">The strain at the peak stress in compression</param>
        /// <param name="strainUCompression">The ultimate strain in compression</param>
        /// <param name="strainYTension">The strain at the tensile strength</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="stressYCompression">The peak stress in compression</param>
        /// <param name="stressUCompression">The ultimate stress in compression</param>
        /// <param name="stressYTension">The tensile strength</param>
        /// <param name="stressUTension">The ultimate stress in tension</param>
        /// <param name="stressStrainTableCompression">The table in compression</param>
        /// <param name="stressStrainTableTension">The table in tension</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="density">The density</param>
        protected ConcreteMaterialEuropeanCommon(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson, double alfaThermalExpansion, double density)
            : base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression,
                  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension,
                  stressStrainTableCompression, stressStrainTableTension, concreteType, poisson, alfaThermalExpansion, density)
        {
        }
        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="ConcreteMaterial"/>, fck, fctk, fctu and the class of cement (ClassN if missing)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ConcreteMaterialEuropeanCommon(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ConcreteMaterialEuropeanCommonVersion");
            }
            catch (Exception)
            {
                try
                {
                    version = info.GetInt32("MaterialVersion");
                }
                catch (Exception)
                {
                    version = 1;
                }
            }

            if (version == 2)
            {
                _compressionStressStrainDiagrams = (CompressionStressStrainDiagrams)info.GetInt32("CompressionStressStrainDiagrams");
                _tensionStressStrainDiagrams = (TensionStressStrainDiagrams)info.GetInt32("TensionStressStrainDiagrams");
            }
            else if (version == 1) { }
            else if (version == 3) { }

            _fck = info.GetDouble("Fck");
            _fctk = info.GetDouble("Fctk");
            _fctu = info.GetDouble("Fctu");
            try
            {
                _cementType = (CementTypes)info.GetValue("CementType", typeof(CementTypes));
            }
            catch (Exception)
            {
                _cementType = CementTypes.ClassN;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Characteristic tensile strength (5% fractile) at an age: 0.7 fctm(t)
        /// </summary>
        /// <param name="days">The age in days</param>
        /// <returns>fctk,0.05(t)</returns>
        public virtual double GetFctk05(double days)
        {
            return 0.7 * GetFctm(days);
        }

        /// <summary>
        /// Characteristic tensile strength (95% fractile) at an age: 1.3 fctm(t)
        /// </summary>
        /// <param name="days">The age in days</param>
        /// <returns>fctk,0.95(t)</returns>
        public virtual double GetFctk95(double days)
        {
            return 1.3 * GetFctm(days);
        }

        /// <summary>
        /// Secant modulus at an age: (fcm(t) / fcm)^0.3 Ecm
        /// </summary>
        /// <param name="fcm">The mean compressive strength at 28 days (with the sign of <see cref="Fcm"/>: a positive value with the negative fcm(t) gives NaN)</param>
        /// <param name="days">The age in days</param>
        /// <returns>Elastic secant modulus Fib 2010 § 7.2.3.1.2</returns>
        public virtual double GetEcm(double fcm, double days)
        {
            return Math.Pow(GetFcm(days) / fcm, 0.3) * GetEcm(fcm);
        }

        /// <summary>
        /// Mean tensile strength at an age: fctm βcc(t)^α, α = 1 before 28 days, 2/3 after
        /// </summary>
        /// <param name="days">The age in days</param>
        /// <returns>fctm(t)</returns>
        public virtual double GetFctm(double days)
        {
            return GetFctm() * Math.Pow(GetBetaCC(days), days < 28 ? 1 : 2.0 / 3.0);
        }

        /// <summary>
        /// Mean compressive strength at an age: βcc(t) fcm
        /// </summary>
        /// <param name="days">The age in days</param>
        /// <returns>fcm(t) (negative)</returns>
        /// <remarks>Fib 2010 § 7.2.3.1</remarks>
        protected virtual double GetFcm(double days)
        {
            return GetFcm() * GetBetaCC(days);
        }

        /// <summary>
        /// The coefficient of the strength development: βcc(t) = exp(s (1 - sqrt(28 / t)))
        /// </summary>
        /// <param name="days">The age in days</param>
        /// <returns>βcc(t)</returns>
        public virtual double GetBetaCC(double days)
        {
            return Math.Exp(GetCementSCoefficient() * (1.0 - Math.Pow(28.0 / days, 0.5)));
        }

        /// <summary>
        /// Calculate the creep deformation at infinite time (EN 1992-1-1 annex B): φ0 = φRH β(fcm) β(t0)
        /// </summary>
        /// <param name="sigmaC">The costant compressive stress</param>
        /// <param name="RH">The relative humidity %</param>
        /// <param name="areaC">The area of concrete</param>
        /// <param name="u">The perimeter of that part of the cross section which is exposed to drying</param>
        /// <param name="T0">The age of concrete at loading in days</param>
        /// <param name="deltaTemperature">The delta temperature in °C during the time period. Default value = 0</param>
        /// <param name="deltaDaysTemperature">is the number of days where a temperature <paramref name="deltaTemperature"/> prevails.
        /// Default value = 0</param>
        /// <returns>The creep strain |σ| φ0 / Ec for |σ| up to 0.45 fck; beyond it the non linear coefficient φ0 exp(1.5 (kσ - 0.45)) (not multiplied
        /// by σ / Ec: see the list of the defects found)</returns>
        public virtual double GetEpsilonCCInfiniteTime(double sigmaC, double RH, double areaC,
            double u, double T0 = 7, double deltaTemperature = 0, double deltaDaysTemperature = 0)
        {
            if (deltaTemperature != 0)
            {
                double alpha;
                if (_cementType == CementTypes.ClassS)
                    alpha = -1.0;
                else if (_cementType == CementTypes.ClassN)
                    alpha = 0.0;
                else
                    alpha = 1.0;

                double t0T = Math.Pow(10, -(4000.0 / (273.0 + deltaTemperature) - 13.65)) * deltaDaysTemperature;
                double T0Mod = t0T * Math.Pow(9.0 / (2.0 + Math.Pow(t0T, 1.20)) + 1, alpha);
                T0 = Math.Max(0.5, T0Mod);
            }

            double fcm = GetFcm();
            double h0 = 2 * areaC / u;
            double betat0 = 1.0 / (0.1 + Math.Pow(T0, 0.2));
            double betaFcm = 16.8 / Math.Sqrt(Math.Abs(fcm));
            double gammaRH;

            if (Math.Abs(fcm) <= 35.0)
                gammaRH = 1 + (1 - RH / 100.0) / (0.1 * Math.Pow(h0, 1.0 / 3.0));
            else
            {
                double alpha1 = Math.Pow(35.0 / Math.Abs(fcm), 0.7);
                double alpha2 = Math.Pow(35.0 / Math.Abs(fcm), 0.2);

                gammaRH = (1 + (1 - RH / 100.0) / (0.1 * Math.Pow(h0, 1.0 / 3.0)) * alpha1) * alpha2;
            }

            double gamma0 = gammaRH * betaFcm * betat0;
            double phi = gamma0;

            if (Math.Abs(sigmaC) <= 0.45 * Math.Abs(Fck))
                return Math.Sign(sigmaC) * phi * sigmaC / Ec;
            else
                return Math.Sign(sigmaC) * phi * Math.Pow(Math.E, 1.5 * (Math.Abs(sigmaC) / Math.Abs(Fck) - 0.45));
        }

        /// <summary>
        /// Calculate the total shrinkage strain at infinite time (EN 1992-1-1 3.1.4 and annex B): drying and autogenous shrinkage
        /// </summary>
        /// <param name="RH">The relative humidity %</param>
        /// <param name="areaC">The area of concrete</param>
        /// <param name="u">The perimeter of that part of the cross section which is exposed to drying</param>
        /// <returns>εcd,∞ + εca,∞ (see the list of the defects found: kh and the sign of fck)</returns>
        /// <exception cref="ArgumentException">If the class of cement is not defined</exception>
        public virtual double GetEpsilonCSInfiniteTime(double RH, double areaC, double u)
        {
            double alphads1;
            double alphads2;

            if (_cementType == CementTypes.ClassS)
            {
                alphads1 = 3.0;
                alphads2 = 0.13;
            }
            else if (_cementType == CementTypes.ClassN)
            {
                alphads1 = 4;
                alphads2 = 0.12;
            }
            else if (_cementType == CementTypes.ClassR)
            {
                alphads1 = 6.0;
                alphads2 = 0.11;
            }
            else
                throw new ArgumentException();

            double RH0 = 100;
            double betaRH = 1.55 * (1 - Math.Pow(RH / RH0, 3.0));
            double Fcm0 = 10;

            double epsilonCD0 = 0.85 * ((220 + 110 * alphads1) * Math.Pow(Math.E, (-alphads2 * GetFcm() / Fcm0))) *
                Math.Pow(10, -6) * betaRH;

            double h0 = 2 * areaC / u;
            double kh = 0;

            if (h0 <= 100)
                kh = 100;
            else if (h0 <= 200 && h0 > 100)
                Interpolation.GetLinearInterpolation(100, 200, 1.0, 0.85, kh);
            else if (h0 <= 300 && h0 > 200)
                Interpolation.GetLinearInterpolation(200, 300, 0.85, 0.75, kh);
            else if (h0 <= 500 && h0 > 300)
                Interpolation.GetLinearInterpolation(300, 500, 0.75, 0.70, kh);
            else
                kh = 0.70;

            double epsilonCDInf = kh * epsilonCD0;

            double epsilonCAInf = 2.5 * (Fck - 10) * Math.Pow(10, -6);

            return epsilonCDInf + epsilonCAInf;
        }

        /// <summary>
        /// Calculate increased characteristic strength and strains of confined concrete (EN 1992-1-1 3.1.9)
        /// </summary>
        /// <param name="sigma2">The effective lateral compressive stress at the ULS due to confinement</param>
        /// <param name="epsilonCC">New compressive strain in the concrete at the peak stress fc</param>
        /// <param name="epsilonCuC">New ultimate compressive strain in the concrete</param>
        /// <returns>The new characteristic compressive cylinder strength of concrete at 28 days</returns>
        public virtual double GetConfinedConcreteResistance(double sigma2, out double epsilonCC, out double epsilonCuC)
        {
            double fckc;
            if (sigma2 <= 0.05 * Fck)
                fckc = Fck * (1.0 + 5.0 * sigma2 / Fck);
            else
                fckc = Fck * (1.125 + 2.5 * sigma2 / Fck);

            if (_compressionStressStrainDiagrams == CompressionStressStrainDiagrams.ParabolaRectangle)
            {
                epsilonCC = _strainYCompression * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;
            }
            else if (_compressionStressStrainDiagrams == CompressionStressStrainDiagrams.Bilinear)
            {
                epsilonCC = _strainYCompression * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;
            }
            else
            {
                epsilonCC = _strainYCompression * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;

                //TODO: implementare questo caso
            }

            return fckc;
        }

        /// <summary>
        /// The limit compressive stress for the quasi-permanent combination: fck × the coefficient of the standard
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The limit stress</returns>
        public virtual double GetConcreteServiceabilityQuasiPermanentStress(StandardModelCode2010 standardModelCode2010)
        {
            return Fck * standardModelCode2010.ServiceabilityStressConcreteCoefficientForQuasiPermanentCombination;
        }

        /// <summary>
        /// The limit compressive stress for the characteristic combination: fck × the coefficient of the standard
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The limit stress</returns>
        public virtual double GetConcreteServiceabilityCharacteristicStress(StandardModelCode2010 standardModelCode2010)
        {
            return Fck * standardModelCode2010.ServiceabilityStressConcreteCoefficientForCharacteristicCombination;
        }

        #endregion

        #region Public Methods Override/Overload

        /// <summary>
        /// The design compressive strength: αcc fck / γc (× η for the stress block) for Model Code 2010, fck for ACI 318
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <returns>The design compressive strength (negative)</returns>
        /// <exception cref="ArgumentException">For the other standards, or fck greater than 90 with the stress block</exception>
        public override double CalculateDesignCompressiveStrength(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
            {
                if (CompressionStressStrainDiagram == ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.StressBlock)
                {
                    if (Math.Abs(Fck) > 90)
                        throw new ArgumentException("Fck > 90 not supported by Stress block");

                    double eta;
                    if (Math.Abs(Fck) <= 50.0)
                        eta = 1.0;
                    else
                        eta = 1.0 - (Math.Abs(Fck) - 50.0) / 200;

                    return eta * standardModelCode2010.AlphaCC * Fck / standardModelCode2010.GammaC;
                }
                else
                {
                    return standardModelCode2010.AlphaCC * Fck / standardModelCode2010.GammaC;
                }
            }
            else if (standard is StandardACI318)
            {
                return Fck;
            }
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design compressive strength (see <see cref="CalculateDesignCompressiveStrength"/>)
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The design compressive strength (negative)</returns>
        public virtual double CalculateFcd(StandardModelCode2010 standardModelCode2010)
        {
            return GetFcdReduction(standardModelCode2010) * Fck;
        }

        /// <summary>
        /// The design tensile strength: αct fctk,0.05 / γc for Model Code 2010, fctk,0.05 for ACI 318
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <returns>The design tensile strength</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
        public override double CalculateDesignTensileStrength(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
                return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaC;
            else if (standard is StandardACI318)
                return Fctk05;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design tensile strength: αct fctk,0.05 / γc (γF for the fiber reinforced concrete)
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The design tensile strength; 0 for other types</returns>
        public virtual double CalculateFctd(StandardModelCode2010 standardModelCode2010)
        {
            if (ConcreteType == ConcreteTypes.Concrete)
                return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaC;
            else if (ConcreteType == ConcreteTypes.FRC)
                return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaF;
            else
                return 0;
        }

        /// <summary>
        /// The design compressive strength for the accidental combinations: αcc fck / γc,acc (fck for ACI 318)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <returns>The design compressive strength (negative)</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
        public virtual double CalculateFcdAccidental(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
                return standardModelCode2010.AlphaCC * Fck / standardModelCode2010.GammaCAccidental;
            else if (standard is StandardACI318)
                return Fck;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design compressive strength for the accidental combinations: αcc fck / γc,acc
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The design compressive strength (negative)</returns>
        public virtual double CalculateFcdAccidental(StandardModelCode2010 standardModelCode2010)
        {
            return standardModelCode2010.AlphaCC * Fck / standardModelCode2010.GammaCAccidental;
        }

        /// <summary>
        /// The design tensile strength for the accidental combinations: αct fctk,0.05 / γc,acc (fctk,0.05 for ACI 318)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <returns>The design tensile strength</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
        public virtual double CalculateFctdAccidental(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
                return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaCAccidental;
            else if (standard is StandardACI318)
                return Fctk05;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design tensile strength for the accidental combinations: αct fctk,0.05 / γc,acc
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The design tensile strength</returns>
        public virtual double CalculateFctdAccidental(StandardModelCode2010 standardModelCode2010)
        {
            return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaCAccidental;
        }

        /// <summary>
        /// The design elastic modulus: E / γcE (E for ACI 318)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <returns>The design elastic modulus</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
        public virtual double CalculateECd(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
                return ElasticModulusCompression / standardModelCode2010.GammaCE;
            else if (standard is StandardACI318)
                return ElasticModulusCompression;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design elastic modulus: E / γcE
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The design elastic modulus</returns>
        public virtual double CalculateECd(StandardModelCode2010 standardModelCode2010)
        {
            return ElasticModulusCompression / standardModelCode2010.GammaCE;
        }

        /// <summary>
        /// The design stress for a strain: the design value of the characteristic stress of the tables
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <param name="strain">The strain (negative in compression)</param>
        /// <returns>The design stress</returns>
        public override double CalculateDesignStressConcrete(Standards.Standard standard, double strain)
        {
            return CalculateDesignStressFromCharacteristic(standard, GetStress(strain));
        }

        /// <summary>
        /// The design stress for a strain (see <see cref="CalculateDesignStressFromCharacteristic(StandardModelCode2010, double)"/>)
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <param name="strain">The strain (negative in compression)</param>
        /// <returns>The design stress</returns>
        public double CalculateDesignStressConcrete(StandardModelCode2010 standardModelCode2010, double strain)
        {
            return CalculateDesignStressFromCharacteristic(standardModelCode2010, GetStress(strain));
        }

        /// <summary>
        /// The design stress from the characteristic one: see the overload for Model Code 2010; the stress itself for ACI 318
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <returns>The design stress</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
        public override double CalculateDesignStressFromCharacteristic(Standard standard, double stress)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
                return CalculateDesignStressFromCharacteristic(standardModelCode2010, stress);
            else if (standard is StandardACI318)
                return stress;
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design stress from the characteristic one: compression × αcc / γc (× η for the stress block), tension × αct / γF
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <param name="stress">The characteristic stress (negative in compression)</param>
        /// <returns>The design stress</returns>
        public double CalculateDesignStressFromCharacteristic(StandardModelCode2010 standard, double stress)
        {
            if (stress < 0)
                return stress * GetFcdReduction(standard);
            else
                return stress * standard.AlphaCT / standard.GammaF;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Builds the characteristic table in compression of a diagram: bilinear, parabola-rectangle (10 points), stress block, non linear
        /// (EN 1992-1-1 3.1.5, 14 points, stresses scaled by fck) or generic (empty)
        /// </summary>
        /// <param name="fck">The characteristic compressive strength (negative)</param>
        /// <param name="strainYCompression">The strain at the peak</param>
        /// <param name="strainUCompression">The ultimate strain</param>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        /// <exception cref="NotSupportedException">For an unknown diagram</exception>
        /// <remarks>Sign convention: Stress and Strain negative if compression</remarks>
        protected void SetStressStrainTableCompression(double fck, double strainYCompression, double strainUCompression,
            CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.Bilinear:
                    _stressStrainTableCompression = new StressStrainTable(new double[] { 0, fck, fck },
                        new double[] { 0, strainYCompression, strainUCompression });
                    break;

                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    double[] stresses = new double[10];
                    double[] strains = new double[10] { 0,
                            strainYCompression / 8.0 * 1, strainYCompression / 8.0 * 2,
                            strainYCompression / 8.0 * 3, strainYCompression / 8.0 * 4,
                            strainYCompression / 8.0 * 5, strainYCompression / 8.0 * 6,
                            strainYCompression / 8.0 * 7, strainYCompression,
                            strainUCompression }; // discretiziamo il diagramma in 10 punti totali

                    stresses[0] = 0;

                    for (int i = 0; i < strains.Length; i++)
                    {
                        stresses[i] = GetParabolaStress(strains[i], strainYCompression);
                    }

                    _stressStrainTableCompression = new StressStrainTable(stresses, strains);
                    break;

                case CompressionStressStrainDiagrams.StressBlock:
                    _stressStrainTableCompression = new StressStrainTable(new double[] { 0, 0, fck, fck },
                        new double[] { 0, strainYCompression, strainYCompression, strainUCompression });
                    break;

                case CompressionStressStrainDiagrams.NonLinear:
                    double fcm = GetFcm();
                    double K = 1.05 * GetEcm(Math.Abs(fcm)) * Math.Abs(strainYCompression) / Math.Abs(fcm);

                    double[] stressesNl = new double[14];
                    double[] strainsNl = new double[14] { 0,
                            strainYCompression / 8.0 * 1, strainYCompression / 8.0 * 2,
                            strainYCompression / 8.0 * 3, strainYCompression / 8.0 * 4,
                            strainYCompression / 8.0 * 5, strainYCompression / 8.0 * 6,
                            strainYCompression / 8.0 * 7, strainYCompression,
                            (strainUCompression - strainYCompression) / 4.0 * 1 + strainYCompression,
                            (strainUCompression - strainYCompression) / 4.0 * 2 + strainYCompression,
                            (strainUCompression - strainYCompression) / 4.0 * 3 + strainYCompression,
                            (strainUCompression - strainYCompression) / 4.0 * 4 + strainYCompression,
                            strainUCompression }; // discretiziamo il diagramma in 10 punti totali

                    stressesNl[0] = 0;

                    for (int i = 0; i < strainsNl.Length; i++)
                    {
                        double eta = Math.Abs(strainsNl[i] / strainYCompression);
                        stressesNl[i] = fck * (K * eta - eta * eta) / (1.0 + (K - 2.0) * eta);
                    }

                    _stressStrainTableCompression = new StressStrainTable(stressesNl, strainsNl);
                    break;

                case CompressionStressStrainDiagrams.Generic:
                    _stressStrainTableCompression = new StressStrainTable();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Builds the characteristic table in tension of a diagram: linear, bilinear, rigid-plastic or generic (empty)
        /// </summary>
        /// <param name="fctk">The tensile strength (peak)</param>
        /// <param name="fctu">The ultimate (residual) strength</param>
        /// <param name="strainYTension">The strain at the peak</param>
        /// <param name="strainUTension">The ultimate strain</param>
        /// <param name="tensionStressStrainDiagrams">The diagram</param>
        /// <exception cref="NotSupportedException">For an unknown diagram</exception>
        protected void SetStressStrainTableTension(double fctk, double fctu, double strainYTension, double strainUTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            switch (tensionStressStrainDiagrams)
            {
                case TensionStressStrainDiagrams.Linear:
                    _stressStrainTableTension = new StressStrainTable(new double[] { 0, fctk }, new double[] { 0, strainYTension });
                    break;

                case TensionStressStrainDiagrams.Bilinear:
                    _stressStrainTableTension = new StressStrainTable(new double[] { 0, fctk, fctu },
                        new double[] { 0, strainYTension, strainUTension });
                    break;

                case TensionStressStrainDiagrams.RigidPlastic:
                    _stressStrainTableTension = new StressStrainTable(new double[] { fctu, fctu }, new double[] { 0, strainUTension });
                    break;

                case TensionStressStrainDiagrams.Generic:
                    _stressStrainTableTension = new StressStrainTable();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Sets fck, the elastic moduli, the strains of the compression diagram and the strengths and strains in tension
        /// (<see cref="Material._elasticModulusTension"/>, <see cref="Material._elasticModulusCompression"/>, <see cref="_fctk"/>, <see cref="_fck"/>)
        /// </summary>
        /// <param name="fck">The characteristic compressive strength (negative)</param>
        /// <param name="fctk">The tensile strength; 0: fctk,0.05 with a linear diagram</param>
        /// <param name="fFtu">The ultimate tensile strength</param>
        /// <param name="strainYTension">The strain at the tensile strength (0: fctk / E)</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="tensionStressStrainDiagrams">The diagram in tension</param>
        /// <param name="strainYCompression">The strain at the peak compression of a generic diagram (0: the strain of the minimum stress)</param>
        /// <exception cref="NotSupportedException">For an unknown diagram</exception>
        protected void SetMechanicalProperties(double fck, double fctk, double fFtu, double strainYTension, double strainUTension,
            CompressionStressStrainDiagrams compressionStressStrainDiagrams, TensionStressStrainDiagrams tensionStressStrainDiagrams, double strainYCompression = 0)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.Bilinear:
                case CompressionStressStrainDiagrams.ParabolaRectangle:
                case CompressionStressStrainDiagrams.StressBlock:
                case CompressionStressStrainDiagrams.NonLinear:

                    _fck = fck;
                    _elasticModulusCompression = GetEcm(GetFcm());
                    _strainUCompression = GetStrainUCompression(compressionStressStrainDiagrams);
                    _strainYCompression = GetStrainYCompression(compressionStressStrainDiagrams, _strainUCompression);
                    break;

                case CompressionStressStrainDiagrams.Generic:

                    _fck = _stressStrainTableCompression.GetMinimumStress(out double fckStrain);
                    _elasticModulusCompression = GetEcm(GetFcm());
                    _strainUCompression = _stressStrainTableCompression.GetLastStrain();
                    if (strainYCompression == 0)
                        _strainYCompression = fckStrain;
                    else
                        _strainYCompression = strainYCompression;
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (fctk == 0)
            {
                _fctk = GetFctk05();
                _fctu = _fctk;
                _elasticModulusTension = GetEcm(Math.Abs(GetFcm()));
                _strainYTension = _fctk / _elasticModulusTension;
                _strainUTension = _strainYTension;
            }
            else
            {
                switch (tensionStressStrainDiagrams)
                {
                    case TensionStressStrainDiagrams.Linear:
                        _fctk = fctk;
                        _fctu = fctk;
                        _elasticModulusTension = GetEcm(Math.Abs(GetFcm()));

                        if (strainYTension > 0)
                            _strainYTension = strainYTension;
                        else
                            _strainYTension = fctk / _elasticModulusTension;
                        _strainUTension = _strainYTension;
                        break;

                    case TensionStressStrainDiagrams.Bilinear:
                        _fctk = fctk;
                        _fctu = fFtu;
                        _elasticModulusTension = GetEcm(Math.Abs(GetFcm()));

                        if (strainYTension > 0)
                            _strainYTension = strainYTension;
                        else
                            _strainYTension = fctk / _elasticModulusTension;
                        _strainUTension = strainUTension;
                        break;

                    case TensionStressStrainDiagrams.Generic:
                        _fctk = fctk;
                        _fctu = _stressStrainTableTension.GetLastStress();
                        _elasticModulusTension = GetEcm(Math.Abs(GetFcm()));

                        if (strainYTension > 0)
                            _strainYTension = strainYTension;
                        else
                            _strainYTension = fctk / _elasticModulusTension;
                        _strainUTension = _stressStrainTableTension.GetLastStrain();
                        break;

                    case TensionStressStrainDiagrams.RigidPlastic:
                        _fctk = fFtu;
                        _fctu = fFtu;
                        _elasticModulusTension = GetEcm(Math.Abs(GetFcm()));

                        if (strainYTension > 0)
                            _strainYTension = strainYTension;
                        else
                            _strainYTension = fctk / _elasticModulusTension;

                        _strainUTension = strainUTension;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Sets diagrams, mechanical properties, tables and class of cement
        /// </summary>
        /// <param name="fck">The characteristic compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="ffts">The tensile strength (peak)</param>
        /// <param name="fFtu">The ultimate tensile strength</param>
        /// <param name="strainYTension">The strain at the peak</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="tensionStressStrainDiagrams">The diagram in tension</param>
        /// <param name="cementType">The class of cement</param>
        protected virtual void SetProperties(double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams, CementTypes cementType)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = tensionStressStrainDiagrams;

            SetMechanicalProperties(-Math.Abs(fck), Math.Abs(ffts), Math.Abs(fFtu), Math.Abs(strainYTension),
                Math.Abs(strainUTension), compressionStressStrainDiagrams, tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, tensionStressStrainDiagrams);

            SetStressProperties();

            _cementType = cementType;
        }

        /// <summary>
        /// The cubic strength of a strength class (C8/10 ... C100/115)
        /// </summary>
        /// <param name="fck">The cylinder strength (with its sign)</param>
        /// <returns>Rck of the class; fck / 0.83 for the other values (with the sign of <paramref name="fck"/>)</returns>
        protected virtual double GetFckCube(double fck)
        {
            // the classes are tabulated with positive values, while the fck of the material is negative
            return Math.Sign(fck) * GetFckCubeOfClass(Math.Abs(fck));
        }

        /// <summary>
        /// The cubic strength of a strength class from the positive cylinder strength
        /// </summary>
        /// <param name="fck">The cylinder strength (positive)</param>
        /// <returns>Rck of the class; fck / 0.83 for the other values</returns>
        private static double GetFckCubeOfClass(double fck)
        {
            switch (fck)
            {
                case (8.0):
                    return 10;
                case (12.0):
                    return 15;
                case (16.0):
                    return 20;
                case (20.0):
                    return 25;
                case (25.0):
                    return 30;
                case (30.0):
                    return 37;
                case (35.0):
                    return 45;
                case (40.0):
                    return 50;
                case (45.0):
                    return 55;
                case (50.0):
                    return 60;
                case (55.0):
                    return 67;
                case (60.0):
                    return 75;
                case (70.0):
                    return 85;
                case (80.0):
                    return 95;
                case (90.0):
                    return 105;
                case (100.0):
                    return 115;
                default:
                    return 1.0 / 0.83 * fck;
            }
        }

        /// <summary>
        /// fck from fcm: fcm - 8 (with the sign)
        /// </summary>
        /// <param name="fcm">The mean strength</param>
        /// <returns>fck</returns>
        protected virtual double GetFck(double fcm)
        {
            return Math.Sign(fcm) * (Math.Abs(fcm) - 8.0);
        }

        /// <summary>
        /// Secant modulus for a mean strength: 22000 (|fcm| / 10)^0.3
        /// </summary>
        /// <param name="fcm">The mean compressive strength</param>
        /// <returns>Elastic secant modulus Fib 2010 § 7.2.3.1.2</returns>
        protected virtual double GetEcm(double fcm)
        {
            return Math.Abs(22.0 * Math.Pow(Math.Abs(fcm) / 10.0, 0.30) * 1000);
        }

        /// <summary>
        /// Secant modulus: 22000 (|fcm| / 10)^0.3
        /// </summary>
        /// <returns>Elastic secant modulus Fib 2010 § 7.2.3.1.2</returns>
        protected virtual double GetEcm()
        {
            return Math.Abs(22.0 * Math.Pow(Math.Abs(GetFcm()) / 10.0, 0.30) * 1000);
        }

        /// <summary>
        /// fctk,0.05 = 0.7 fctm
        /// </summary>
        /// <returns>The characteristic tensile strength (5%)</returns>
        protected virtual double GetFctk05()
        {
            return 0.7 * GetFctm();
        }

        /// <summary>
        /// fctk,0.95 = 1.3 fctm
        /// </summary>
        /// <returns>The characteristic tensile strength (95%)</returns>
        protected virtual double GetFctk95()
        {
            return 1.3 * GetFctm();
        }

        /// <summary>
        /// Mean compressive strength: fck - 8 (fck negative)
        /// </summary>
        /// <returns>fcm (negative)</returns>
        /// <remarks>Fib 2010 § 7.2.3.1</remarks>
        protected virtual double GetFcm()
        {
            return Math.Sign(_fck) * (Math.Abs(_fck) + 8.0);
        }

        /// <summary>
        /// Mean tensile strength: 0.3 |fck|^(2/3) up to C50, 2.12 ln(1 + |fcm| / 10) beyond
        /// </summary>
        /// <returns>fctm</returns>
        protected virtual double GetFctm()
        {
            if (Math.Abs(_fck) <= 50)
                return 0.3 * Math.Pow(Math.Abs(_fck), 2.0 / 3.0);
            else
                return 2.12 * Math.Log(1.0 + Math.Abs(GetFcm()) / 10.0);
        }

        /// <summary>
        /// The coefficient s of the strength development: 0.20 class R, 0.25 class N, 0.38 class S
        /// </summary>
        /// <returns>s</returns>
        /// <exception cref="ArgumentException">If the class of cement is not defined</exception>
        protected virtual double GetCementSCoefficient()
        {
            switch (_cementType)
            {
                case CementTypes.ClassN:
                    return 0.25;

                case CementTypes.ClassR:
                    return 0.20;

                case CementTypes.ClassS:
                    return 0.38;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// The exponent n of the parabola: 2 up to C50, 1.4 + 23.4 ((90 - fck) / 100)^4 beyond
        /// </summary>
        /// <returns>n</returns>
        protected virtual double GetParabolaNCoefficient()
        {
            if (Math.Abs(_fck) <= 50)
                return 2.0;
            else
                return 1.4 + 23.4 * Math.Pow((90.0 - Math.Abs(_fck)) / 100.0, 4.0);
        }

        /// <summary>
        /// The stress of the parabola-rectangle: fck (1 - (1 - ε / εc2)^n), fck beyond εc2
        /// </summary>
        /// <param name="strain">The strain</param>
        /// <param name="strainY">εc2</param>
        /// <returns>The stress (negative)</returns>
        /// <exception cref="ArgumentException">If <paramref name="strainY"/> is zero</exception>
        protected virtual double GetParabolaStress(double strain, double strainY)
        {
            if (strainY == 0)
                throw new ArgumentException();

            if (Math.Abs(strain) > Math.Abs(strainY))
                return _fck;
            else if (strain == 0)
                return 0;
            else
                return _fck * (1.0 - Math.Pow(1.0 - Math.Abs(strain / strainY), GetParabolaNCoefficient()));
        }

        /// <summary>
        /// The strain at the peak of a diagram: εc2 (parabola-rectangle), εc3 (bilinear), (1 - λ) εcu (stress block), εc1 (non linear), the strain of
        /// the minimum stress (generic)
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        /// <param name="strainU">The ultimate strain (stress block)</param>
        /// <returns>The strain (negative)</returns>
        /// <exception cref="ArgumentException">For an unknown diagram</exception>
        /// <remarks>Sign convention: Stress and strain negative if compression. For the non linear diagram fcm is computed with the overload of the age
        /// (fck is passed as number of days)</remarks>
        protected virtual double GetStrainYCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double strainU = 0)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    if (Math.Abs(_fck) <= 50.0)
                        return -2.0 / 1000.0;
                    else
                        return -(2.0 + 0.085 * Math.Pow(Math.Abs(_fck) - 50.0, 0.53)) / 1000.0;

                case CompressionStressStrainDiagrams.Bilinear:
                    if (Math.Abs(_fck) <= 50.0)
                        return -1.75 / 1000.0;
                    else
                        return -(1.75 + 0.55 * ((Math.Abs(_fck) - 50.0) / 40.0)) / 1000.0;

                case CompressionStressStrainDiagrams.StressBlock:
                    {
                        double lambda;

                        if (Math.Abs(_fck) <= 50.0)
                            lambda = 0.8;
                        else
                            lambda = 0.8 - (Math.Abs(_fck) - 50.0) / 400;

                        return strainU * (1.0 - lambda);
                    }

                case CompressionStressStrainDiagrams.Generic:
                    _stressStrainTableCompression.GetMinimumStress(out double strain);
                    return strain;

                case CompressionStressStrainDiagrams.NonLinear:
                    return Math.Max(-0.7 * Math.Pow(Math.Abs(GetFcm(Math.Abs(_fck))), 0.31), -2.8) / 1000.0;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// The ultimate strain of a diagram: εcu2 / εcu3 (3.5 ‰ up to C50), εcu1 (non linear), the last strain (generic)
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        /// <returns>The strain (negative)</returns>
        /// <exception cref="ArgumentException">For an unknown diagram</exception>
        protected virtual double GetStrainUCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    if (Math.Abs(_fck) <= 50)
                        return -3.5 / 1000.0;
                    else
                        return -(2.6 + 35.0 * Math.Pow((90.0 - Math.Abs(_fck)) / 100.0, 4)) / 1000.0;

                case CompressionStressStrainDiagrams.Bilinear:
                    if (Math.Abs(_fck) <= 50)
                        return -3.5 / 1000.0;
                    else
                        return -(2.6 + 35.0 * Math.Pow((90.0 - Math.Abs(_fck)) / 100.0, 4)) / 1000.0;

                case CompressionStressStrainDiagrams.StressBlock:
                    if (Math.Abs(_fck) <= 50)
                        return -3.5 / 1000.0;
                    else
                        return -(2.6 + 35.0 * Math.Pow((90.0 - Math.Abs(_fck)) / 100.0, 4)) / 1000.0;

                case CompressionStressStrainDiagrams.Generic:
                    return _stressStrainTableCompression.Strains.Last();

                case CompressionStressStrainDiagrams.NonLinear:
                    if (Math.Abs(_fck) <= 50)
                        return -3.5 / 1000.0;
                    else
                        return -(2.8 + 27.0 * Math.Pow((98.0 - Math.Abs(GetFcm(Math.Abs(_fck)))) / 100.0, 4.0)) / 1000.0;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// The strain at the tensile strength of a diagram: fctk / E (0 for the rigid-plastic one)
        /// </summary>
        /// <param name="fctk">The tensile strength</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="tensionStressStrainDiagrams">The diagram</param>
        /// <returns>The strain</returns>
        /// <exception cref="ArgumentException">For the other diagrams</exception>
        /// <remarks>Sign convention: Stress and strain positive if tension</remarks>
        protected virtual double GetStrainYTension(double fctk, double elasticModulusTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            switch (tensionStressStrainDiagrams)
            {
                case TensionStressStrainDiagrams.Linear:
                    return fctk / elasticModulusTension;

                case TensionStressStrainDiagrams.Bilinear:
                    return fctk / elasticModulusTension;

                case TensionStressStrainDiagrams.RigidPlastic:
                    return 0;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// The ultimate strain in tension of a diagram: fctk / E (0 for the rigid-plastic one)
        /// </summary>
        /// <param name="fctk">The tensile strength</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="tensionStressStrainDiagrams">The diagram</param>
        /// <returns>The strain</returns>
        /// <exception cref="ArgumentException">For the other diagrams</exception>
        /// <remarks>Sign convention: Stress and strain positive if tension</remarks>
        protected virtual double GetStrainUTension(double fctk, double elasticModulusTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            switch (tensionStressStrainDiagrams)
            {
                case TensionStressStrainDiagrams.Linear:
                    return fctk / elasticModulusTension;

                case TensionStressStrainDiagrams.Bilinear:
                    return fctk / elasticModulusTension;

                case TensionStressStrainDiagrams.RigidPlastic:
                    return 0;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// The strain at the peak for the pure compression: the one of the diagram (the parabola-rectangle one for the stress block)
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        /// <returns>The strain (negative)</returns>
        /// <exception cref="ArgumentException">For an unknown diagram</exception>
        protected virtual double GetStrainYPureCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            // Per tutti i diagrammi torna la stessa strain y che viene usata per il grafico.
            // Per lo stress block ritorna quella del parabola rettangolo
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.ParabolaRectangle:
                case CompressionStressStrainDiagrams.Bilinear:
                case CompressionStressStrainDiagrams.Generic:
                case CompressionStressStrainDiagrams.NonLinear:
                    return GetStrainYCompression(compressionStressStrainDiagrams);

                case CompressionStressStrainDiagrams.StressBlock:
                    return GetStrainYCompression(CompressionStressStrainDiagrams.ParabolaRectangle);

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// The factor from fck to fcd: αcc / γc (× η for the stress block)
        /// </summary>
        /// <param name="standardModelCode2010">The standard (Model Code 2010)</param>
        /// <returns>The factor</returns>
        /// <exception cref="ArgumentException">If fck is greater than 90 with the stress block</exception>
        protected double GetFcdReduction(StandardModelCode2010 standardModelCode2010)
        {
            if (CompressionStressStrainDiagram == ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.StressBlock)
            {
                if (Math.Abs(Fck) > 90)
                    throw new ArgumentException("Fck > 90 not supported by Stress block");

                double eta;
                if (Math.Abs(Fck) <= 50.0)
                    eta = 1.0;
                else
                    eta = 1.0 - (Math.Abs(Fck) - 50.0) / 200;

                return eta * standardModelCode2010.AlphaCC / standardModelCode2010.GammaC;
            }
            else
            {
                return standardModelCode2010.AlphaCC / standardModelCode2010.GammaC;
            }
        }


        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the data of <see cref="ConcreteMaterial"/>, fck, fctk, fctu and the class of cement (version 3)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            int version = 3;
            info.AddValue("ConcreteMaterialEuropeanCommonVersion", version);

            info.AddValue("Fck", _fck);
            info.AddValue("Fctk", _fctk);
            info.AddValue("Fctu", _fctu);
            info.AddValue("CementType", _cementType);
        }

        /// <summary>
        /// Equality of fck, fctk, fctu, class of cement and the data of <see cref="ConcreteMaterial"/>
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal concrete</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialEuropeanCommon objCasted) &&
                objCasted._fck.Equals(_fck) &&
                objCasted._fctk.Equals(_fctk) &&
                objCasted._fctu.Equals(_fctu) &&
                objCasted._cementType.Equals(_cementType) &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the data of <see cref="ConcreteMaterial"/>, fck, fctk, fctu and class of cement
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _fck.GetHashCode();
                hashCode = hashCode * -17 + _fctk.GetHashCode();
                hashCode = hashCode * -17 + _fctu.GetHashCode();
                hashCode = hashCode * -17 + _cementType.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first concrete (not null, unless both are null)</param>
        /// <param name="obj2">The second concrete</param>
        /// <returns>True if the materials are equal</returns>
        public static bool operator ==(ConcreteMaterialEuropeanCommon obj1, ConcreteMaterialEuropeanCommon obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first concrete</param>
        /// <param name="obj2">The second concrete</param>
        /// <returns>True if the materials are different</returns>
        public static bool operator !=(ConcreteMaterialEuropeanCommon obj1, ConcreteMaterialEuropeanCommon obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
