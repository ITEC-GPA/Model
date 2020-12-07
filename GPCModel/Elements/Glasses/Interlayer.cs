using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Abstract class that represent the interlayer between two monolithic glasses to compose a laminated glass
    /// </summary>
    [Serializable]
    public class Interlayer : ElementProperty, IFemGlassProperty
    {
        #region VARIABLES

        protected double _thickness;

        protected InterlayerMaterial _interlayerMaterial;

        #endregion

        #region PROPERTIES

        public double Thickness => _thickness;

        public InterlayerMaterial Material => _interlayerMaterial;

        #endregion


        public Interlayer(string name, double thickness, InterlayerMaterial interlayerMaterial, Guid guid)
            : base(name, guid)
        {
            this._thickness = thickness;
            this._interlayerMaterial = interlayerMaterial;
        }

        public Interlayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _interlayerMaterial = (InterlayerMaterial)info.GetValue("InterlayerMaterial", typeof(InterlayerMaterial));
            _thickness = info.GetDouble("Thickness");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("InterlayerMaterial", _interlayerMaterial);
            info.AddValue("Thickness", _thickness);
        }
    }
}