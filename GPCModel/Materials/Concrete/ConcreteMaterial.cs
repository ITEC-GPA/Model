using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Converters;

namespace GPC.Model.Materials
{
    [Serializable]
    public abstract class ConcreteMaterial : Material, ISerializable
    {
        #region Public Enum        

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum ConcreteTypes
        {
            [Description("Normal")]
            Normal,

            [Description("Fiber-Reinforced")]
            FRC,
        }

        #endregion

        #region Variables

        protected StressStrainTable _stressStrainTableCompression;
        protected StressStrainTable _stressStrainTableTension;

        protected double _elasticModulusTension;
        protected ConcreteTypes _concreteType;

        #endregion

        #region Properties

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

        /// <summary>
        /// Type of concrete
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

        #endregion

        #region Public Constructor

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

            _elasticModulusTension = elasticModulusTension;
            _elasticModulus = elasticModulusCompression;
        }

        protected ConcreteMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
            _stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
            _elasticModulusTension = info.GetDouble("ElasticModulusTension");
        }

        #endregion

        #region Public abstract Methods

        public abstract double CalculateDesignStressConcrete(Standards.Standard standard, double strain);

        public abstract double CalculateDesignCompressiveStrength(Standards.Standard standard);

        public abstract double CalculateDesignTensileStrength(Standards.Standard standard);

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

        #region Public Methods

        /// <summary>
        /// Override if you want to validate the value before assign it
        /// </summary>
        /// <param name="concreteType">The value to assign</param>
        public virtual void SetConcreteType(ConcreteTypes concreteType)
        {
            _concreteType = concreteType;
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

        public virtual void RecalculateMechanicalProperties()
        {

        }

        #endregion

        #region Equals - hashcode - operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("TableCompression", _stressStrainTableCompression);
            info.AddValue("TableTension", _stressStrainTableTension);
            info.AddValue("ElasticModulusTension", _elasticModulusTension);
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

        #endregion
    }
}
