using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public class ConcreteMaterialModelCode2010FRC : ConcreteMaterialModelCode2010
    {

        public ConcreteMaterialModelCode2010FRC(string name, double strainYTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension,
            double poisson, double density, double alfaThermalExpansion, CementType cementType = CementType.ClassN)
            : base(name, strainYTension, stressStrainTableCompression, stressStrainTableTension, poisson, density, alfaThermalExpansion, cementType)
        {

        }


        public ConcreteMaterialModelCode2010FRC(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams, double poisson, double density,
            double alfaThermalExpansion, CementType cementType = CementType.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, ffts, fFtu, strainYTension, strainUTension, tensionStressStrainDiagrams, poisson, density, alfaThermalExpansion, cementType)
        {

        }

        public ConcreteMaterialModelCode2010FRC(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

    }
}
