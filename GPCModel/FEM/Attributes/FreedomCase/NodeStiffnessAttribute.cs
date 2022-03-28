using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.FreedomCases;
using GPC.Model.Restrains;

namespace GPC.Model.FEM.Attributes
{
    public class NodeStiffnessAttribute : FreedomCaseAttribute, ISerializable, IEquatable<NodeStiffnessAttribute>, INodeFreedomCaseAttribute
    {

        private List<DofRestrain> _stiffness;
        private CoordinateSystem _coordinateSystem;


        public List<DofRestrain> Stiffnesses => _stiffness;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        public NodeStiffnessAttribute(string freedomCaseName, CoordinateSystem coordinateSystem)
           : this(freedomCaseName, coordinateSystem, string.Empty, Guid.NewGuid())
        {

        }

        public NodeStiffnessAttribute(string freedomCaseName, CoordinateSystem coordinateSystem, string name)
            : this(freedomCaseName, coordinateSystem, name, Guid.NewGuid())
        {

        }

        public NodeStiffnessAttribute(string freedomCaseName, CoordinateSystem coordinateSystem, string name, Guid guid)
            : base(freedomCaseName, name, guid)
        {
            _stiffness = new List<DofRestrain>();
            _coordinateSystem = coordinateSystem;
        }

        public NodeStiffnessAttribute(NodeStiffnessAttribute nodeStiffnessAttribute)
            : base(nodeStiffnessAttribute.FreedomCaseName, nodeStiffnessAttribute.Name, nodeStiffnessAttribute.Guid)
        {
            _stiffness = nodeStiffnessAttribute._stiffness;
            _coordinateSystem = nodeStiffnessAttribute.CoordinateSystem;
        }


        protected NodeStiffnessAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _stiffness = (List<DofRestrain>)info.GetValue("stiffnesses", typeof(List<DofRestrain>));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        public void AddStiffness(LinearSolver.DOF dof, double stiffness)
        {
            if (_stiffness.Where(i => i.Dof == dof).Count() > 0)
            {
                // Facendo cosi sovrascrivo il valore precedente se presente
                _stiffness.Where(i => i.Dof == dof).FirstOrDefault().SetStiffness(stiffness);
            }
            else
            {
                _stiffness.Add(new DofRestrain(dof, stiffness));
            }
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
            hashCode = hashCode * -17 + EqualityComparer<List<DofRestrain>>.Default.GetHashCode(_stiffness);
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            return hashCode;
        }

        public override object Clone()
        {
            return new NodeStiffnessAttribute(this);
        }
    }
}
