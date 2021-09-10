using GPC.Model.FEM.Materials;
using GPC.Model.Standards;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Concrete EN1992-1-1", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterialEN1992 : ConcreteMaterial
    {
		#region ENUMERATOR

        public enum StressStrainDiagram
		{
            ParabolaRectangle,
            Bilinear,
            StressBlock,
        }

        #endregion

        #region VARIABLES

        protected StandardEn1992p11 _standard;

        protected double _fckCube;
        protected double _fctk05;
        protected double _fctk95;
        protected double _fcm;
        protected double _fctm;

        protected double _eCm;
        protected double _eCd;
        protected double _eCEff;

        protected double _epsilonC1;
        protected double _epsilonCu1;
        protected double _n;
        protected double _epsilonC2;
        protected double _epsilonCu2;
        protected double _epsilonC3;
        protected double _epsilonCu3;
        protected double _lambda;
        protected double _eta;

        protected double _fcd;
        protected double _fctd;
        protected double _fcdAccidental;
        protected double _fctdAccidental;

        protected double _niCracked;

        #endregion VARIABLES

        #region PROPERTIES

        /// <summary>
        /// Standard EN1992 or a relative national annex
        /// </summary>
        public StandardEn1992p11 Standard => _standard;

        /// <summary>
        /// characteristic cubic strength
        /// </summary>
        public double FckCube => _fckCube;

        /// <summary>
        /// Characteristic tensile strength 0.05%
        /// </summary>
        public double Fctk05 => _fctk05;

        /// <summary>
        /// Characteristic tensile strength 0.95%
        /// </summary>
        public double Fctk95 => _fctk95;

        /// <summary>
        /// Mean compressive strength at 28 days
        /// </summary>
        public double Fcm => _fcm;

        /// <summary>
        /// Mean tensile strength at 28 days
        /// </summary>
        public double Fctm => _fctm;

        /// <summary>
        /// Secant modulus of elasticity value between sigmac = 0 and 0,4fcm
        /// </summary>
        public double ECm => _eCm;

        /// <summary>
        /// Modulus of elasticity value for ultimate limit state calculations
        /// </summary>
        public double ECd => _eCd;

        /// <summary>
        /// Modulus of elasticity value for long-term deflection calculations
        /// </summary>
        public double ECeff => _eCEff;

        /// <summary>
        /// EpsilonC1 in the parabola-rectangle stress-strain diagram
        /// </summary>
        public double EpsilonC1 => _epsilonC1;

        /// <summary>
        /// Exponent in the parabola-rectangle stress-strain diagram
        /// </summary>
        public double EpsilonCu1 => _epsilonCu1;

        /// <summary>
        /// EpsilonCu1 in the parabola-rectangle stress-strain diagram
        /// </summary>
        public double N => _n;

        /// <summary>
        /// EpsilonC2 in the bi-linear stress-strain diagram
        /// </summary>
        public double EpsilonC2 => _epsilonC2;

        /// <summary>
        /// EpsilonCu2 in the bi-linear stress-strain diagram
        /// </summary>
        public double EpsilonCu2 => _epsilonCu2;

        /// <summary>
        /// EpsilonC3 in the rectangular stress distribution
        /// </summary>
        public double EpsilonC3 => _epsilonC3;

        /// <summary>
        /// EpsilonCu3 in the rectangular stress distribution
        /// </summary>
        public double EpsilonCu3 => _epsilonCu3;

        /// <summary>
        /// Lambda in the rectangular stress distribution
        /// </summary>
        public double Lambda => _lambda;

        /// <summary>
        /// Eta in the rectangular stress distribution
        /// </summary>
        public double Eta => _eta;

        /// <summary>
        /// Design compressive strength for persistent design
        /// </summary>
        public double Fcd => _fcd;

        /// <summary>
        /// Design compressive strength for accidental design
        /// </summary>
        public double FcdAccidental => _fcdAccidental;

        /// <summary>
        /// Design tensile strength for persistent design
        /// </summary>
        public double Fctd => _fctd;

        /// <summary>
        /// Design tensile strength for accidental design
        /// </summary>
        public double FctdAccidental => _fctdAccidental;

        /// <summary>
        /// Poisson’s ratio for cracked concrete
        /// </summary>
        public double NiCracked => _niCracked;

        #endregion PROPERTIES

        #region CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="standard">Standard EN1992 or a relative national annex</param>
        public ConcreteMaterialEN1992(string name, double fck, StandardEn1992p11 standard)
            : base(name, fck)
        {
            _standard = standard;
            SetProperties();
        }

        public ConcreteMaterialEN1992(string name, double fck)
            : this(name, fck, new StandardEn1992p11())
		{

		}

        public ConcreteMaterialEN1992(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fck = info.GetDouble("Fck");
            _fckCube = info.GetDouble("FckCube");
            _fctk05 = info.GetDouble("Fctk05");
            _fctk95 = info.GetDouble("Fctk95");
            _fcm = info.GetDouble("Fcm");
            _fctm = info.GetDouble("Fctm");

            _eCm = info.GetDouble("Ecm");
            _eCd = info.GetDouble("Ecd");
            _eCEff = info.GetDouble("Eceff");

            _epsilonC1 = info.GetDouble("epsilonC1");
            _epsilonCu1 = info.GetDouble("epsilonCu1");
            _epsilonC2 = info.GetDouble("epsilonC2");
            _epsilonCu2 = info.GetDouble("epsilonCu2");
            _epsilonC3 = info.GetDouble("epsilonC3");
            _epsilonCu3 = info.GetDouble("epsilonCu3");
            _n = info.GetDouble("n");
            _lambda = info.GetDouble("lambda");
            _eta = info.GetDouble("eta");

            _niCracked = info.GetDouble("NiCracked");
        }

        #endregion CONSTRUCTORS

        #region PUBLIC METHODS

		#endregion

		#region PROTECTED METHODS

        protected virtual void SetProperties()
		{
            _fckCube = CalculateFckCube();
            _fcm = CalculateFcm();
            _fctm = CalculateFctm();
			_fctk05 = CalculateFctk05();
            _fctk95 = CalculateFctk95();
            _eCm = CalculateEcm();
            _eCd = CalculateEcd();
            
            _fcd = CalculateFcd();
            _fctd = CalculateFctd();
            _fcdAccidental = CalculateFcdAcc();
            _fctdAccidental = CalculateFctdAcc();

            _epsilonC1 = CalculateEpsilonC1();
            _epsilonCu1 = CalculateEpsilonCu1();
            _epsilonC2 = CalculateEpsilonC2();
            _epsilonCu2 = CalculateEpsilonCu2();
            _epsilonC3 = CalculateEpsilonC3();
            _epsilonCu3 = CalculateEpsilonCu3();
            _n = CalculateN();
            _lambda = CalculateLambda();
            _eta = CalculateEta();

            _ni = 0.2;
        }

        protected virtual double CalculateFckCube()
		{
            switch(_fck)
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

        protected virtual double CalculateFcm()
		{
            return _fck + 8;
		}

        protected virtual double CalculateFctm() 
        {
            if (_fck <= 50)
                return 0.30 * Math.Pow(_fck, 2.0 / 3.0);
            else
                return 2.12 * Math.Log(1 + (_fcm / 10.0));
        }

        protected virtual double CalculateFctk05()
		{
            return 0.7 * _fctm;
		}

        protected virtual double CalculateFctk95()
		{
            return 1.30 * _fctm;
		}

        protected virtual double CalculateEcm()
		{
            return 22.0 * Math.Pow(_fcm / 10.0, 0.30);
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
                return 3.5;
            else
                return 2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4);
        }

        protected virtual double CalculateEpsilonC2()
		{
            if (_fck <= 50)
                return 2.0;
            else
                return 2.0 + 0.085 * Math.Pow(_fck - 50.0, 0.53);
        }

        protected virtual double CalculateEpsilonCu2()
        {
            if (_fck <= 50)
                return 3.5;
            else
                return 2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4);
        }

        protected virtual double CalculateEpsilonC3()
        {
            if (_fck <= 50)
                return 1.75;
            else
                return 1.75 + 0.55 * ((_fck - 50.0) / 40.0);
        }

        protected virtual double CalculateEpsilonCu3()
        {
            if (_fck <= 50)
                return 3.5;
            else
                return 2.6 + 35.0 * Math.Pow(((90.0 - _fck) / 100.0), 4);
        }

        protected virtual double CalculateN()
		{
            if (_fck <= 50)
                return 2.0;
            else
                return 1.4 + 13.4 * Math.Pow(((90.0 - _fck) / 100.0), 4);
        }

        protected virtual double CalculateLambda()
		{
            if (_fck <= 50.0)
                return 0.8;
            else
                return 0.8 - (_fck - 50.0) / 400;
		}

        protected virtual double CalculateEta()
		{
            if (_fck <= 50.0)
                return 1.0;
            else
                return 1.0 - (_fck - 50.0) / 200;
        }

        protected virtual double CalculateFcd()
		{
            return Standard.AlphaCC * Fck / Standard.GammaC;
		}

        protected virtual double CalculateFctd()
		{
            return Standard.AlphaCT * _fctk05 / Standard.GammaC;
		}

        protected virtual double CalculateFcdAcc()
        {
            return Standard.AlphaCC * Fck / Standard.GammaCAccidental;
        }

        protected virtual double CalculateFctdAcc()
        {
            return Standard.AlphaCT * _fctk05 / Standard.GammaCAccidental;
        }

        protected virtual double CalculateEcd()
		{
            return ECeff / Standard.GammaCE;
		}

        #endregion
    }
}
