using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ConcreteMaterial : Material
    {

        protected StressStrainTable _stressStrainTableCompression;
        protected StressStrainTable _stressStrainTableTension;

        protected double _elasticModulusTension;


        public StressStrainTable StressStrainTableCompression => _stressStrainTableCompression;
        public StressStrainTable StressStrainTableTension => _stressStrainTableTension;



        protected ConcreteMaterial(string name, double poisson, double density, double alfaThermalExpansion)
            : base(name)
        {

            if (poisson > 0.5)
                throw new ArgumentException($"{nameof(poisson)} cannot be greater than 0.5");

            _ni = poisson < 0 ? throw new ArgumentException($"Poisson cannot be lower than zero") : poisson;

            _alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be lower than zero") : alfaThermalExpansion;

            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;

        }


        public ConcreteMaterial(string name, StressStrainTable stressStrainTableCompression, 
                                             StressStrainTable stressStrainTableTension,
                                             double elasticModulusCompression, double elasticModulusTension, 
                                             double poisson, double density, double alfaThermalExpansion)
            : base(name)
        {

            if (poisson > 0.5)
                throw new ArgumentException($"{nameof(poisson)} cannot be greater than 0.5");

            _ni = poisson < 0 ? throw new ArgumentException($"Poisson cannot be lower than zero") : poisson;

            _alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be lower than zero") : alfaThermalExpansion;

            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;


            _stressStrainTableCompression = stressStrainTableCompression;
            _stressStrainTableTension = stressStrainTableTension;

            _elasticModulusTension = elasticModulusTension;
            _elasticModulus = elasticModulusCompression;

        }


        public ConcreteMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
            _stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
            _elasticModulusTension = info.GetDouble("ElasticModulusTension");
        }



    }


}
