using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Aluminium", Group = "Materials", Kind = "Material")]
    public class AluminiumMaterialEN1999 : AluminiumMaterial
    {
        #region Constructor

        public AluminiumMaterialEN1999(string name, double elasticModulus, double fo, double fu, double strainU = 0.045,
            AluminiumTypes steelType = AluminiumTypes.Structural, double thicknessMax = 5, double poisson = 0.3, double density = 0.0027, double alfaThermalExpansion = 23e-6)
            : base(name, elasticModulus, fo, fu, strainU, steelType, thicknessMax, poisson, density, alfaThermalExpansion)
        {
        }

        public AluminiumMaterialEN1999(string name)
            : base(name, AluminiumTypes.Structural)
        {
        }

        public AluminiumMaterialEN1999(string name, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression,
            double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, AluminiumTypes steelType = AluminiumTypes.Structural, double thicknessMax = 5,
            double poisson = 0.3, double density = 0.0027, double alfaThermalExpansion = 23e-6)
            : base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression,
                  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension,
                  stressStrainTableCompression, stressStrainTableTensio, steelType, poisson, thicknessMax, density, alfaThermalExpansion)
        {
        }

        protected AluminiumMaterialEN1999(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected AluminiumMaterialEN1999(string name, double elasticModulus, double poisson, double fo, double fu, double strainU,
            AluminiumTypes steelType, double thicknessMax, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, fo, fu, strainU, steelType, thicknessMax, density, alfaThermalExpansion)
        {
        }

        #endregion
    }
}
