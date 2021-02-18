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
    public class NodeRestrainAttribute : FreedomCaseAttribute, ISerializable, IEquatable<NodeRestrainAttribute>, INodeFreedomCaseAttribute
    {
        #region variables
        private Dictionary<FEMModel.DOF, double> _restrains;
        private CoordinateSystem _csys;
        #endregion

        #region properties
        public Dictionary<FEMModel.DOF, double> Restrains => _restrains;
        public CoordinateSystem CSys => _csys;
        #endregion

        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem csys, Dictionary<FEMModel.DOF,double> values, string name, Guid guid)
            : base(freedomCase, name, guid)
        {
            _restrains = new Dictionary<FEMModel.DOF, double>();
            _restrains = values;
            _csys = csys;
        }

        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem csys) : this(freedomCase, csys, new Dictionary<FEMModel.DOF, double>(), string.Empty, Guid.NewGuid())
        {

        }

        protected NodeRestrainAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _restrains = (Dictionary<FEMModel.DOF, double>)info.GetValue("Restrains", typeof(Dictionary<FEMModel.DOF, double>));
            _csys = (CoordinateSystem)info.GetValue("CSys", typeof(CoordinateSystem));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Restrains", _restrains);
        }

        public void AddRestrain(FEMModel.DOF dof)
        {
            _restrains.Add(dof,0);
        }

        public void AddDisplacement(FEMModel.DOF dof, double value)
        {
            _restrains.Add(dof,value);
        }

        public override bool Equals(object obj)
        {
            return obj is NodeRestrainAttribute attribute &&
                   base.Equals(obj) &&
                   EqualityComparer<Dictionary<FEMModel.DOF, double>>.Default.Equals(_restrains, attribute._restrains) &&
                   EqualityComparer<CoordinateSystem>.Default.Equals(_csys, attribute._csys);
        }

        public override int GetHashCode()
        {
            int hashCode = 789669813;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<FEMModel.DOF, double>>.Default.GetHashCode(_restrains);
            hashCode = hashCode * -1521134295 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_csys);
            return hashCode;
        }

        public bool Equals(NodeRestrainAttribute other)
        {
            return Equals((object)other);
        }
    }
}
