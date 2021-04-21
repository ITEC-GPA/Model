using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.FEM;

namespace GPC.Model.Restrains
{
    /// <summary>
    /// External restrain or stiffness applied to a Dof
    /// </summary>
    [Serializable]
    public class DofRestrain : ModelObject
    {
        #region Variables

        private Solver.DOF _dof;
        private bool _restrained;
        private double _imposedDisplacement;
        private double _stiffness;

        #endregion

        #region Properties

        public Solver.DOF Dof => _dof;

        public bool Restrained => _restrained;

        public double Stiffness => _stiffness;

        public double ImposedDisplacement => _imposedDisplacement;

        #endregion

        #region Constructors

        public DofRestrain(Solver.DOF dof)
            : this(dof, false, 0, 0, Guid.NewGuid(), string.Empty)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dof"></param>
        /// <param name="stiffness"></param>
        /// <remarks>If <paramref name="stiffness"/> is different than 0 then <see cref="Restrained"/> is true</remarks>
        public DofRestrain(Solver.DOF dof, double stiffness)
            : this(dof, stiffness != 0 ? true : false, 0, stiffness, Guid.NewGuid(), string.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dof"></param>
        /// <param name="restrained"></param>
        public DofRestrain(Solver.DOF dof, bool restrained)
            : this(dof, restrained, 0, 0, Guid.NewGuid(), string.Empty)
        {
        }


        private DofRestrain(Solver.DOF dof, bool restrained, double imposedDisplacement, double stiffness, Guid guid, string name) 
            : base(guid, name)
        {
            _dof = dof; 
            _restrained = restrained;
            _imposedDisplacement = imposedDisplacement;
            _stiffness = stiffness;
        }

        public DofRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dof = (Solver.DOF)info.GetValue("Dof", typeof(Solver.DOF));
            _restrained = info.GetBoolean("Restrained");
            _stiffness = info.GetDouble("Stiffness");
            _imposedDisplacement = info.GetDouble("ImposedDisplacement");
        }

        #endregion

        #region Public methods

        public void SetImposedDisplacement(double value)
        {
            _imposedDisplacement = value;
        }

        public void SetRestrain(bool condition)
        {
            _restrained = condition;
        }

        public void SetStiffness(double value)
        {
            _stiffness = value;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Dof", _dof, typeof(Solver.DOF));
            info.AddValue("Restrained", _restrained);
            info.AddValue("Stiffness", _stiffness);
            info.AddValue("ImposedDisplacement", _imposedDisplacement);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Tells if a DofRestrain is equal to another
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is null || !Equals(obj) || !(obj is DofRestrain dr))
                return false;
            else
                return _dof == dr._dof && _restrained == dr._restrained && _stiffness == dr._stiffness && _imposedDisplacement == dr._imposedDisplacement;
        }

        /// <summary>
        /// Calculate the hash code
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = base.GetHashCode();
            hashCode = hashCode * 19 + EqualityComparer<Solver.DOF>.Default.GetHashCode(_dof);
            hashCode = hashCode * 19 + EqualityComparer<bool>.Default.GetHashCode(_restrained);
            hashCode = hashCode * 19 + EqualityComparer<double>.Default.GetHashCode(_stiffness);
            hashCode = hashCode * 19 + EqualityComparer<double>.Default.GetHashCode(_imposedDisplacement);

            return hashCode;
        }

        #endregion
    }
}
