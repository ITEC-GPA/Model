using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    public class PlateProperty : ElementProperty
    {
        #region Variables
        protected double _tb;

        protected double _tm;

        protected Material _material;
        #endregion

        #region Properties
        public double Tb => _tb;

        public double Tm => _tm;

        public Material Material => _material;

        #endregion

        #region Public Constructors

        /// <summary>
        /// <param name="_tb"> Bending thickness</param>
        /// <param name="_tm"> Membranal thickness</param>
        /// </summary>
        public PlateProperty(Material material, double tb, double tm)
            : base()
        {
            _tb = tb;
            _tm = tm;
            _material = material;
        }

        public PlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _tb = info.GetDouble("BendingThickness");
            _tm = info.GetDouble("MembranalThickness");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion Public Constructors

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BendingThickness", _tb);
            info.AddValue("MembranalThickness", _tm);
            info.AddValue("Material", _material);
        }
    }
}
