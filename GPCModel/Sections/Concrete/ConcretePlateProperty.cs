using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Concrete
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public class ConcretePlateProperty : PlateProperty, ISerializable
    {
        #region Variables

        protected ConcreteMaterial _material;
        protected double _bendingThickness;
        protected double _membraneThickness;

        #endregion

        #region Properties

        public double BendingThickness { get => _bendingThickness; set => _bendingThickness = value; }

        public double MembraneThickness { get => _membraneThickness; set => _membraneThickness = value; }

        public ConcreteMaterial Material { get => _material; set => _material = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// <param name="material"></param>
        /// <param name="bendingThickness"> Bending _thickness</param>
        /// <param name="membraneThickness"> Membranal _thickness</param>
        /// <param name="name"></param>
        /// </summary>
        public ConcretePlateProperty(ConcreteMaterial material, double bendingThickness, double membraneThickness, string name = "")
            : base(name)
        {
            _bendingThickness = bendingThickness;
            _membraneThickness = membraneThickness;
            _material = material ?? throw new ArgumentNullException("Material cannot be null");
        }

        protected ConcretePlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bendingThickness = info.GetDouble("BendingThickness");
            _membraneThickness = info.GetDouble("MembranalThickness");
            _material = (ConcreteMaterial)info.GetValue("Material", typeof(ConcreteMaterial));
        }

        #endregion

        #region Equals, HasCode and operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BendingThickness", _bendingThickness);
            info.AddValue("MembranalThickness", _membraneThickness);
            info.AddValue("Material", _material, typeof(ConcreteMaterial));
        }

        public override bool Equals(object obj)
        {
            return obj is ConcretePlateProperty objCasted &&
                _bendingThickness == objCasted._bendingThickness &&
                _membraneThickness == objCasted._membraneThickness &&
                _material == objCasted._material &&
                base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _bendingThickness.GetHashCode();
                hashCode = hashCode * -17 + _membraneThickness.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<ConcreteMaterial>.Default.GetHashCode(_material);
                return hashCode;
            }
        }

        private string GetDebuggerDisplay()
        {
            return $"SteelPlateProperty: {_name}";
        }

        public static bool operator ==(ConcretePlateProperty obj1, ConcretePlateProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcretePlateProperty obj1, ConcretePlateProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
