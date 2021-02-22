using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;
using GPC.Model.Elements.Glasses;


namespace GPC.Model.FEM.Properties
{
    public sealed class InterlayerProperty : PlateProperty, IGlassProperty, IEquatable<InterlayerProperty>
    {
        private double _temperature;

        private double _loadDuration;

        public double Temperature => _temperature;

        public double LoadDuration => _loadDuration;

        public InterlayerProperty(Interlayer interlayer, double temperature, double loadDuration)
            : this(interlayer.Thickness, interlayer.Thickness, interlayer.Material, temperature, loadDuration)
        {

        }

        public InterlayerProperty(double tb, double tm, InterlayerMaterial material, double temperature, double loadDuration)
            : base(material, tb, tm)
        {
            this._temperature = temperature > 0 ? temperature : throw new ArgumentException("Temperature can not be lower or equal to zero");
            this._loadDuration = loadDuration > 0 ? loadDuration : throw new ArgumentException("Temperature can not be lower or equal to zero");
        }


        public InterlayerProperty(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ni fissato pari a 0.49. E diventa 2.98*G
        /// </summary>
        /// <returns></returns>
        public override double GetE()
        {
            return 2.98* GetG();
        }

        /// <summary>
        /// ni fissato pari a 0.49. E diventa 2.98*G
        /// </summary>
        /// <returns></returns>
        public override double GetNi()
        {
            return 0.49;
        }

        public override double GetG()
        {
            return ((InterlayerMaterial)_material)[_temperature, _loadDuration];
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public bool Equals(InterlayerProperty other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) &&
                        _temperature.Equals(other._temperature) &&
                        _loadDuration.Equals(other._loadDuration) && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as InterlayerProperty);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _temperature.GetHashCode();
            hashCode = hashCode * -17 + _loadDuration.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(InterlayerProperty obj1, InterlayerProperty obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(InterlayerProperty obj1, InterlayerProperty obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
