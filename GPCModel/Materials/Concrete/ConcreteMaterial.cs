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
            [Description("Concrete")]
            Concrete,

            [Description("Fiber-Reinforced")]
            FRC,
        }

        #endregion

        #region Variables

        protected ConcreteTypes _concreteType;

        #endregion

        #region Properties

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

        public ConcreteMaterial(string name, StressStrainTable stressStrainTableCompression,
            StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
            double poisson, double density, double alfaThermalExpansion)
            : base(name, stressStrainTableCompression, stressStrainTableTension, elasticModulusCompression, elasticModulusTension,
                  poisson, density, alfaThermalExpansion)
        {            
        }

        protected ConcreteMaterial(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion)
        {            
        }

        protected ConcreteMaterial(string name, double poisson, double density, double alfaThermalExpansion)
            : base(name, 0, poisson, density, alfaThermalExpansion)
        {
        }

        protected ConcreteMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ConcreteMaterialVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if(version == 1)
			{
                _stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
                _stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
                _elasticModulusTension = info.GetDouble("ElasticModulusTension");

                _strainYCompression = info.GetDouble("StrainYCompression");
                _strainUCompression = info.GetDouble("StrainUCompression");
                _strainYTension = info.GetDouble("StrainYTension");
                _strainUTension = info.GetDouble("StrainUTension");

                _stressYCompression = _stressStrainTableCompression.GetStress(_strainYCompression);
                _stressUCompression = _stressStrainTableCompression.GetStress(_strainUCompression);
                _stressYTension = _stressStrainTableTension.GetStress(_strainYTension);
                _stressUTension = _stressStrainTableTension.GetStress(_strainUTension);
            }
        }

		protected ConcreteMaterial(string name, double elasticModulusCompression, double elasticModulusTension, 
            double strainYCompression, double strainUCompression, double strainYTension, double strainUTension, 
            double stressYCompression, double stressUCompression, double stressYTension, double stressUTension, 
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson, double alfaThermalExpansion, double density) 
            : base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, 
                  strainUCompression, strainYTension, strainUTension, stressYCompression, 
                  stressUCompression, stressYTension, stressUTension, stressStrainTableCompression, 
                  stressStrainTableTension, poisson, alfaThermalExpansion, density)
		{
            _concreteType = concreteType;
        }

		#endregion

		#region Public abstract Methods

		public abstract double CalculateDesignStressConcrete(Standards.Standard standard, double strain);

        public abstract double CalculateDesignCompressiveStrength(Standards.Standard standard);

        public abstract double CalculateDesignTensileStrength(Standards.Standard standard);

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

        public abstract void RecalculateMechanicalProperties();

        #endregion

        #region Equals - hashcode - operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;

            info.AddValue("ConcreteMaterialVersion", version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterial objCasted) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();                
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
