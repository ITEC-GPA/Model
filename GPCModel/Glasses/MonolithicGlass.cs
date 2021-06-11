using GPC.Model.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Glasses
{
    /// <summary>
    /// Monolithic glass. This represent the simpler glass panel. It is composed by a single layer of glass
    /// </summary>
    [Serializable]
    [UI(Description = "Monolithic", Group = "Glasses", Kind = "Glass")]
    public sealed class MonolithicGlass : Glass, IGlassPanel, IEquatable<MonolithicGlass>
    {
        #region Variables

        private readonly GlassMaterial _material;

        private readonly double _thickness;

        #endregion

        #region Properties

        public double Thickness => _thickness;

        public GlassMaterial Material => _material;

        public double TotalThickness => _thickness;

        #endregion

        #region Constructors

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="thickness">The minimum thickness of the panel (the one used for calculation)</param>
        /// <param name="glassMaterial"></param>
        public MonolithicGlass(string name, double thickness, GlassMaterial glassMaterial)
            : this(name, thickness, glassMaterial, Guid.NewGuid())
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid">The guid of the glass</param>
        /// <param name="thickness">The minimum thickness of the panel (the one used for calculation)</param>
        /// <param name="glassMaterial"></param>
        public MonolithicGlass(string name, double thickness, GlassMaterial glassMaterial, Guid guid)
            : base(guid, name)
        {
            if (thickness <= 0.001)
            {
                throw new ArgumentException($"{nameof(thickness)} cannot be zero or lower");
            }

            this._thickness = thickness;
            this._material = glassMaterial;
        }

        public MonolithicGlass(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _thickness = info.GetDouble("Thickness");
            _material = (GlassMaterial)info.GetValue("Material", typeof(GlassMaterial));
        }

        #endregion


        public IGlassPackage[] GetGlassPackage()
        {
            return new IGlassPackage[] { this };
        }


        /// <inheritdoc cref="IGlassPanel.GetElasticModulus()"/>
        public double GetElasticModulus()
        {
            return _material.E;
        }

        /// <inheritdoc cref="IGlassPanel.GetPoissonRatios()"/>
        public double GetPoissonRatios()
        {
            return _material.Ni;
        }

        /// <inheritdoc cref="IGlassPanel.GetSelfWeightPerUnitArea()"/>
        public double GetSelfWeightPerUnitArea()
        {
            // mm * T/mm3 => T / mm2
            return _thickness * _material.Density;
        }

        /// <inheritdoc cref="IGlassPanel.GetDensity()"/>
        public double GetDensity()
        {
            return _material.Density;
        }

        #region Equals - HashCode - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Thickness", _thickness);
            info.AddValue("Material", _material);
        }

        public bool Equals(MonolithicGlass other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._thickness.Equals(_thickness) 
                                    && other._material.Equals(_material) 
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as MonolithicGlass);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<GlassMaterial>.Default.GetHashCode(_material);
            hashCode = hashCode * -17 + _thickness.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MonolithicGlass obj1, MonolithicGlass obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(MonolithicGlass obj1, MonolithicGlass obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
