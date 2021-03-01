using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.FreedomCases;
using GPC.Model.Restrains;

namespace GPC.Model.FEM.Attributes
{
    public class NodeRestrainAttribute : FreedomCaseAttribute, ISerializable, IEquatable<NodeRestrainAttribute>, INodeFreedomCaseAttribute
    {
        #region variables

        private List<DofRestrain> _restrains;
        private CoordinateSystem _coordinateSystem;

        #endregion

        #region properties

        public List<DofRestrain> Restrains => _restrains;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="freedomCase"></param>
        /// <param name="coordinateSystem">Coordinate system where the restrains are applied</param>
        /// <param name="restrains"></param>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, string name, Guid guid)
            : base(freedomCase, name, guid)
        {
            _restrains = new List<DofRestrain>();
            if (restrains != null)
                _restrains.AddRange(restrains); //TODO: aggiungere la validazione. Fare in modo che non ci siano Dofrestrain con lo stesso dof dentro _Restrains

            _coordinateSystem = coordinateSystem;
        }

        public NodeRestrainAttribute(FreedomCase freedomCase, CoordinateSystem coordinateSystem)
            : this(freedomCase, coordinateSystem, new List <DofRestrain>(), string.Empty, Guid.NewGuid())
        {

        }

        protected NodeRestrainAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _restrains = (List<DofRestrain>)info.GetValue("Restrains", typeof(List<DofRestrain>));
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
            if (_restrains.Where(i => i.Dof == dof).Count() > 0)
            {
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().SetRestrain(true);
            }
            else
            {
                _restrains.Add(new DofRestrain(dof, true));
            }
        }

        public void AddImposedDisplacement(LinearSolver.DOF dof, double displacement)
        {
            if (_restrains.Where(i => i.Dof == dof).Count() > 0)
            {
                // Facendo cosi sovrascrivo il valore precedente se presente
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().SetImposedDisplacement(displacement);
            }
            else
            {
                var d = new DofRestrain(dof);
                d.SetImposedDisplacement(displacement);
                _restrains.Add(d);
            }
        }


        public override bool Equals(object obj)
        {
            return obj is NodeRestrainAttribute attribute &&
                   base.Equals(obj) &&
                   EqualityComparer<List<DofRestrain>>.Default.Equals(_restrains, attribute._restrains) &&
                   EqualityComparer<CoordinateSystem>.Default.Equals(_coordinateSystem, attribute._coordinateSystem);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<List<DofRestrain>>.Default.GetHashCode(_restrains);
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            return hashCode;
        }

        public bool Equals(NodeRestrainAttribute other)
        {
            return Equals((object)other);
        }
    }
}
