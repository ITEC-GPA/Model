using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    public class NormalPressureLoad : Load
    {
        private double _pressure;

        public double Pressure => _pressure;

        #region PUBLIC CONSTRUCTOR

        public NormalPressureLoad(double pressure, LoadCase loadCase, Guid guid)
            : base(loadCase, guid)
        {
            this._pressure = pressure;
        }

        public NormalPressureLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _pressure = info.GetDouble("Pressure");
        }

        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Pressure", _pressure);
        }
    }
}