using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultBrickForces : ResultType, IEquatable<ResultBrickForces>, ISerializable, IBrickResult, IResult<ResultBrickForces>
    {
        #region Variables

        /// Local Forces
        private double _fxx;
        private double _fyy;
        private double _fzz;
        private double _fxy;
        private double _fxz;
        private double _fyz;

        /// Local Moments
        private double _mxx;
        private double _myy;
        private double _mzz;
        private double _mxy;
        private double _mxz;
        private double _myz;

        #endregion

        #region Properties

        public double Fxx { get => _fxx; set => _fxx = value; }
        public double Fyy { get => _fyy; set => _fyy = value; }
        public double Fzz { get => _fzz; set => _fzz = value; }
        public double Fxy { get => _fxy; set => _fxy = value; }
        public double Fxz { get => _fxz; set => _fxz = value; }
        public double Fyz { get => _fyz; set => _fyz = value; }
        public double Mxx { get => _mxx; set => _mxx = value; }
        public double Myy { get => _myy; set => _myy = value; }
        public double Mzz { get => _mzz; set => _mzz = value; }
        public double Mxy { get => _mxy; set => _mxy = value; }
        public double Mxz { get => _mxz; set => _mxz = value; }
        public double Myz { get => _myz; set => _myz = value; }

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
        public ResultBrickForces(CoordinateSystem coordinateSystem,
            double fxx, double fyy, double fzz, double fxy, double fxz, double fyz, double mxx, double myy, double mzz, double mxy, double mxz, double myz,
            int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(coordinateSystem, name, id)
        {
            _fxx = fxx;
            _fyy = fyy;
            _fzz = fzz;
            _fxy = fxy;
            _fxz = fxz;
            _fyz = fyz;
            _mxx = mxx;
            _myy = myy;
            _mzz = mzz;
            _mxy = mxy;
            _mxz = mxz;
            _myz = myz;
        }

        private ResultBrickForces(SerializationInfo info, StreamingContext context)
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

        public ResultBrickForces ToCoordinateSystem(CoordinateSystem coordinateSystem)
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

            return Equals(obj as ResultBrickForces);
        }

        public bool Equals(ResultBrickForces other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _fxx == other._fxx &&
                _fyy == other._fyy &&
                _fzz == other._fzz &&
                _fxy == other._fxy &&
                _fxz == other._fxz &&
                _fyz == other._fyz &&
                _mxx == other._mxx &&
                _myy == other._myy &&
                _mzz == other._mzz &&
                _mxy == other._mxy &&
                _mxz == other._mxz &&
                _myz == other._myz &&
                base.Equals(other);
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

        public static bool operator ==(ResultBrickForces obj1, ResultBrickForces obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultBrickForces obj1, ResultBrickForces obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
