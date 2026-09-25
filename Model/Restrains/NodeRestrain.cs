using GPC.Geometry;
using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Restrains
{
    /// <summary>
    /// Restrains of the degrees of freedom of a node
    /// </summary>
    public class NodeRestrain : GeometryRestrain
    {
        #region Variables

        /// <summary>
        /// The restrained node
        /// </summary>
        private NodeElement _point;

        #endregion

        #region Properties

        /// <summary>
        /// The restrained node
        /// </summary>
        public NodeElement Point { get => _point; set => _point = value; }


        #endregion

        #region Constructors

        /// <summary>
        /// Creates the restrain in the global coordinate system
        /// </summary>
        /// <param name="point">The node</param>
        /// <param name="restrains">The restrains of the degrees of freedom</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="point"/> is null</exception>
        public NodeRestrain(NodeElement point, List<DofRestrain> restrains, string name = "")
            : this(point, CoordinateSystem.Global, restrains, name)
        {

        }

        /// <summary>
        /// Creates the restrain
        /// </summary>
        /// <param name="point">The node</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="restrains">The restrains of the degrees of freedom</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="point"/> or <paramref name="coordinateSystem"/> is null</exception>
        public NodeRestrain(NodeElement point, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, string name = "")
            : base(coordinateSystem, restrains, name)
        {
            _point = point ?? throw new ArgumentNullException("Base point can't be null");
        }

        /// <summary>
        /// Creates the restrain without restrained degrees of freedom
        /// </summary>
        /// <param name="point">The node</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="point"/> or <paramref name="coordinateSystem"/> is null</exception>
        public NodeRestrain(NodeElement point, CoordinateSystem coordinateSystem, string name = "")
            : this(point, coordinateSystem, new List<DofRestrain>(), name)
        {
            _point = point ?? throw new ArgumentNullException("Base point can't be null");
        }

        /// <summary>
        /// Deserialization constructor (the base one is not implemented)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        /// <exception cref="NotImplementedException">Always</exception>
        protected NodeRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _point = (NodeElement)info.GetValue("Point", typeof(NodeElement));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// A restrain of a node with all the degrees of freedom restrained
        /// </summary>
        /// <param name="point">The node</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <returns>The new restrain</returns>
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
        /// A restrain of a node with <see cref="GeometryRestrain.DOF.DX"/>, <see cref="GeometryRestrain.DOF.DY"/> and
        /// <see cref="GeometryRestrain.DOF.DZ"/> restrained
        /// </summary>
        /// <param name="point">The node</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <returns>The new restrain</returns>
        public static NodeRestrain GetAllDisplacementFixed(NodeElement point, CoordinateSystem coordinateSystem)
        {
            return new NodeRestrain(point, coordinateSystem, new List<DofRestrain>
            {
                new DofRestrain(DOF.DX),
                new DofRestrain(DOF.DY),
                new DofRestrain(DOF.DZ)
            });
        }

        /// <summary>
        /// The position of the node
        /// </summary>
        /// <returns>The position</returns>
        public override GeometryBase GetGeometry() => _point.Position;
        /// <summary>
        /// The node
        /// </summary>
        /// <returns>The node</returns>
        public override Element GetElement() => _point;

        /// <summary>
        /// Restrains a degree of freedom (an existing restrain loses its stiffness and imposed displacement)
        /// </summary>
        /// <param name="dof">The degree of freedom</param>
        public void AddExternalRestrain(DOF dof)
        {
            if (Restrains.Where(i => i.Dof == dof).Count() > 0)
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().IsRestrained = true;
            else
                _restrains.Add(new DofRestrain(dof));
        }

        /// <summary>
        /// Sets the imposed displacement of a degree of freedom (a new one is added if not present)
        /// </summary>
        /// <param name="dof">The degree of freedom</param>
        /// <param name="displacement">The imposed displacement</param>
        public void AddImposedDisplacement(DOF dof, double displacement)
        {
            if (_restrains.Where(i => i.Dof == dof).Count() > 0)
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().ImposedDisplacement = displacement;
            else
                _restrains.Add(new DofRestrain(dof) { ImposedDisplacement = displacement });
        }

        /// <summary>
        /// Sets the stiffness of a degree of freedom. If the degree of freedom is not present the new restrain gets the value as IMPOSED
        /// DISPLACEMENT, not as stiffness
        /// </summary>
        /// <param name="dof">The degree of freedom</param>
        /// <param name="stiffness">The stiffness</param>
        public void AddStiffness(DOF dof, double stiffness)
        {
            if (_restrains.Where(i => i.Dof == dof).Count() > 0)
                _restrains.Where(i => i.Dof == dof).FirstOrDefault().Stiffness = stiffness;
            else
                _restrains.Add(new DofRestrain(dof) { ImposedDisplacement = stiffness });
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the restrain (the base one is not implemented)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        /// <exception cref="NotImplementedException">Always</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point);
        }

        /// <summary>
        /// Equality of the nodes and of the base (see <see cref="GeometryRestrain.Equals(object)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal restrain</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is NodeRestrain objCasted) && _point.Equals(objCasted.Point) && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the node and of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return -391 + _point.GetHashCode() * -17 + base.GetHashCode();
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first restrain</param>
        /// <param name="obj2">The second restrain</param>
        /// <returns>True if the restrains are equal</returns>
        public static bool operator ==(NodeRestrain obj1, NodeRestrain obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first restrain</param>
        /// <param name="obj2">The second restrain</param>
        /// <returns>True if the restrains are different</returns>
        public static bool operator !=(NodeRestrain obj1, NodeRestrain obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
