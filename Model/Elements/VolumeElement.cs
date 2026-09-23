using GPC.Geometry;
using GPC.Model.ElementProperties;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    public class VolumeElement : Element
    {
        #region VARIABLES

        protected NodeElement[] _nodes;
        protected BrickProperty _plateProperty;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The shape of the element
        /// </summary>
        public NodeElement[] Nodes { get => _nodes; set => _nodes = value; }

        public BrickProperty PlateProperty { get => _plateProperty; set => _plateProperty = value; }

        #endregion

        #region PUBLIC CONSTRUCTORS

        /// <param name="shape">The shape of the glass</param>
        /// <param name="id"></param>
        public VolumeElement(NodeElement[] nodeElements, BrickProperty plateProperty, CoordinateSystem coordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, coordinateSystem)
        {
            _nodes = nodeElements;

            _plateProperty = plateProperty;
        }

        protected VolumeElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _nodes = (NodeElement[])info.GetValue("NodeElements", typeof(NodeElement[]));
            _plateProperty = (BrickProperty)info.GetValue("BrickProperty", typeof(BrickProperty));
        }

        #endregion

        #region Public methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("NodeElements", _nodes);
            info.AddValue("BrickProperty", _plateProperty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is AreaElement surface) &&
                surface.Points.Equals(_nodes) &&
                surface.PlateProperty.Equals(_plateProperty) &&
                base.Equals(surface);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -391 + base.GetHashCode();
                hashCode = hashCode * -17 + _nodes.GetHashCode();
                hashCode = hashCode * -17 + _plateProperty.GetHashCode();
                return hashCode;
            }
        }

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

        public static bool operator !=(VolumeElement obj1, VolumeElement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}