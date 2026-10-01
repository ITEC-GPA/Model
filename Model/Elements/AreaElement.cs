using GPC.Geometry;
using GPC.Model.ElementProperties;
using System.Runtime.Serialization;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GPC.Model.Elements
{
    /// <summary>
    /// An area element: a planar shape with a plate property
    /// </summary>
    [Serializable]
    public class AreaElement : Element
    {
        public GPC.Model.PostProcessing.ShellAssignments Assignments { get; private set; } = new GPC.Model.PostProcessing.ShellAssignments();
        #region VARIABLES

        /// <summary>
        /// The shape
        /// </summary>
        protected Shape _shape;
        /// <summary>
        /// The plate property
        /// </summary>
        protected PlateProperty _plateProperty;
        private NodeElement[] _nodes;
        public IReadOnlyList<NodeElement> Nodes => Array.AsReadOnly(_nodes ?? new NodeElement[0]);

        public void ConnectNodes(params NodeElement[] nodes)
        {
            if (nodes == null || (nodes.Length != 3 && nodes.Length != 4) || nodes.Any(n => n is null))
                throw new ArgumentException("A linear shell requires three or four explicit nodes.", nameof(nodes));
            _nodes = (NodeElement[])nodes.Clone();
            _shape = null;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The shape of the element
        /// </summary>
        public Shape Shape { get => _nodes == null ? _shape : new Shape(new Polygon3d(_nodes.Select(n => n.Position))); set { if (_nodes != null) throw new InvalidOperationException("Edit connected node positions instead of replacing shell geometry."); _shape = value; } }

        /// <summary>
        /// The external border of the shape (<see cref="Shape.Fill"/>)
        /// </summary>
        public Polygon3d Fill { get => Shape.Fill; }

        /// <summary>
        /// The holes of the shape (<see cref="Shape.Holes"/>; null if it has none)
        /// </summary>
        public Polygon3d[] Holes { get => Shape.Holes; }

        /// <summary>
        /// The vertices of the shape (fill, holes and children, see <see cref="Shape.GetPoints"/>)
        /// </summary>
        public Point3d[] Points { get => _nodes == null ? (_shape?.GetPoints() ?? new Point3d[0]) : _nodes.Select(n => n.Position).ToArray(); }

        /// <summary>
        /// The plate property
        /// </summary>
        public PlateProperty PlateProperty { get => _plateProperty; set => _plateProperty = value; }

        #endregion

        #region PUBLIC CONSTRUCTORS

        /// <summary>
        /// Creates an area element (the shape instance is kept)
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <param name="plateProperty">The plate property</param>
        /// <param name="coordinateSystem">The local coordinate system; null: the global one</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public AreaElement(Shape shape, PlateProperty plateProperty, CoordinateSystem coordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, coordinateSystem)
        {
            _shape = shape;
            _plateProperty = plateProperty;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Element"/>, the shape and the plate property
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected AreaElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Assignments = SerializationFields.Read(info, "AreaAssignments", new GPC.Model.PostProcessing.ShellAssignments());
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
            _plateProperty = (PlateProperty)info.GetValue("PlateProperty", typeof(PlateProperty));
            _nodes = SerializationFields.Read<NodeElement[]>(info, "ConnectedNodes");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Serializes the data of <see cref="Element"/>, the shape and the plate property
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AreaAssignments", Assignments);
            info.AddValue("Shape", _shape);
            info.AddValue("PlateProperty", _plateProperty);
            info.AddValue("ConnectedNodes", _nodes);
        }

        /// <summary>
        /// Equality with another area element
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True only if <paramref name="obj"/> is the same instance: the comparison of the points with the shape (<c>Points.Equals(_shape)</c>) is always false</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is AreaElement && base.Equals(obj);
        }

        /// <summary>
        /// The hash code of the name, of the coordinate system, of the shape and of the plate property
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null elements are equal
        /// </summary>
        /// <param name="obj1">The first element</param>
        /// <param name="obj2">The second element</param>
        /// <returns>True if the elements are equal</returns>
        public static bool operator ==(AreaElement obj1, AreaElement obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first element</param>
        /// <param name="obj2">The second element</param>
        /// <returns>True if the elements are different</returns>
        public static bool operator !=(AreaElement obj1, AreaElement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
