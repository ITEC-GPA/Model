using GPC.Model.Glasses;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Properties
{
    [Serializable]
    public sealed class InterlayerBrickProperty : BrickProperty, IGlassProperty, IEquatable<InterlayerBrickProperty>, ISerializable
    {
        private double _temperature;

        private double _loadDuration;

        public double Temperature => _temperature;

        public double LoadDuration => _loadDuration;

        public InterlayerBrickProperty(InterlayerMaterial material, double temperature, double loadDuration, string name) 
            : base(material, name)
        {
            this._temperature = temperature > 0 ? temperature : throw new ArgumentException("Temperature can not be lower or equal to zero");
            this._loadDuration = loadDuration > 0 ? loadDuration : throw new ArgumentException("Temperature can not be lower or equal to zero");
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }


        /// <summary>
        /// Poisson fixed to <see cref="FemOptions.InterlayerPoissonValue"/>. Elastic modulus is then 2(1+ni)G
        /// </summary>
        /// <returns>The elastic modulus</returns>
        public override double GetE()
        {
            var e = 2.0 * (1.0 + FemOptions.Instance.InterlayerPoissonValue) * GetShearModule();
            return e > 0 ? e : FemOptions.Instance.ZeroElasticModulus;
        }

        /// <summary>
        /// Poisson fixed to <see cref="FemOptions.InterlayerPoissonValue"/>. E becomes equal to 2.98*G
        /// </summary>
        public override double GetNi()
        {
            return FemOptions.Instance.InterlayerPoissonValue;
        }


        /// <returns>The shear modulus, calculated by means of <see cref="InterlayerMaterial.GetShearModule(double, double)"/></returns>
        /// <remarks>Can return zero</remarks>
        public override double GetShearModule()
        {
            return (_material as InterlayerMaterial).GetShearModule(_loadDuration, _temperature);
        }


        #region Equals - Hashcode - Operators

        public bool Equals(InterlayerBrickProperty other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _temperature.Equals(other._temperature)
                                    && _loadDuration.Equals(other._loadDuration) 
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as InterlayerBrickProperty);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _temperature.GetHashCode();
            hashCode = hashCode * -17 + _loadDuration.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(InterlayerBrickProperty obj1, InterlayerBrickProperty obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(InterlayerBrickProperty obj1, InterlayerBrickProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
