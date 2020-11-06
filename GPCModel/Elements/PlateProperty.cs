using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    public class PlateProperty : ElementProperty
    {
        #region Variables
        /// <summary>
        /// <param name="_tb"> Bending thickness</param>
        /// <param name="_tm"> Membranal thickness</param>
        /// </summary>
        protected double _tb;
        protected double _tm;
        #endregion

        #region Properties
        public double Tb => _tb;
        public double Tm => _tm;
        #endregion


        #region Public Constructors
        protected PlateProperty(Material material, double tb, double tm)
            : base(material)
        {
            _tb = tb;
            _tm = tm;
        }

        protected PlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _tb = info.GetDouble("BendingThickness");
            _tm = info.GetDouble("MembranalThickness");
        }

        #endregion Public Constructors

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BendingThickness", _tb);
            info.AddValue("MembranalThickness", _tm);
        }
    }
}
