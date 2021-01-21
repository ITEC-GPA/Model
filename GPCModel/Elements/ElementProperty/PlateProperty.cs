using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    public class PlateProperty : ElementProperty, IPlateProperty
    {
        #region Variables

        protected double _tb;

        protected double _tm;

        protected Material _material;

        #endregion

        #region Properties

        public double Tb => _tb;

        public double Tm => _tm;

        #endregion

        #region Public Constructors

        /// <summary>
        /// <param name="_tb"> Bending thickness</param>
        /// <param name="_tm"> Membranal thickness</param>
        /// </summary>
        public PlateProperty(Material material, double tb, double tm)
            : base(Guid.NewGuid())
        {
            _tb = tb;
            _tm = tm;
            _material = material ?? throw new ArgumentNullException("Plate property material cannot be null");
        }

        protected PlateProperty(double tb, double tm)
            : base(Guid.NewGuid())
        {
            _tb = tb;
            _tm = tm;
        }

        public PlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _tb = info.GetDouble("BendingThickness");
            _tm = info.GetDouble("MembranalThickness");
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
            info.AddValue("BendingThickness", _tb);
            info.AddValue("MembranalThickness", _tm);
            info.AddValue("Material", _material);
        }
    }
}
