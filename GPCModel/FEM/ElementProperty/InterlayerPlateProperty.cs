using System;
using System.Runtime.Serialization;
using GPC.Model.Fem.Materials;
using GPC.Model.Materials;
using GPC.Model.Sections.Glass;

namespace GPC.Model.Fem.Properties
{
    [Serializable]
    public sealed class InterlayerPlateProperty : PlateProperty, IGlassProperty, IPlateProperty, IEquatable<InterlayerPlateProperty>, ISerializable
    {
        private readonly double _temperature;

        private readonly double _loadDuration;

        public double Temperature => _temperature;

        public double LoadDuration => _loadDuration;

        public InterlayerPlateProperty(Interlayer interlayer, FemMaterial material, double temperature, double loadDuration, string name)
            : this(interlayer.Thickness, interlayer.Thickness, material, temperature, loadDuration, name)
        {

        }

        public InterlayerPlateProperty(double tb, double tm, FemMaterial material, double temperature, double loadDuration, string name)
            : base(material, tb, tm, name)
        {
            _temperature = temperature > 0 ? temperature : throw new ArgumentException("Temperature can not be lower or equal to zero");
            _loadDuration = loadDuration > 0 ? loadDuration : throw new ArgumentException("Temperature can not be lower or equal to zero");
        }


        private InterlayerPlateProperty(SerializationInfo info, StreamingContext context) : base(info, context)
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



        #region Equals - HashCode - Operators


        public bool Equals(InterlayerPlateProperty other)
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

            return Equals(obj as InterlayerPlateProperty);
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

        public static bool operator ==(InterlayerPlateProperty obj1, InterlayerPlateProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(InterlayerPlateProperty obj1, InterlayerPlateProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
