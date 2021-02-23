using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [Description("Silicon"), Category("Materials")]
    public class SiliconMaterial : Material
    {
        private double _adhesiveStress;

        public double AdhesiveStress => _adhesiveStress;

        public SiliconMaterial(double adhesiveStress, double density, double alfaThermalExpansion, Guid guid)
            : base("", 0, 0, density, alfaThermalExpansion, guid)
        {
            if (adhesiveStress <= 0.001)
            {
                throw new ArgumentException($"{nameof(adhesiveStress)} cannot be zero or lower");
            }

            this._adhesiveStress = adhesiveStress;
        }

        public SiliconMaterial(double adhesiveStress, double density, double alfaThermalExpansion)
            : this(adhesiveStress, density, alfaThermalExpansion, Guid.NewGuid())
        {

        }

        public SiliconMaterial(SerializationInfo info, StreamingContext context)
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