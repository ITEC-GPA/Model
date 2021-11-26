using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.LoadCases;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultBeamForces : ResultType, ISerializable, IBeamResult, IResult<ResultBeamForces>
    {
        #region Variables

        private readonly double _N;
        private readonly double _V1;
        private readonly double _V2;
        private readonly double _T;
        private readonly double _M1;
        private readonly double _M2;

        #endregion 

        #region Properties

        public double N => _N;
        public double V1 => _V1;
        public double V2 => _V2;
        public double T => _T;
        public double M1 => _M1;
        public double M2 => _M2;

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
            _N  = N;
            _V1 = V1;
            _V2 = V2;
            _T  = T;
            _M1 = M1;
            _M2 = M2;
        }

        protected ResultBeamForces(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _N  = (double)info.GetValue("N" , typeof(double));
            _V1 = (double)info.GetValue("V1", typeof(double));
            _V2 = (double)info.GetValue("V2", typeof(double));
            _T  = (double)info.GetValue("T" , typeof(double));
            _M1 = (double)info.GetValue("M1", typeof(double));
            _M2 = (double)info.GetValue("M2", typeof(double));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("N" , _N );
            info.AddValue("V1", _V1);
            info.AddValue("V2", _V2);
            info.AddValue("T" , _T );
            info.AddValue("M1", _M1);
            info.AddValue("M2", _M2);
        }
        #endregion

        #region Public Methods


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
            return obj is ResultBeamForces other && _N == other._N && _V1 == other._V1 && 
                _V2 == other._V2 && _T == other._T && _M1 == other._M1 && _M2 == other._M2;
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
            return left.Equals(right);
        }

        public static bool operator !=(ResultBeamForces left, ResultBeamForces right)
        {
            return !(left == right);
        }

        #endregion
    }
}