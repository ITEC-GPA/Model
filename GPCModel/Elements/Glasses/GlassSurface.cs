using GPC.Geometry;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GPC.Model.Loads;

namespace GPC.Model.Elements.Glasses
{
    public sealed class GlassSurface : Element, IEquatable<GlassSurface>
    {
        #region VARIABLES

        private Shape _shape;

        #endregion

        #region PROPERTIES
                
        public Shape Shape => _shape;


        #endregion

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(Glass glass, Shape shape, Guid guid)
            : base(guid)
        {
            this._shape = shape;
        }


        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
            throw new NotSupportedException();
        }

        #endregion

        #region Public methods
                
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Shape", _shape);
            throw new NotSupportedException();
        }

        public bool Equals(GlassSurface other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._shape.Equals(_shape)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GlassSurface);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<Shape>.Default.GetHashCode(_shape);
            return hashCode;
        }

        public static bool operator ==(GlassSurface obj1, GlassSurface obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(GlassSurface obj1, GlassSurface obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}