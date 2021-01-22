using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    public class PlateProperty : ElementProperty, IPlateProperty, IEquatable<PlateProperty>
    {
        #region Variables

        protected double _bendingThickness;

        protected double _membraneThickness;

        protected Material _material;

        #endregion

        #region Properties

        public double BendingThickness => _bendingThickness;

        public double MembraneThickness => _membraneThickness;

        #endregion

        #region Public Constructors

        /// <summary>
        /// <param name="bendingThickness"> Bending thickness</param>
        /// <param name="membraneThickness"> Membranal thickness</param>
        /// </summary>
        public PlateProperty(Material material, double bendingThickness, double membraneThickness)
            : base(Guid.NewGuid())
        {
            _bendingThickness = bendingThickness;
            _membraneThickness = membraneThickness;
            _material = material ?? throw new ArgumentNullException("Plate property material cannot be null");
        }

        protected PlateProperty(double tb, double tm)
            : base(Guid.NewGuid())
        {
            _bendingThickness = tb;
            _membraneThickness = tm;
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

        public bool Equals(PlateProperty other)
        {
            return !(other is null) && base.Equals(other) &&
                                    _bendingThickness == other._bendingThickness &&
                                    _membraneThickness == other._membraneThickness &&
                                    _material == other._material;
        }
    }
}
