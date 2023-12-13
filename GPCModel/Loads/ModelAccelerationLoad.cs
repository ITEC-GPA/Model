using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// Represent an acceleration attribute of the <see cref="Models.Model"/>
    /// </summary>
    [Serializable]
    public class ModelAccelerationLoad : Load, ISerializable
    {
        #region Variables

        protected double _a1;
        protected double _a2;
        protected double _a3;

        #endregion

        #region Properties

        /// <summary>
        /// Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]
        /// </summary>
        public double A1 { get => _a1; set => _a1 = value; }
        /// <summary>
        /// Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]
        /// </summary>
        public double A2 { get => _a2; set => _a2 = value; }
        /// <summary>
        /// Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]
        /// </summary>
        public double A3 { get => _a3; set => _a3 = value; }

        #endregion

        #region Constructor

        /// <param name="coordinateSystem"></param>
        /// <param name="a1">Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]</param>
        /// <param name="a2">Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]</param>
        /// <param name="a3">Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]</param>
        public ModelAccelerationLoad(double a1, double a2, double a3, LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            _a1 = a1;
            _a2 = a2;
            _a3 = a3;
        }


        public ModelAccelerationLoad(double a1, double a2, double a3, LoadCaseBase loadCase, string name = "", int id = IDUNASSIGNED)
            : this(a1, a2, a3, loadCase, CoordinateSystem.Global, name, id)
        {
        }

        public ModelAccelerationLoad(ModelAccelerationLoad loadCaseAttribute)
            : this(loadCaseAttribute.A1, loadCaseAttribute.A2, loadCaseAttribute.A3, loadCaseAttribute.LoadCase,
                  loadCaseAttribute.CoordinateSystem, loadCaseAttribute.Name, loadCaseAttribute.Id)
        {
        }

        protected ModelAccelerationLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _a1 = info.GetDouble("A1");
            _a2 = info.GetDouble("A2");
            _a3 = info.GetDouble("A3");
        }

        #endregion

        #region Equals, HashCode and operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A1", _a1);
            info.AddValue("A2", _a2);
            info.AddValue("A3", _a3);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ModelAccelerationLoad objCasted = obj as ModelAccelerationLoad;
            return objCasted != null &&
                   base.Equals(objCasted) &&
                   A3 == objCasted.A3 &&
                   A1 == objCasted.A1 &&
                   A2 == objCasted.A2 &&
                   EqualityComparer<CoordinateSystem>.Default.Equals(CoordinateSystem, objCasted.CoordinateSystem);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + A3.GetHashCode();
            hashCode = hashCode * -17 + A1.GetHashCode();
            hashCode = hashCode * -17 + A2.GetHashCode();
            return hashCode;
        }

        public override GeometryBase GetGeometryBase()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ModelAccelerationLoad obj1, ModelAccelerationLoad obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ModelAccelerationLoad obj1, ModelAccelerationLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
