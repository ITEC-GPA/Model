using GPC.Model.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Glass
{
    /// <summary>
    /// Abstract class that represent the interlayer between two monolithic glasses to compose a laminated glass
    /// </summary>
    [Serializable]
    [UI(Description = "Interlayer", Group = "Glasses", Kind = "Interlayer")]
    public sealed class Interlayer : ModelObjectId, IEquatable<Interlayer>, IGlassLayer
    {
        #region VARIABLES

        /// <summary>
        /// The thickness
        /// </summary>
        private double _thickness;

        /// <summary>
        /// The material
        /// </summary>
        private InterlayerMaterial _interlayerMaterial;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The thickness
        /// </summary>
        public double Thickness { get => _thickness; set => _thickness = value; }

        /// <summary>
        /// The material
        /// </summary>
        public InterlayerMaterial Material { get => _interlayerMaterial; set => _interlayerMaterial = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the interlayer
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="interlayerMaterial">The material</param>
        public Interlayer(string name, double thickness, InterlayerMaterial interlayerMaterial)
            : base(name)
        {
            _thickness = thickness;
            _interlayerMaterial = interlayerMaterial;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Interlayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _interlayerMaterial = (InterlayerMaterial)info.GetValue("InterlayerMaterial", typeof(InterlayerMaterial));
            _thickness = info.GetDouble("Thickness");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Serializes the interlayer
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("InterlayerMaterial", _interlayerMaterial, typeof(InterlayerMaterial));
            info.AddValue("Thickness", _thickness);
        }

        /// <summary>
        /// Equality of the thickness, of the material and of the name
        /// </summary>
        /// <param name="other">The interlayer to compare</param>
        /// <returns>True if the interlayers are equal</returns>
        public bool Equals(Interlayer other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) &&
                other._thickness.Equals(_thickness) &&
                other._interlayerMaterial.Equals(_interlayerMaterial) &&
                base.Equals(other);
        }

        /// <summary>
        /// Equality with another interlayer (see <see cref="Equals(Interlayer)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal interlayer</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Interlayer);
        }

        /// <summary>
        /// The hash code of the name, of the thickness and of the material
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _thickness.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<InterlayerMaterial>.Default.GetHashCode(_interlayerMaterial);
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(Interlayer)"/>)
        /// </summary>
        /// <param name="obj1">The first interlayer</param>
        /// <param name="obj2">The second interlayer</param>
        /// <returns>True if the interlayers are equal</returns>
        public static bool operator ==(Interlayer obj1, Interlayer obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(Interlayer)"/>)
        /// </summary>
        /// <param name="obj1">The first interlayer</param>
        /// <param name="obj2">The second interlayer</param>
        /// <returns>True if the interlayers are different</returns>
        public static bool operator !=(Interlayer obj1, Interlayer obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}