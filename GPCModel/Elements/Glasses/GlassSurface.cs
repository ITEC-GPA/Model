using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    public class GlassSurface : Element
    {
        #region VARIABLES

        protected GlassProperty _glass;
        protected Shape2d _shape;
        protected int _index;

        #endregion VARIABLES

        #region PROPERTIES

        public GlassProperty Glass => _glass;
        public Shape2d Shape => _shape;

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(GlassProperty glass, Shape2d shape, int index, Guid guid)
            : base(guid)
        {
            if (shape.HasHoles())
                throw new ArgumentException("Shape cannot have holes");

            this._glass = glass;
            this._shape = shape;
            this._index = index;
        }

        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _glass = (GlassProperty)info.GetValue("Glass", typeof(GlassProperty));
            _shape = (Shape2d)info.GetValue("Shape2d", typeof(Shape2d));
            _index = info.GetInt32("Index");
        }

        #endregion PUBLIC CONSTRUCTORS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Glass", _glass);
            info.AddValue("Shape2d", _shape);
            info.AddValue("Index", _index);
        }
    }
}