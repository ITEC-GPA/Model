using GPC.Geometry;
using GPC.Model.Fem;
using GPC.Model.FreedomCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Restrains
{
    public abstract class GeometryRestrain : ModelObject
    {
        #region Variables

        private FreedomCase _freedomCases;
        private CoordinateSystem _coordinateSystem;
        private List<DofRestrain> _restrains;

        #endregion

        #region Properties

        public FreedomCase FreedomCase { get => _freedomCases; set => _freedomCases = value; }

        /// <summary>
        /// The coordinate system of the restrain
        /// </summary>
        public CoordinateSystem CoordinateSystem { get => _coordinateSystem; set => _coordinateSystem = value; }

        /// <summary>
        /// List of restrain, one for each direction
        /// </summary>
        public List<DofRestrain> Restrains { get => _restrains; set => _restrains = value; }

        #endregion

        #region Constructor

        public GeometryRestrain(FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, string name)
            : base(name)
        {
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
            _freedomCases = freedomCase ?? throw new ArgumentNullException(nameof(freedomCase));
            _restrains = restrains ?? new List<DofRestrain>();
        }

        protected GeometryRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            _coordinateSystem = coordinateSystem;
        }

        public void AddRestain(DofRestrain dofRestrain)
        {
            _restrains.Add(dofRestrain);
        }

        public Vector3d GetV1() => _coordinateSystem.V1;

        public Vector3d GetV2() => _coordinateSystem.V2;

        public Vector3d GetV3() => _coordinateSystem.V3;

        public Point3d GetCoordinateSystemOrigin() => _coordinateSystem.Origin;

        public abstract GeometryBase GetGeometry();

        /// <returns>Dictionary of each restrained DOF where <see cref="DofRestrain.IsRestrained"/> is <see langword="true"/></returns>
        public Dictionary<Solver.DOF, bool> GetRestrains()
        {
            Dictionary<Solver.DOF, bool> kvp = new Dictionary<Solver.DOF, bool>();

            for (int i = 0; i < _restrains.Count; i++)
            {
                if (_restrains[i].IsRestrained)
                {
                    if (kvp.ContainsKey(_restrains[i].Dof))
                    {
                        kvp[_restrains[i].Dof] = kvp[_restrains[i].Dof] | kvp[_restrains[i].Dof]; // in teoria ritorna questo: V V = V, V F = V, F F = F. Cioè se è vincolato almeno una volta resta vincolato
                    }
                    else
                    {
                        kvp[_restrains[i].Dof] = _restrains[i].IsRestrained;
                    }
                }
            }

            return kvp;
        }

        /// <returns>Dictionary of each restrained DOF where <see cref="DofRestrain.Stiffness"/> is != 0</returns>
        public Dictionary<Solver.DOF, double> GetStiffnesses()
        {
            Dictionary<Solver.DOF, double> kvp = new Dictionary<Solver.DOF, double>();

            for (int i = 0; i < _restrains.Count; i++)
            {
                if (_restrains[i].Stiffness != 0)
                {
                    kvp[_restrains[i].Dof] += _restrains[i].Stiffness;
                }
            }

            return kvp;
        }

        /// <returns>Dictionary of each restrained DOF where <see cref="DofRestrain.ImposedDisplacement"/> is != 0</returns>
        public Dictionary<Solver.DOF, double> GetImposedDisplacement()
        {
            Dictionary<Solver.DOF, double> kvp = new Dictionary<Solver.DOF, double>();

            for (int i = 0; i < _restrains.Count; i++)
            {
                if (_restrains[i].ImposedDisplacement != 0)
                {
                    kvp[_restrains[i].Dof] += _restrains[i].ImposedDisplacement;
                }
            }

            return kvp;
        }

        /// <summary>
        /// Set all the <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/>, <see cref="Solver.DOF.DZ"/> and <see cref="Solver.DOF.RX"/>, <see cref="Solver.DOF.RY"/> and <see cref="Solver.DOF.RZ"/> restrained for the given line and freedomcase
        /// </summary>
        public void FixAll()
        {
            Restrains = new List<DofRestrain>() {
                new DofRestrain(Solver.DOF.DX), new DofRestrain(Solver.DOF.DY), new DofRestrain(Solver.DOF.DZ),
                new DofRestrain(Solver.DOF.RX), new DofRestrain(Solver.DOF.RY), new DofRestrain(Solver.DOF.RZ) };
        }

        /// <summary>
        /// Set <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/> and <see cref="Solver.DOF.DZ"/> restrained for the given line and freedomcase
        /// </summary>
        public void FixDisplacement()
        {
            Restrains = new List<DofRestrain>() {
                new DofRestrain(Solver.DOF.DX), new DofRestrain(Solver.DOF.DY), new DofRestrain(Solver.DOF.DZ),
                new DofRestrain(Solver.DOF.RX, false), new DofRestrain(Solver.DOF.RY, false), new DofRestrain(Solver.DOF.RZ, false) };
        }

        /// <summary>
        /// Set all the <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/>, <see cref="Solver.DOF.DZ"/> and <see cref="Solver.DOF.RX"/>, <see cref="Solver.DOF.RY"/> and <see cref="Solver.DOF.RZ"/> release for the given line and freedomcase
        /// </summary>
        public void ReleaseAll()
        {
            Restrains = new List<DofRestrain>() {
                new DofRestrain(Solver.DOF.DX, false), new DofRestrain(Solver.DOF.DY, false), new DofRestrain(Solver.DOF.DZ, false),
                new DofRestrain(Solver.DOF.RX, false), new DofRestrain(Solver.DOF.RY, false), new DofRestrain(Solver.DOF.RZ, false) };
        }

        #endregion

        #region Equals, HasCode and operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is GeometryRestrain objCasted) && _freedomCases.Equals(objCasted._freedomCases) &&
                                                          _coordinateSystem.Equals(objCasted._coordinateSystem) &&
                                                          _restrains.SequenceEqual(objCasted._restrains) &&
                                                          base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + EqualityComparer<FreedomCase>.Default.GetHashCode(_freedomCases);
                hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
                hashCode = hashCode * -17 + EqualityComparer<List<DofRestrain>>.Default.GetHashCode(_restrains);
                return hashCode;
            }
        }

        #endregion
    }
}
