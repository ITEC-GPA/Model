using System;
using System.Runtime.Serialization;

using GPC.Model.LoadCases;

namespace GPC.Model.FEM.Attributes
{
    [Serializable]
    public sealed class PlateNormalPressureAttribute : LoadCaseAttribute, IPlateLoadCaseAttribute, IEquatable<PlateNormalPressureAttribute>, ISerializable
    {
        private readonly double _pressure;

        public double Pressure => _pressure;


        public PlateNormalPressureAttribute(string loadCaseName, double pressure)
            : this(loadCaseName, pressure, string.Empty)
        {

        }

        public PlateNormalPressureAttribute(string loadCaseName, double pressure, string name) 
            : base(loadCaseName, name)
        {
            this._pressure = pressure;
        }


        public PlateNormalPressureAttribute(PlateNormalPressureAttribute plateNormalPressureAttribute)
            : base(plateNormalPressureAttribute)
        {
            this._pressure = plateNormalPressureAttribute.Pressure;
        }


        public PlateNormalPressureAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _pressure = info.GetDouble("Pressure");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Pressure", _pressure);
        }


        public bool Equals(PlateNormalPressureAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _pressure.Equals(other._pressure)
                                    && base.Equals(other);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as PlateNormalPressureAttribute);
        }


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _pressure.GetHashCode();
            return hashCode;
        }

        public override object Clone()
        {
            return new PlateNormalPressureAttribute(this);
        }



        #region Override Operator

        public static bool operator ==(PlateNormalPressureAttribute obj1, PlateNormalPressureAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PlateNormalPressureAttribute obj1, PlateNormalPressureAttribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
