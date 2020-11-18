using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    public class GlassSurface : Element
    {
        #region VARIABLES

        protected GlassProperty _glassProperty;
        protected Shape _shape;
        protected int _index;

        #endregion VARIABLES

        #region PROPERTIES

        public GlassProperty GlassProperty => _glassProperty;
        public Shape Shape => _shape;

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(GlassProperty glassProperty, Shape shape, int index, Guid guid)
            : base(guid)
        {
            this._glassProperty = glassProperty;
            this._shape = shape;
            this._index = index;
        }

        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _glassProperty = (GlassProperty)info.GetValue("GlassProperty", typeof(GlassProperty));
            _shape = (Shape)info.GetValue("Shape2d", typeof(Shape));
            _index = info.GetInt32("Index");
        }

        #endregion PUBLIC CONSTRUCTORS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassProperty", _glassProperty);
            info.AddValue("Shape2d", _shape);
            info.AddValue("Index", _index);
        }
    }
}