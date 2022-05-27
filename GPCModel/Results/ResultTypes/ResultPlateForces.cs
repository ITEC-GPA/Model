using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultPlateForces : ResultType, IEquatable<ResultPlateForces>, ISerializable, IPlateResult, IResult<ResultPlateForces>
    {
		#region Variables

		/// Local Forces
		private readonly double _fxx;
        private readonly double _fyy;
        private readonly double _fzz;
        private readonly double _fxy;
        private readonly double _fxz;
        private readonly double _fyz;

        /// Local Moments
        private readonly double _mxx;
        private readonly double _myy;
        private readonly double _mzz;
        private readonly double _mxy;
        private readonly double _mxz;
        private readonly double _myz;

		#endregion

		#region Properties

		/// Local Forces
		public double Fxx => _fxx;
        public double Fyy => _fyy;
        public double Fzz => _fzz;
        public double Fxy => _fxy;
        public double Fxz => _fxz;
        public double Fyz => _fyz;

        /// Local Moments      
        public double Mxx => _mxx;
        public double Myy => _myy;
        public double Mzz => _mzz;
        public double Mxy => _mxy;
        public double Mxz => _mxz;
        public double Myz => _myz;

		#endregion

		#region Constructor

		/// <param name="coordinateSystem">Coordinate system where these result are provided</param>
		/// <param name="fxx"></param>
		/// <param name="fyy"></param>
		/// <param name="fxy"></param>
		/// <param name="fxz"></param>
		/// <param name="fyz"></param>
		/// <param name="mxx"></param>
		/// <param name="myy"></param>
		/// <param name="mxy"></param>
		/// <param name="id"></param>
		public ResultPlateForces(CoordinateSystem coordinateSystem, 
            double fxx, double fyy, double fxy, double fxz, double fyz, double mxx, double myy, double mxy, int id = ModelObjectId.IDUNASSIGNED) 
            : base(coordinateSystem, string.Empty, id)
        {
            _fxx = fxx;
            _fyy = fyy;
            _fzz = 0;
            _fxy = fxy;
            _fxz = fxz;
            _fyz = fyz;
            _mxx = mxx;
            _myy = myy;
            _mzz = 0;
            _mxy = mxy;
            _mxz = 0;
            _myz = 0;
        }

        private ResultPlateForces(SerializationInfo info, StreamingContext context)
            : base(info, context)
		{
            _fxx = info.GetDouble("Fxx");
            _fyy = info.GetDouble("Fyy");
            _fzz = info.GetDouble("Fzz");
            _fxy = info.GetDouble("Fxy");
            _fxz = info.GetDouble("Fxz");
            _fyz = info.GetDouble("Fyz");

            _mxx = info.GetDouble("Mxx");
            _myy = info.GetDouble("Myy");
            _mzz = info.GetDouble("Mzz");
            _mxy = info.GetDouble("Mxy");
            _mxz = info.GetDouble("Mxz");
            _myz = info.GetDouble("Myz");
        }

        #endregion

        #region Public Methods

        public ResultPlateForces ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            throw new NotImplementedException();
        }

		#endregion

		#region Equals, hashcode, operators

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fxx", _fxx);
            info.AddValue("Fyy", _fyy);
            info.AddValue("Fzz", _fzz);
            info.AddValue("Fxy", _fxy);
            info.AddValue("Fxz", _fxz);
            info.AddValue("Fyz", _fyz);

            info.AddValue("Mxx", _mxx);
            info.AddValue("Myy", _myy);
            info.AddValue("Mzz", _mzz);
            info.AddValue("Mxy", _mxy);
            info.AddValue("Mxz", _mxz);
            info.AddValue("Myz", _myz);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultPlateForces);
        }

        public bool Equals(ResultPlateForces other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _fxx == other._fxx && _fyy == other._fyy
                                    && _fzz == other._fzz && _fxy == other._fxy
                                    && _fxz == other._fxz && _fyz == other._fyz

                                    && _mxx == other._mxx && _myy == other._myy
                                    && _mzz == other._mzz && _mxy == other._mxy
                                    && _mxz == other._mxz && _myz == other._myz
                                    && base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _fxx.GetHashCode();
                hashCode = hashCode * -17 + _fyy.GetHashCode();
                hashCode = hashCode * -17 + _fzz.GetHashCode();
                hashCode = hashCode * -17 + _fxy.GetHashCode();
                hashCode = hashCode * -17 + _fxz.GetHashCode();
                hashCode = hashCode * -17 + _fyz.GetHashCode();
                hashCode = hashCode * -17 + _mxx.GetHashCode();
                hashCode = hashCode * -17 + _myy.GetHashCode();
                hashCode = hashCode * -17 + _mzz.GetHashCode();
                hashCode = hashCode * -17 + _mxy.GetHashCode();
                hashCode = hashCode * -17 + _mxz.GetHashCode();
                hashCode = hashCode * -17 + _myz.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ResultPlateForces obj1, ResultPlateForces obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultPlateForces obj1, ResultPlateForces obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
