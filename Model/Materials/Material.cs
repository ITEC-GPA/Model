using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// A material: elastic moduli in compression and tension, yield and ultimate strains and stresses, characteristic stress-strain tables,
    /// Poisson's ratio, thermal expansion and density. Strains and stresses are positive in tension, negative in compression
    /// </summary>
    [Serializable]
    public class Material : ModelObject, ISerializable
    {
        #region Variables

        /// <summary>
        /// The elastic modulus in compression
        /// </summary>
        protected double _elasticModulusCompression;
        /// <summary>
        /// The elastic modulus in tension
        /// </summary>
        protected double _elasticModulusTension;

        /// <summary>
        /// The strain at the yield stress in compression
        /// </summary>
        protected double _strainYCompression;
        /// <summary>
        /// The ultimate strain in compression
        /// </summary>
        protected double _strainUCompression;

        /// <summary>
        /// The strain at the yield stress in tension
        /// </summary>
        protected double _strainYTension;
        /// <summary>
        /// The ultimate strain in tension
        /// </summary>
        protected double _strainUTension;

        /// <summary>
        /// The yield stress in compression
        /// </summary>
        protected double _stressYCompression;
        /// <summary>
        /// The ultimate stress in compression
        /// </summary>
        protected double _stressUCompression;

        /// <summary>
        /// The yield stress in tension
        /// </summary>
        protected double _stressYTension;
        /// <summary>
        /// The ultimate stress in tension
        /// </summary>
        protected double _stressUTension;

        /// <summary>
        /// The Poisson's ratio
        /// </summary>
        protected double _ni;
        /// <summary>
        /// The coefficient of thermal expansion
        /// </summary>
        protected double _alfaThermalExpansion;
        /// <summary>
        /// The density
        /// </summary>
        protected double _density;

        /// <summary>
        /// The characteristic stress-strain table in compression
        /// </summary>
        protected StressStrainTable _stressStrainTableCompression;
        /// <summary>
        /// The characteristic stress-strain table in tension
        /// </summary>
        protected StressStrainTable _stressStrainTableTension;

        /// <summary>
        /// True if the properties are derived from the main characteristics according to the standard (see <see cref="AccordingToStandard"/>)
        /// </summary>
        protected bool _accordingToStandard;
        /// <summary>
        /// True for the materials of the standards, not editable by the user (see <see cref="IsReadOnly"/>)
        /// </summary>
        protected bool _isReadOnly;

        #endregion

        #region Properties

        /// <summary>
        /// Elastic modulus of material in compression
        /// </summary>
        public virtual double ElasticModulusCompression { get => _elasticModulusCompression; set => _elasticModulusCompression = value; }

        /// <summary>
        /// Elastic modulus of material in tension
        /// </summary>
        public double ElasticModulusTension { get => _elasticModulusTension; set => _elasticModulusTension = value; }

        /// <summary>
        /// The elastic modulus, when it is the same in compression and in tension; 0 otherwise
        /// </summary>
        public double E
        {
            get
            {
                if (_elasticModulusCompression == _elasticModulusTension)
                    return _elasticModulusCompression;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Strain in the material at the yielding stress in compression
        /// </summary>
        public double StrainYCompression { get => _strainYCompression; set => _strainYCompression = value; }

        /// <summary>
        /// Ultimate strain in compression
        /// </summary>
        public double StrainUCompression { get => _strainUCompression; set => _strainUCompression = value; }

        /// <summary>
        /// Strain in the material at the yielding stress in tension
        /// </summary>
        public double StrainYTension { get => _strainYTension; set => _strainYTension = value; }

        /// <summary>
        /// Ultimate strain in tension
        /// </summary>
        public double StrainUTension { get => _strainUTension; set => _strainUTension = value; }

        /// <summary>
        /// Yielding stress in compression
        /// </summary>
        public double StressYCompression { get => _stressYCompression; set => _stressYCompression = value; }

        /// <summary>
        /// Ultimate stress in compression
        /// </summary>
        public double StressUCompression { get => _stressUCompression; set => _stressUCompression = value; }

        /// <summary>
        /// Yielding stress in tension
        /// </summary>
        public double StressYTension { get => _stressYTension; set => _stressYTension = value; }

        /// <summary>
        /// Ultimate stress in tension
        /// </summary>
        public double StressUTension { get => _stressUTension; set => _stressUTension = value; }

        /// <summary>
        /// Poisson's ratio of material
        /// </summary>
        public double Ni { get => _ni; set => _ni = value; }

        /// <summary>
        /// Alfa thermal expansion coefficient of material
        /// </summary>
        public double AlfaThermalExpansion { get => _alfaThermalExpansion; set => _alfaThermalExpansion = value; }

        /// <summary>
        /// Density of material
        /// </summary>
        public double Density { get => _density; set => _density = value; }

        /// <summary>
        /// Characteristic Stress strain table in compression
        /// </summary>
        public StressStrainTable StressStrainTableCompression { get => _stressStrainTableCompression; set => _stressStrainTableCompression = value; }

        /// <summary>
        /// Characteristic Stress strain table in tension
        /// </summary>
        public StressStrainTable StressStrainTableTension { get => _stressStrainTableTension; set => _stressStrainTableTension = value; }

        /// <summary>
        /// It is used to say whether this material is taken from the standard and created by us, so it is not editable.
        /// If true --> not user-modifiable, created by us and defined by standard.
        /// If false --> materials added and editable by user.
        /// </summary>
        public bool IsReadOnly { get => _isReadOnly; set => _isReadOnly = value; }

        /// <summary>
        /// Set whether the material is entirely defined through its main characteristics or user-defined.<br/>
        /// If true then in the case of steel the properties will be derived from the f_y of yield and f_u of fracture.<br/>
        /// If true, then for concrete the properties will be derived from the f_ck characteristic strength.
        /// </summary>
        public bool AccordingToStandard { get => _accordingToStandard; set => _accordingToStandard = value; }

        #endregion

        #region Public Constructor

        /// <summary>
        /// Creates a material from its stress-strain tables and elastic constants (read only, according to the standard)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="stressStrainTableCompression">The characteristic stress-strain table in compression</param>
        /// <param name="stressStrainTableTension">The characteristic stress-strain table in tension</param>
        /// <param name="elasticModulusCompression">The elastic modulus in compression</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="poisson">The Poisson's ratio, from 0 to 0.5</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <exception cref="ArgumentException">If the Poisson's ratio is out of [0, 0.5] or a modulus, the density or the thermal coefficient is negative</exception>
        public Material(string name, StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, double elasticModulusCompression,
            double elasticModulusTension, double poisson, double density, double alfaThermalExpansion)
            : base(name)
        {
            if (poisson > 0.5)
                throw new ArgumentException($"{nameof(poisson)} cannot be greater than 0.5");

            _ni = poisson < 0 ? throw new ArgumentException($"Poisson cannot be lower than zero") : poisson;
            _alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be lower than zero") : alfaThermalExpansion;
            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;

            _stressStrainTableCompression = stressStrainTableCompression;
            _stressStrainTableTension = stressStrainTableTension;

            _elasticModulusTension = elasticModulusTension < 0 ? throw new ArgumentException($"{nameof(elasticModulusTension)} cannot be lower than zero") : elasticModulusTension;
            _elasticModulusCompression = elasticModulusCompression < 0 ? throw new ArgumentException($"{nameof(elasticModulusCompression)} cannot be lower than zero") : elasticModulusCompression;

            _isReadOnly = true;
            _accordingToStandard = true;
        }

        /// <summary>
        /// Creates a linear elastic material with the same modulus in compression and tension and empty stress-strain tables
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Elastic Modulus [MPa]</param>
        /// <param name="poisson">Poisson's ratio, from 0 to 0.5</param>
        /// <param name="density">Density [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Thermal expansion constant</param>
        /// <exception cref="ArgumentException">If the Poisson's ratio is out of [0, 0.5] or a value is negative</exception>
        public Material(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : this(name, new StressStrainTable(null, null), new StressStrainTable(null, null), elasticModulus, elasticModulus,
                  poisson, density, alfaThermalExpansion)
        {
        }

        /// <summary>
        /// Creates a material with only the name (read only, according to the standard): the derived classes set the properties
        /// </summary>
        /// <param name="name">The name</param>
        protected Material(string name)
            : base(Guid.NewGuid(), name)
        {
            _isReadOnly = true;
            _accordingToStandard = true;
        }

        /// <summary>
        /// Creates a material from all its properties (read only, according to the standard)
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
        /// <param name="stressStrainTableTension">The characteristic stress-strain table in tension</param>
        /// <param name="poisson">The Poisson's ratio, from 0 to 0.5</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="density">The density</param>
        /// <exception cref="ArgumentException">If the Poisson's ratio is out of [0, 0.5] or a modulus, the density or the thermal coefficient is negative</exception>
        protected Material(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension,
            double poisson, double alfaThermalExpansion, double density)
            : base(name)
        {
            if (poisson > 0.5)
                throw new ArgumentException($"{nameof(poisson)} cannot be greater than 0.5");

            _elasticModulusTension = elasticModulusTension < 0 ? throw new ArgumentException($"{nameof(elasticModulusTension)} cannot be lower than zero") : elasticModulusTension;
            _elasticModulusCompression = elasticModulusCompression < 0 ? throw new ArgumentException($"{nameof(elasticModulusCompression)} cannot be lower than zero") : elasticModulusCompression;

            _strainYCompression = strainYCompression;
            _strainUCompression = strainUCompression;
            _strainYTension = strainYTension;
            _strainUTension = strainUTension;
            _stressYCompression = stressYCompression;
            _stressUCompression = stressUCompression;
            _stressYTension = stressYTension;
            _stressUTension = stressUTension;

            _ni = poisson < 0 ? throw new ArgumentException($"Poisson cannot be lower than zero") : poisson;
            _alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be lower than zero") : alfaThermalExpansion;
            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;

            _stressStrainTableCompression = stressStrainTableCompression;
            _stressStrainTableTension = stressStrainTableTension;

            _isReadOnly = true;
            _accordingToStandard = true;
        }

        /// <summary>
        /// Deserialization constructor: reads the data according to the version (1: only one elastic modulus; 2: moduli, strains, stresses and tables;
        /// 3: read only flag; 4: according to standard flag)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Material(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            double version;
            try
            {
                version = info.GetInt64("MaterialVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version >= 2)
            {
                _elasticModulusCompression = info.GetDouble("ElasticModulusCompression");
                _elasticModulusTension = info.GetDouble("ElasticModulusTension");

                _strainYCompression = info.GetDouble("StrainYCompression");
                _strainUCompression = info.GetDouble("StrainUCompression");
                _strainYTension = info.GetDouble("StrainYTension");
                _strainUTension = info.GetDouble("StrainUTension");

                _stressYCompression = info.GetDouble("StressYCompression");
                _stressUCompression = info.GetDouble("StressUCompression");
                _stressYTension = info.GetDouble("StressYTension");
                _stressUTension = info.GetDouble("StressUTension");

                _stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
                _stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
            }
            else
            {
                _elasticModulusCompression = info.GetDouble("ElasticModulus");
            }

            _alfaThermalExpansion = info.GetDouble("AlfaThermalExpansion");
            _density = info.GetDouble("Density");
            _ni = info.GetDouble("Ni");

            if (version >= 3)
                _isReadOnly = info.GetBoolean("IsReadOnly");
            else
                _isReadOnly = true;

            if (version >= 4)
                _accordingToStandard = info.GetBoolean("AccordingToStandard");
            else
                _accordingToStandard = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The shear modulus of an isotropic material: E / (2 (1 + ν)), with the modulus in compression
        /// </summary>
        /// <returns>The shear modulus</returns>
        public virtual double GetShearModule()
        {
            return ElasticModulusCompression / (2.0 * (1.0 + Ni));
        }

        /// <summary>
        /// Changes the name (null is ignored)
        /// </summary>
        /// <param name="name">The new name</param>
        public void SetName(string name)
        {
            if (name != null)
                _name = name;
        }

        /// <summary>
        /// The characteristic stress for a strain, from the table in tension (positive strain) or in compression
        /// </summary>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <returns>The characteristic stress related to <paramref name="strain"/></returns>
        public double GetStress(double strain)
        {
            if (strain > 0)
            {
                return StressStrainTableTension.GetStress(strain);
            }
            else
            {
                return StressStrainTableCompression.GetStress(strain);
            }
        }

        /// <summary>
        /// Sets the yield and ultimate stresses from the tables at the yield and ultimate strains
        /// </summary>
        protected virtual void SetStressProperties()
        {
            _stressYCompression = _stressStrainTableCompression.GetStress(_strainYCompression);
            _stressUCompression = _stressStrainTableCompression.GetStress(_strainUCompression);
            _stressYTension = _stressStrainTableTension.GetStress(_strainYTension);
            _stressUTension = _stressStrainTableTension.GetStress(_strainUTension);
        }

        /// <summary>
        /// The elastic matrix of plane stress of a linear elastic isotropic material: E / (1 - ν²) [[1, ν, 0], [ν, 1, 0], [0, 0, (1 - ν) / 2]]
        /// </summary>
        /// <returns>The 3x3 matrix (zero if the moduli in compression and tension are different, see <see cref="E"/>)</returns>
        public virtual Matrix<double> GetPlaneStress()
        {
            Matrix<double> D = Matrix<double>.Build.Dense(3, 3);
            D[0, 0] = 1.0;
            D[0, 1] = Ni;
            D[1, 0] = Ni;
            D[1, 1] = 1.0;
            D[2, 2] = (1.0 - Ni) / 2.0;
            D = E / (1.0 - Ni * Ni) * D;
            return D;
        }

        /// <summary>
        /// The elastic matrix of a linear elastic isotropic solid (6x6: normal and shear components)
        /// </summary>
        /// <returns>The matrix (zero if the moduli in compression and tension are different, see <see cref="E"/>)</returns>
        /// <remarks>reference eq. 11.10 - Finite element method by Rao</remarks>
        public Matrix<double> GetBrickD()
        {
            double factor = E / ((1.0 + Ni) * (1.0 - 2.0 * Ni));

            Matrix<double> d = Matrix<double>.Build.Dense(6, 6);

            d[0, 0] = 1.0 - Ni;
            d[0, 1] = Ni;
            d[0, 2] = Ni;

            d[1, 0] = Ni;
            d[1, 1] = 1.0 - Ni;
            d[1, 2] = Ni;

            d[2, 0] = Ni;
            d[2, 1] = Ni;
            d[2, 2] = 1.0 - Ni;

            d[3, 3] = (1.0 - 2.0 * Ni) / 2.0;

            d[4, 4] = (1.0 - 2.0 * Ni) / 2.0;

            d[5, 5] = (1.0 - 2.0 * Ni) / 2.0;

            return factor * d;
        }

        #endregion

        #region Equals - HashCode - Operators

        /// <summary>
        /// Serializes the data of <see cref="ModelObject"/> and the properties (version 4)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 4;

            info.AddValue("MaterialVersion", version);

            info.AddValue("AlfaThermalExpansion", _alfaThermalExpansion);
            info.AddValue("Density", _density);

            info.AddValue("ElasticModulusCompression", _elasticModulusCompression);
            info.AddValue("ElasticModulusTension", _elasticModulusTension);

            info.AddValue("StrainYCompression", _strainYCompression);
            info.AddValue("StrainUCompression", _strainUCompression);
            info.AddValue("StrainYTension", _strainYTension);
            info.AddValue("StrainUTension", _strainUTension);

            info.AddValue("StressYCompression", _stressYCompression);
            info.AddValue("StressUCompression", _stressUCompression);
            info.AddValue("StressYTension", _stressYTension);
            info.AddValue("StressUTension", _stressUTension);

            info.AddValue("Ni", _ni);
            info.AddValue("TableCompression", _stressStrainTableCompression);
            info.AddValue("TableTension", _stressStrainTableTension);

            info.AddValue("IsReadOnly", _isReadOnly);
            info.AddValue("AccordingToStandard", _accordingToStandard);
        }

        /// <summary>
        /// The hash code of name, elastic moduli, Poisson's ratio, thermal coefficient, density and tables
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _elasticModulusCompression.GetHashCode();
                hashCode = hashCode * -17 + _elasticModulusTension.GetHashCode();
                hashCode = hashCode * -17 + _ni.GetHashCode();
                hashCode = hashCode * -17 + _alfaThermalExpansion.GetHashCode();
                hashCode = hashCode * -17 + _density.GetHashCode();
                hashCode = hashCode * -17 + _stressStrainTableCompression.GetHashCode();
                hashCode = hashCode * -17 + _stressStrainTableTension.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality of name and of all the properties (exact values)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal material</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is Material material &&
                   base.Equals(obj) &&
                   _elasticModulusCompression == material._elasticModulusCompression &&
                   _elasticModulusTension == material._elasticModulusTension &&
                   _strainYCompression == material._strainYCompression &&
                   _strainUCompression == material._strainUCompression &&
                   _strainYTension == material._strainYTension &&
                   _strainUTension == material._strainUTension &&
                   _stressYCompression == material._stressYCompression &&
                   _stressUCompression == material._stressUCompression &&
                   _stressYTension == material._stressYTension &&
                   _stressUTension == material._stressUTension &&
                   _ni == material._ni &&
                   _alfaThermalExpansion == material._alfaThermalExpansion &&
                   _density == material._density &&
                   _accordingToStandard == material._accordingToStandard &&
                   EqualityComparer<StressStrainTable>.Default.Equals(_stressStrainTableCompression, material._stressStrainTableCompression) &&
                   EqualityComparer<StressStrainTable>.Default.Equals(_stressStrainTableTension, material._stressStrainTableTension);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null materials are equal
        /// </summary>
        /// <param name="obj1">The first material</param>
        /// <param name="obj2">The second material</param>
        /// <returns>True if the materials are equal</returns>
        public static bool operator ==(Material obj1, Material obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first material</param>
        /// <param name="obj2">The second material</param>
        /// <returns>True if the materials are different</returns>
        public static bool operator !=(Material obj1, Material obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
