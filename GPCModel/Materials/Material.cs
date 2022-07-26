using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public class Material : ModelObject, ISerializable
    {
        #region Variables

        protected double _elasticModulusCompression;
        protected double _elasticModulusTension;

        protected double _ni;
        protected double _alfaThermalExpansion;
        protected double _density;

        protected StressStrainTable _stressStrainTableCompression;
        protected StressStrainTable _stressStrainTableTension;

        #endregion

        #region Properties

        /// <summary>
        /// Elastic modulus of material in compression
        /// </summary>
        public virtual double ElasticModulusCompression => _elasticModulusCompression;

        /// <summary>
        /// Elastic modulus of material in tension
        /// </summary>
        public double ElasticModulusTension => _elasticModulusTension;

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
        /// Poisson's ratio of material
        /// </summary>
        public double Ni => _ni;

        /// <summary>
        /// Alfa thermal expansion coefficient of material
        /// </summary>
        public double AlfaThermalExpansion => _alfaThermalExpansion;

        /// <summary>
        /// Density of material
        /// </summary>
        public double Density => _density;

        /// <summary>
        /// Characteristic Stress strain table in comrpession
        /// </summary>
        public StressStrainTable StressStrainTableCompression => _stressStrainTableCompression;

        /// <summary>
        /// Characteristic Stress strain table in tension
        /// </summary>
        public StressStrainTable StressStrainTableTension => _stressStrainTableTension;

        #endregion

        #region Public Constructor

        public Material(string name, StressStrainTable stressStrainTableCompression,
            StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
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

            _elasticModulusTension = elasticModulusTension < 0 ? throw new ArgumentException($"{nameof(elasticModulusTension)} cannot be lower than zero") : elasticModulusTension;
            _elasticModulusCompression = elasticModulusCompression < 0 ? throw new ArgumentException($"{nameof(elasticModulusCompression)} cannot be lower than zero") : elasticModulusCompression;
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus"> Elastic Modulus [MPa]</param>
        /// <param name="poisson"> Poisson modulus </param>
        /// <param name="alfaThermalExpansion"> Thermal expansion constant</param>
        /// <param name="density"> Density [T/mm^3]</param>
        public Material(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : this(name, new StressStrainTable(), new StressStrainTable(), elasticModulus, elasticModulus,
                  poisson, density, alfaThermalExpansion)
        {
        }

        protected Material(string name)
            : base(Guid.NewGuid(), name)
        {

        }

        protected Material(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _alfaThermalExpansion = info.GetDouble("AlfaThermalExpansion");
            _density = info.GetDouble("Density");
            _elasticModulusCompression = info.GetDouble("ElasticModulusCompression");
            _elasticModulusTension = info.GetDouble("ElasticModulusTension");
            _ni = info.GetDouble("Ni");
            _stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
            _stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
        }

        #endregion

        #region Public Methods

        public virtual double GetShearModule()
        {
            return ElasticModulusCompression / (2.0 * (1.0 + Ni));
        }

        public virtual Fem.Materials.IsotropicFemMaterial GetIsotropicFemMaterial()
        {
            throw new NotImplementedException("");
        }

        public virtual Fem.Materials.OrthotropicFemMaterial GetOrthotropicFemMaterial()
        {
            throw new NotImplementedException("");
        }

        public void SetName(string name)
        {
            if (name != null)
                _name = name;
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

        #endregion

        #region Public Setter

        public void SetStressStrainTableCompression(StressStrainTable stressStrainTable)
        {
            _stressStrainTableCompression = stressStrainTable;
        }

        public void SetStressStrainTableTension(StressStrainTable stressStrainTable)
        {
            _stressStrainTableTension = stressStrainTable;
        }

        #endregion

        #region Equals - HashCode - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AlfaThermalExpansion", _alfaThermalExpansion);
            info.AddValue("Density", _density);
            info.AddValue("ElasticModulusCompression", _elasticModulusCompression);
            info.AddValue("ElasticModulusTension", _elasticModulusTension);
            info.AddValue("Ni", _ni);
            info.AddValue("TableCompression", _stressStrainTableCompression);
            info.AddValue("TableTension", _stressStrainTableTension);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is Material objCasted) && objCasted._elasticModulusCompression.Equals(_elasticModulusCompression) &&
                                                  objCasted._elasticModulusTension.Equals(_elasticModulusTension) &&
                                                  objCasted._ni.Equals(_ni) &&
                                                  objCasted._alfaThermalExpansion.Equals(_alfaThermalExpansion) &&
                                                  objCasted._density.Equals(_density) &&
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

        public static bool operator ==(Material obj1, Material obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Material obj1, Material obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
