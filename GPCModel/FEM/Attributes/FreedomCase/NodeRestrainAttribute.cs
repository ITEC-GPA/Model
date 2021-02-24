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
        private CoordinateSystem _coordinateSystem;

        #endregion

        #region properties

        public Dictionary<LinearSolver.DOF, double> Restrains => _restrains;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        #endregion
		
        /// <summary>
        /// WARNING: da modificare da Dictionary<LinearSolver.DOF,double> a Dictionary<LinearSolver.LocalDOF,double>
        /// il vincolo/spostamento imposto deve essere definito in un sistema locale definito da csys
        /// </summary>
        /// <param name="freedomCase"></param>
        /// <param name="csys"></param>
        /// <param name="values"></param>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem coordinateSystem, Dictionary<LinearSolver.DOF,double> values, string name, Guid guid)
            : base(freedomCase, name, guid)
        {
            _restrains = new Dictionary<LinearSolver.DOF, double>();
            _restrains = values;
            _coordinateSystem = coordinateSystem;
        }

        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem coordinateSystem)
            : this(freedomCase, coordinateSystem, new Dictionary<LinearSolver.DOF, double>(), string.Empty, Guid.NewGuid())
        {

        }

        protected NodeRestrainAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _restrains = (Dictionary<LinearSolver.DOF, double>)info.GetValue("Restrains", typeof(Dictionary<LinearSolver.DOF, double>));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Restrains", _restrains);
            throw new NotImplementedException();
        }

        public void AddExternalRestrain(LinearSolver.DOF dof)
        {
            _restrains[dof] = 0;
        }

        public void AddImposedDisplacement(LinearSolver.DOF dof, double displacement)
        {
            _restrains[dof] = displacement; // Facendo cosi sovrascrivo il valore precedente se presente, es è vincolato e lo rimpiazzo con spostamento imposto
        }

        public override bool Equals(object obj)
        {
            return obj is NodeRestrainAttribute attribute &&
                   base.Equals(obj) &&
                   EqualityComparer<Dictionary<LinearSolver.DOF, double>>.Default.Equals(_restrains, attribute._restrains) &&
                   EqualityComparer<CoordinateSystem>.Default.Equals(_coordinateSystem, attribute._coordinateSystem);
        }

        public override int GetHashCode()
        {
            int hashCode = 789669813;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<LinearSolver.DOF, double>>.Default.GetHashCode(_restrains);
            hashCode = hashCode * -1521134295 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            return hashCode;
        }

        public bool Equals(NodeRestrainAttribute other)
        {
            return Equals((object)other);
        }
    }
}
