using System;
using System.Runtime.Serialization;
using GPC.Model.FEM.Materials;
using GPC.Model.Standards;
using GPC.Utilities.Attributes;
using GPC.Utilities.Maths;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Concrete material in according to <see cref="StandardEN1992p11"/>
    /// </summary>
    /// <remarks>BS EN 1992-1-1:2004\AC:2014</remarks>
    [Serializable]
    [UI(Description = "Concrete EN1992-1-1", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterialEN1992 : ConcreteMaterialModelCode2010
    {
		#region Static Constructor

		public static ConcreteMaterialEN1992 C25_30 => new ConcreteMaterialEN1992(25, CompressionStressStrainDiagrams.ParabolaRectangle, "C25/30");
        public static ConcreteMaterialEN1992 C30_37 => new ConcreteMaterialEN1992(30, CompressionStressStrainDiagrams.ParabolaRectangle, "C30/37");
        public static ConcreteMaterialEN1992 C35_45 => new ConcreteMaterialEN1992(35, CompressionStressStrainDiagrams.ParabolaRectangle, "C35/45");
        public static ConcreteMaterialEN1992 C40_50 => new ConcreteMaterialEN1992(40, CompressionStressStrainDiagrams.ParabolaRectangle, "C40/50");
        public static ConcreteMaterialEN1992 C45_55 => new ConcreteMaterialEN1992(45, CompressionStressStrainDiagrams.ParabolaRectangle, "C45/55");
        public static ConcreteMaterialEN1992 C50_60 => new ConcreteMaterialEN1992(50, CompressionStressStrainDiagrams.ParabolaRectangle, "C50/60");
        public static ConcreteMaterialEN1992 C55_67 => new ConcreteMaterialEN1992(55, CompressionStressStrainDiagrams.ParabolaRectangle, "C55/67");
        public static ConcreteMaterialEN1992 C60_75 => new ConcreteMaterialEN1992(60, CompressionStressStrainDiagrams.ParabolaRectangle, "C60/75");
        public static ConcreteMaterialEN1992 C70_85 => new ConcreteMaterialEN1992(70, CompressionStressStrainDiagrams.ParabolaRectangle, "C70/85");
        public static ConcreteMaterialEN1992 C80_95 => new ConcreteMaterialEN1992(80, CompressionStressStrainDiagrams.ParabolaRectangle, "C80/90");
        public static ConcreteMaterialEN1992 C90_105 => new ConcreteMaterialEN1992(90, CompressionStressStrainDiagrams.ParabolaRectangle, "C90/105");

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
		/// <param name="stressStrainDiagram">The stress-strain diagram type</param>
		/// <param name="typeOfCement">The type of cement. See §3.4.1</param>
		/// <remarks>Elastic modulus is automatically calculated according to EN1992 §3 (Ecm)</remarks>
		public ConcreteMaterialEN1992(string name, double fck, double ni, double niCracked, double alphaT, double density,
            ConcreteMaterialModelCode2010.CompressionStressStrainDiagrams stressStrainDiagram, ConcreteMaterialModelCode2010.TypeOfCements typeOfCement)
            : base(name, fck, ni, niCracked, alphaT, density, stressStrainDiagram, typeOfCement)
        {

        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
        /// <param name="strainYCompression">Yielding compression strain</param>
        /// <param name="strainUCompression">Ultimate compression strain</param>
        /// <param name="strainYTension">Strain in the concrete at the peak tensile stress ftc</param>
        /// <param name="ni">Poisson's ratio</param>
        /// <param name="niCracked">Poisson's ratio in cracked concrete</param>
        /// <param name="alphaT">Linear thermal expasion coefficient</param>
        /// <param name="density">The density of concrete</param>
        /// <param name="stressStrainDiagram">The stress-strain diagram type</param>
        /// <param name="typeOfCement">The type of cement. See §3.4.1</param>
        public ConcreteMaterialEN1992(string name, double fck, double strainYCompression, double strainUCompression, 
            double strainYTension, double ni, double niCracked, double alphaT, double density,
            ConcreteMaterialModelCode2010.CompressionStressStrainDiagrams stressStrainDiagram, ConcreteMaterialModelCode2010.TypeOfCements typeOfCement)
            : base(name, fck, strainYCompression, strainUCompression, strainYTension, ni, niCracked, alphaT, density, stressStrainDiagram, typeOfCement)
        {

        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
		/// <param name="stressStrainDiagram">The stress-strain diagram type</param>
		/// <param name="name">Material name</param>
		/// <remarks>Value: ni = 0.2, niCracked = 0.0; alfaThermalExpansion = 1e-6; density = 0.0025 T/mm^3; standard = StandardEn1992p11; type of cements = classN</remarks>
		public ConcreteMaterialEN1992(double fck, CompressionStressStrainDiagrams stressStrainDiagram = CompressionStressStrainDiagrams.StressBlock, string name = null)
            : this(name, fck, 0.2, 0.0, 1e-6, 0.0025, stressStrainDiagram, TypeOfCements.ClassN)
        {

        }

        public ConcreteMaterialEN1992(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _fck = info.GetDouble("Fck");
                        
            _niCracked = info.GetDouble("NiCracked");
            _epsilonCompressionY = info.GetDouble("EpsilonY");
            _epsilonCompressionU = info.GetDouble("EpsilonU");
        }

        #endregion 

        #region Public Methods

        public override double CalculateEcm(int days)
        {
            return Math.Pow(CalculateFcm(days) / Fcm, 0.3) * E;
        }

        public override double CalculateFcm(int days)
        {
            return Fcm * CalculateBetaCC(days);
        }

        public override double CalculateFctm(int days)
        {
            double betaCC = CalculateBetaCC(days);
            double alpha;
            if (days < 28)
                alpha = 1.0;
            else
                alpha = 2.0 / 3.0;

            return Math.Pow(betaCC, alpha) * Fctk;
        }

        /// <summary>
        /// Calculate increased characteristic strength and strains of confined concrete 
        /// </summary>
        /// <param name="sigma2">The effective lateral compressive stress at the ULS due to confinement</param>
        /// <param name="epsilonCC">New compressive strain in the concrete at the peak stress fc</param>
        /// <param name="epsilonCuC">New ultimate compressive strain in the concrete</param>
        /// <returns>Thw new characteristic compressive cylinder strength of concrete at 28 days</returns>
        public override double CalculateConfinedConcreteResistance(double sigma2, out double epsilonCC, out double epsilonCuC)
        {
            double fckc;
            if (sigma2 <= 0.05 * Fck)
                fckc = Fck * (1.0 + 5.0 * sigma2 / Fck);
            else
                fckc = Fck * (1.125 + 2.5 * sigma2 / Fck);

            if (StressStrainDiagram == CompressionStressStrainDiagrams.ParabolaRectangle)
            {
                epsilonCC = EpsilonCompressionY * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;
            }
            else if (StressStrainDiagram == CompressionStressStrainDiagrams.Bilinear)
            {
                epsilonCC = EpsilonCompressionY * Math.Pow(fckc / Fck, 2.0);
                epsilonCuC = epsilonCC + 0.2 * sigma2 / Fck;
            }
            else
            {
                epsilonCC = EpsilonCompressionY * Math.Pow(fckc / Fck, 2.0);
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
        public override double CalculateEpsilonCCInfiniteTime(double sigmaC, double RH, double AreaC, double u, double T0 = 7, double deltaTemperature = 0, double deltaDaysTemperature = 0)
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
        public override double CalculateEpsilonCSInfiniteTime(double RH, double AreaC, double u)
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

        public override double CalculateN()
        {
            if (_fck <= 50)
                return 2.0;
            else
                return 1.4 + 13.4 * Math.Pow(((90.0 - _fck) / 100.0), 4);
        }

        #endregion

        #region Protected Methods

        protected override void CalculateEpsilonY()
		{
            if (StressStrainDiagram == CompressionStressStrainDiagrams.ParabolaRectangle)
            {
                if (_fck <= 50)
                    _epsilonCompressionY = - 2.0 / 1000.0;
                else
                    _epsilonCompressionY = - (2.0 + 0.085 * Math.Pow(_fck - 50.0, 0.53)) / 1000.0;
            }
            else if (StressStrainDiagram == CompressionStressStrainDiagrams.Bilinear)
            {
                if (_fck <= 50)
                    _epsilonCompressionY = - 1.75 / 1000.0;
                else
                    _epsilonCompressionY = - (1.75 + 0.55 * ((_fck - 50.0) / 40.0)) / 1000.0;
            }
            else if (StressStrainDiagram == CompressionStressStrainDiagrams.StressBlock)
            {
                double lambda;

                if (_fck <= 50.0)
                    lambda = 0.8;
                else
                    lambda = 0.8 - (_fck - 50.0) / 400;

                _epsilonCompressionY = - _epsilonCompressionU * (1 - lambda);
            }
            else
                throw new ArgumentException();
		}

        protected override void CalculateEpsilonU()
        {
            if (StressStrainDiagram == CompressionStressStrainDiagrams.ParabolaRectangle)
            {
                if (_fck <= 50)
                    _epsilonCompressionU = - 3.5 / 1000.0;
                else
                    _epsilonCompressionU = - (2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4)) / 1000.0;
            }
            else if (StressStrainDiagram == CompressionStressStrainDiagrams.Bilinear)
            {
                if (_fck <= 50)
                    _epsilonCompressionU = - 3.5 / 1000.0;
                else
                    _epsilonCompressionU = - (2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4)) / 1000.0;
            }
            else if (StressStrainDiagram == CompressionStressStrainDiagrams.StressBlock)
            {
                if (_fck <= 50)
                    _epsilonCompressionU = - 3.5 / 1000.0;
                else
                    _epsilonCompressionU = - (2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4)) / 1000.0;
            }
            else
                throw new ArgumentException();
        }

        protected override double CalculateFckCube()
        {
            switch (Fck)
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
                    return 1.0 / 0.83 * Fck;
            }
        }

        protected override double CalculateFctm()
        {
            if (_fck <= 50.0)
                return 0.30 * Math.Pow(Fck, 2.0 / 3.0);
            else
                return 2.12 * Math.Log(1 + (Fcm / 10.0));
        }

        protected override double CalculateEcm()
        {
            return 22.0 * Math.Pow(Fcm / 10.0, 0.30) * 1000;
        }

        protected override double CalculateBetaCC(int days)
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

        protected override void CalculateEpsilonTensionT()
		{
            _strainTensionY = Fctk / E;
		}

        #endregion
    }    
}
