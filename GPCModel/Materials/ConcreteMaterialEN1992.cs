using System;
using System.Runtime.Serialization;
using GPC.Model.FEM.Materials;
using GPC.Model.Standards;
using GPC.Utilities.Attributes;
using GPC.Utilities.Maths;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Concrete EN1992-1-1", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterialEN1992 : ConcreteMaterial
    {
        #region Enumerator

        public enum StressStrainDiagrams
        {
            ParabolaRectangle,
            Bilinear,
            StressBlock,
        }

        public enum TypeOfCements
        {
            ClassR,
            ClassN,
            ClassS,
        }

        #endregion

        #region Variables

        protected readonly StandardEn1992p11 _standard;

        protected double _epsilonY;
        protected double _epsilonU;
        protected double _fcd;

        protected readonly double _niCracked;

        protected readonly StressStrainDiagrams _stressStrainDiagram;
        protected readonly TypeOfCements _typeOfCement;

        #endregion

        #region Properties

        /// <summary>
        /// Standard EN1992 or a relative national annex
        /// </summary>
        public StandardEn1992p11 Standard => _standard;

        /// <summary>
        /// Compressive strain in the concrete at the peak stress fc
        /// </summary>
        public double EpsilonY => _epsilonY;

        /// <summary>
        /// Ultimate compressive strain in the concrete
        /// </summary>
        public double EpsilonU => _epsilonU;

        /// <summary>
        /// characteristic cubic strength
        /// </summary>
        public double FckCube => CalculateFckCube();

        /// <summary>
        /// Mean tensile strength at 28 days
        /// </summary>
        public double Fctm => CalculateFctm();

        /// <summary>
        /// Characteristic tensile strength 0.05%
        /// </summary>
        public double Fctk05 => 0.7 * CalculateFctm();

        /// <summary>
        /// Characteristic tensile strength 0.95%
        /// </summary>
        public double Fctk95 => 1.30 * CalculateFctm();

        /// <summary>
        /// Mean compressive strength at 28 days
        /// </summary>
        public double Fcm => Fck + 8;

        /// <summary>
        /// Design compressive strength for persistent design
        /// </summary>
        public double Fcd => _fcd;

        /// <summary>
        /// Design compressive strength for accidental design
        /// </summary>
        public double FcdAccidental => Standard.AlphaCC * Fck / Standard.GammaCAccidental;

        /// <summary>
        /// Design tensile strength for persistent design
        /// </summary>
        public double Fctd => Standard.AlphaCT * Fctk05 / Standard.GammaC;

        /// <summary>
        /// Design tensile strength for accidental design
        /// </summary>
        public double FctdAccidental => Standard.AlphaCT * Fctk05 / Standard.GammaCAccidental;

        /// <summary>
        /// Secant modulus of elasticity value between sigmac = 0 and 0,4fcm (Ecm)
        /// </summary>
        public override double E => CalculateEcm();

        /// <summary>
        /// Modulus of elasticity value for ultimate limit state calculations
        /// </summary>
        public double ECd => E / Standard.GammaCE;

        /// <summary>
        /// Tangent modulus of elasticity
        /// </summary>
        public double Ec => 1.05 * CalculateEcm();

        /// <summary>
        /// Poisson’s ratio for cracked concrete
        /// </summary>
        public double NiCracked => _niCracked;

        /// <summary>
        /// The stress-strain relationship
        /// </summary>
        public StressStrainDiagrams StressStrainDiagram => _stressStrainDiagram;

        /// <summary>
        /// The strength class of cement
        /// </summary>
        public TypeOfCements TypeOfCement => _typeOfCement;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
        /// <param name="ni">Poisson's ratio</param>
        /// <param name="niCracked">Poisson's ratio in cracked concrete</param>
        /// <param name="alphaT">Linear thermal expasion coefficient</param>
        /// <param name="density">The density of concrete</param>
        /// <param name="standard">Standard EN1992 or a relative national annex</param>
        /// <param name="stressStrainDiagram">The stress-strain diagram type</param>
        /// <param name="typeOfCement">The type of cement. See §3.4.1</param>
        public ConcreteMaterialEN1992(string name, double fck, double ni, double niCracked, double alphaT, double density,
            StandardEn1992p11 standard, StressStrainDiagrams stressStrainDiagram, TypeOfCements typeOfCement)
            : base(name, fck)
        {
            if (fck < 0.0)
                throw new ArgumentException($"{nameof(fck)} must be > 0");

            _ni = ni < 0 ? throw new ArgumentException($"{nameof(ni)} cannot be zero or lower") : ni;
            if(ni > 0.5)
                throw new ArgumentException($"{nameof(ni)} must be < 0.5");

            _niCracked = niCracked < 0 ? throw new ArgumentException($"{nameof(niCracked)} cannot be zero or lower") : niCracked;
            if (niCracked > 0.5)
                throw new ArgumentException($"{nameof(niCracked)} must be < 0.5");

            _density = density <= 0 ? throw new ArgumentException($"{nameof(density)} cannot be zero or lower") : density;

            _standard = standard;                        
            _alfaThermalExpansion = alphaT;            

            _stressStrainDiagram = stressStrainDiagram;
            _typeOfCement = typeOfCement;

            _elasticModulus = CalculateEcm();
            if (_elasticModulus < 0)
                throw new ArgumentException($"{nameof(_elasticModulus)} must be > 0");

            SetEpsilonU();
            SetEpsilonY();
            SetFcd();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
        /// <param name="ni">Poisson's ratio</param>
        /// <param name="niCracked">Poisson's ratio in cracked concrete</param>
        /// <param name="alphaT">Linear thermal expasion coefficient</param>
        /// <param name="density">The density of concrete</param>
        /// <param name="standard">Standard EN1992 or a relative national annex</param>
        /// <param name="stressStrainDiagram">The stress-strain diagram type</param>
        /// <remarks>Type of cements is ClassN</remarks>
        public ConcreteMaterialEN1992(string name, double fck, double ni, double niCracked, double alphaT, double density,
            StandardEn1992p11 standard, StressStrainDiagrams stressStrainDiagram)
            : this(name, fck, ni, niCracked, alphaT, density, standard, stressStrainDiagram, TypeOfCements.ClassN)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
        /// <param name="standard">Standard EN1992 or a relative national annex</param>
        /// <param name="stressStrainDiagram">The stress-strain diagram type</param>
        /// <remarks>Value: ni = 0.2, niCracked = 0.0; alfaThermalExpansion = 1e-6; density = 0.0025 T/mm^3; type of cements = classN</remarks>
        public ConcreteMaterialEN1992(string name, double fck, StandardEn1992p11 standard, StressStrainDiagrams stressStrainDiagram)
            : this(name, fck, 0.2, 0.0, 1e-6, 0.0025, standard, stressStrainDiagram, TypeOfCements.ClassN)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
        /// <param name="stressStrainDiagram">The stress-strain diagram type</param>
        /// <remarks>Value: ni = 0.2, niCracked = 0.0; alfaThermalExpansion = 1e-6; density = 0.0025 T/mm^3; standard = StandardEn1992p11; type of cements = classN</remarks>
        public ConcreteMaterialEN1992(double fck, StressStrainDiagrams stressStrainDiagram)
            : this("", fck, 0.2, 0.0, 1e-6, 0.0025, new StandardEn1992p11(), stressStrainDiagram, TypeOfCements.ClassN)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
        /// <remarks>Value: ni = 0.2, niCracked = 0.0; alfaThermalExpansion = 1e-6; density = 0.0025 T/mm^3; 
        /// standard = StandardEn1992p11; StressStrainDiagrams = StressBlock; type of cements = classN</remarks>
        public ConcreteMaterialEN1992(double fck)
            : this("", fck, 0.2, 0.0, 1e-6, 0.0025, new StandardEn1992p11(), StressStrainDiagrams.StressBlock, TypeOfCements.ClassN)
        {

        }

        public ConcreteMaterialEN1992(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _fck = info.GetDouble("Fck");
                        
            _niCracked = info.GetDouble("NiCracked");
            _epsilonY = info.GetDouble("EpsilonY");
            _epsilonU = info.GetDouble("EpsilonU");
        }

        #endregion 

        #region Public Methods

        public virtual double CalculateSigmaC(double epsilonC)
        {
            if (StressStrainDiagram == StressStrainDiagrams.ParabolaRectangle)
            {
                if (epsilonC >= EpsilonY)
                    return Fcd;
                else
                    return Fcd * (1 - Math.Pow(1 - epsilonC / EpsilonY, CalculateN()));
            }
            else if (StressStrainDiagram == StressStrainDiagrams.Bilinear)
            {
                if (epsilonC >= EpsilonY)
                    return Fcd;
                else
                    return 0.0;
            }
            else if (StressStrainDiagram == StressStrainDiagrams.StressBlock)
            {
                if (epsilonC >= EpsilonY)
                    return Fcd;
                else
                    return Fcd * epsilonC / EpsilonY;
            }
            else
                throw new ArgumentException("");
        }

        public virtual double CalculateEcm(int days)
        {
            return Math.Pow(CalculateFcm(days) / Fcm, 0.3) * E;
        }

        public virtual double CalculateFcm(int days)
        {
            return Fcm * CalculateBetaCC(days);
        }

        public virtual double CalculateFctm(int days)
        {
            double betaCC = CalculateBetaCC(days);
            double alpha;
            if (days < 28)
                alpha = 1.0;
            else
                alpha = 2.0 / 3.0;

            return Math.Pow(betaCC, alpha) * Fctm;
        }

        /// <summary>
        /// Calculate increased characteristic strength and strains of confined concrete 
        /// </summary>
        /// <param name="sigma2">The effective lateral compressive stress at the ULS due to confinement</param>
        /// <param name="epsilonCC">New compressive strain in the concrete at the peak stress fc</param>
        /// <param name="epsilonCuC">New ultimate compressive strain in the concrete</param>
        /// <returns>Thw new characteristic compressive cylinder strength of concrete at 28 days</returns>
        public virtual double CalculateConfinedConcreteResistance(double sigma2, out double epsilonCC, out double epsilonCuC)
        {
            double fckc;
            if (sigma2 <= 0.05 * Fck)
                fckc = Fck * (1.0 + 5.0 * sigma2 / Fck);
            else
                fckc = Fck * (1.125 + 2.5 * sigma2 / Fck);

            if (StressStrainDiagram == StressStrainDiagrams.ParabolaRectangle)
            {
                epsilonCC = EpsilonY * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;
            }
            else if (StressStrainDiagram == StressStrainDiagrams.Bilinear)
            {
                epsilonCC = EpsilonY * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;
            }
            else
            {
                epsilonCC = EpsilonY * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;

                //TODO: implementare questo caso
            }

            return fckc;
        }

        /// <summary>
        /// Calculate the creep deformation at infinite time
        /// </summary>
        /// <param name="sigmaC">The costant compressive stress</param>
        /// <param name="RH">The relative humidity %</param>
        /// <param name="AreaC">The area of concrete</param>
        /// <param name="u">The perimeter of that part of the cross section which is exposed to drying</param>
        /// <param name="T0">The age of concrete at loading in days</param>
        /// <param name="deltaTemperature">The delta temperature in °C during the time period. Default value = 0</param>
        /// <param name="deltaDaysTemperature">is the number of days where a temperature <paramref name="deltaTemperature"/> prevails. Default value = 0</param>
        /// <returns></returns>
        public virtual double CalculateEpsilonCCInfiniteTime(double sigmaC, double RH, double AreaC, double u, double T0 = 7, double deltaTemperature = 0, double deltaDaysTemperature = 0)
        {
            if (deltaTemperature != 0)
            {
                double alpha;
                if (TypeOfCement == TypeOfCements.ClassS)
                    alpha = -1.0;
                else if (TypeOfCement == TypeOfCements.ClassN)
                    alpha = 0.0;
                else // if (TypeOfCement == TypeOfCements.ClassR)
                    alpha = 1.0;

                double t0T = Math.Pow(10, -(4000 / (273 + deltaTemperature) - 13.65)) * deltaDaysTemperature;
                double T0Mod = t0T * Math.Pow(9 / (2 + Math.Pow(t0T, 1.20)) + 1, alpha);
                T0 = Math.Max(0.5, T0Mod);
            }


            double h0 = 2 * AreaC / u;
            double betat0 = 1.0 / (0.1 + Math.Pow(T0, 0.2));
            double betaFcm = 16.8 / Math.Sqrt(Fcm);
            double gammaRH;

            if (Fcm <= 35.0)
                gammaRH = 1 + (1 - RH / 100.0) / (0.1 * Math.Pow(h0, 1.0 / 3.0));
            else
            {
                double alpha1 = Math.Pow(35.0 / Fcm, 0.7);
                double alpha2 = Math.Pow(35.0 / Fcm, 0.2);

                gammaRH = (1 + (1 - RH / 100.0) / (0.1 * Math.Pow(h0, 1.0 / 3.0)) * alpha1) * alpha2;
            }

            double gamma0 = gammaRH * betaFcm * betat0;
            double phi = gamma0;  // * beta(t, t0) = 1.0 a tempo infinito

            if (sigmaC <= 0.45 * Fck)
                return phi * sigmaC / Ec;
            else
                return phi * Math.Pow(Math.E, 1.5 * (sigmaC / Fck - 0.45));
        }

        /// <summary>
        /// Calculate the total shrinkage strain
        /// </summary>
        /// <param name="RH">The relative humidity %</param>
        /// <param name="AreaC">The area of concrete</param>
        /// <param name="u">The perimeter of that part of the cross section which is exposed to drying</param>
        /// <returns></returns>
        public virtual double CalculateEpsilonCSInfiniteTime(double RH, double AreaC, double u)
		{
            double alphads1;
            double alphads2;

            if (TypeOfCement == TypeOfCements.ClassS)
            {
                alphads1 = 3.0;
                alphads2 = 0.13;
            }
            else if (TypeOfCement == TypeOfCements.ClassN)
            {
                alphads1 = 4;
                alphads2 = 0.12;
            }
            else if (TypeOfCement == TypeOfCements.ClassR)
            {
                alphads1 = 6.0;
                alphads2 = 0.11;
            }
            else
                throw new ArgumentException();

            double RH0 = 100;
            double betaRH = 1.55 * (1 - Math.Pow(RH / RH0, 3.0));
            double Fcm0 = 10;

            double epsilonCD0 = 0.85 * ((220 + 110 * alphads1) * Math.Pow(Math.E,(-alphads2 * Fcm / Fcm0))) * Math.Pow(10, -6) * betaRH;

            double h0 = 2 * AreaC / u;
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
            // betaAS = 1.0

            return epsilonCDInf + epsilonCAInf;
        }

        #endregion

        #region Protected Methods

        protected virtual void SetEpsilonY()
		{
            if (StressStrainDiagram == StressStrainDiagrams.ParabolaRectangle)
            {
                if (_fck <= 50)
                    _epsilonY = 2.0 / 10.0;
                else
                    _epsilonY = (2.0 + 0.085 * Math.Pow(_fck - 50.0, 0.53)) / 10.0;
            }
            else if (StressStrainDiagram == StressStrainDiagrams.Bilinear)
            {
                if (_fck <= 50)
                    _epsilonY = 1.75 / 10.0;
                else
                    _epsilonY = (1.75 + 0.55 * ((_fck - 50.0) / 40.0)) / 10.0;
            }
            else if (StressStrainDiagram == StressStrainDiagrams.StressBlock)
            {
                double lambda;

                if (_fck <= 50.0)
                    lambda = 0.8;
                else
                    lambda = 0.8 - (_fck - 50.0) / 400;

                _epsilonY = _epsilonU * (1 - lambda);
            }
            else
                throw new ArgumentException();
		}

        protected virtual void SetEpsilonU()
        {
            if (StressStrainDiagram == StressStrainDiagrams.ParabolaRectangle)
            {
                if (_fck <= 50)
                    _epsilonU = 3.5 / 10.0;
                else
                    _epsilonU = (2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4)) / 10.0;
            }
            else if (StressStrainDiagram == StressStrainDiagrams.Bilinear)
            {
                if (_fck <= 50)
                    _epsilonU = 3.5 / 10.0;
                else
                    _epsilonU = (2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4)) / 10.0;
            }
            else if (StressStrainDiagram == StressStrainDiagrams.StressBlock)
            {
                if (_fck <= 50)
                    _epsilonU = 3.5 / 10.0;
                else
                    _epsilonU = (2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4)) / 10.0;
            }
            else
                throw new ArgumentException();
        }

        protected virtual void SetFcd()
		{
            if (StressStrainDiagram == StressStrainDiagrams.StressBlock)
            {
                double eta;
                if (_fck <= 50.0)
                    eta = 1.0;
                else
                    eta = 1.0 - (_fck - 50.0) / 200;

                _fcd = eta * Standard.AlphaCC * Fck / Standard.GammaC;
            }
            else
            {
                _fcd = Standard.AlphaCC * Fck / Standard.GammaC;
            }
		}



        protected virtual double CalculateEpsilonC1()
        {
            if (_fck <= 50)
            {
                if (_fck <= 12.0)
                    return 1.8;
                else if (_fck <= 16.0)
                    return 1.9;
                else if (_fck <= 20.0)
                    return 2.0;
                else if (_fck <= 25.0)
                    return 2.1;
                else if (_fck <= 30.0)
                    return 2.2;
                else if (_fck <= 35.0)
                    return 2.25;
                else if (_fck <= 40.0)
                    return 2.3;
                else if (_fck <= 45.0)
                    return 2.4;
                else // if (_fck <= 50.0)
                    return 2.45;
            }
            else
                return 2.0 + 0.085 * Math.Pow(_fck - 50.0, 0.53);
        }

        protected virtual double CalculateEpsilonCu1()
        {
            if (_fck <= 50)
                return 3.5 / 10.0;
            else
                return (2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4)) / 10.0;
        }

        protected virtual double CalculateFckCube()
        {
            switch (_fck)
            {
                case (8):
                    return 10;
                case (12):
                    return 15;
                case (16):
                    return 20;
                case (20):
                    return 25;
                case (25):
                    return 30;
                case (30):
                    return 37;
                case (35):
                    return 45;
                case (40):
                    return 50;
                case (45):
                    return 55;
                case (50):
                    return 60;
                case (55):
                    return 67;
                case (60):
                    return 75;
                case (70):
                    return 85;
                case (80):
                    return 95;
                case (90):
                    return 105;
                case (100):
                    return 115;
                default:
                    return 1.0 / 0.83 * _fck;
            }
        }

        protected virtual double CalculateFctm()
        {
            if (_fck <= 50)
                return 0.30 * Math.Pow(_fck, 2.0 / 3.0);
            else
                return 2.12 * Math.Log(1 + (Fcm / 10.0));
        }

        protected virtual double CalculateN()
        {
            if (_fck <= 50)
                return 2.0;
            else
                return 1.4 + 13.4 * Math.Pow(((90.0 - _fck) / 100.0), 4);
        }

        protected virtual double CalculateEcm()
        {
            return 22.0 * Math.Pow(Fcm / 10.0, 0.30);
        }

        protected virtual double CalculateBetaCC(int days)
        {
            double s;
            if (TypeOfCement == TypeOfCements.ClassR)
                s = 0.20;
            else if (TypeOfCement == TypeOfCements.ClassN)
                s = 0.25;
            else //if (typeOfCement == TypeOfCement.ClassS)
                s = 0.38;

            return Math.Pow(Math.E, (s * (1 - Math.Pow(28 / days, 0.5))));
        }

        #endregion
    }    
}
