using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Restrains
{
    /// <summary>
    /// External restrain or stiffness applied to a Dof
    /// </summary>
    [Serializable]
    public sealed class DofRestrain : ModelObject, IEquatable<DofRestrain>
    {
        #region Variables

        /// <summary>
        /// The degree of freedom
        /// </summary>
        private readonly GeometryRestrain.DOF _dof;
        /// <summary>
        /// True if the degree of freedom is restrained
        /// </summary>
        private bool _restrained;
        /// <summary>
        /// The imposed displacement (0 if none)
        /// </summary>
        private double _imposedDisplacement;
        /// <summary>
        /// The stiffness of the elastic restrain (0 if none)
        /// </summary>
        private double _stiffness;

        #endregion

        #region Properties

        /// <summary>
        /// The degree of freedom
        /// </summary>
        public GeometryRestrain.DOF Dof => _dof;

        /// <summary>
        /// <see langword="true"/> if the <see cref="Dof"/> is restrained (setting it to true resets the stiffness and the imposed displacement to
        /// zero). <see langword="false"/> if it is released or there is a stiffness or an imposed displacement
        /// </summary>
        public bool IsRestrained
        {
            get => _restrained;
            set
            {
                _restrained = value;
                if (value)
                {
                    _imposedDisplacement = 0;
                    _stiffness = 0;
                }
            }
        }

        /// <summary>
        /// True if the stiffness is not zero
        /// </summary>
        public bool HasStiffness => _stiffness != 0;

        /// <summary>
        /// True if the imposed displacement is not zero
        /// </summary>
        public bool HasImposedDisplacement => _imposedDisplacement != 0;

        /// <summary>
        /// The stiffness of the elastic restrain (a value different from zero sets <see cref="IsRestrained"/> to false; the setter does not check
        /// the sign)
        /// </summary>
        public double Stiffness
        {
            get => _stiffness;
            set
            {
                _stiffness = value;
                if (value != 0)
                    _restrained = false;
            }
        }

        /// <summary>
        /// The imposed displacement (a value different from zero sets <see cref="IsRestrained"/> to false)
        /// </summary>
        public double ImposedDisplacement
        {
            get => _imposedDisplacement;
            set
            {
                _imposedDisplacement = value;
                if (value != 0)
                    _restrained = false;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Set the <paramref name="dof"/> as restrained or released
        /// </summary>
        /// <param name="dof">The degree of freedom</param>
        /// <param name="restrained">True if restrained, false if released</param>
        public DofRestrain(GeometryRestrain.DOF dof, bool restrained = true)
            : this(dof, restrained, 0, 0)
        {

        }

        /// <summary>
        /// Set the stiffness associated to <paramref name="dof"/>
        /// </summary>
        /// <param name="dof">The degree of freedom</param>
        /// <param name="stiffness">The stiffness (not negative)</param>
        /// <remarks>If <paramref name="stiffness"/> is equal to zero then <see cref="IsRestrained"/> is true</remarks>
        /// <exception cref="ArgumentException">If <paramref name="stiffness"/> is negative</exception>
        public DofRestrain(GeometryRestrain.DOF dof, double stiffness)
            : this(dof, stiffness == 0, 0, stiffness)
        {

        }

        /// <summary>
        /// Creates a restrain of a degree of freedom (the values are not made consistent: see <see cref="IsRestrained"/>)
        /// </summary>
        /// <param name="dof">The degree of freedom</param>
        /// <param name="restrained">True if restrained</param>
        /// <param name="imposedDisplacement">The imposed displacement</param>
        /// <param name="stiffness">The stiffness (not negative)</param>
        /// <exception cref="ArgumentException">If <paramref name="stiffness"/> is negative</exception>
        public DofRestrain(GeometryRestrain.DOF dof, bool restrained, double imposedDisplacement, double stiffness)
            : base(Guid.NewGuid(), "")
        {
            _dof = dof;
            _restrained = restrained;
            _imposedDisplacement = imposedDisplacement;
            _stiffness = stiffness < 0 ? throw new ArgumentException($"Stiffness is lower than zero: {stiffness}") : stiffness;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private DofRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dof = (GeometryRestrain.DOF)info.GetValue("Dof", typeof(GeometryRestrain.DOF));
            _restrained = info.GetBoolean("Restrained");
            _stiffness = info.GetDouble("Stiffness");
            _imposedDisplacement = info.GetDouble("ImposedDisplacement");
        }

        #endregion

        #region Equals, hascode, operators

        /// <summary>
        /// Serializes the restrain
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Dof", _dof, typeof(GeometryRestrain.DOF));
            info.AddValue("Restrained", _restrained);
            info.AddValue("Stiffness", _stiffness);
            info.AddValue("ImposedDisplacement", _imposedDisplacement);
        }

        /// <summary>
        /// Equality of the degree of freedom, of the values and of the name
        /// </summary>
        /// <param name="other">The restrain to compare</param>
        /// <returns>True if the restrains are equal</returns>
        public bool Equals(DofRestrain other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _dof.Equals(other.Dof) && _restrained.Equals(other.IsRestrained)
                                          && _stiffness.Equals(other.Stiffness)
                                          && _imposedDisplacement.Equals(other.ImposedDisplacement)
                                          && base.Equals(other);
        }

        /// <summary>
        /// Equality with another restrain (the method calls itself with a <see cref="DofRestrain"/>: it throws <see cref="StackOverflowException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>False if <paramref name="obj"/> is not a <see cref="DofRestrain"/></returns>
        public override bool Equals(object obj)
        {
            if (obj is DofRestrain)
                return Equals(obj);

            return false;
        }

        /// <summary>
        /// The hash code of the name, of the degree of freedom and of the values
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17 * base.GetHashCode();
                hashCode = hashCode * -19 + EqualityComparer<GeometryRestrain.DOF>.Default.GetHashCode(_dof);
                hashCode = hashCode * -19 + EqualityComparer<bool>.Default.GetHashCode(_restrained);
                hashCode = hashCode * -19 + EqualityComparer<double>.Default.GetHashCode(_stiffness);
                hashCode = hashCode * -19 + EqualityComparer<double>.Default.GetHashCode(_imposedDisplacement);

                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(DofRestrain)"/>)
        /// </summary>
        /// <param name="obj1">The first restrain</param>
        /// <param name="obj2">The second restrain</param>
        /// <returns>True if the restrains are equal</returns>
        public static bool operator ==(DofRestrain obj1, DofRestrain obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(DofRestrain)"/>)
        /// </summary>
        /// <param name="obj1">The first restrain</param>
        /// <param name="obj2">The second restrain</param>
        /// <returns>True if the restrains are different</returns>
        public static bool operator !=(DofRestrain obj1, DofRestrain obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}