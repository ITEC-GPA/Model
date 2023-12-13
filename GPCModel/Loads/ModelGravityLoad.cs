using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    public class ModelGravityLoad : Load, ISerializable
    {
        /// <summary>
        /// Value of the gravity acceleration [mm/s^2]
        /// </summary>
        public const double GRAVITYACCELERATION = 9806.65;

        #region Variables

        protected Vector3d _versor;
        protected double _acceleration;

        #endregion

        #region Properties

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

        public double Acceleration { get => _acceleration; set => _acceleration = value; }

        public Vector3d GravityVector => _versor * _acceleration;

        #endregion

        #region Constructor

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

        protected ModelGravityLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _versor = (Vector3d)info.GetValue("Vector", typeof(Vector3d));
            _acceleration = (double)info.GetValue("Acceleration", typeof(double));
        }

        #endregion

        #region Equals, HashCode and operators

        public void SetGravityX(double value)
        {
            GravityVersor = CoordinateSystem.Global.V1;
            Acceleration = value;
        }

        public void SetGravityY(double value)
        {
            GravityVersor = CoordinateSystem.Global.V2;
            Acceleration = value;
        }

        public void SetGravityZ(double value)
        {
            GravityVersor = CoordinateSystem.Global.V3;
            Acceleration = value;
        }

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


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + GravityVersor.GetHashCode();
            hashCode = hashCode * -17 + Acceleration.GetHashCode();
            return hashCode;
        }

        public override GeometryBase GetGeometryBase()
        {
            throw new System.NotImplementedException();
        }

        public static bool operator ==(ModelGravityLoad obj1, ModelGravityLoad obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(ModelGravityLoad obj1, ModelGravityLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
