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

        /// <summary>
        /// The acceleration along V1
        /// </summary>
        protected double _a1;
        /// <summary>
        /// The acceleration along V2
        /// </summary>
        protected double _a2;
        /// <summary>
        /// The acceleration along V3
        /// </summary>
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

        /// <summary>
        /// Creates an acceleration of the model
        /// </summary>
        /// <param name="a1">Acceleration along the axis: <see cref="CoordinateSystem.V1"/> [L/T^2]</param>
        /// <param name="a2">Acceleration along the axis: <see cref="CoordinateSystem.V2"/> [L/T^2]</param>
        /// <param name="a3">Acceleration along the axis: <see cref="CoordinateSystem.V3"/> [L/T^2]</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">The coordinate system of the components</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public ModelAccelerationLoad(double a1, double a2, double a3, LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            _a1 = a1;
            _a2 = a2;
            _a3 = a3;
        }


        /// <summary>
        /// Creates an acceleration of the model with components in the global system
        /// </summary>
        /// <param name="a1">Acceleration along X [L/T^2]</param>
        /// <param name="a2">Acceleration along Y [L/T^2]</param>
        /// <param name="a3">Acceleration along Z [L/T^2]</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public ModelAccelerationLoad(double a1, double a2, double a3, LoadCaseBase loadCase, string name = "", int id = IDUNASSIGNED)
            : this(a1, a2, a3, loadCase, CoordinateSystem.Global, name, id)
        {
        }

        /// <summary>
        /// Creates a copy of an acceleration (same components, load case, coordinate system, name and id)
        /// </summary>
        /// <param name="loadCaseAttribute">The acceleration to copy</param>
        public ModelAccelerationLoad(ModelAccelerationLoad loadCaseAttribute)
            : this(loadCaseAttribute.A1, loadCaseAttribute.A2, loadCaseAttribute.A3, loadCaseAttribute.LoadCase,
                  loadCaseAttribute.CoordinateSystem, loadCaseAttribute.Name, loadCaseAttribute.Id)
        {
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Load"/> and the components
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ModelAccelerationLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _a1 = info.GetDouble("A1");
            _a2 = info.GetDouble("A2");
            _a3 = info.GetDouble("A3");
        }

        #endregion

        #region Equals, HashCode and operators

        /// <summary>
        /// Serializes the data of <see cref="Load"/> and the components
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("A1", _a1);
            info.AddValue("A2", _a2);
            info.AddValue("A3", _a3);
        }

        /// <summary>
        /// Equality of name, load case, coordinate system and components (exact)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal acceleration</returns>
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

        /// <summary>
        /// The hash code of name, load case, coordinate system and components
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + A3.GetHashCode();
            hashCode = hashCode * -17 + A1.GetHashCode();
            hashCode = hashCode * -17 + A2.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Not implemented: the acceleration has no geometry
        /// </summary>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public override GeometryBase GetGeometryBase()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null loads are equal
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are equal</returns>
        public static bool operator ==(ModelAccelerationLoad obj1, ModelAccelerationLoad obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are different</returns>
        public static bool operator !=(ModelAccelerationLoad obj1, ModelAccelerationLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
