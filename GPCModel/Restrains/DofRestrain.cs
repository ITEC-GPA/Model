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

        private readonly GeometryRestrain.DOF _dof;
        private bool _restrained;
        private double _imposedDisplacement;
        private double _stiffness;

        #endregion

        #region Properties

        public GeometryRestrain.DOF Dof => _dof;

        /// <summary>
        /// <see langword="True"/> if the <see cref="_dof"/> is restrained. 
        /// <see langword="False"/> if there is a stiffness or imposed displacement
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

        public bool HasStiffness => _stiffness != 0;

        public bool HasImposedDisplacement => _imposedDisplacement != 0;

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
        public DofRestrain(GeometryRestrain.DOF dof, bool restrained = true)
            : this(dof, restrained, 0, 0)
        {

        }

        /// <summary>
        /// Set the stiffness associated to <paramref name="dof"/>
        /// </summary>
        /// <remarks>If <paramref name="stiffness"/> is equal to zero then <see cref="DofRestrain.IsRestrained"/> is true</remarks>
        public DofRestrain(GeometryRestrain.DOF dof, double stiffness)
            : this(dof, stiffness == 0, 0, stiffness)
        {

        }

        public DofRestrain(GeometryRestrain.DOF dof, bool restrained, double imposedDisplacement, double stiffness)
            : base(Guid.NewGuid(), "")
        {
            _dof = dof;
            _restrained = restrained;
            _imposedDisplacement = imposedDisplacement;
            _stiffness = stiffness < 0 ? throw new ArgumentException($"Stiffness is lower than zero: {stiffness}") : stiffness;
        }

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Dof", _dof, typeof(GeometryRestrain.DOF));
            info.AddValue("Restrained", _restrained);
            info.AddValue("Stiffness", _stiffness);
            info.AddValue("ImposedDisplacement", _imposedDisplacement);
        }

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

        public override bool Equals(object obj)
        {
            if (obj is DofRestrain)
                return Equals(obj);

            return false;
        }

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

        public static bool operator ==(DofRestrain obj1, DofRestrain obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(DofRestrain obj1, DofRestrain obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}