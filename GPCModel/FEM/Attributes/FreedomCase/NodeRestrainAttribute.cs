using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.FreedomCases;

namespace GPC.Model.FEM.Attributes
{
    public class NodeRestrainAttribute : FreedomCaseAttribute, ISerializable, IEquatable<NodeRestrainAttribute>, INodeFreedomCaseAttribute
    {
        #region variables
        private Dictionary<LinearSolver.DOF, double> _restrains;
        private CoordinateSystem _csys;
        #endregion

        #region properties
        public Dictionary<LinearSolver.DOF, double> Restrains => _restrains;
        public CoordinateSystem CSys => _csys;
        #endregion

        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem csys, Dictionary<LinearSolver.DOF,double> values, string name, Guid guid)
            : base(freedomCase, name, guid)
        {
            _restrains = new Dictionary<LinearSolver.DOF, double>();
            _restrains = values;
            _csys = csys;
        }

        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem csys) : this(freedomCase, csys, new Dictionary<LinearSolver.DOF, double>(), string.Empty, Guid.NewGuid())
        {

        }

        protected NodeRestrainAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _restrains = (Dictionary<LinearSolver.DOF, double>)info.GetValue("Restrains", typeof(Dictionary<LinearSolver.DOF, double>));
            _csys = (CoordinateSystem)info.GetValue("CSys", typeof(CoordinateSystem));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Restrains", _restrains);
        }

        public void AddRestrain(LinearSolver.DOF dof)
        {
            _restrains.Add(dof,0);
        }

        public void AddDisplacement(LinearSolver.DOF dof, double value)
        {
            _restrains.Add(dof,value);
        }

        public override bool Equals(object obj)
        {
            return obj is NodeRestrainAttribute attribute &&
                   base.Equals(obj) &&
                   EqualityComparer<Dictionary<LinearSolver.DOF, double>>.Default.Equals(_restrains, attribute._restrains) &&
                   EqualityComparer<CoordinateSystem>.Default.Equals(_csys, attribute._csys);
        }

        public override int GetHashCode()
        {
            int hashCode = 789669813;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<LinearSolver.DOF, double>>.Default.GetHashCode(_restrains);
            hashCode = hashCode * -1521134295 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_csys);
            return hashCode;
        }

        public bool Equals(NodeRestrainAttribute other)
        {
            return Equals((object)other);
        }
    }
}
