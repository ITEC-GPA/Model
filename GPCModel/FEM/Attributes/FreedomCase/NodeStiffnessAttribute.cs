using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.FreedomCases;

namespace GPC.Model.FEM.Attributes
{
    public class NodeStiffnessAttribute : FreedomCaseAttribute, ISerializable, IEquatable<NodeStiffnessAttribute>, INodeFreedomCaseAttribute
    {

        private Dictionary<LinearSolver.DOF, double> _stiffness;
        private CoordinateSystem _coordinateSystem;


        public Dictionary<LinearSolver.DOF, double> Stiffnesses => _stiffness;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        public NodeStiffnessAttribute(FreedomCase freedomCase, CoordinateSystem coordinateSystem)
           : this(freedomCase, coordinateSystem, string.Empty, Guid.NewGuid())
        {

        }

        public NodeStiffnessAttribute(FreedomCase freedomCase, CoordinateSystem coordinateSystem, string name) 
            : this(freedomCase, coordinateSystem, name, Guid.NewGuid())
        {

        }

        public NodeStiffnessAttribute(FreedomCase freedomCase, CoordinateSystem coordinateSystem, string name, Guid guid) 
            : base(freedomCase, name, guid)
        {
            _stiffness = new Dictionary<LinearSolver.DOF, double>();
            _coordinateSystem = coordinateSystem;
        }

        protected NodeStiffnessAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _stiffness = (Dictionary<LinearSolver.DOF, double>)info.GetValue("stiffnesses", typeof(Dictionary<LinearSolver.DOF, double>));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        public void AddStiffness(LinearSolver.DOF dof, double value)
        {
            _stiffness[dof] = value;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("stiffnesses", _stiffness);
            info.AddValue("CoordinateSystem", _coordinateSystem);
            throw new NotImplementedException();
        }


        public bool Equals(NodeStiffnessAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _stiffness.SequenceEqual(other._stiffness) && _coordinateSystem.Equals(other._coordinateSystem) && base.Equals(other);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as NodeStiffnessAttribute);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<Dictionary<LinearSolver.DOF, double>>.Default.GetHashCode(_stiffness);
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            return hashCode;
        }
    }
}
