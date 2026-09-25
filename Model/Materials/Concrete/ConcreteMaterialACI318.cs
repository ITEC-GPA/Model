using GPC.Model.Standards;
using GPC.Utilities.Attributes;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// A concrete of ACI 318: Ec = 57000 sqrt(f'c) psi, fr = 7.5 sqrt(f'c) psi, Hognestad parabola, bilinear, stress block (β1) or generic diagrams.
    /// f'c and the stresses in compression are negative
    /// </summary>
    [Serializable]
    [UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterialACI318 : ConcreteMaterial, ISerializable
    {
        #region Variables

        /// <summary>
        /// The specified compressive strength f'c (negative)
        /// </summary>
        protected double _fc;
        /// <summary>
        /// The tensile strength (modulus of rupture fr for the plain concrete)
        /// </summary>
        protected double _fct;
        /// <summary>
        /// The ultimate (residual) tensile strength
        /// </summary>
        protected double _fctu;
        /// <summary>
        /// The factor of the peak of the parabola (f''c = 0.85 f'c): it defines ε0 = 2 × factor × f'c / Ec
        /// </summary>
        protected double _concreteStrengthReduction;

        #endregion

        #region Properties

        /// <summary>
        /// Specified compressive strength f'c (negative); the setter recalculates the mechanical properties
        /// </summary>
        public double Fc
        {
            get => _fc;
            set
            {
                if (_fc != value)
                {
                    _fc = value;
                    RecalculateMechanicalProperties();
                }
            }
        }

        /// <summary>
        /// Tensile strength of concrete (modulus of rupture 7.5 sqrt(f'c) psi for the plain concrete)
        /// </summary>
        public double Fct { get => _fct; set => _fct = value; }

        /// <summary>
        /// Ultimate (residual) tensile strength
        /// </summary>
        public double Fctu { get => _fctu; set => _fctu = value; }

        /// <summary>
        /// The factor of the peak of the parabola (default 0.85); the setter recalculates the mechanical properties
        /// </summary>
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

        /// <summary>
        /// Creates a plain concrete from f'c (tension: linear up to fr)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="fc">The specified compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
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

        /// <summary>
        /// Creates a fiber reinforced concrete from f'c and the residual strengths
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="fck">The specified compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="ffts">The tensile strength (peak)</param>
        /// <param name="fFtu">The ultimate residual strength</param>
        /// <param name="strainYTension">The strain at the peak (0: from Ec)</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="tensionStressStrainDiagrams">The diagram in tension</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
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

        /// <summary>
        /// Creates a concrete from its stress-strain tables and elastic constants
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="stressStrainTableCompression">The table in compression</param>
        /// <param name="stressStrainTableTension">The table in tension</param>
        /// <param name="elasticModulusCompression">The elastic modulus in compression</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public ConcreteMaterialACI318(string name, StressStrainTable stressStrainTableCompression,
            StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
            double poisson, double density, double alfaThermalExpansion)
            : base(name, stressStrainTableCompression, stressStrainTableTension, elasticModulusCompression, elasticModulusTension, poisson, density, alfaThermalExpansion)
        {
            _concreteStrengthReduction = 0.85;
        }

        /// <summary>
        /// Creates a concrete with f'c = 25 MPa and the parabola diagram
        /// </summary>
        /// <param name="name">The name</param>
        public ConcreteMaterialACI318(string name)
            : this(name, 25, CompressionStressStrainDiagrams.ParabolaRectangle)
        {
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="ConcreteMaterial"/>, f'c, fct, fctu and (version 4) the factor of the parabola
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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
                _concreteStrengthReduction = info.GetDouble("ConcreteStrengthReduction");
            else
                _concreteStrengthReduction = 0.85;

            _fc = info.GetDouble("Fc");
            _fct = info.GetDouble("Fct");
            _fctu = info.GetDouble("Fctu");
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Sets diagrams, mechanical properties and tables
        /// </summary>
        /// <param name="fck">The specified compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="ffts">The tensile strength (peak)</param>
        /// <param name="fFtu">The ultimate tensile strength</param>
        /// <param name="strainYTension">The strain at the peak</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="tensionStressStrainDiagrams">The diagram in tension</param>
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

        /// <summary>
        /// Recalculates the mechanical properties and the tables from f'c; the second call with zero tension values resets the tension to the linear
        /// diagram up to fr (also for the fiber reinforced concrete)
        /// </summary>
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

        /// <summary>
        /// The elastic modulus of ACI 318: 57000 sqrt(f'c [psi]) psi, in MPa
        /// </summary>
        /// <param name="fc">The compressive strength [MPa] (the sign is ignored)</param>
        /// <returns>Ec [MPa]</returns>
        protected double CalculateElasticModulus(double fc)
        {
            return 57000 * Math.Sqrt(Math.Abs(fc) / 0.00689476) * 0.00689476;
        }

        /// <summary>
        /// Builds the characteristic table in compression of a diagram: bilinear, stress block, generic (empty), Hognestad parabola (10 points),
        /// non linear (EN 1992-1-1 3.1.5)
        /// </summary>
        /// <param name="fc">The compressive strength (negative)</param>
        /// <param name="strainYCompression">The strain at the peak</param>
        /// <param name="strainUCompression">The ultimate strain</param>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        /// <exception cref="NotSupportedException">For an unknown diagram</exception>
        /// <remarks>Sign convention: Stress and Strain negative if compression</remarks>
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
        /// Sets f'c, the elastic moduli, the strains of the compression diagram and the strengths and strains in tension
        /// </summary>
        /// <param name="fc">The compressive strength (negative)</param>
        /// <param name="fctk">The tensile strength; 0: fr with a linear diagram</param>
        /// <param name="fFtu">The ultimate tensile strength</param>
        /// <param name="strainYTension">The strain at the tensile strength (0: from Ec)</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression (the non linear one is not supported)</param>
        /// <param name="tensionStressStrainDiagrams">The diagram in tension</param>
        /// <exception cref="NotSupportedException">For an unsupported diagram</exception>
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

        /// <summary>
        /// The factor β1 of the depth of the equivalent rectangular stress block (ACI 318-14/19 Table 22.2.2.4.3, SI): 0.85 for f'c up to 28 MPa,
        /// 0.85 - 0.05 (f'c - 28) / 7 between 28 and 55 MPa, 0.65 from 55 MPa. Before, 0.85 up to 30 MPa, 0.65 from 58 MPa and in between an
        /// interpolation with the arguments in the wrong order (e.g. -5451 for f'c 40 MPa instead of 0.764)
        /// </summary>
        /// <returns>β1</returns>
        public double GetBeta1()
        {
            double fc = Math.Abs(_fc);

            if (fc <= 28.0)
                return 0.85;

            if (fc >= 55.0)
                return 0.65;

            return 0.85 - 0.05 * (fc - 28.0) / 7.0;
        }

        /// <summary>
        /// The strain at the peak of a diagram: f'c / Ec (bilinear), (1 - β1) εcu (stress block), ε0 (parabola), the strain of the minimum stress (generic)
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        /// <param name="strainU">The ultimate strain (stress block)</param>
        /// <returns>The strain (negative)</returns>
        /// <exception cref="ArgumentException">For the other diagrams</exception>
        /// <remarks>Sign convention: Stress and strain negative if compression. β1 of the stress block: see <see cref="GetBeta1"/></remarks>
        protected virtual double GetStrainYCompression(CompressionStressStrainDiagrams compressionStressStrainDiagrams, double strainU = 0)
        {
            switch (compressionStressStrainDiagrams)
            {
                case CompressionStressStrainDiagrams.Bilinear:
                    return -Math.Abs(_fc) / CalculateElasticModulus(Math.Abs(_fc));

                case CompressionStressStrainDiagrams.StressBlock:
                    return strainU * (1.0 - GetBeta1());

                case CompressionStressStrainDiagrams.Generic:
                    _stressStrainTableCompression.GetMinimumStress(out double strain);
                    return strain;

                case CompressionStressStrainDiagrams.ParabolaRectangle:
                    return GetEpsilon0();

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// The ultimate strain of a diagram: 3 ‰ (the last strain of the table for the generic diagram)
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        /// <returns>The strain (negative)</returns>
        /// <exception cref="ArgumentException">For the other diagrams</exception>
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

        /// <summary>
        /// The strain ε0 of the peak of the Hognestad parabola: 2 × factor × f'c / Ec
        /// </summary>
        /// <returns>ε0 (negative)</returns>
        protected virtual double GetEpsilon0()
        {
            return 2 * ConcreteStrengthReduction * _fc / CalculateElasticModulus(Math.Abs(_fc));
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
        /// The modulus of rupture: 7.5 λ sqrt(f'c [psi]) psi (λ = 1), in MPa
        /// </summary>
        /// <param name="fc">The compressive strength [MPa] (the sign is ignored)</param>
        /// <returns>fr [MPa]</returns>
        protected virtual double GetFct(double fc)
        {
            return 7.5 * 1.0 * Math.Sqrt(Math.Abs(fc / 0.00689476)) * 0.00689476;
        }

        /// <summary>
        /// The stress of the Hognestad parabola: f'c (2 ε / ε0 - (ε / ε0)²), f'c beyond the strain of the peak
        /// </summary>
        /// <param name="strain">The strain</param>
        /// <param name="strain0">ε0</param>
        /// <param name="strainY">The strain of the peak</param>
        /// <returns>The stress (negative)</returns>
        /// <exception cref="ArgumentException">If <paramref name="strainY"/> is zero</exception>
        protected virtual double GetParabolaStress(double strain, double strain0, double strainY)
        {
            if (strainY == 0)
                throw new ArgumentException();

            if (Math.Abs(strain) > Math.Abs(strainY))
                return _fc;
            else if (strain == 0)
                return 0;
            else
                return _fc * (2 * (strain / strain0) - Math.Pow(strain / strain0, 2));
        }

        /// <summary>
        /// The mean compressive strength f'c - 8 (f'c negative), for the non linear diagram
        /// </summary>
        /// <returns>fcm (negative)</returns>
        protected virtual double GetFcm()
        {
            return Math.Sign(_fc) * (Math.Abs(_fc) + 8.0);
        }

        /// <summary>
        /// The secant modulus of Model Code 2010 for the non linear diagram: 22000 (|fcm| / 10)^0.3
        /// </summary>
        /// <param name="fcm">The mean compressive strength</param>
        /// <returns>Ecm [MPa]</returns>
        protected virtual double GetEcm(double fcm)
        {
            return Math.Abs(22.0 * Math.Pow(Math.Abs(fcm) / 10.0, 0.30) * 1000);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// The design compressive strength: αcc f'c / γc (× η for the stress block) for Model Code 2010, see the overload for ACI 318
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <returns>The design compressive strength (negative)</returns>
        /// <exception cref="ArgumentException">For the other standards, or f'c greater than 90 with the stress block</exception>
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

        /// <summary>
        /// The design compressive strength: the concrete strength reduction factor of the standard × f'c
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The design compressive strength (negative)</returns>
        public virtual double CalculateDesignCompressiveStrength(StandardACI318 standard)
        {
            return standard.ConcreteStrengthReductionFactor * Fc;
        }

        /// <summary>
        /// The design tensile strength: αct fct / γc for Model Code 2010, see the overload for ACI 318
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <returns>The design tensile strength</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
        public override double CalculateDesignTensileStrength(Standards.Standard standard)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
                return standardModelCode2010.AlphaCT * Fct / standardModelCode2010.GammaC;
            else if (standard is StandardACI318 standardACI318)
                return CalculateDesignTensileStrength(standardACI318);
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design tensile strength: the concrete strength reduction factor of the standard × fct
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <returns>The design tensile strength</returns>
        public virtual double CalculateDesignTensileStrength(StandardACI318 standard)
        {
            return standard.ConcreteStrengthReductionFactor * Fct;
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
        /// The design stress for a strain (see <see cref="CalculateDesignStressFromCharacteristic(StandardACI318, double)"/>)
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <param name="strain">The strain (negative in compression)</param>
        /// <returns>The design stress</returns>
        public double CalculateDesignStressConcrete(StandardACI318 standard, double strain)
        {
            return CalculateDesignStressFromCharacteristic(standard, GetStress(strain));
        }

        /// <summary>
        /// The design stress from the characteristic one: scaled by the ratio design / characteristic strength for Model Code 2010, see the
        /// overload for ACI 318
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010 or ACI 318)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <returns>The design stress</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
        public override double CalculateDesignStressFromCharacteristic(Standard standard, double stress)
        {
            if (standard is StandardModelCode2010 standardModelCode2010)
            {
                if (stress < 0)
                    return stress * Math.Abs(CalculateDesignCompressiveStrength(standardModelCode2010) / Fc);
                else
                    return stress * Math.Abs(CalculateDesignTensileStrength(standardModelCode2010) / Fct);
            }
            else if (standard is StandardACI318 standardACI318)
                return CalculateDesignStressFromCharacteristic(standardACI318, stress);
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// The design stress from the characteristic one: the concrete strength reduction factor of the standard × stress
        /// </summary>
        /// <param name="standard">The standard (ACI 318)</param>
        /// <param name="stress">The characteristic stress</param>
        /// <returns>The design stress</returns>
        public double CalculateDesignStressFromCharacteristic(StandardACI318 standard, double stress)
        {
            return standard.ConcreteStrengthReductionFactor * stress;
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the data of <see cref="ConcreteMaterial"/>, f'c, fct, fctu and the factor of the parabola (version 4)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Equality of f'c, fct, fctu and the data of <see cref="ConcreteMaterial"/>
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal concrete</returns>
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

        /// <summary>
        /// The hash code of the data of <see cref="ConcreteMaterial"/>, f'c, fct and fctu
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first concrete (not null, unless both are null)</param>
        /// <param name="obj2">The second concrete</param>
        /// <returns>True if the materials are equal</returns>
        public static bool operator ==(ConcreteMaterialACI318 obj1, ConcreteMaterialACI318 obj2)
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
        public static bool operator !=(ConcreteMaterialACI318 obj1, ConcreteMaterialACI318 obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
