using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    /// <summary>
    /// The forces per unit length of a plate (area) element in its local coordinate system
    /// </summary>
    [Serializable]
    public sealed class ResultPlateForces : ResultType, IEquatable<ResultPlateForces>, ISerializable, IPlateResult, IResult<ResultPlateForces>
    {
        #region Variables

        /// <summary>
        /// Membrane force per unit length xx (local)
        /// </summary>
        private double _fxx;
        /// <summary>
        /// Membrane force per unit length yy (local)
        /// </summary>
        private double _fyy;
        /// <summary>
        /// Membrane shear force per unit length xy (local)
        /// </summary>
        private double _fxy;
        /// <summary>
        /// Transverse shear force per unit length xz (local)
        /// </summary>
        private double _fxz;
        /// <summary>
        /// Transverse shear force per unit length yz (local)
        /// </summary>
        private double _fyz;

        /// <summary>
        /// Bending moment per unit length xx (local)
        /// </summary>
        private double _mxx;
        /// <summary>
        /// Bending moment per unit length yy (local)
        /// </summary>
        private double _myy;
        /// <summary>
        /// Twisting moment per unit length xy (local)
        /// </summary>
        private double _mxy;

        #endregion

        #region Properties

        /// <summary>
        /// Membrane force per unit length xx (local)
        /// </summary>
        public double Fxx { get => _fxx; set => _fxx = value; }
        /// <summary>
        /// Membrane force per unit length yy (local)
        /// </summary>
        public double Fyy { get => _fyy; set => _fyy = value; }
        /// <summary>
        /// Membrane shear force per unit length xy (local)
        /// </summary>
        public double Fxy { get => _fxy; set => _fxy = value; }
        /// <summary>
        /// Transverse shear force per unit length xz (local)
        /// </summary>
        public double Fxz { get => _fxz; set => _fxz = value; }
        /// <summary>
        /// Transverse shear force per unit length yz (local)
        /// </summary>
        public double Fyz { get => _fyz; set => _fyz = value; }
        /// <summary>
        /// Bending moment per unit length xx (local)
        /// </summary>
        public double Mxx { get => _mxx; set => _mxx = value; }
        /// <summary>
        /// Bending moment per unit length yy (local)
        /// </summary>
        public double Myy { get => _myy; set => _myy = value; }
        /// <summary>
        /// Twisting moment per unit length xy (local)
        /// </summary>
        public double Mxy { get => _mxy; set => _mxy = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        /// <param name="fxx">Membrane force per unit length xx</param>
        /// <param name="fyy">Membrane force per unit length yy</param>
        /// <param name="fxy">Membrane shear force per unit length xy</param>
        /// <param name="fxz">Transverse shear force per unit length xz</param>
        /// <param name="fyz">Transverse shear force per unit length yz</param>
        /// <param name="mxx">Bending moment per unit length xx</param>
        /// <param name="myy">Bending moment per unit length yy</param>
        /// <param name="mxy">Twisting moment per unit length xy</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If <paramref name="coordinateSystem"/> is null</exception>
        public ResultPlateForces(CoordinateSystem coordinateSystem,
            double fxx, double fyy, double fxy, double fxz, double fyz, double mxx, double myy, double mxy, int id = ModelObjectId.IDUNASSIGNED)
            : base(coordinateSystem, string.Empty, id)
        {
            _fxx = fxx;
            _fyy = fyy;
            _fxy = fxy;
            _fxz = fxz;
            _fyz = fyz;
            _mxx = mxx;
            _myy = myy;
            _mxy = mxy;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private ResultPlateForces(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _fxx = info.GetDouble("Fxx");
            _fyy = info.GetDouble("Fyy");
            _fxy = info.GetDouble("Fxy");
            _fxz = info.GetDouble("Fxz");
            _fyz = info.GetDouble("Fyz");

            _mxx = info.GetDouble("Mxx");
            _myy = info.GetDouble("Myy");
            _mxy = info.GetDouble("Mxy");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The same result in another coordinate system (not implemented)
        /// </summary>
        /// <param name="coordinateSystem">The new coordinate system</param>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public ResultPlateForces ToCoordinateSystem(CoordinateSystem coordinateSystem)
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
            info.AddValue("Fxy", _fxy);
            info.AddValue("Fxz", _fxz);
            info.AddValue("Fyz", _fyz);

            info.AddValue("Mxx", _mxx);
            info.AddValue("Myy", _myy);
            info.AddValue("Mxy", _mxy);
        }

        /// <summary>
        /// Equality with another result (see <see cref="Equals(ResultPlateForces)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal result</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultPlateForces);
        }

        /// <summary>
        /// Exact equality of the components and of the name (the coordinate system is not compared)
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(ResultPlateForces other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _fxx == other._fxx &&
                _fyy == other._fyy &&
                _fxy == other._fxy &&
                _fxz == other._fxz &&
                _fyz == other._fyz &&
                _mxx == other._mxx &&
                _myy == other._myy &&
                _mxy == other._mxy &&
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
                hashCode = hashCode * -17 + _fxy.GetHashCode();
                hashCode = hashCode * -17 + _fxz.GetHashCode();
                hashCode = hashCode * -17 + _fyz.GetHashCode();
                hashCode = hashCode * -17 + _mxx.GetHashCode();
                hashCode = hashCode * -17 + _myy.GetHashCode();
                hashCode = hashCode * -17 + _mxy.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(ResultPlateForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
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

        /// <summary>
        /// Inequality operator (see <see cref="Equals(ResultPlateForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(ResultPlateForces obj1, ResultPlateForces obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
