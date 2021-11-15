using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{

    public abstract class ConcreteMaterial : Material
    {

        protected StressStrainTable _stressStrainTableCompression;
        protected StressStrainTable _stressStrainTableTension;

        protected double _elasticModulusTension;

        /// <summary>
        /// Characteristic Stress strain table in comrpession
        /// </summary>
        public StressStrainTable StressStrainTableCompression => _stressStrainTableCompression;
        
        /// <summary>
        /// Characteristic Stress strain table in tension
        /// </summary>
        public StressStrainTable StressStrainTableTension => _stressStrainTableTension;

        /// <summary>
        /// Elastic modulus of concrete in tension
        /// </summary>
        public double ElasticModulusTension => _elasticModulusTension;


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



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterial objCasted) && objCasted._elasticModulusTension.Equals(_elasticModulusTension) &&
                                                          objCasted._stressStrainTableCompression.Equals(_stressStrainTableCompression) &&
                                                          objCasted._stressStrainTableTension.Equals(_stressStrainTableTension) &&
                                                          base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _elasticModulusTension.GetHashCode();
                hashCode = hashCode * -17 + _stressStrainTableCompression.GetHashCode();
                hashCode = hashCode * -17 + _stressStrainTableTension.GetHashCode();
                return hashCode;
            }
        }


        public static bool operator ==(ConcreteMaterial obj1, ConcreteMaterial obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcreteMaterial obj1, ConcreteMaterial obj2)
        {
            return !(obj1 == obj2);
        }


    }

}
