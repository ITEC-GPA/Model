using GPC.Geometry;
using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Restrains
{
    public class NodeRestrain : GeometryRestrain
    {
        #region Variables

        private NodeElement _point;

        #endregion

        #region Properties

        public NodeElement Point { get => _point; set => _point = value; }


        #endregion

        #region Constructors

        /// <remarks><see cref="GeometryRestrain.CoordinateSystem"/> set to Global</remarks>
        public NodeRestrain(NodeElement point, List<DofRestrain> restrains, string name = "")
            : this(point, CoordinateSystem.Global, restrains, name)
        {

        }

        public NodeRestrain(NodeElement point, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, string name = "")
            : base(coordinateSystem, restrains, name)
        {
            _point = point ?? throw new ArgumentNullException("Base point can't be null");
        }

        public NodeRestrain(NodeElement point, CoordinateSystem coordinateSystem, string name = "")
            : this(point, coordinateSystem, new List<DofRestrain>(), name)
        {
            _point = point ?? throw new ArgumentNullException("Base point can't be null");
        }

        protected NodeRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _point = (NodeElement)info.GetValue("Point", typeof(NodeElement));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set all the <see cref="Solver.DOF"/> to restrained for the given point and freedomcase
        /// </summary>
        public static NodeRestrain GetAllFixed(NodeElement point, CoordinateSystem coordinateSystem)
        {

            return new NodeRestrain(point, coordinateSystem, new List<DofRestrain>
            {
                new DofRestrain(DOF.DX),
                new DofRestrain(DOF.DY),
                new DofRestrain(DOF.DZ),
                new DofRestrain(DOF.RX),
                new DofRestrain(DOF.RY),
                new DofRestrain(DOF.RZ)
            });
        }

        /// <summary>
        /// Set <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/> and <see cref="Solver.DOF.DZ"/> to restrained for the given line and freedomcase
        /// </summary>
        public static NodeRestrain GetAllDisplacementFixed(NodeElement point, CoordinateSystem coordinateSystem)
        {
            return new NodeRestrain(point, coordinateSystem, new List<DofRestrain>
            {
                new DofRestrain(DOF.DX),
                new DofRestrain(DOF.DY),
                new DofRestrain(DOF.DZ)
            });
        }

        public override GeometryBase GetGeometry() => _point.Position;
        public override Element GetElement() => _point;

        public void AddExternalRestrain(DOF dof)
        {
            if (Restrains.Where(i => i.Dof == dof).Count() > 0)
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().IsRestrained = true;
            else
                _restrains.Add(new DofRestrain(dof));
        }

        public void AddImposedDisplacement(DOF dof, double displacement)
        {
            if (_restrains.Where(i => i.Dof == dof).Count() > 0)
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().ImposedDisplacement = displacement;
            else
                _restrains.Add(new DofRestrain(dof) { ImposedDisplacement = displacement });
        }

        public void AddStiffness(DOF dof, double stiffness)
        {
            if (_restrains.Where(i => i.Dof == dof).Count() > 0)
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().Stiffness = stiffness;
            else
                _restrains.Add(new DofRestrain(dof) { ImposedDisplacement = stiffness });
        }

        #endregion

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is NodeRestrain objCasted) && _point.Equals(objCasted.Point) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return -391 + _point.GetHashCode() * -17 + base.GetHashCode();
            }
        }

        public static bool operator ==(NodeRestrain obj1, NodeRestrain obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(NodeRestrain obj1, NodeRestrain obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
