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

        public Shape Shape => _shape;


        #endregion

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(Shape shape)
            : base(Guid.NewGuid())
        {
            _shape = shape;
        }

        public GlassSurface(Shape shape, int id)
            : this(shape, id, Guid.NewGuid())
        {

        }

        public GlassSurface(Shape shape, int id, Guid guid)
            : base(id, guid)
        {
            _shape = shape;
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
            int hashCode = -391 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<Shape>.Default.GetHashCode(_shape);
            return hashCode;
        }

        public static bool operator ==(GlassSurface obj1, GlassSurface obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

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