using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Base of the concrete materials: type (plain or fiber reinforced), stress-strain diagrams in compression and tension and the design values
    /// of the standards
    /// </summary>
    [Serializable]
    public abstract class ConcreteMaterial : Material, ISerializable
    {
        #region Public Enum        

        /// <summary>
        /// The types of concrete
        /// </summary>
        public enum ConcreteTypes
        {
            /// <summary>Plain concrete</summary>
            [Description("Concrete")] Concrete,
            /// <summary>Fiber reinforced concrete (residual tensile strength)</summary>
            [Description("Fiber-Reinforced")] FRC,
        }

        /// <summary>
        /// The stress-strain diagrams in compression
        /// </summary>
        public enum CompressionStressStrainDiagrams
        {
            /// <summary>Parabola-rectangle</summary>
            [Description("Parabola-Rectangle")] ParabolaRectangle,
            /// <summary>Bilinear</summary>
            [Description("Bilinear")] Bilinear,
            /// <summary>Rectangular stress block</summary>
            [Description("Stress Block")] StressBlock,
            /// <summary>Non linear (for the structural analysis)</summary>
            [Description("Non Linear")] NonLinear,
            /// <summary>Generic (user defined table)</summary>
            [Description("Generic")] Generic,
        }

        /// <summary>
        /// The stress-strain diagrams in tension
        /// </summary>
        public enum TensionStressStrainDiagrams
        {
            /// <summary>Linear up to the tensile strength</summary>
            [Description("Linear")] Linear,
            /// <summary>Bilinear</summary>
            [Description("Bilinear")] Bilinear,
            /// <summary>Rigid-plastic</summary>
            [Description("Rigid-Plastic")] RigidPlastic,
            /// <summary>Generic (user defined table)</summary>
            [Description("Generic")] Generic,
        }

        /// <summary>
        /// The classes of cement (strength development)
        /// </summary>
        public enum CementTypes
        {
            /// <summary>Rapid hardening</summary>
            ClassR,
            /// <summary>Normal hardening</summary>
            ClassN,
            /// <summary>Slow hardening</summary>
            ClassS,
        }

        #endregion

        #region Variables

        /// <summary>
        /// The type of concrete
        /// </summary>
        protected ConcreteTypes _concreteType;
        /// <summary>
        /// The diagram in compression
        /// </summary>
        protected CompressionStressStrainDiagrams _compressionStressStrainDiagrams;
        /// <summary>
        /// The diagram in tension
        /// </summary>
        protected TensionStressStrainDiagrams _tensionStressStrainDiagrams;

        #endregion

        #region Properties

        /// <summary>
        /// Type of concrete; the setter recalculates the mechanical properties
        /// </summary>
        public ConcreteTypes ConcreteType
        {
            get => _concreteType;
            set
            {
                SetConcreteType(value);
                RecalculateMechanicalProperties();
            }
        }

        /// <summary>
        /// The compression stress-strain relationship; the setter recalculates the mechanical properties
        /// </summary>
        public CompressionStressStrainDiagrams CompressionStressStrainDiagram
        {
            get => _compressionStressStrainDiagrams;
            set
            {
                SetCompressionStressStrainDiagram(value);
                RecalculateMechanicalProperties();
            }
        }

        /// <summary>
        /// The tension stress-strain relationship; the setter recalculates the mechanical properties
        /// </summary>
        public TensionStressStrainDiagrams TensionStressStrainDiagram
        {
            get => _tensionStressStrainDiagrams;
            set
            {
                SetTensionStressStrainDiagrams(value);
                RecalculateMechanicalProperties();
            }
        }

        #endregion

        #region Public Constructor

        /// <summary>
        /// Creates a concrete from its stress-strain tables and elastic constants
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="stressStrainTableCompression">The characteristic stress-strain table in compression</param>
        /// <param name="stressStrainTableTension">The characteristic stress-strain table in tension</param>
        /// <param name="elasticModulusCompression">The elastic modulus in compression</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public ConcreteMaterial(string name, StressStrainTable stressStrainTableCompression,
            StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
            double poisson, double density, double alfaThermalExpansion)
            : base(name, stressStrainTableCompression, stressStrainTableTension, elasticModulusCompression, elasticModulusTension,
                  poisson, density, alfaThermalExpansion)
        {
        }

        /// <summary>
        /// Creates a concrete with one elastic modulus (the derived classes set the other properties)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">The elastic modulus</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        protected ConcreteMaterial(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion)
        {
        }

        /// <summary>
        /// Creates a concrete with zero elastic modulus (the derived classes compute it)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        protected ConcreteMaterial(string name, double poisson, double density, double alfaThermalExpansion)
            : base(name, 0, poisson, density, alfaThermalExpansion)
        {
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
        /// <param name="stressStrainTableCompression">The characteristic stress-strain table in compression</param>
        /// <param name="stressStrainTableTension">The characteristic stress-strain table in tension</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="density">The density</param>
        protected ConcreteMaterial(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson, double alfaThermalExpansion, double density)
            : base(name, elasticModulusCompression, elasticModulusTension, strainYCompression,
                  strainUCompression, strainYTension, strainUTension, stressYCompression,
                  stressUCompression, stressYTension, stressUTension, stressStrainTableCompression,
                  stressStrainTableTension, poisson, alfaThermalExpansion, density)
        {
            _concreteType = concreteType;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Material"/> and the version (1: tables and strains, type deduced from them;
        /// 2: type; 3: diagrams)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ConcreteMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ConcreteMaterialVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version == 1)
            {
                _stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
                _stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
                _elasticModulusTension = info.GetDouble("ElasticModulusTension");

                _strainYCompression = info.GetDouble("StrainYCompression");
                _strainUCompression = info.GetDouble("StrainUCompression");
                _strainYTension = info.GetDouble("StrainYTension");
                _strainUTension = info.GetDouble("StrainUTension");

                if (_strainYTension < _strainUTension)
                    _concreteType = ConcreteTypes.FRC;
                else
                    _concreteType = ConcreteTypes.Concrete;

                SetStressProperties();
            }
            if (version >= 2)
            {
                _concreteType = (ConcreteTypes)info.GetValue("ConcreteType", typeof(ConcreteTypes));
            }
            if (version >= 3)
            {
                _compressionStressStrainDiagrams = (CompressionStressStrainDiagrams)info.GetInt32("CompressionStressStrainDiagrams");
                _tensionStressStrainDiagrams = (TensionStressStrainDiagrams)info.GetInt32("TensionStressStrainDiagrams");
            }
        }

        #endregion

        #region Public abstract Methods

        /// <summary>
        /// The design stress of the concrete for a strain, according to the standard
        /// </summary>
        /// <param name="standard">The standard</param>
        /// <param name="strain">The strain (negative in compression)</param>
        /// <returns>The design stress</returns>
        public abstract double CalculateDesignStressConcrete(Standards.Standard standard, double strain);

        /// <summary>
        /// Calculate the design value from the characteristic stress.
        /// </summary>
        /// <param name="standard">The standard</param>
        /// <param name="stress">Characteristic stress.</param>
        /// <returns>The design stress</returns>
        public abstract double CalculateDesignStressFromCharacteristic(Standards.Standard standard, double stress);

        /// <summary>
        /// The design compressive strength according to the standard
        /// </summary>
        /// <param name="standard">The standard</param>
        /// <returns>The design compressive strength</returns>
        public abstract double CalculateDesignCompressiveStrength(Standards.Standard standard);

        /// <summary>
        /// The design tensile strength according to the standard
        /// </summary>
        /// <param name="standard">The standard</param>
        /// <returns>The design tensile strength</returns>
        public abstract double CalculateDesignTensileStrength(Standards.Standard standard);

        #endregion

        #region Public Methods

        /// <summary>
        /// Override if you want to validate the value before assign it
        /// </summary>
        /// <param name="concreteType">The value to assign</param>
        public virtual void SetConcreteType(ConcreteTypes concreteType)
        {
            _concreteType = concreteType;
        }

        /// <summary>
        /// Recalculates the mechanical properties and the stress-strain tables from the main characteristics
        /// </summary>
        public abstract void RecalculateMechanicalProperties();

        /// <summary>
        /// Sets the diagram in compression (override to validate the value)
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        public virtual void SetCompressionStressStrainDiagram(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
        }

        /// <summary>
        /// Sets the diagram in tension (override to validate the value)
        /// </summary>
        /// <param name="tensionStressStrainDiagrams">The diagram</param>
        public virtual void SetTensionStressStrainDiagrams(TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            _tensionStressStrainDiagrams = tensionStressStrainDiagrams;
        }

        #endregion

        #region Equals - hashcode - operators

        /// <summary>
        /// Serializes the data of <see cref="Material"/>, type and diagrams (version 3)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 3;
            info.AddValue("ConcreteMaterialVersion", version);

            info.AddValue("ConcreteType", _concreteType);
            info.AddValue("CompressionStressStrainDiagrams", _compressionStressStrainDiagrams);
            info.AddValue("TensionStressStrainDiagrams", _tensionStressStrainDiagrams);
        }

        /// <summary>
        /// Equality of type, diagrams and the data of <see cref="Material"/>
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal concrete</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterial objCasted) &&
                objCasted._concreteType.Equals(_concreteType) &&
                objCasted._compressionStressStrainDiagrams.Equals(_compressionStressStrainDiagrams) &&
                objCasted._tensionStressStrainDiagrams.Equals(_tensionStressStrainDiagrams) &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the data of <see cref="Material"/>, type and diagrams
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _concreteType.GetHashCode();
                hashCode = hashCode * -17 + _compressionStressStrainDiagrams.GetHashCode();
                hashCode = hashCode * -17 + _tensionStressStrainDiagrams.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first concrete (not null, unless both are null)</param>
        /// <param name="obj2">The second concrete</param>
        /// <returns>True if the materials are equal</returns>
        public static bool operator ==(ConcreteMaterial obj1, ConcreteMaterial obj2)
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
        public static bool operator !=(ConcreteMaterial obj1, ConcreteMaterial obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
