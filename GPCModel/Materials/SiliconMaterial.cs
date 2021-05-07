using GPC.Model.FEM.Materials;
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

        public override IsotropicFemMaterial GetIsotropicFemMaterial()
        {
            // da impostare il valore corretto di E e di NI
            return new IsotropicFemMaterial(E, Ni, AlfaThermalExpansion, Density);
        }

        public override OrthotropicFemMaterial GetOrthotropicFemMaterial()
        {
            // da impostare il valore corretto di E e di NI e di G

            return new OrthotropicFemMaterial(E, E, E, GetShearModule(), GetShearModule(), GetShearModule(), Ni, Ni, Ni, AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AdhesiveStress", _adhesiveStress);
        }

        #endregion PUBLIC METHODS
    }
}