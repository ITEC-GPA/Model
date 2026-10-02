using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// An aluminium alloy of EN 1999-1-1 (structural by default)
    /// </summary>
    [Serializable]
    [UI(Description = "Aluminium", Group = "Materials", Kind = "Material")]
    public class AluminiumMaterialEN1999 : AluminiumMaterial
    {
        #region Constructor

        /// <summary>
        /// Creates an aluminium alloy (see <see cref="AluminiumMaterial"/>)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Elastic modulus</param>
        /// <param name="fo">Characteristic value of 0.2% proof strength</param>
        /// <param name="fu">Characteristic value of ultimate tensile strength</param>
        /// <param name="strainU">Ultimate strain</param>
        /// <param name="steelType">The kind of aluminium</param>
        /// <param name="thicknessMax">Maximum thickness</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public AluminiumMaterialEN1999(string name, double elasticModulus, double fo, double fu, double strainU = 0.045,
            AluminiumTypes steelType = AluminiumTypes.Structural, double thicknessMax = 5, double poisson = 0.3, double density = AluminiumDensity, double alfaThermalExpansion = 23e-6)
            : base(name, elasticModulus, fo, fu, strainU, steelType, thicknessMax, poisson, density, alfaThermalExpansion)
        {
        }

        /// <summary>
        /// Creates a structural alloy 6061-T6 (see <see cref="AluminiumMaterial(string, AluminiumMaterial.AluminiumTypes)"/>)
        /// </summary>
        /// <param name="name">The name</param>
        public AluminiumMaterialEN1999(string name)
            : base(name, AluminiumTypes.Structural)
        {
        }

        /// <summary>
        /// Creates an aluminium alloy from all the properties
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
        /// <param name="steelType">The kind of aluminium</param>
        /// <param name="thicknessMax">Maximum thickness</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density, t/mm³</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <exception cref="ArgumentException">If the Poisson's ratio is greater than 0.5</exception>
        public AluminiumMaterialEN1999(string name, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression,
            double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, AluminiumTypes steelType = AluminiumTypes.Structural, double thicknessMax = 5,
            double poisson = 0.3, double density = AluminiumDensity, double alfaThermalExpansion = 23e-6)
            : base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression,
                  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension,
                  stressStrainTableCompression, stressStrainTableTensio, steelType, thicknessMax, poisson, density, alfaThermalExpansion)
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="AluminiumMaterial"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected AluminiumMaterialEN1999(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Protected constructor (see <see cref="AluminiumMaterial"/>)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Elastic modulus</param>
        /// <param name="poisson">Poisson's Ratio</param>
        /// <param name="fo">Characteristic value of 0.2% proof strength</param>
        /// <param name="fu">Characteristic value of ultimate tensile strength</param>
        /// <param name="strainU">Ultimate strain</param>
        /// <param name="steelType">The kind of aluminium</param>
        /// <param name="thicknessMax">Maximum thickness</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expansion coefficient</param>
        protected AluminiumMaterialEN1999(string name, double elasticModulus, double poisson, double fo, double fu, double strainU,
            AluminiumTypes steelType, double thicknessMax, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, fo, fu, strainU, steelType, thicknessMax, density, alfaThermalExpansion)
        {
        }

        #endregion
    }
}
