using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;
using GPC.Model.Elements.Glasses;


namespace GPC.Model.Elements
{
    public class InterlayerProperty : PlateProperty, IGlassProperty
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
    }
}
