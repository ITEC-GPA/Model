using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// A structural silicone: only the adhesive strength (the elastic modulus is zero)
    /// </summary>
    [Serializable]
    [UI(Description = "Silicon", Group = "Materials", Kind = "Material")]
    public class SiliconMaterial : Material
    {
        /// <summary>
        /// The design adhesive stress
        /// </summary>
        private double _adhesiveStress;

        /// <summary>
        /// The design adhesive stress
        /// </summary>
        public double AdhesiveStress { get => _adhesiveStress; set => _adhesiveStress = value; }

        /// <summary>
        /// Creates a silicone without name, with zero elastic modulus and Poisson's ratio
        /// </summary>
        /// <param name="adhesiveStress">The adhesive stress (greater than 0.001)</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <exception cref="ArgumentException">If the adhesive stress is not greater than 0.001</exception>
        public SiliconMaterial(double adhesiveStress, double density, double alfaThermalExpansion)
            : base("", 0, 0, density, alfaThermalExpansion)
        {
            _adhesiveStress = adhesiveStress <= 0.001 ? throw new ArgumentException($"{nameof(adhesiveStress)} cannot be zero or lower") : adhesiveStress;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Material"/> and the adhesive stress
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SiliconMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _adhesiveStress = info.GetDouble("AdhesiveStress");
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Serializes the data of <see cref="Material"/> and the adhesive stress
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AdhesiveStress", _adhesiveStress);
        }

        #endregion PUBLIC METHODS
    }
}