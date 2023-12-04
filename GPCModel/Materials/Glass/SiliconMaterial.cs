using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Silicon", Group = "Materials", Kind = "Material")]
    public class SiliconMaterial : Material
    {
        private double _adhesiveStress;

        public double AdhesiveStress { get => _adhesiveStress; set => _adhesiveStress = value; }

        public SiliconMaterial(double adhesiveStress, double density, double alfaThermalExpansion)
            : base("", 0, 0, density, alfaThermalExpansion)
        {
            _adhesiveStress = adhesiveStress <= 0.001 ? throw new ArgumentException($"{nameof(adhesiveStress)} cannot be zero or lower") : adhesiveStress;
        }

        protected SiliconMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _adhesiveStress = info.GetDouble("AdhesiveStress");
        }

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AdhesiveStress", _adhesiveStress);
        }

        #endregion PUBLIC METHODS
    }
}