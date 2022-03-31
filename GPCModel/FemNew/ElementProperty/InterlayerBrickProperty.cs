using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.Materials;
using GPC.Model.Glasses;
using GPC.Model.Materials;

namespace GPC.Model.Fem.Properties
{
    [Serializable]
    public sealed class InterlayerBrickProperty : BrickProperty, IGlassProperty, IEquatable<InterlayerBrickProperty>, ISerializable
    {
        private readonly double _temperature;

        private readonly double _loadDuration;

        public double Temperature => _temperature;

        public double LoadDuration => _loadDuration;


        public InterlayerBrickProperty(FemMaterial material, double temperature, double loadDuration, string name)
            : base(material, name)
        {
            _temperature = temperature > 0 ? temperature : throw new ArgumentException("Temperature can not be lower or equal to zero");
            _loadDuration = loadDuration > 0 ? loadDuration : throw new ArgumentException("Temperature can not be lower or equal to zero");
        }

        private InterlayerBrickProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _temperature = (double)info.GetValue("Temperature", typeof(double));
            _loadDuration = (double)info.GetValue("LoadDuration", typeof(double));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Temperature", _temperature);
            info.AddValue("LoadDuration", _loadDuration);
        }


        #region Equals - Hashcode - Operators

        public bool Equals(InterlayerBrickProperty other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _temperature.Equals(other._temperature) && _loadDuration.Equals(other._loadDuration)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is Group group && Equals(group);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _temperature.GetHashCode();
                hashCode = hashCode * -17 + _loadDuration.GetHashCode();
                return hashCode; 
            }
        }

        public static bool operator ==(InterlayerBrickProperty obj1, InterlayerBrickProperty obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(InterlayerBrickProperty obj1, InterlayerBrickProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
