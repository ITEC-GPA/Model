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
        /// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="density">Density of the material [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        protected GlassMaterial(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be equal to zero");

            if (poisson == 0)
                throw new ArgumentException($"{nameof(poisson)} cannot be equal to zero");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="density">Density of the material [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        protected GlassMaterial(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : this(name, elasticModulus, poisson, density, alfaThermalExpansion, Guid.Empty)
        {

        }

        protected GlassMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #region PUBLIC METHODS
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }
        #endregion
    }
}