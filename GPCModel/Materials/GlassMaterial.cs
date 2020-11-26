using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public abstract class GlassMaterial : Material
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        protected GlassMaterial(double elasticModulus, double poisson, double density, double alfaThermalExpansion, Guid guid)
            : base(density, elasticModulus, alfaThermalExpansion, guid)
        {
            if (elasticModulus <= 0)
            {
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero or lower");
            }
            if (poisson <= 0)
            {
                throw new ArgumentException($"{nameof(poisson)} cannot be zero or lower");
            }
            else if (poisson >= 1)
            {
                throw new ArgumentException($"{nameof(poisson)} cannot be greater than 1");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        protected GlassMaterial(double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : this(elasticModulus, poisson, density, alfaThermalExpansion, Guid.Empty)
        {

        }

        protected GlassMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        #region PUBLIC METHODS
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
        #endregion
    }
}