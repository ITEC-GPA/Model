using GPC.Geometry;
using GPC.Model.ElementProperties;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// A volume (brick) element: its nodes and a brick property
    /// </summary>
    [System.Serializable]
    public class VolumeElement : Element
    {
        #region VARIABLES

        /// <summary>
        /// The nodes
        /// </summary>
        protected NodeElement[] _nodes;
        /// <summary>
        /// The brick property
        /// </summary>
        protected BrickProperty _plateProperty;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The nodes of the element (the array of the element)
        /// </summary>
        public NodeElement[] Nodes { get => _nodes; set => _nodes = value; }

        /// <summary>
        /// The brick property
        /// </summary>
        public BrickProperty PlateProperty { get => _plateProperty; set => _plateProperty = value; }

        #endregion

        #region PUBLIC CONSTRUCTORS

        /// <summary>
        /// Creates a volume element (the array of the nodes is kept)
        /// </summary>
        /// <param name="nodeElements">The nodes</param>
        /// <param name="plateProperty">The brick property</param>
        /// <param name="coordinateSystem">The local coordinate system; null: the global one</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public VolumeElement(NodeElement[] nodeElements, BrickProperty plateProperty, CoordinateSystem coordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, coordinateSystem)
        {
            _nodes = nodeElements;

            _plateProperty = plateProperty;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Element"/>, the nodes and the brick property
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected VolumeElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _nodes = (NodeElement[])info.GetValue("NodeElements", typeof(NodeElement[]));
            _plateProperty = (BrickProperty)info.GetValue("BrickProperty", typeof(BrickProperty));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Serializes the data of <see cref="Element"/>, the nodes and the brick property
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("NodeElements", _nodes);
            info.AddValue("BrickProperty", _plateProperty);
        }

        /// <summary>
        /// Equality with another volume element
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True only if <paramref name="obj"/> is the same instance: the method checks if <paramref name="obj"/> is an <see cref="AreaElement"/></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is VolumeElement && base.Equals(obj);
        }

        /// <summary>
        /// The hash code of the name, of the coordinate system, of the array of the nodes (as instance) and of the property
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
        public static bool operator ==(VolumeElement obj1, VolumeElement obj2)
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
        public static bool operator !=(VolumeElement obj1, VolumeElement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
