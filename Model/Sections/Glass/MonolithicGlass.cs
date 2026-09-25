using GPC.Model.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Glass
{
    /// <summary>
    /// Monolithic glass. This represent the simpler glass panel. It is composed by a single layer of glass
    /// </summary>
    [Serializable]
    [UI(Description = "Monolithic", Group = "Glasses", Kind = "Glass")]
    public sealed class MonolithicGlass : ModelObjectId, IEquatable<MonolithicGlass>, IGlassLayer
    {
        #region Variables

        /// <summary>
        /// The material
        /// </summary>
        private GlassMaterial _material;

        /// <summary>
        /// The thickness
        /// </summary>
        private double _thickness;

        #endregion

        #region Properties

        /// <summary>
        /// The thickness (the one used for calculation)
        /// </summary>
        public double Thickness { get => _thickness; set => _thickness = value; }

        /// <summary>
        /// The material
        /// </summary>
        public GlassMaterial Material { get => _material; set => _material = value; }

        /// <summary>
        /// The thickness (the same of <see cref="Thickness"/>)
        /// </summary>
        public double TotalThickness { get => _thickness; set => _thickness = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the glass
        /// </summary>
        /// <param name="name">Name of the glass</param>
        /// <param name="thickness">The minimum thickness of the panel (the one used for calculation)</param>
        /// <param name="glassMaterial">The material</param>
        /// <exception cref="ArgumentException">If <paramref name="thickness"/> is not bigger than 0.001</exception>
        public MonolithicGlass(string name, double thickness, GlassMaterial glassMaterial)
            : base(name)
        {
            _thickness = thickness <= 0.001 ? throw new ArgumentException($"{nameof(thickness)} cannot be zero or lower") : thickness;
            _material = glassMaterial;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private MonolithicGlass(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _thickness = info.GetDouble("Thickness");
            _material = (GlassMaterial)info.GetValue("Material", typeof(GlassMaterial));
        }

        #endregion

        /// <summary>
        /// The elastic modulus of the glass
        /// </summary>
        /// <returns>E of the material</returns>
        public double GetElasticModulus()
        {
            return _material.E;
        }

        /// <summary>
        /// The Poisson ratio of the glass
        /// </summary>
        /// <returns>ν of the material</returns>
        public double GetPoissonRatios()
        {
            return _material.Ni;
        }

        /// <summary>
        /// The self weight per unit area: thickness by density
        /// </summary>
        /// <returns>The weight per unit area (e.g. T/mm² with mm and T/mm³)</returns>
        public double GetSelfWeightPerUnitArea()
        {
            // mm * T/mm3 => T / mm2
            return _thickness * _material.Density;
        }

        /// <summary>
        /// The density of the glass
        /// </summary>
        /// <returns>The density of the material</returns>
        public double GetDensity()
        {
            return _material.Density;
        }

        #region Equals - HashCode - Operators

        /// <summary>
        /// Serializes the glass
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Thickness", _thickness);
            info.AddValue("Material", _material);
        }

        /// <summary>
        /// Equality of the name, of the thickness and of the material
        /// </summary>
        /// <param name="other">The glass to compare</param>
        /// <returns>True if the glasss are equal</returns>
        public bool Equals(MonolithicGlass other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) &&
                base.Equals(other) &&
                other._thickness.Equals(_thickness) &&
                other._material.Equals(_material);
        }

        /// <summary>
        /// Equality with another glass (see <see cref="Equals(MonolithicGlass)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal glass</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as MonolithicGlass);
        }

        /// <summary>
        /// The hash code of the name, of the material and of the thickness
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<GlassMaterial>.Default.GetHashCode(_material);
                hashCode = hashCode * -17 + _thickness.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(MonolithicGlass)"/>)
        /// </summary>
        /// <param name="obj1">The first glass</param>
        /// <param name="obj2">The second glass</param>
        /// <returns>True if the glasss are equal</returns>
        public static bool operator ==(MonolithicGlass obj1, MonolithicGlass obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(MonolithicGlass)"/>)
        /// </summary>
        /// <param name="obj1">The first glass</param>
        /// <param name="obj2">The second glass</param>
        /// <returns>True if the glasss are different</returns>
        public static bool operator !=(MonolithicGlass obj1, MonolithicGlass obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
