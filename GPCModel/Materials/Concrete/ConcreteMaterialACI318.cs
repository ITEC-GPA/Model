using GPC.Model.Standards;
using GPC.Utilities.Converters;
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
    public class ConcreteMaterialACI318 : ConcreteMaterial, ISerializable
    {
        #region Public Enum        

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CompressionStressStrainDiagrams
        {
            [Description("Parabola-Rectangle")]
            ParabolaRectangle,

            [Description("Bilinear")]
            Bilinear,

            [Description("Stress Block")]
            StressBlock,

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

		#endregion

		#region Static Properties

		public static ConcreteMaterialACI318 Fc3000 => new ConcreteMaterialACI318("fc' 3000 psi", 20.6843, CompressionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialACI318 Fc4000 => new ConcreteMaterialACI318("fc' 4000 psi", 27.579, CompressionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialACI318 Fc5000 => new ConcreteMaterialACI318("fc' 5000 psi", 34.4738, CompressionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialACI318 Fc6000 => new ConcreteMaterialACI318("fc' 6000 psi", 41.3685, CompressionStressStrainDiagrams.Bilinear);

        #endregion

        #region Variables

        private double _fc;
        protected double _fct;
        protected double _fctu;

        protected double _strainYCompression;
        protected double _strainUCompression;

        protected double _strainYTension;
        protected double _strainUTension;

        protected CompressionStressStrainDiagrams _compressionStressStrainDiagrams;
        protected TensionStressStrainDiagrams _tensionStressStrainDiagrams;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic compressive cylinder strength of concrete at 28 days
        /// </summary>
        public double Fc  => _fc;

        /// <summary>
        /// Characteristic tensile strength of concrete
        /// </summary>
        /// <remarks>Mean tensile strength at 28 days</remarks>
        public double Fct => _fct;

        /// <summary>
        /// Ultimate strain in tension
        /// </summary>
        public double Fctu => _fctu;

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

        #endregion

        #region Constructor

        public ConcreteMaterialACI318(string name, double fc, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6)
            : base(name, poisson, density, alfaThermalExpansion)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = TensionStressStrainDiagrams.Linear;

            SetMechanicalProperties(-Math.Abs(fc), 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fc, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fct, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);
        }

        // Costruttore per cls frc
        public ConcreteMaterialACI318(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, TensionStressStrainDiagrams tensionStressStrainDiagrams,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6)
            : base(name, poisson, density, alfaThermalExpansion)
        {
            _tensionStressStrainDiagrams = tensionStressStrainDiagrams;
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;

            SetMechanicalProperties(-Math.Abs(fck), Math.Abs(ffts), Math.Abs(fFtu), Math.Abs(strainYTension), Math.Abs(strainUTension),
                _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fc, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fct, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);
        }

        public ConcreteMaterialACI318(string name, StressStrainTable stressStrainTableCompression,
            StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
            double poisson, double density, double alfaThermalExpansion)
            : base(name, stressStrainTableCompression, stressStrainTableTension, elasticModulusCompression, elasticModulusTension, poisson, density, alfaThermalExpansion)
        {

        }

        protected ConcreteMaterialACI318(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _fc = info.GetDouble("Fc");
            _fct = info.GetDouble("Fct");
            _fctu = info.GetDouble("Fctu");

            _strainYCompression = info.GetDouble("StrainYCompression");
            _strainUCompression = info.GetDouble("StrainUCompression");
            _strainYTension = info.GetDouble("StrainYTension");
            _strainUTension = info.GetDouble("StrainUTension");

            _compressionStressStrainDiagrams = (CompressionStressStrainDiagrams)info.GetInt32("CompressionStressStrainDiagrams");
            _tensionStressStrainDiagrams = (TensionStressStrainDiagrams)info.GetInt32("TensionStressStrainDiagrams");
        }

        #endregion

        #region Public methods

        #endregion

        #region Protected methods

        protected virtual void SetProperties(double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            _tensionStressStrainDiagrams = tensionStressStrainDiagrams;

            SetMechanicalProperties(-Math.Abs(fck), Math.Abs(ffts), Math.Abs(fFtu), Math.Abs(strainYTension),
                Math.Abs(strainUTension), compressionStressStrainDiagrams, tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fc, _strainYCompression, _strainUCompression, compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fct, _fctu, _strainYTension, _strainUTension, tensionStressStrainDiagrams);
        }

        public override void RecalculateMechanicalProperties()
        {
            SetMechanicalProperties(_fc, _fct, _fctu, _strainYTension, _strainUTension,
                _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fc, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fct, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

            SetMechanicalProperties(_fc, 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fc, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fct, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);
        }

        protected double CalculateElasticModulus(double fc)
		{
            return 57000 * Math.Sqrt(Math.Abs(fc) / 0.00689476) * 0.00689476;
		}

        /// <remarks> Sign convention: Stress and Strain negative if compression </remarks>
        protected void SetStressStrainTableCompression(double fc, double strainYCompression, double strainUCompression,
            CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.Bilinear:
                    _stressStrainTableCompression = new StressStrainTable(new double[] { 0, fc, fc },
                        new double[] { 0, strainYCompression, strainUCompression });
                    break;

                case CompressionStressStrainDiagrams.StressBlock:
                    _stressStrainTableCompression = new StressStrainTable(new double[] { 0, 0, fc, fc },
                        new double[] { 0, strainYCompression, strainYCompression, strainUCompression });
                    break;

                case CompressionStressStrainDiagrams.Generic:
                    _stressStrainTableCompression = new StressStrainTable();
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
        protected void SetMechanicalProperties(double fc, double fctk, double fFtu, double strainYTension, double strainUTension,
            CompressionStressStrainDiagrams compressionStressStrainDiagrams, TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.StressBlock:
                    _fc = 0.85 * fc;
                    _elasticModulus = CalculateElasticModulus(fc);
                    _strainUCompression = GetStrainUCompression(compressionStressStrainDiagrams);
                    _strainYCompression = GetStrainYCompression(compressionStressStrainDiagrams, _strainUCompression);
                    break;

                case CompressionStressStrainDiagrams.Bilinear:
                case CompressionStressStrainDiagrams.ParabolaRectangle:

                    _fc = fc;
                    _elasticModulus = CalculateElasticModulus(fc);
                    _strainUCompression = GetStrainUCompression(compressionStressStrainDiagrams);
                    _strainYCompression = GetStrainYCompression(compressionStressStrainDiagrams, _strainUCompression);
                    break;

                case CompressionStressStrainDiagrams.Generic:

                    _fc = _stressStrainTableCompression.GetMinimumStress(out double fckStrain);
                    _elasticModulus = CalculateElasticModulus(fc);
                    _strainUCompression = _stressStrainTableCompression.GetLastStrain();
                    _strainYCompression = fckStrain;
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (fctk == 0)
            {
                _fct = GetFct(fc);
                _fctu = _fct;
                _elasticModulusTension = CalculateElasticModulus(fc);
                _strainYTension = _fct / _elasticModulusTension;
                _strainUTension = _strainYTension;
            }
            else
            {
                switch (tensionStressStrainDiagrams)
                {
                    case TensionStressStrainDiagrams.Linear:
                        _fct = fctk;
                        _fctu = fctk;
                        _elasticModulusTension = strainYTension == 0 ? CalculateElasticModulus(fc) : fctk / strainYTension;

                        _strainYTension = _fct / _elasticModulusTension;
                        _strainUTension = _strainYTension;
                        break;

                    case TensionStressStrainDiagrams.Bilinear:
                        _fct = fctk;
                        _fctu = fFtu;
                        _elasticModulusTension = strainYTension == 0 ? CalculateElasticModulus(fc) : fctk / strainYTension;

                        _strainYTension = _fct / _elasticModulusTension;
                        _strainUTension = strainUTension;
                        break;

                    case TensionStressStrainDiagrams.Generic:
                        _fct = fctk;
                        _fctu = _stressStrainTableTension.GetLastStress();
                        _elasticModulusTension = strainYTension == 0 ? CalculateElasticModulus(fc) : fctk / strainYTension;

                        _strainYTension = _fct / _elasticModulusTension;
                        _strainUTension = _stressStrainTableTension.GetLastStrain();
                        break;

                    case TensionStressStrainDiagrams.RigidPlastic:
                        _fct = fctk;
                        _fct = fctk;
                        _elasticModulusTension = CalculateElasticModulus(fc);

                        _strainYTension = 0.0;
                        _strainUTension = strainUTension;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <remarks>Sign convention: Stress and strain negative if compression</remarks>
        protected virtual double GetStrainYCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double strainU = 0)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.Bilinear:
                    return - Math.Abs(_fc) / CalculateElasticModulus(Math.Abs(_fc));

                case CompressionStressStrainDiagrams.StressBlock:
                    {
                        double lambda;

                        if (Math.Abs(_fc) <= 30.0)
                            lambda = 0.85;
                        else if (Math.Abs(_fc) >= 58.0)
                            lambda = 0.65;
                        else
                            lambda = Utilities.Maths.Interpolation.GetLinearInterpolation(0.85, 0.65, 30.0, 58.0, Math.Abs(_fc));

                        return strainU * (1.0 - lambda);
                    }

                case CompressionStressStrainDiagrams.Generic:
                    _stressStrainTableCompression.GetMinimumStress(out double strain);
                    return strain;

                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    return -2.0 / 1000.0;

                default:
                    throw new ArgumentException();
            }
        }

        protected virtual double GetStrainUCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.StressBlock:
                case CompressionStressStrainDiagrams.Bilinear:
                    return -3.0 / 1000.0;

                case CompressionStressStrainDiagrams.Generic:
                    return _stressStrainTableCompression.Strains.Last();

                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    return -3.0 / 1000.0;

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

        protected virtual double GetFct(double fc)
		{
            return 7.5*1.0*Math.Sqrt(Math.Abs(fc / 0.00689476)) * 0.00689476;
		}

        protected virtual double GetParabolaStress(double strain, double strainY)
        {
            if (strainY == 0)
                throw new ArgumentException();

            if (Math.Abs(strain) > Math.Abs(strainY))
                return _fc;
            else if (strain == 0)
                return 0;
            else
                return _fc * (1.0 - Math.Pow(1.0 - Math.Abs(strain / strainY), 2.0));
        }

        #endregion

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fc", _fc);
            info.AddValue("Fct", _fct);
            info.AddValue("Fctu", _fctu);
            info.AddValue("StrainYCompression", _strainYCompression);
            info.AddValue("StrainUCompression", _strainUCompression);
            info.AddValue("StrainYTension", _strainYTension);
            info.AddValue("StrainUTension", _strainUTension);
            info.AddValue("CompressionStressStrainDiagrams", _compressionStressStrainDiagrams);
            info.AddValue("TensionStressStrainDiagrams", _tensionStressStrainDiagrams);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialACI318 objCasted) &&
                objCasted._fc.Equals(_fc) &&
               objCasted._fct.Equals(_fct) &&
               objCasted._fctu.Equals(_fctu) &&
               objCasted._strainUCompression.Equals(_strainUCompression) &&
               objCasted._strainYCompression.Equals(_strainYCompression) &&
               objCasted._strainYTension.Equals(_strainYTension) &&
               objCasted._strainUTension.Equals(_strainUTension) &&
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
                hashCode = hashCode * -17 + _fc.GetHashCode();
                hashCode = hashCode * -17 + _fct.GetHashCode();
                hashCode = hashCode * -17 + _fctu.GetHashCode();
                hashCode = hashCode * -17 + _strainUCompression.GetHashCode();
                hashCode = hashCode * -17 + _strainYCompression.GetHashCode();
                hashCode = hashCode * -17 + _strainYTension.GetHashCode();
                hashCode = hashCode * -17 + _strainYCompression.GetHashCode();
                hashCode = hashCode * -17 + _compressionStressStrainDiagrams.GetHashCode();
                hashCode = hashCode * -17 + _tensionStressStrainDiagrams.GetHashCode();
                return hashCode;
            }
        }

		public override double CalculateDesignStressConcrete(Standards.Standard standard, double strain)
		{
			throw new NotImplementedException();
		}

		public override double CalculateFcd(Standards.Standard standard)
		{
			throw new NotImplementedException();
		}

		public override double CalculateFctd(Standards.Standard standard)
		{
			throw new NotImplementedException();
		}

		public override double CalculateFcdAccidental(Standards.Standard standard)
		{
			throw new NotImplementedException();
		}

		public override double CalculateFctdAccidental(Standards.Standard standard)
		{
			throw new NotImplementedException();
		}

		public override double CalculateECd(Standards.Standard standard)
		{
			throw new NotImplementedException();
		}

		public static bool operator ==(ConcreteMaterialACI318 obj1, ConcreteMaterialACI318 obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcreteMaterialACI318 obj1, ConcreteMaterialACI318 obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
