using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Base of the glass materials: linear elastic, with the strength given by the standard (see <see cref="GetGlassResistance"/>)
    /// </summary>
    [Serializable]
    public abstract class GlassMaterial : Material
    {
        /// <summary>
        /// Abstract constructor of generic glass material
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
        /// <param name="poisson">Poisson's ratio of the glass</param>
        /// <param name="density">Density of the material [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <exception cref="ArgumentException">If the elastic modulus or the Poisson's ratio is zero (or invalid, see <see cref="Material"/>)</exception>
        protected GlassMaterial(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion)
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be equal to zero");

            if (poisson == 0)
                throw new ArgumentException($"{nameof(poisson)} cannot be equal to zero");
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="Material"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected GlassMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #region Public method

        /// <summary>
        /// The strength of the glass
        /// </summary>
        /// <param name="edgeResistance">True for the strength at the edge</param>
        /// <param name="loadDuration">The load duration [seconds]</param>
        /// <returns>The strength</returns>
        public abstract double GetGlassResistance(bool edgeResistance, double loadDuration);

        #endregion

        #region Equals - haschode - operators - serialization

        /// <summary>
        /// Serializes the data of <see cref="Material"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(GlassMaterial)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal glass</returns>
        public override bool Equals(object obj)
        {
            if (obj is GlassMaterial glassMaterial)
                return Equals(glassMaterial);

            return false;
        }

        /// <summary>
        /// Equality of the data of <see cref="Material"/>
        /// </summary>
        /// <param name="glassMaterial">The glass to compare</param>
        /// <returns>True if the materials are equal</returns>
        public bool Equals(GlassMaterial glassMaterial)
        {
            if (ReferenceEquals(this, glassMaterial))
                return true;

            return glassMaterial != null && base.Equals(glassMaterial);
        }

        /// <summary>
        /// The hash code of the data of <see cref="Material"/>
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null materials are equal
        /// </summary>
        /// <param name="obj1">The first material</param>
        /// <param name="obj2">The second material</param>
        /// <returns>True if the materials are equal</returns>
        public static bool operator ==(GlassMaterial obj1, GlassMaterial obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first material</param>
        /// <param name="obj2">The second material</param>
        /// <returns>True if the materials are different</returns>
        public static bool operator !=(GlassMaterial obj1, GlassMaterial obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
