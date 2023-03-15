using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultBeamForces : ResultType, ISerializable, IBeamResult, IResult<ResultBeamForces>
    {
        #region Variables

        private double _N;
        private double _V1;
        private double _V2;
        private double _T;
        private double _M1;
        private double _M2;

        #endregion

        #region Properties

        public double N { get => _N; set => _N = value; }
        public double V1 { get => _V1; set => _V1 = value; }
        public double V2 { get => _V2; set => _V2 = value; }
        public double T { get => _T; set => _T = value; }
        public double M1 { get => _M1; set => _M1 = value; }
        public double M2 { get => _M2; set => _M2 = value; }

        #endregion

        #region Public Constructors

        /// <param name="N"> axial force </param>
        /// <param name="V1"> shear along principal axis 1</param>
        /// <param name="V2"> shear along principal axis 2</param>
        /// <param name="T"> torque moment </param>
        /// <param name="M1"> Bending moment around axis 1 (in plane 2, right hand rule) </param>
        /// <param name="M2"> Bending moment around axis 2 (in plane 1, right hand rule) </param>
        /// <param name="coordinateSystem">The beam coordinateSystem</param>
        /// <param name="id"></param>
        public ResultBeamForces(double N, double V1, double V2, double T, double M1, double M2, CoordinateSystem coordinateSystem, int id = ModelObjectId.IDUNASSIGNED)
            : base(coordinateSystem, string.Empty, id)
        {
            _N = N;
            _V1 = V1;
            _V2 = V2;
            _T = T;
            _M1 = M1;
            _M2 = M2;
        }

        protected ResultBeamForces(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _N = info.GetDouble("N");
            _V1 = info.GetDouble("V1");
            _V2 = info.GetDouble("V2");
            _T = info.GetDouble("T");
            _M1 = info.GetDouble("M1");
            _M2 = info.GetDouble("M2");
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("N", _N);
            info.AddValue("V1", _V1);
            info.AddValue("V2", _V2);
            info.AddValue("T", _T);
            info.AddValue("M1", _M1);
            info.AddValue("M2", _M2);
        }

        public ResultBeamForces ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            Vector3d vector3dForce = new Vector3d(V1, V2, N);
            Vector3d vector3dMoment = new Vector3d(M1, M2, T);

            Vector3d vector3dvector3dForceGlobal = CoordinateSystem.ToGlobal(vector3dForce);
            Vector3d vector3dvector3dMomentGlobal = CoordinateSystem.ToGlobal(vector3dMoment);

            var forceNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dForceGlobal);
            var momentNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dMomentGlobal);

            return new ResultBeamForces(forceNewCoordinate.Z, forceNewCoordinate.X, forceNewCoordinate.Y,
                momentNewCoordinate.Z, momentNewCoordinate.X, momentNewCoordinate.Y, coordinateSystem);
        }

        /// <summary>
        /// Similar to <see cref="ToCoordinateSystem(CoordinateSystem)"/> but with eccentricity.
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        public ResultBeamForces ToCoordinateSystemWithEccentricity(CoordinateSystem coordinateSystem)
        {
            Vector3d vector3dForce = new Vector3d(V1, V2, N);
            Vector3d vector3dMoment = new Vector3d(M1, M2, T);
            Vector3d eccentricity = this.CoordinateSystem.Origin - coordinateSystem.Origin;

            Vector3d vector3dvector3dForceGlobal = CoordinateSystem.ToGlobal(vector3dForce);
            Vector3d vector3dvector3dMomentGlobal = CoordinateSystem.ToGlobal(vector3dMoment);
            vector3dvector3dMomentGlobal += eccentricity.CrossProduct(vector3dvector3dForceGlobal);

            var forceNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dForceGlobal);
            var momentNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dMomentGlobal);

            return new ResultBeamForces(forceNewCoordinate.Z, forceNewCoordinate.X, forceNewCoordinate.Y,
                momentNewCoordinate.Z, momentNewCoordinate.X, momentNewCoordinate.Y, coordinateSystem);
        }

        /// <summary>
        /// Return new ResultBeamForces in global coordinate system
        /// </summary>
        /// <returns>New ResultBeamForces</returns>
        public ResultBeamForces ToGlobalCoordinateSystem()
        {
            if (CoordinateSystem == CoordinateSystem.Global)
                return this;
            else
                return ToCoordinateSystem(CoordinateSystem.Global);
        }

        /// <summary>
        /// Return the combined bending moment between M1 and M2
        /// </summary>
        /// <returns>The combined bending moment</returns>
        public double GetCombinedBendingMoment()
        {
            return Math.Sqrt(Math.Pow(M1, 2) + Math.Pow(M2, 2));
        }

        /// <summary>
        /// Return the combined bending moment between V1 and V2
        /// </summary>
        /// <returns>he combined shear force</returns>
        public double GetCombinedShearForce()
        {
            return Math.Sqrt(Math.Pow(V1, 2) + Math.Pow(V2, 2));
        }
        public override bool Equals(object obj)
        {
            return Equals(obj, GeometryBase.Tolerance);
        }

        public bool Equals(object obj, in double tollerance = GeometryBase.Tolerance)
        {
            var other = obj as ResultBeamForces;
            if (other == null)
                return false;

            var otherSamePos = other.ToCoordinateSystemWithEccentricity(this.CoordinateSystem);
            return
                Math.Abs(_N - otherSamePos._N) < tollerance &&
                Math.Abs(_V1 - otherSamePos._V1) < tollerance &&
                Math.Abs(_V2 - otherSamePos._V2) < tollerance &&
                Math.Abs(_T - otherSamePos._T) < tollerance &&
                Math.Abs(_M1 - otherSamePos._M1) < tollerance &&
                Math.Abs(_M2 - otherSamePos._M2) < tollerance;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _N.GetHashCode();
                hashCode = hashCode * -23 + _V1.GetHashCode();
                hashCode = hashCode * -23 + _V2.GetHashCode();
                hashCode = hashCode * -23 + _T.GetHashCode();
                hashCode = hashCode * -23 + _M1.GetHashCode();
                hashCode = hashCode * -23 + _M2.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ResultBeamForces left, ResultBeamForces right)
        {
            if (left is null)
            {
                if (right is null)
                    return true;
                else
                    return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(ResultBeamForces left, ResultBeamForces right)
        {
            return !(left == right);
        }

        public static ResultBeamForces operator +(ResultBeamForces left, ResultBeamForces right)
        {
            var rightInRightPos = right.ToCoordinateSystemWithEccentricity(left.CoordinateSystem);
            return new ResultBeamForces(
                left.N + rightInRightPos.N,
                left.V1 + rightInRightPos.V1,
                left.V2 + rightInRightPos.V2,
                left.T + rightInRightPos.T,
                left.M1 + rightInRightPos.M1,
                left.M2 + rightInRightPos.M2,
                left.CoordinateSystem);
        }

        public static ResultBeamForces operator /(ResultBeamForces left, double denom)
        {
            return new ResultBeamForces(
                left.N / denom,
                left.V1 / denom,
                left.V2 / denom,
                left.T / denom,
                left.M1 / denom,
                left.M2 / denom,
                left.CoordinateSystem);
        }

        #endregion
    }
}