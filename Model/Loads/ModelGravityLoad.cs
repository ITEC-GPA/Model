using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// The gravity acting on the whole model: direction and acceleration. Its load case must be a self weight <see cref="LoadCases.LoadCase"/>.
    /// It belongs to <see cref="Models.Model.ModelLoads"/>
    /// </summary>
    [Serializable]
    public class ModelGravityLoad : Load, ISerializable
    {
        /// <summary>
        /// Value of the gravity acceleration [mm/s^2]
        /// </summary>
        public const double GRAVITYACCELERATION = 9806.65;

        #region Variables

        /// <summary>
        /// The unit direction of the gravity
        /// </summary>
        protected Vector3d _versor;
        /// <summary>
        /// The acceleration
        /// </summary>
        protected double _acceleration;

        #endregion

        #region Properties

        /// <summary>
        /// The unit direction of the gravity (the setter unitizes the given vector in place)
        /// </summary>
        public Vector3d GravityVersor
        {
            get => _versor;
            set
            {
                if (_versor != value)
                {
                    value.Unitize();
                    _versor = value;
                }
            }
        }

        /// <summary>
        /// The gravity acceleration [L/T^2] (<see cref="GRAVITYACCELERATION"/> in mm/s^2)
        /// </summary>
        public double Acceleration { get => _acceleration; set => _acceleration = value; }

        /// <summary>
        /// The gravity vector: direction × acceleration
        /// </summary>
        public Vector3d GravityVector => _versor * _acceleration;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the gravity of the model
        /// </summary>
        /// <param name="axis">The direction (unitized in place)</param>
        /// <param name="acceleration">The acceleration</param>
        /// <param name="loadCase">A self weight <see cref="LoadCases.LoadCase"/></param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentException">If the load case is not a self weight <see cref="LoadCases.LoadCase"/></exception>
        public ModelGravityLoad(Vector3d axis, double acceleration, LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            if (loadCase is LoadCase lc)
            {
                if (lc.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
                    throw new ArgumentException($"LoadCaseType must be {LoadCases.LoadCase.LoadCaseTypes.SelfWeight}");
                axis.Unitize();
                GravityVersor = axis;
                Acceleration = acceleration;
            }
            else
                throw new ArgumentException($"LoadCaseType must be a SelfWeight Load Case ");
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Load"/>, "Vector" and "Acceleration"
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ModelGravityLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _versor = (Vector3d)info.GetValue("Vector", typeof(Vector3d));
            _acceleration = (double)info.GetValue("Acceleration", typeof(double));
        }

        /// <summary>
        /// Serializes the data of <see cref="Load"/>, the direction ("Vector") and the acceleration
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Vector", _versor, typeof(Vector3d));
            info.AddValue("Acceleration", _acceleration);
        }

        #endregion

        #region Equals, HashCode and operators

        /// <summary>
        /// Sets the gravity along the global X axis
        /// </summary>
        /// <param name="value">The acceleration</param>
        public void SetGravityX(double value)
        {
            GravityVersor = CoordinateSystem.Global.V1;
            Acceleration = value;
        }

        /// <summary>
        /// Sets the gravity along the global Y axis
        /// </summary>
        /// <param name="value">The acceleration</param>
        public void SetGravityY(double value)
        {
            GravityVersor = CoordinateSystem.Global.V2;
            Acceleration = value;
        }

        /// <summary>
        /// Sets the gravity along the global Z axis
        /// </summary>
        /// <param name="value">The acceleration</param>
        public void SetGravityZ(double value)
        {
            GravityVersor = CoordinateSystem.Global.V3;
            Acceleration = value;
        }

        /// <summary>
        /// Equality of name, load case, coordinate system, direction and acceleration
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal gravity load</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ModelGravityLoad objCasted = obj as ModelGravityLoad;

            return objCasted != null &&
                base.Equals(objCasted) &&
                GravityVersor == objCasted.GravityVersor &&
                Acceleration == objCasted.Acceleration;
        }


        /// <summary>
        /// The hash code of name, load case, coordinate system, direction and acceleration
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + GravityVersor.GetHashCode();
            hashCode = hashCode * -17 + Acceleration.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Not implemented: the gravity has no geometry
        /// </summary>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public override GeometryBase GetGeometryBase()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null loads are equal
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are equal</returns>
        public static bool operator ==(ModelGravityLoad obj1, ModelGravityLoad obj2)
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
        public static bool operator !=(ModelGravityLoad obj1, ModelGravityLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
