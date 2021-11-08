using GPC.Model.Standards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{

#if DEBUG

    public class ConcreteMaterialACI318 : ConcreteMaterial
    {

        private double _fc;

        public ConcreteMaterialACI318(double fc, string name = "")
            : base(name, 0.2, 0.0025, 1e-6)
        {
            _fc = fc;
        }


        public ConcreteMaterialACI318(string name, StressStrainTable stressStrainTableCompression,
            StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
            double poisson, double density, double alfaThermalExpansion)
            : base(name, stressStrainTableCompression, stressStrainTableTension, elasticModulusCompression, elasticModulusTension, poisson, density, alfaThermalExpansion)
        {

        }


        public ConcreteMaterialACI318(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }



    } 

#endif
}
