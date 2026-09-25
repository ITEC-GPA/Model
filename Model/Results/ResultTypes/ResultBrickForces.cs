using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    /// <summary>
    /// The forces and moments of a brick (volume) element in a local coordinate system
    /// </summary>
    [Serializable]
    public sealed class ResultBrickForces : ResultType, IEquatable<ResultBrickForces>, ISerializable, IBrickResult, IResult<ResultBrickForces>
    {
        #region Variables

        /// <summary>
        /// Force component xx (local)
        /// </summary>
        private double _fxx;
        /// <summary>
        /// Force component yy (local)
        /// </summary>
        private double _fyy;
        /// <summary>
        /// Force component zz (local)
        /// </summary>
        private double _fzz;
        /// <summary>
        /// Force component xy (local)
        /// </summary>
        private double _fxy;
        /// <summary>
        /// Force component xz (local)
        /// </summary>
        private double _fxz;
        /// <summary>
        /// Force component yz (local)
        /// </summary>
        private double _fyz;

        /// <summary>
        /// Moment component xx (local)
        /// </summary>
        private double _mxx;
        /// <summary>
        /// Moment component yy (local)
        /// </summary>
        private double _myy;
        /// <summary>
        /// Moment component zz (local)
        /// </summary>
        private double _mzz;
        /// <summary>
        /// Moment component xy (local)
        /// </summary>
        private double _mxy;
        /// <summary>
        /// Moment component xz (local)
        /// </summary>
        private double _mxz;
        /// <summary>
        /// Moment component yz (local)
        /// </summary>
        private double _myz;

        #endregion

        #region Properties

        /// <summary>
        /// Force component xx (local)
        /// </summary>
        public double Fxx { get => _fxx; set => _fxx = value; }
        /// <summary>
        /// Force component yy (local)
        /// </summary>
        public double Fyy { get => _fyy; set => _fyy = value; }
        /// <summary>
        /// Force component zz (local)
        /// </summary>
        public double Fzz { get => _fzz; set => _fzz = value; }
        /// <summary>
        /// Force component xy (local)
        /// </summary>
        public double Fxy { get => _fxy; set => _fxy = value; }
        /// <summary>
        /// Force component xz (local)
        /// </summary>
        public double Fxz { get => _fxz; set => _fxz = value; }
        /// <summary>
        /// Force component yz (local)
        /// </summary>
        public double Fyz { get => _fyz; set => _fyz = value; }
        /// <summary>
        /// Moment component xx (local)
        /// </summary>
        public double Mxx { get => _mxx; set => _mxx = value; }
        /// <summary>
        /// Moment component yy (local)
        /// </summary>
        public double Myy { get => _myy; set => _myy = value; }
        /// <summary>
        /// Moment component zz (local)
        /// </summary>
        public double Mzz { get => _mzz; set => _mzz = value; }
        /// <summary>
        /// Moment component xy (local)
        /// </summary>
        public double Mxy { get => _mxy; set => _mxy = value; }
        /// <summary>
        /// Moment component xz (local)
        /// </summary>
        public double Mxz { get => _mxz; set => _mxz = value; }
        /// <summary>
        /// Moment component yz (local)
        /// </summary>
        public double Myz { get => _myz; set => _myz = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        /// <param name="fxx">Force component xx</param>
        /// <param name="fyy">Force component yy</param>
        /// <param name="fzz">Force component zz</param>
        /// <param name="fxy">Force component xy</param>
        /// <param name="fxz">Force component xz</param>
        /// <param name="fyz">Force component yz</param>
        /// <param name="mxx">Moment component xx</param>
        /// <param name="myy">Moment component yy</param>
        /// <param name="mzz">Moment component zz</param>
        /// <param name="mxy">Moment component xy</param>
        /// <param name="mxz">Moment component xz</param>
        /// <param name="myz">Moment component yz</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="coordinateSystem"/> is null</exception>
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

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// The same result in another coordinate system (not implemented)
        /// </summary>
        /// <param name="coordinateSystem">The new coordinate system</param>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public ResultBrickForces ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the result
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Equality with another result (see <see cref="Equals(ResultBrickForces)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal result</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultBrickForces);
        }

        /// <summary>
        /// Exact equality of the components and of the name (the coordinate system is not compared)
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
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

        /// <summary>
        /// The hash code of the name and of the components
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(ResultBrickForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
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

        /// <summary>
        /// Inequality operator (see <see cref="Equals(ResultBrickForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(ResultBrickForces obj1, ResultBrickForces obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
