using GPC.Utilities.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// An aluminium alloy defined by the elastic modulus, the 0.2% proof strength fo and the ultimate strength fu (EN 1999-1-1)
    /// </summary>
    [Serializable]
    [UI(Description = "Aluminium", Group = "Materials", Kind = "Material")]
    public class AluminiumMaterial : Material
    {
        #region Public Enum        

        /// <summary>
        /// The kinds of aluminium
        /// </summary>
        public enum AluminiumTypes
        {
            /// <summary>Not defined</summary>
            Undefined,
            /// <summary>Structural aluminium</summary>
            [Description("Structural aluminium material")] Structural,
            /// <summary>Bolts</summary>
            [Description("Bolt aluminium material")] Bolt,
        }

        /// <summary>
        /// The shapes of the stress-strain curve
        /// </summary>
        public enum StressStrainCurveType
        {
            /// <summary>Not defined (the tables are not rebuilt)</summary>
            Undefined = 0,
            /// <summary>
            /// Elastic and perfect plastic without hardening/softening.
            /// </summary>
            ElasticPerfectPlastic = 1,
            /// <summary>
            /// Elastic and then hardening.
            /// </summary>
            ElasticHardening = 2
        }

        #endregion

        #region Variables

        /// <summary>
        /// The characteristic 0.2% proof strength
        /// </summary>
        protected double _fo;
        /// <summary>
        /// The characteristic ultimate tensile strength
        /// </summary>
        protected double _fu;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic value of 0.2% proof strength. The setter rebuilds the tables from the yield and ultimate stresses and then takes fo back from
        /// <see cref="Material.StressYTension"/>, which it does not change: the new value is lost (see the list of the defects found)
        /// </summary>
        public double Fo
        {
            get => _fo;
            set
            {
                if (_fo != value)
                {
                    _fo = value;
                    RecalculateMechanicalProperties();
                }
            }
        }

        /// <summary>
        /// Characteristic value of ultimate tensile strength. The setter has the same defect of <see cref="Fo"/>
        /// </summary>
        public double Fu
        {
            get => _fu;
            set
            {
                if (_fu != value)
                {
                    _fu = value;
                    RecalculateMechanicalProperties();
                }
            }
        }

        /// <summary>
        /// Type of aluminium.
        /// </summary>
        public AluminiumTypes AluminiumType { get; set; }

        /// <summary>
        /// Maximum thickness.
        /// </summary>
        public double ThicknessMax { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates an aluminium alloy with an elastic-hardening curve (read only, according to the standard)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Elastic modulus</param>
        /// <param name="fo">Characteristic value of 0.2% proof strength</param>
        /// <param name="fu">Characteristic value of ultimate tensile strength</param>
        /// <param name="strainU">Ultimate strain, ε_uni in annex F of 1999-1-1:2023</param>
        /// <param name="aluminiumType">The kind of aluminium</param>
        /// <param name="thicknessMax">Maximum thickness</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public AluminiumMaterial(string name, double elasticModulus, double fo, double fu, double strainU = 0.1, AluminiumTypes aluminiumType = AluminiumTypes.Undefined, double thicknessMax = 5, double poisson = 0.3, double density = AluminiumDensity, double alfaThermalExpansion = 23e-6)
            : this(name, elasticModulus, poisson, fo, fu, strainU, aluminiumType, thicknessMax, density, alfaThermalExpansion)
        {
        }

        /// <summary>
        /// Creates an alloy 6061-T6 (E = 70000, fo = 240, fu = 290, εu = 0.045)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="aluminiumType">The kind of aluminium</param>
        public AluminiumMaterial(string name, AluminiumTypes aluminiumType)
            : this(name, 70000, 240, 290, 0.045, aluminiumType)
        {
            // 6061 - T6
        }

        /// <summary>
        /// Creates an aluminium alloy from all the properties; fo and fu are the yield and ultimate stresses in tension
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
        /// <param name="stressYTension">The yield stress in tension (fo)</param>
        /// <param name="stressUTension">The ultimate stress in tension (fu)</param>
        /// <param name="stressStrainTableCompression">The characteristic stress-strain table in compression</param>
        /// <param name="stressStrainTableTensio">The characteristic stress-strain table in tension</param>
        /// <param name="aluminiumType">The kind of aluminium</param>
        /// <param name="thicknessMax">Maximum thickness</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density, t/mm³</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public AluminiumMaterial(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, AluminiumTypes aluminiumType = AluminiumTypes.Undefined, double thicknessMax = 5,
             double poisson = 0.3, double density = AluminiumDensity, double alfaThermalExpansion = 23e-6)
            : base(name, elasticModulusCompression, elasticModulusTension,
            strainYCompression, strainUCompression, strainYTension, strainUTension,
            stressYCompression, stressUCompression, stressYTension, stressUTension,
            stressStrainTableCompression, stressStrainTableTensio, poisson, alfaThermalExpansion, density)
        {
            AluminiumType = aluminiumType;
            _fo = stressYTension;
            _fu = stressUTension;
            ThicknessMax = thicknessMax;
        }

        /// <summary>
        /// Protected constructor: the tables are elastic-hardening from fo, fu and εu
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Elastic modulus</param>
        /// <param name="poisson">Poisson's Ratio</param>
        /// <param name="fo">Characteristic value of 0.2% proof strength</param>
        /// <param name="fu">Characteristic value of ultimate tensile strength</param>
        /// <param name="strainU">Ultimate strain, ε_uni in annex F of 1999-1-1:2023</param>
        /// <param name="aluminiumType">The kind of aluminium</param>
        /// <param name="thicknessMax">Maximum thickness</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expansion coefficient</param>
        protected AluminiumMaterial(string name, double elasticModulus, double poisson, double fo,
            double fu, double strainU, AluminiumTypes aluminiumType, double thicknessMax, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion)
        {
            _fu = Math.Abs(fu);
            _fo = Math.Abs(fo);
            _strainUTension = Math.Abs(strainU);
            _strainUCompression = -Math.Abs(strainU);
            AluminiumType = aluminiumType;
            ThicknessMax = thicknessMax;
            SetDefaultMechanicalProperties();
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Material"/>, fu, fo, the kind of aluminium and the maximum thickness
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected AluminiumMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            int version = info.GetInt32("AluminiumMaterialVersion");
            _fu = info.GetDouble("Fu");
            _fo = info.GetDouble("Fo");
            AluminiumType = (AluminiumTypes)info.GetInt32("AluminiumType");
            ThicknessMax = info.GetDouble("ThicknessMax");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Rebuilds the tables from the yield and ultimate stresses and the yield strains; fo and fu are taken from the stresses in tension
        /// </summary>
        public virtual void RecalculateMechanicalProperties()
        {
            _stressStrainTableCompression = new StressStrainTable(
                new double[] { 0, _stressYCompression, _stressUCompression },
                new double[] { 0, _stressYCompression / _elasticModulusCompression, _strainUCompression });
            _stressStrainTableTension = new StressStrainTable(
                new double[] { 0, _stressYTension, _stressUTension },
                new double[] { 0, _stressYTension / _elasticModulusTension, _strainUTension });

            _strainYTension = _stressYTension / _elasticModulusTension;
            _strainYCompression = _stressYCompression / _elasticModulusCompression;

            _fu = _stressUTension;
            _fo = _stressYTension;
        }

        /// <summary>
        /// Builds elastic-hardening tables and sets yield strains and yield and ultimate stresses from fo, fu and E
        /// </summary>
        public virtual void SetDefaultMechanicalProperties()
        {
            SetStressStrain(StressStrainCurveType.ElasticHardening);

            _strainYTension = _fo / _elasticModulusTension;
            _strainYCompression = -_fo / _elasticModulusCompression;

            _stressUCompression = -_fu;
            _stressUTension = _fu;
            _stressYCompression = -_fo;
            _stressYTension = _fo;
        }

        /// <summary>
        /// Builds the characteristic stress-strain tables of a curve type: elastic perfectly plastic (fo up to εu) or hardening (fo to fu at εu)
        /// </summary>
        /// <param name="stressStrainCurveType">The curve type (<see cref="StressStrainCurveType.Undefined"/>: nothing is done)</param>
        public virtual void SetStressStrain(StressStrainCurveType stressStrainCurveType)
        {
            switch (stressStrainCurveType)
            {
                case StressStrainCurveType.ElasticPerfectPlastic:
                case StressStrainCurveType.ElasticHardening:
                    {
                        double fRupture;
                        if (stressStrainCurveType == StressStrainCurveType.ElasticPerfectPlastic)
                            fRupture = _fo;
                        else
                            fRupture = _fu;

                        _stressStrainTableCompression = new StressStrainTable(
                            new double[] { 0, -_fo, -fRupture },
                            new double[] { 0, -_fo / _elasticModulusCompression, _strainUCompression });
                        _stressStrainTableTension = new StressStrainTable(
                            new double[] { 0, _fo, fRupture },
                            new double[] { 0, _fo / _elasticModulusTension, _strainUTension });
                        return;
                    }
                default:
                    return;
            }
        }

        #endregion

        #region Public Methods Override

        /// <summary>
        /// Serializes the data of <see cref="Material"/>, fo, fu, the kind of aluminium and the maximum thickness (version 1)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 1;

            info.AddValue("AluminiumMaterialVersion", version);
            info.AddValue("Fo", _fo);
            info.AddValue("Fu", _fu);
            info.AddValue("AluminiumType", AluminiumType);
            info.AddValue("ThicknessMax", ThicknessMax);
        }

        /// <summary>
        /// Equality of the data of <see cref="Material"/>, fo, fu, kind of aluminium and maximum thickness
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal aluminium material</returns>
        public override bool Equals(object obj)
        {
            return obj is AluminiumMaterial material &&
                   base.Equals(obj) &&
                   _fo == material._fo &&
                   _fu == material._fu &&
                   AluminiumType == material.AluminiumType &&
                   ThicknessMax == material.ThicknessMax;
        }

        /// <summary>
        /// The hash code of the data of <see cref="Material"/>, fo, fu, kind of aluminium and maximum thickness
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _fo.GetHashCode();
                hashCode = hashCode * -17 + _fu.GetHashCode();
                hashCode = hashCode * -17 + AluminiumType.GetHashCode();
                hashCode = hashCode * -17 + ThicknessMax.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
