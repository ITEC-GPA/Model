using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    /// <summary>
    /// The internal forces of a beam in a coordinate system: V1 and V2 along the first and second axes (the cross-section plane), N along the
    /// third axis (the beam axis); M1, M2 and T around the same axes
    /// </summary>
    [Serializable]
    public class ResultBeamForces : ResultType, ISerializable, IBeamResult, IResult<ResultBeamForces>
    {
        #region Variables

        /// <summary>
        /// The axial force
        /// </summary>
        private double _N;
        /// <summary>
        /// The shear along axis 1
        /// </summary>
        private double _V1;
        /// <summary>
        /// The shear along axis 2
        /// </summary>
        private double _V2;
        /// <summary>
        /// The torque moment
        /// </summary>
        private double _T;
        /// <summary>
        /// The bending moment around axis 1
        /// </summary>
        private double _M1;
        /// <summary>
        /// The bending moment around axis 2
        /// </summary>
        private double _M2;

        #endregion

        #region Properties

        /// <summary>
        /// The axial force (along the third axis)
        /// </summary>
        public double N { get => _N; set => _N = value; }
        /// <summary>
        /// The shear along axis 1
        /// </summary>
        public double V1 { get => _V1; set => _V1 = value; }
        /// <summary>
        /// The shear along axis 2
        /// </summary>
        public double V2 { get => _V2; set => _V2 = value; }
        /// <summary>
        /// The torque moment (around the third axis)
        /// </summary>
        public double T { get => _T; set => _T = value; }
        /// <summary>
        /// The bending moment around axis 1 (in plane 2, right hand rule)
        /// </summary>
        public double M1 { get => _M1; set => _M1 = value; }
        /// <summary>
        /// The bending moment around axis 2 (in plane 1, right hand rule)
        /// </summary>
        public double M2 { get => _M2; set => _M2 = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the forces
        /// </summary>
        /// <param name="N">The axial force (along the third axis)</param>
        /// <param name="V1">The shear along axis 1</param>
        /// <param name="V2">The shear along axis 2</param>
        /// <param name="T">The torque moment (around the third axis)</param>
        /// <param name="M1">The bending moment around axis 1 (in plane 2, right hand rule)</param>
        /// <param name="M2">The bending moment around axis 2 (in plane 1, right hand rule)</param>
        /// <param name="coordinateSystem">The beam coordinate system (its origin is the point of application)</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="coordinateSystem"/> is null</exception>
        public ResultBeamForces(double N, double V1, double V2, double T, double M1, double M2, CoordinateSystem coordinateSystem, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(coordinateSystem, name, id)
        {
            _N = N;
            _V1 = V1;
            _V2 = V2;
            _T = T;
            _M1 = M1;
            _M2 = M2;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Serializes the forces
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// The same forces in another coordinate system: the forces and the moments are rotated, the origins are not considered (see
        /// <see cref="ToCoordinateSystemWithEccentricity(CoordinateSystem)"/>)
        /// </summary>
        /// <param name="coordinateSystem">The new coordinate system</param>
        /// <returns>New forces with the same id and name</returns>
        public ResultBeamForces ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            Vector3d vector3dForce = new Vector3d(V1, V2, N);
            Vector3d vector3dMoment = new Vector3d(M1, M2, T);

            Vector3d vector3dvector3dForceGlobal = CoordinateSystem.ToGlobal(vector3dForce);
            Vector3d vector3dvector3dMomentGlobal = CoordinateSystem.ToGlobal(vector3dMoment);

            var forceNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dForceGlobal);
            var momentNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dMomentGlobal);

            return new ResultBeamForces(forceNewCoordinate.Z, forceNewCoordinate.X, forceNewCoordinate.Y,
                momentNewCoordinate.Z, momentNewCoordinate.X, momentNewCoordinate.Y, coordinateSystem, _id, _name);
        }

        /// <summary>
        /// The same forces in another coordinate system, moved to its origin: the moments get the transport moment of the forces
        /// (M' = M + (O - O') x F). See <see cref="ToCoordinateSystem(CoordinateSystem)"/>
        /// </summary>
        /// <param name="coordinateSystem">The new coordinate system</param>
        /// <returns>New forces with the same id and name</returns>
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
                momentNewCoordinate.Z, momentNewCoordinate.X, momentNewCoordinate.Y, coordinateSystem, _id, _name);
        }

        /// <summary>
        /// The same forces in the global coordinate system (rotation only, see <see cref="ToCoordinateSystem(CoordinateSystem)"/>)
        /// </summary>
        /// <returns>This instance if the coordinate system is already the global one, otherwise new forces</returns>
        public ResultBeamForces ToGlobalCoordinateSystem()
        {
            if (CoordinateSystem == CoordinateSystem.Global)
                return this;
            else
                return ToCoordinateSystem(CoordinateSystem.Global);
        }

        /// <summary>
        /// The combined bending moment: sqrt(M1² + M2²)
        /// </summary>
        /// <returns>The combined bending moment</returns>
        public double GetCombinedBendingMoment()
        {
            return Math.Sqrt(Math.Pow(M1, 2) + Math.Pow(M2, 2));
        }

        /// <summary>
        /// The combined shear force: sqrt(V1² + V2²)
        /// </summary>
        /// <returns>The combined shear force</returns>
        public double GetCombinedShearForce()
        {
            return Math.Sqrt(Math.Pow(V1, 2) + Math.Pow(V2, 2));
        }
        /// <summary>
        /// Equality within <see cref="GeometryBase.Tolerance"/> (see <see cref="Equals(object, in double)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> are equal forces</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj, GeometryBase.Tolerance);
        }

        /// <summary>
        /// Equality of the forces within a tolerance: <paramref name="obj"/> is moved to this coordinate system (see
        /// <see cref="ToCoordinateSystemWithEccentricity(CoordinateSystem)"/>) and each component is compared (absolute difference)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <param name="tollerance">The tolerance on each component</param>
        /// <returns>True if <paramref name="obj"/> are equal forces</returns>
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

        /// <summary>
        /// The hash code of the name and of the exact components (forces equal within the tolerance or in another coordinate system can have
        /// different hash codes)
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first forces</param>
        /// <param name="right">The second forces</param>
        /// <returns>True if the forces are equal</returns>
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

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first forces</param>
        /// <param name="right">The second forces</param>
        /// <returns>True if the forces are different</returns>
        public static bool operator !=(ResultBeamForces left, ResultBeamForces right)
        {
            return !(left == right);
        }

        /// <summary>
        /// The sum of the forces in the coordinate system of <paramref name="left"/> (<paramref name="right"/> is moved there, see
        /// <see cref="ToCoordinateSystemWithEccentricity(CoordinateSystem)"/>)
        /// </summary>
        /// <param name="left">The first forces</param>
        /// <param name="right">The second forces</param>
        /// <returns>New forces with the id and the name of <paramref name="left"/></returns>
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
                left.CoordinateSystem,
                left.Id,
                left._name);
        }

        /// <summary>
        /// The forces divided by a number
        /// </summary>
        /// <param name="left">The forces</param>
        /// <param name="denom">The divisor</param>
        /// <returns>New forces with the same coordinate system, id and name</returns>
        public static ResultBeamForces operator /(ResultBeamForces left, double denom)
        {
            return new ResultBeamForces(
                left.N / denom,
                left.V1 / denom,
                left.V2 / denom,
                left.T / denom,
                left.M1 / denom,
                left.M2 / denom,
                left.CoordinateSystem,
                left.Id,
                left._name);
        }

        /// <summary>
        /// The opposite forces
        /// </summary>
        /// <param name="other">The forces</param>
        /// <returns>New forces with the same coordinate system, id and name</returns>
        public static ResultBeamForces operator -(ResultBeamForces other)
        {
            return new ResultBeamForces(
                -other.N,
                -other.V1,
                -other.V2,
                -other.T,
                -other.M1,
                -other.M2,
                other.CoordinateSystem,
                other.Id,
                other._name);
        }

        #endregion
    }
}