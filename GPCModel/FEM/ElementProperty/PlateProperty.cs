using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.FEM.Properties
{
    public class PlateProperty : ElementProperty, IPlateProperty
    {
        #region Variables

        protected double _bendingThickness;

        protected double _membraneThickness;

        protected Material _material;

        #endregion

        #region Properties

        public double BendingThickness => _bendingThickness;

        public double MembraneThickness => _membraneThickness;

        public Material Material => _material;
        #endregion

        #region Public Constructors


        /// <summary>
        /// <param name="bendingThickness"> Bending thickness</param>
        /// <param name="membraneThickness"> Membranal thickness</param>
        /// </summary>
        internal PlateProperty(Material material, double bendingThickness, double membraneThickness)
            : this(material, bendingThickness, membraneThickness, string.Empty)
        {

        }

        /// <summary>
        /// <param name="bendingThickness"> Bending thickness</param>
        /// <param name="membraneThickness"> Membranal thickness</param>
        /// </summary>
        public PlateProperty(Material material, double bendingThickness, double membraneThickness, string name)
            : base(name, Guid.NewGuid())
        {
            _bendingThickness = bendingThickness;
            _membraneThickness = membraneThickness;
            _material = material ?? throw new ArgumentNullException("Plate property material cannot be null");
        }


        public PlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bendingThickness = info.GetDouble("BendingThickness");
            _membraneThickness = info.GetDouble("MembranalThickness");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion 

        public virtual double GetE()
        {
            return _material.E;
        }

        public virtual double GetNi()
        {
            return _material.Ni;
        }

        public virtual double GetG()
        {
            return GetE() / (2.0 * (1.0 + GetNi()));
        }

        public virtual double GetDensity()
        {
            return _material.Density;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BendingThickness", _bendingThickness);
            info.AddValue("MembranalThickness", _membraneThickness);
            info.AddValue("Material", _material);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            PlateProperty objCasted = obj as PlateProperty;
            return !(objCasted is null) && _bendingThickness == objCasted._bendingThickness &&
                                           _membraneThickness == objCasted._membraneThickness &&
                                           _material.Equals(objCasted._material) &&
                                           base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _bendingThickness.GetHashCode();
            hashCode = hashCode * -17 + _membraneThickness.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<Material>.Default.GetHashCode(_material);
            return hashCode;
        }
    }
}
