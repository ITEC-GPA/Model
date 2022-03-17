using GPC.Utilities.Converters;
using GPC.Utilities.Maths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
    [Serializable]
    public abstract class ConcreteMaterialEuropeanCommon : ConcreteMaterial, ISerializable
    {
        #region Enum

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CompressionStressStrainDiagrams
        {
            [Description("Parabola-Rectangle")]
            ParabolaRectangle,

            [Description("Bilinear")]
            Bilinear,

            [Description("Stress Block")]
            StressBlock,

            [Description("Non Linear")]
            NonLinear,

            [Description("Generic")]
            Generic,
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum TensionStressStrainDiagrams
        {
            [Description("Linear")]
            Linear,

            [Description("Bilinear")]
            Bilinear,

            [Description("Rigid-Plastic")]
            RigidPlastic,

            [Description("Generic")]
            Generic,
        }

        public enum CementType
        {
            ClassR,
            ClassN,
            ClassS,
        }

		#endregion

		#region Variables 

		protected double _fck;
        protected double _fctk;
        protected double _fctu;
        
        protected double _strainYCompression;
        protected double _strainUCompression;
        
        protected double _strainYTension;
        protected double _strainUTension;
        
        protected CementType _cementType;

        protected CompressionStressStrainDiagrams _compressionStressStrainDiagrams;
        protected TensionStressStrainDiagrams _tensionStressStrainDiagrams;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic compressive cylinder strength of concrete at 28 days
        /// </summary>
        public double Fck => _fck;

        /// <summary>
        /// Characteristic tensile strength of concrete
        /// </summary>
        /// <remarks>Mean tensile strength at 28 days</remarks>
        public double Fctk => _fctk;

        /// <summary>
        /// Ultimate strain in tension
        /// </summary>
        public double Fctu => _fctu;

        /// <summary>
        /// Mean compressive strength at 28 days
        /// </summary>
        public double Fcm => GetFcm();

        /// <summary>
        /// Mean characteristic tensile strength 
        /// </summary>
        public double Fctm => GetFctm();

        /// <summary>
        /// Characteristic tensile strength 0.95%
        /// </summary>
        public double Fctk95 => GetFctk95();

        /// <summary>
        /// Characteristic tensile strength 0.05%
        /// </summary>
        public double Fctk05 => GetFctk05();

        /// <summary>
        /// Strain in the concrete for the pure compression case
        /// </summary>
        public double StrainYPureCompression => GetStrainYPureCompression(CompressionStressStrainDiagram);

        /// <summary>
        /// Strain in the concrete at the peak compressive stress fc
        /// </summary>
        public double StrainYCompression => _strainYCompression;

        /// <summary>
        /// Ultimate strain in compression
        /// </summary>
        public double StrainUCompression => _strainUCompression;

        /// <summary>
        /// Strain in the concrete at the peak tensile stress ftc
        /// </summary>
        public double StrainYTension => _strainYTension;

        /// <summary>
        /// Ultimate strain in tension
        /// </summary>
        public double StrainUTension => _strainUTension;

        /// <summary>
        /// The compression stress-strain relationship 
        /// </summary>
        public CompressionStressStrainDiagrams CompressionStressStrainDiagram => _compressionStressStrainDiagrams;

        /// <summary>
        /// The tension stress-strain relationship 
        /// </summary>
        public TensionStressStrainDiagrams TensionStressStrainDiagram => _tensionStressStrainDiagrams;

        /// <summary>
        /// Tangent modulus of elasticity
        /// </summary>
        public double Ec => 1.05 * E;

        #endregion

        #region Constructor

        // Costruttore per cls normale
        public ConcreteMaterialEuropeanCommon(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams, ConcreteTypes concreteType,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementType cementType = CementType.ClassN)
            : base(name, poisson, density, alfaThermalExpansion)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = TensionStressStrainDiagrams.Linear;
            _concreteType = concreteType;

            SetMechanicalProperties(-Math.Abs(fck), 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

            _cementType = cementType;
        }

        // Costruttore per cls frc
        public ConcreteMaterialEuropeanCommon(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, TensionStressStrainDiagrams tensionStressStrainDiagrams, ConcreteTypes concreteType,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementType cementType = CementType.ClassN)
            : base(name, poisson, density, alfaThermalExpansion)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = tensionStressStrainDiagrams;
            _concreteType = concreteType;

            SetMechanicalProperties(-Math.Abs(fck), Math.Abs(ffts), Math.Abs(fFtu), Math.Abs(strainYTension), Math.Abs(strainUTension),
                _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

            _cementType = cementType;
        }

        // Costruttore per cls con tabella generica
        public ConcreteMaterialEuropeanCommon(string name, double strainYTension, double strainYCompression,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6,
            CementType cementType = CementType.ClassN)
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

            _cementType = cementType;
        }

        protected ConcreteMaterialEuropeanCommon(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _fck = info.GetDouble("Fck");
            _fctk = info.GetDouble("Fctk");
            _fctu = info.GetDouble("Fctu");

            _strainYCompression = info.GetDouble("StrainYCompression");
            _strainUCompression = info.GetDouble("StrainUCompression");
            _strainYTension = info.GetDouble("StrainYTension");
            _strainUTension = info.GetDouble("StrainUTension");

            _cementType = (CementType)info.GetInt32("CementType");

            _compressionStressStrainDiagrams = (CompressionStressStrainDiagrams)info.GetInt32("CompressionStressStrainDiagrams");
            _tensionStressStrainDiagrams = (TensionStressStrainDiagrams)info.GetInt32("TensionStressStrainDiagrams");
        }

        #endregion

        #region Public methods

        public virtual double GetFctk05(double days)
        {
            return 0.7 * GetFctm(days);
        }

        public virtual double GetFctk95(double days)
        {
            return 1.3 * GetFctm(days);
        }

        /// <returns>Elastic secant modulus Fib 2010 § 7.2.3.1.2 </returns>
        public virtual double GetEcm(double fcm, double days)
        {
            return Math.Pow(GetFcm(days) / fcm, 0.3) * GetEcm(fcm);
        }

        public virtual double GetFctm(double days)
        {
            return GetFctm() * Math.Pow(GetBetaCC(days), days < 28 ? 1 : 2.0 / 3.0);
        }

        /// <param name="days"></param>
        /// <remarks>Fib 2010 § 7.2.3.1 </remarks>
        protected virtual double GetFcm(double days)
        {
            return GetFcm() * GetBetaCC(days);
        }

        public virtual double GetBetaCC(double days)
        {
            return Math.Exp(GetCementSCoefficient() * (1.0 - Math.Pow(28.0 / days, 0.5) ));
        }

        /// <summary>
        /// Calculate the creep deformation at infinite time
        /// </summary>
        /// <param name="sigmaC">The costant compressive stress</param>
        /// <param name="RH">The relative humidity %</param>
        /// <param name="areaC">The area of concrete</param>
        /// <param name="u">The perimeter of that part of the cross section which is exposed to drying</param>
        /// <param name="T0">The age of concrete at loading in days</param>
        /// <param name="deltaTemperature">The delta temperature in °C during the time period. Default value = 0</param>
        /// <param name="deltaDaysTemperature">is the number of days where a temperature <paramref name="deltaTemperature"/> prevails. 
        /// Default value = 0</param>
        /// <returns></returns>
        public virtual double GetEpsilonCCInfiniteTime(double sigmaC, double RH, double areaC, 
            double u, double T0 = 7, double deltaTemperature = 0, double deltaDaysTemperature = 0)
        {
            if (deltaTemperature != 0)
            {
                double alpha;
                if (_cementType == CementType.ClassS)
                    alpha = -1.0;
                else if (_cementType == CementType.ClassN)
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
        /// Calculate the total shrinkage strain
        /// </summary>
        /// <param name="RH">The relative humidity %</param>
        /// <param name="areaC">The area of concrete</param>
        /// <param name="u">The perimeter of that part of the cross section which is exposed to drying</param>
        /// <returns></returns>
        public virtual double GetEpsilonCSInfiniteTime(double RH, double areaC, double u)
        {
            double alphads1;
            double alphads2;

            if (_cementType == CementType.ClassS)
            {
                alphads1 = 3.0;
                alphads2 = 0.13;
            }
            else if (_cementType == CementType.ClassN)
            {
                alphads1 = 4;
                alphads2 = 0.12;
            }
            else if (_cementType == CementType.ClassR)
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
        /// Calculate increased characteristic strength and strains of confined concrete 
        /// </summary>
        /// <param name="sigma2">The effective lateral compressive stress at the ULS due to confinement</param>
        /// <param name="epsilonCC">New compressive strain in the concrete at the peak stress fc</param>
        /// <param name="epsilonCuC">New ultimate compressive strain in the concrete</param>
        /// <returns>Thw new characteristic compressive cylinder strength of concrete at 28 days</returns>
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

        #endregion

        #region Public Methods Override

        public override double CalculateFcd(Standards.Standard standard)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                if (CompressionStressStrainDiagram == ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.StressBlock)
                {
                    if (Fck > 90)
                        throw new ArgumentException("Fck > 90 not supported by Stress block");

                    double eta;
                    if (Fck <= 50.0)
                        eta = 1.0;
                    else
                        eta = 1.0 - (Fck - 50.0) / 200;

                    return eta * standardModelCode2010.AlphaCC * Fck / standardModelCode2010.GammaC;
                }
                else
                {
                    return standardModelCode2010.AlphaCC * Fck / standardModelCode2010.GammaC;
                }
            }
            else
                throw new ArgumentException();
        }

        public override double CalculateFctd(Standards.Standard standard)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaC;
            }
            else
                throw new ArgumentException();
        }

        public override double CalculateFcdAccidental(Standards.Standard standard)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                return standardModelCode2010.AlphaCC * Fck / standardModelCode2010.GammaCAccidental;
            }
            else
                throw new ArgumentException();
        }

        public override double CalculateFctdAccidental(Standards.Standard standard)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaCAccidental;
            }
            else
                throw new ArgumentException();
        }

        public override double CalculateECd(Standards.Standard standard)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                return E / standardModelCode2010.GammaCE;
            }
            else
                throw new ArgumentException();
        }

        public override double CalculateDesignStressConcrete(Standards.Standard standard, double strain)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                if (strain < 0)
                {
                    // compressione
                    return GetStress(strain) * Math.Abs(CalculateFcd(standardModelCode2010) / Fck);
                }
                else
                {
                    return GetStress(strain) * Math.Abs(CalculateFctd(standardModelCode2010) / Fctk05);
                }
            }
            else
                throw new ArgumentException();
        }

        #endregion

        #region Protected methods

        /// <remarks> Sign convention: Stress and Strain negative if compression </remarks>
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

                    _stressStrainTableTension = new StressStrainTable(new double[] { fctk, fctk }, new double[] { 0, strainUTension });
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Set <see cref="ConcreteMaterial._elasticModulusTension"/>, <see cref="Material._elasticModulus"/>
        /// <see cref="ConcreteMaterialEuropeanCommon._fctk"/>, 
        /// <see cref="ConcreteMaterialEuropeanCommon._fck"/>
        /// </summary>
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
                    _elasticModulus = GetEcm(GetFcm());
                    _strainUCompression = GetStrainUCompression(compressionStressStrainDiagrams);
                    _strainYCompression = GetStrainYCompression(compressionStressStrainDiagrams, _strainUCompression);
                    break;

                case CompressionStressStrainDiagrams.Generic:

                    _fck = _stressStrainTableCompression.GetMinimumStress(out double fckStrain);
                    _elasticModulus = GetEcm(GetFcm());
                    _strainUCompression = _stressStrainTableCompression.GetLastStrain();
                    if(strainYCompression == 0)
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
                        _elasticModulusTension = strainYTension == 0 ? GetEcm(GetFcm()) : fctk / strainYTension;

                        _strainYTension = _fctk / _elasticModulusTension;
                        _strainUTension = _strainYTension;
                        break;

                    case TensionStressStrainDiagrams.Bilinear:
                        _fctk = fctk;
                        _fctu = fFtu;
                        _elasticModulusTension = strainYTension == 0 ? GetEcm(GetFcm()) : fctk / strainYTension;

                        _strainYTension = _fctk / _elasticModulusTension;
                        _strainUTension = strainUTension;
                        break;

                    case TensionStressStrainDiagrams.Generic:
                        _fctk = fctk;
                        _fctu = _stressStrainTableTension.GetLastStress();
                        _elasticModulusTension = strainYTension == 0 ? GetEcm(GetFcm()) : fctk / strainYTension;

                        _strainYTension = _fctk / _elasticModulusTension;
                        _strainUTension = _stressStrainTableTension.GetLastStrain();
                        break;

                    case TensionStressStrainDiagrams.RigidPlastic:
                        _fctk = fctk;
                        _fctk = fctk;
                        _elasticModulusTension = GetEcm(Math.Abs(GetFcm()));

                        _strainYTension = 0.0;
                        _strainUTension = strainUTension;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        protected virtual void SetProperties(double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, 
            TensionStressStrainDiagrams tensionStressStrainDiagrams, CementType cementType)
		{
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = tensionStressStrainDiagrams;

            SetMechanicalProperties(-Math.Abs(fck), Math.Abs(ffts), Math.Abs(fFtu), Math.Abs(strainYTension), 
                Math.Abs(strainUTension), compressionStressStrainDiagrams, tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, tensionStressStrainDiagrams);

            _cementType = cementType;
        }

        protected virtual double GetFckCube(double fck)
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

        protected virtual double GetFck(double fcm)
        {
            return Math.Sign(fcm) * (Math.Abs(fcm) - 8.0);
        }

        /// <returns>Elastic secant modulus Fib 2010 § 7.2.3.1.2 </returns>
        protected virtual double GetEcm(double fcm)
        {
            return Math.Abs(22.0 * Math.Pow(Math.Abs(fcm) / 10.0, 0.30) * 1000);
        }

        protected virtual double GetFctk05()
        {
            return 0.7 * GetFctm();
        }

        protected virtual double GetFctk95()
        {
            return 1.3 * GetFctm();
        }

        /// <remarks>Fib 2010 § 7.2.3.1 </remarks>
        protected virtual double GetFcm()
        {
            return Math.Sign(_fck) * (Math.Abs(_fck) + 8.0);
        }

        protected virtual double GetFctm()
        {
            if (Math.Abs(_fck) <= 50)
                return 0.3 * Math.Pow(Math.Abs(_fck), 2.0 / 3.0);
            else
                return 2.12 * Math.Log(1.0 + Math.Abs(GetFcm()) / 10.0);
        }

        protected virtual double GetCementSCoefficient()
        {
            switch (_cementType)
            {
                case CementType.ClassN:
                    return 0.25;

                case CementType.ClassR:
                    return 0.20;

                case CementType.ClassS:
                    return 0.38;

                default:
                    throw new ArgumentException();
            }
        }

        protected virtual double GetParabolaNCoefficient()
        {
            if (Math.Abs(_fck) <= 50)
                return 2.0;
            else
                return 1.4 + 23.4 * Math.Pow((90.0 - Math.Abs(_fck)) / 100.0, 4.0);
        }

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
                
        /// <remarks>Sign convention: Stress and strain negative if compression</remarks>
        protected virtual double GetStrainYCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams, 
            double strainU = 0)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    if (Math.Abs(_fck) <= 50.0)
                        return - 2.0 / 1000.0;
                    else
                        return - (2.0 + 0.085 * Math.Pow(Math.Abs(_fck) - 50.0, 0.53)) / 1000.0;

                case CompressionStressStrainDiagrams.Bilinear:
                    if (Math.Abs(_fck) <= 50.0)
                        return - 1.75 / 1000.0;
                    else
                        return - (1.75 + 0.55 * ((Math.Abs(_fck) - 50.0) / 40.0)) / 1000.0;

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
                    return Math.Max(- 0.7 * Math.Pow(Math.Abs(GetFcm(Math.Abs(_fck))), 0.31), - 2.8) / 1000.0;

                default:
                    throw new ArgumentException();
            }
        }

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

        #endregion

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fck", _fck);
            info.AddValue("Fctk", _fctk);
            info.AddValue("Fctu", _fctu);
            info.AddValue("StrainYCompression", _strainYCompression);
            info.AddValue("StrainUCompression", _strainUCompression);
            info.AddValue("StrainYTension", _strainYTension);
            info.AddValue("StrainUTension", _strainUTension);
            info.AddValue("CementType", _cementType);
            info.AddValue("CompressionStressStrainDiagrams", _compressionStressStrainDiagrams);
            info.AddValue("TensionStressStrainDiagrams", _tensionStressStrainDiagrams);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialEuropeanCommon objCasted) && 
                objCasted._fck.Equals(_fck) && 
               objCasted._fctk.Equals(_fctk) &&
               objCasted._fctu.Equals(_fctu) &&
               objCasted._strainUCompression.Equals(_strainUCompression) &&
               objCasted._strainYCompression.Equals(_strainYCompression) &&
               objCasted._strainYTension.Equals(_strainYTension) &&
               objCasted._strainUTension.Equals(_strainUTension) &&
               objCasted._cementType.Equals(_cementType) &&
               objCasted._compressionStressStrainDiagrams.Equals(_compressionStressStrainDiagrams) &&
               objCasted._tensionStressStrainDiagrams.Equals(_tensionStressStrainDiagrams) &&
               base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _fck.GetHashCode();
                hashCode = hashCode * -17 + _fctk.GetHashCode();
                hashCode = hashCode * -17 + _fctu.GetHashCode();
                hashCode = hashCode * -17 + _strainUCompression.GetHashCode();
                hashCode = hashCode * -17 + _strainYCompression.GetHashCode();
                hashCode = hashCode * -17 + _strainYTension.GetHashCode();
                hashCode = hashCode * -17 + _strainYCompression.GetHashCode();
                hashCode = hashCode * -17 + _cementType.GetHashCode();
                hashCode = hashCode * -17 + _compressionStressStrainDiagrams.GetHashCode();
                hashCode = hashCode * -17 + _tensionStressStrainDiagrams.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ConcreteMaterialEuropeanCommon obj1, ConcreteMaterialEuropeanCommon obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcreteMaterialEuropeanCommon obj1, ConcreteMaterialEuropeanCommon obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
