using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Standards;
using GPC.Utilities.Attributes;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterialACI318 : ConcreteMaterial, ISerializable
    {
        #region Variables

        protected double _fc;
        protected double _fct;
        protected double _fctu;
        protected double _concreteStrengthReduction;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic compressive cylinder strength of concrete at 28 days
        /// </summary>
        public double Fc 
        { 
            get => _fc; 
            set 
            {  
                if(_fc != value)
                {
                    _fc = value;
                    RecalculateMechanicalProperties();
                }
            } 
        }

        /// <summary>
        /// Characteristic tensile strength of concrete
        /// </summary>
        /// <remarks>Mean tensile strength at 28 days</remarks>
        public double Fct { get => _fct; set => _fct = value; }

        /// <summary>
        /// Ultimate strain in tension
        /// </summary>
        public double Fctu { get => _fctu; set => _fctu = value; }

        public double ConcreteStrengthReduction
        {
            get => _concreteStrengthReduction;
            set
            {
				_concreteStrengthReduction = value; 
                RecalculateMechanicalProperties();
			}
		}

		#endregion

		#region Constructor

		public ConcreteMaterialACI318(string name, double fc, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6)
            : base(name, poisson, density, alfaThermalExpansion)
        {
            _concreteStrengthReduction = 0.85;
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
			_concreteStrengthReduction = 0.85; 
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
			_concreteStrengthReduction = 0.85;
		}

        protected ConcreteMaterialACI318(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ConcreteMaterialACIVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version == 2)
            {
                _compressionStressStrainDiagrams = (CompressionStressStrainDiagrams)info.GetInt32("CompressionStressStrainDiagrams");
                _tensionStressStrainDiagrams = (TensionStressStrainDiagrams)info.GetInt32("TensionStressStrainDiagrams");
            }
            else if (version == 1) { }
            else if (version == 3) { }
			
            if (version > 3)             
				_concreteStrengthReduction= info.GetDouble("ConcreteStrengthReduction");			
            else
				_concreteStrengthReduction = 0.85;

			_fc = info.GetDouble("Fc");
            _fct = info.GetDouble("Fct");
            _fctu = info.GetDouble("Fctu");
        }

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

                    double eps0 = GetEpsilon0();
					for (int i = 0; i < strains.Length; i++)
                    {
                        stresses[i] = GetParabolaStress(strains[i], eps0, strainYCompression);
                    }

                    _stressStrainTableCompression = new StressStrainTable(stresses, strains);
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
                        stressesNl[i] = fc * (K * eta - eta * eta) / (1.0 + (K - 2.0) * eta);
                    }

                    _stressStrainTableCompression = new StressStrainTable(stressesNl, strainsNl);
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

                case TensionStressStrainDiagrams.Generic:
                    _stressStrainTableTension = new StressStrainTable();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Set <see cref="Material._elasticModulusTension"/>, <see cref="Material._elasticModulusCompression"/>
        /// <see cref="ConcreteMaterialEuropeanCommon._fctk"/>, 
        /// <see cref="ConcreteMaterialEuropeanCommon._fck"/>
        /// </summary>
        protected void SetMechanicalProperties(double fc, double fctk, double fFtu, double strainYTension, double strainUTension,
            CompressionStressStrainDiagrams compressionStressStrainDiagrams, TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.StressBlock:
                    _fc = fc;
                    _elasticModulusCompression = CalculateElasticModulus(fc);
                    _strainUCompression = GetStrainUCompression(compressionStressStrainDiagrams);
                    _strainYCompression = GetStrainYCompression(compressionStressStrainDiagrams, _strainUCompression);
                    break;

                case CompressionStressStrainDiagrams.Bilinear:
                case CompressionStressStrainDiagrams.ParabolaRectangle:

                    _fc = fc;
                    _elasticModulusCompression = CalculateElasticModulus(fc);
                    _strainUCompression = GetStrainUCompression(compressionStressStrainDiagrams);
                    _strainYCompression = GetStrainYCompression(compressionStressStrainDiagrams, _strainUCompression);
                    break;

                case CompressionStressStrainDiagrams.Generic:

                    _fc = _stressStrainTableCompression.GetMinimumStress(out double fckStrain);
                    _elasticModulusCompression = CalculateElasticModulus(fc);
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
        protected virtual double GetStrainYCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams, double strainU = 0)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.Bilinear:
                    return -Math.Abs(_fc) / CalculateElasticModulus(Math.Abs(_fc));

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
                    return GetEpsilon0();

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
                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    return -3.0 / 1000.0;

                case CompressionStressStrainDiagrams.Generic:
                    return _stressStrainTableCompression.Strains.Last();

                default:
                    throw new ArgumentException();
            }
        }

        protected virtual double GetEpsilon0()
        {
            return 2 * ConcreteStrengthReduction * _fc / CalculateElasticModulus(Math.Abs(_fc));
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
            return 7.5 * 1.0 * Math.Sqrt(Math.Abs(fc / 0.00689476)) * 0.00689476;
        }

        protected virtual double GetParabolaStress(double strain, double strain0, double strainY)
        {
            if (strainY == 0)
                throw new ArgumentException();

            if (Math.Abs(strain) > Math.Abs(strainY))
                return ConcreteStrengthReduction * _fc;
            else if (strain == 0)
                return 0;            
            else
                return ConcreteStrengthReduction * _fc * (2 * (strain / strain0) - Math.Pow(strain / strain0, 2));
        }

        protected virtual double GetFcm()
        {
            return Math.Sign(_fc) * (Math.Abs(_fc) + 8.0);
        }

        protected virtual double GetEcm(double fcm)
        {
            return Math.Abs(22.0 * Math.Pow(Math.Abs(fcm) / 10.0, 0.30) * 1000);
        }

        #endregion

        #region Public methods

        public override double CalculateDesignCompressiveStrength(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
            {
                if (CompressionStressStrainDiagram == CompressionStressStrainDiagrams.StressBlock)
                {
                    if (Math.Abs(Fc) > 90)
                        throw new ArgumentException("Fck > 90 not supported by Stress block");

                    double eta;
                    if (Math.Abs(Fc) <= 50.0)
                        eta = 1.0;
                    else
                        eta = 1.0 - (Math.Abs(Fc) - 50.0) / 200;

                    return eta * standardModelCode2010.AlphaCC * Fc / standardModelCode2010.GammaC;
                }
                else
                {
                    return standardModelCode2010.AlphaCC * Fc / standardModelCode2010.GammaC;
                }
            }
            else if (standard is StandardACI318 standardACI318)
            {
                return CalculateDesignCompressiveStrength(standardACI318);
            }
            else
                throw new ArgumentException();
        }

        public virtual double CalculateDesignCompressiveStrength(StandardACI318 standard)
        {       
            return standard.ConcreteStrengthReductionFactor * Fc;
        }

        public override double CalculateDesignTensileStrength(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
                return standardModelCode2010.AlphaCT * Fct / standardModelCode2010.GammaC;
            else if (standard is StandardACI318 standardACI318)
                return CalculateDesignTensileStrength(standardACI318);
            else
                throw new ArgumentException();
        }

        public virtual double CalculateDesignTensileStrength(StandardACI318 standard)
        {
            return standard.ConcreteStrengthReductionFactor * Fct;
        }

        public override double CalculateDesignStressConcrete(Standards.Standard standard, double strain)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
            {
                if (strain < 0)
                    return GetStress(strain) * Math.Abs(CalculateDesignCompressiveStrength(standardModelCode2010) / Fc);
                else
                    return GetStress(strain) * Math.Abs(CalculateDesignTensileStrength(standardModelCode2010) / Fct);
            }
            else if (standard is StandardACI318 standardACI318)
                return CalculateDesignStressConcrete(standardACI318, strain);
            else
                throw new ArgumentException();
        }

        public virtual double CalculateDesignStressConcrete(StandardACI318 standard, double strain)
        {   
            return standard.ConcreteStrengthReductionFactor * GetStress(strain);    
        }

        #endregion

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 4;

            info.AddValue("ConcreteMaterialACIVersion", version);

            info.AddValue("Fc", _fc);
            info.AddValue("Fct", _fct);
            info.AddValue("Fctu", _fctu);
            info.AddValue("ConcreteStrengthReduction", _concreteStrengthReduction);
		}

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialACI318 objCasted) &&
                objCasted._fc.Equals(_fc) &&
                objCasted._fct.Equals(_fct) &&
                objCasted._fctu.Equals(_fctu) &&
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
                return hashCode;
            }
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
