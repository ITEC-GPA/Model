using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Steel
{
    /// <summary>
    /// The property of a steel plate: material and thicknesses for bending and membrane behaviour
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public class SteelPlateProperty : PlateProperty, ISerializable, IFemPlateProperty
    {
        #region Variables

        /// <summary>
        /// The steel
        /// </summary>
        protected SteelMaterial _material;
        /// <summary>
        /// The thickness for the bending stiffness
        /// </summary>
        protected double _bendingThickness;
        /// <summary>
        /// The thickness for the membrane stiffness
        /// </summary>
        protected double _membraneThickness;

        #endregion

        #region Properties

        /// <summary>
        /// The thickness for the bending stiffness
        /// </summary>
        public double BendingThickness { get => _bendingThickness; set => _bendingThickness = value; }

        /// <summary>
        /// The thickness for the membrane stiffness
        /// </summary>
        public double MembraneThickness { get => _membraneThickness; set => _membraneThickness = value; }

        /// <summary>
        /// The steel
        /// </summary>
        public SteelMaterial SteelMaterial { get => _material; set => _material = value; }

        /// <summary>
        /// The steel (as material)
        /// </summary>
        public Material Material { get => _material; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the property
        /// </summary>
        /// <param name="material">The steel</param>
        /// <param name="bendingThickness">Bending thickness</param>
        /// <param name="membraneThickness">Membranal thickness</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="material"/> is null</exception>
        public SteelPlateProperty(SteelMaterial material, double bendingThickness, double membraneThickness, string name = "")
            : base(name)
        {
            _bendingThickness = bendingThickness;
            _membraneThickness = membraneThickness;
            _material = material ?? throw new ArgumentNullException("Material cannot be null");
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SteelPlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bendingThickness = info.GetDouble("BendingThickness");
            _membraneThickness = info.GetDouble("MembranalThickness");
            _material = (SteelMaterial)info.GetValue("Material", typeof(SteelMaterial));
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes the property
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BendingThickness", _bendingThickness);
            info.AddValue("MembranalThickness", _membraneThickness);
            info.AddValue("Material", _material, typeof(SteelMaterial));
        }

        /// <summary>
        /// Equality of the thicknesses, of the material and of the base
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal property</returns>
        public override bool Equals(object obj)
        {
            return obj is SteelPlateProperty objCasted &&
                _bendingThickness == objCasted._bendingThickness &&
                _membraneThickness == objCasted._membraneThickness &&
                _material == objCasted._material &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the base, of the thicknesses and of the material
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _bendingThickness.GetHashCode();
                hashCode = hashCode * -17 + _membraneThickness.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<SteelMaterial>.Default.GetHashCode(_material);
                return hashCode;
            }
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>The text</returns>
        private string GetDebuggerDisplay()
        {
            return $"SteelPlateProperty: {_name}";
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>; a null <paramref name="obj1"/> throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="obj1">The first property</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the properties are equal</returns>
        public static bool operator ==(SteelPlateProperty obj1, SteelPlateProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first property</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the properties are different</returns>
        public static bool operator !=(SteelPlateProperty obj1, SteelPlateProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
