using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    public class GlassSurface : Element
    {
        #region VARIABLES

        private readonly Shape _shape;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The shape of the glass
        /// </summary>
        public Shape Shape => _shape;


        #endregion

        #region PUBLIC CONSTRUCTORS

        /// <param name="shape">The shape of the glass</param>
        public GlassSurface(Shape shape)
            : this(shape, ModelObjectId.IDUNASSIGNED, Guid.NewGuid())
        {

        }

        /// <param name="shape">The shape of the glass</param>
        /// <param name="id"></param>
        public GlassSurface(Shape shape, int id)
            : this(shape, id, Guid.NewGuid())
        {

        }

        /// <param name="shape">The shape of the glass</param>
        /// <param name="id"></param>
        /// <param name="guid"></param>
        public GlassSurface(Shape shape, int id, Guid guid)
            : base(id, guid)
        {
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }


        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
        }

        #endregion

        #region Public methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Shape", _shape);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is GlassSurface surface) && surface._shape.Equals(_shape) && base.Equals(surface);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -391 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Shape>.Default.GetHashCode(_shape);
                return hashCode; 
            }
        }

        public static bool operator ==(GlassSurface obj1, GlassSurface obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(GlassSurface obj1, GlassSurface obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}