using GPC.Model.Fem.Materials;
using GPC.Utilities.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Aluminium", Group = "Materials", Kind = "Material")]
    public class AluminiumMaterial : Material
    {
        #region Public Enum        

        public enum AluminiumTypes
        {
            Undefined,
            [Description("Structural aluminium material")] Structural,
            [Description("Bolt aluminium material")] Bolt,
        }

        public enum StressStrainCurveType
        {
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

        protected double _fo;
        protected double _fu;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic value of 0.2% prrof strength.
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
        /// Characteristic value of ultimate tensile strength.
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

        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus.</param>
        /// <param name="fo">Characteristic value of 0.2% prrof strength.</param>
        /// <param name="fu">Characteristic value of ultimate tensile strength.</param>
        /// <param name="strainU">Ultimate strain, ε_uni in annex F of 1999-1-1:2023.</param>
        /// <param name="aluminiumType"></param>
        /// <param name="thicknessMax">Maximum thickness.</param>
        /// <param name="poisson"></param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion"></param>
        public AluminiumMaterial(string name, double elasticModulus, double fo, double fu, double strainU = 0.1, AluminiumTypes aluminiumType = AluminiumTypes.Undefined, double thicknessMax = 5, double poisson = 0.3, double density = 0.0027, double alfaThermalExpansion = 23e-6)
            : this(name, elasticModulus, poisson, fo, fu, strainU, aluminiumType, thicknessMax, density, alfaThermalExpansion)
        {

        }

        public AluminiumMaterial(string name, AluminiumTypes aluminiumType)
            : this(name, 70000, 240, 290, 0.045, aluminiumType)
        {
            // 6061 - T6
        }

        public AluminiumMaterial(string name, double elasticModulusCompression, double elasticModulusTension,
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, AluminiumTypes aluminiumType = AluminiumTypes.Undefined, double thicknessMax = 5,
             double poisson = 0.3, double density = 0.0027, double alfaThermalExpansion = 23e-6)
            : base(name, elasticModulusCompression, elasticModulusTension,
            strainYCompression, strainUCompression, strainYTension, strainUTension,
            stressYCompression, stressUCompression, stressYTension, stressUTension,
            stressStrainTableCompression, stressStrainTableTensio, poisson, density, alfaThermalExpansion)
        {
            AluminiumType = aluminiumType;
            _fo = stressYTension;
            _fu = stressUTension;
            ThicknessMax = thicknessMax;
        }

        /// <summary>
        /// Protected steelMaterial constructor 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus.</param>
        /// <param name="poisson">Poissoins's Ratio.</param>
        /// <param name="fo">Characteristic value of 0.2% prrof strength.</param>
        /// <param name="fu">Characteristic value of ultimate tensile strength.</param>
        /// <param name="strainU">Ultimate strain, ε_uni in annex F of 1999-1-1:2023.</param>
        /// <param name="aluminiumType">Type of steel.</param>
        /// <param name="density">Density of material.</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient.</param>
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

        public override IsotropicFemMaterial GetIsotropicFemMaterial()
        {
            return new IsotropicFemMaterial(ElasticModulusCompression, Ni, AlfaThermalExpansion, Density);
        }

        public override OrthotropicFemMaterial GetOrthotropicFemMaterial()
        {
            return new OrthotropicFemMaterial(ElasticModulusCompression, ElasticModulusCompression, ElasticModulusCompression, Ni, Ni, Ni, GetShearModule(), GetShearModule(), GetShearModule(), AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
        }

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

        public override bool Equals(object obj)
        {
            return obj is AluminiumMaterial material &&
                   base.Equals(obj) &&
                   _fo == material._fo &&
                   _fu == material._fu &&
                   AluminiumType == material.AluminiumType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _fo.GetHashCode();
                hashCode = hashCode * -17 + _fu.GetHashCode();
                hashCode = hashCode * -17 + AluminiumType.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
