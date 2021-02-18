using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.FreedomCases;

namespace GPC.Model.FEM.Attributes
{
    public class NodeRestrainAttribute : FreedomCaseAttribute, ISerializable, IEquatable<NodeRestrainAttribute>, INodeFreedomCaseAttribute
    {

        private Dictionary<FEMModel.DOF, double> _restrains;


        public Dictionary<FEMModel.DOF, double> Restrains => _restrains;


        public NodeRestrainAttribute(FreedomCase freedomCase)
           : this(freedomCase, string.Empty, Guid.NewGuid())
        {

        }

        public NodeRestrainAttribute(FreedomCase freedomCase, string name)
            : this(freedomCase, name, Guid.NewGuid())
        {

        }

        public NodeRestrainAttribute(FreedomCase freedomCase, string name, Guid guid)
            : base(freedomCase, name, guid)
        {
            _restrains = new Dictionary<FEMModel.DOF, double>();
        }

        protected NodeRestrainAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _restrains = (Dictionary<FEMModel.DOF, double>)info.GetValue("Restrains", typeof(Dictionary<FEMModel.DOF, double>));
        }

        public void AddRestrain(FEMModel.DOF dof)
        {
            _restrains[dof] = 0;
        }

        public void AddDisplacement(FEMModel.DOF dof, double value)
        {
            _restrains[dof] = value;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Restrains", _restrains);
        }


        public bool Equals(NodeRestrainAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _restrains.SequenceEqual(other._restrains) && base.Equals(other);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as NodeRestrainAttribute);
        }


        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<Dictionary<FEMModel.DOF, double>>.Default.GetHashCode(_restrains);
            return hashCode;
        }
    }
}
