using System;
using System.Collections.Generic;
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
            : this(name, elasticModulus, poisson, density, alfaThermalExpansion, Guid.NewGuid())
        {

        }

        protected GlassMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #region Public method

        public abstract double GetGlassResistance(bool edgeResistance, double loadDuration);

        #endregion


        #region Equals - haschode - operators - serialization
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            GlassMaterial objCasted = obj as GlassMaterial;
            return !(objCasted is null) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GlassMaterial obj1, GlassMaterial obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(GlassMaterial obj1, GlassMaterial obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}