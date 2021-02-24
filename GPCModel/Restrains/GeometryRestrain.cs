using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FreedomCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Restrains
{
    public abstract class GeometryRestrain : ModelObject
    {

        private FreedomCase _freedomCases;

        private CoordinateSystem _coordinateSystem;

        private List<DofRestrain> _restrains;

        public FreedomCase FreedomCase => _freedomCases;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;
        public List<DofRestrain> Restrains => _restrains;

        public GeometryRestrain(FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, Guid guid, string name) 
            : base(guid, name)
        {
            this._coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
            this._freedomCases = freedomCase ?? throw new ArgumentNullException(nameof(freedomCase));
            this._restrains = restrains ?? new List<DofRestrain>();
        }

        public GeometryRestrain(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            _coordinateSystem = coordinateSystem;
        }

        public void AddRestain(DofRestrain dofRestrain)
        {
            _restrains.Add(dofRestrain);
        }

        public Vector3d GetV1() => _coordinateSystem.V11;

        public Vector3d GetV2() => _coordinateSystem.V22;

        public Vector3d GetV3() => _coordinateSystem.V33;

        public Point3d GetCoordinateSystemOrigin() => _coordinateSystem.Origin;

        public KeyValuePair<LinearSolver.DOF, bool>[] GetRestrains()
        {
            KeyValuePair<LinearSolver.DOF, bool>[] kvp = new KeyValuePair<LinearSolver.DOF, bool>[Restrains.Count];

            for (int i = 0; i < _restrains.Count; i++)
            {
                kvp[i] = new KeyValuePair<LinearSolver.DOF, bool>(key: _restrains[i].Dof, value: _restrains[i].Restrained);
            }

            return kvp;
        }

        public KeyValuePair<LinearSolver.DOF, double>[] GetStiffnesses()
        {
            KeyValuePair<LinearSolver.DOF, double>[] kvp = new KeyValuePair<LinearSolver.DOF, double>[Restrains.Count];

            for (int i = 0; i < _restrains.Count; i++)
            {
                kvp[i] = new KeyValuePair<LinearSolver.DOF, double>(key: _restrains[i].Dof, value: _restrains[i].Stiffness);
            }

            return kvp;
        }

        public abstract GeometryBase GetGeometry();



        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is GeometryRestrain))
                return false;

            GeometryRestrain objCasted = obj as GeometryRestrain;

            return (objCasted != null) && _freedomCases.Equals(objCasted._freedomCases) && 
                                            _coordinateSystem.Equals(objCasted._coordinateSystem) &&
                                            _restrains.SequenceEqual(objCasted._restrains) &&
                                            base.Equals(obj);
        }


        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + EqualityComparer<FreedomCase>.Default.GetHashCode(_freedomCases);
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            hashCode = hashCode * -17 + EqualityComparer<List<DofRestrain>>.Default.GetHashCode(_restrains);
            return hashCode;
        }
    }
}
