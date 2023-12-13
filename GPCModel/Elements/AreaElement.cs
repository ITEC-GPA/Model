using GPC.Geometry;
using GPC.Model.ElementProperties;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    public class AreaElement : Element
    {
        #region VARIABLES

        protected Shape _shape;
        protected PlateProperty _plateProperty;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The shape of the element
        /// </summary>
        public Shape Shape { get => _shape; set => _shape = value; }

        /// <summary>
        /// The external border of the shape
        /// </summary>
        public Polygon3d Fill { get => _shape.Fill; }

        /// <summary>
        /// The list of the internal hole of the shape
        /// </summary>
        public Polygon3d[] Holes { get => _shape.Holes; }

        /// <summary>
        /// The list of the point of the shape
        /// </summary>
        public Point3d[] Points { get => _shape.GetPoints(); }

        public PlateProperty PlateProperty { get => _plateProperty; set => _plateProperty = value; }

        #endregion

        #region PUBLIC CONSTRUCTORS

        /// <param name="shape">The shape of the glass</param>
        /// <param name="id"></param>
        public AreaElement(Shape shape, PlateProperty plateProperty, CoordinateSystem coordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, coordinateSystem)
        {
            _shape = shape;
            _plateProperty = plateProperty;
        }

        protected AreaElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
            _plateProperty = (PlateProperty)info.GetValue("PlateProperty", typeof(PlateProperty));
        }

        #endregion

        #region Public methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Shape", _shape);
            info.AddValue("PlateProperty", _plateProperty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is AreaElement surface) &&
                surface.Points.Equals(_shape) &&
                surface.PlateProperty.Equals(_plateProperty) &&
                base.Equals(surface);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -391 + base.GetHashCode();
                hashCode = hashCode * -17 + _shape.GetHashCode();
                hashCode = hashCode * -17 + _plateProperty.GetHashCode();
                return hashCode;
            }
        }

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

        public static bool operator !=(AreaElement obj1, AreaElement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}