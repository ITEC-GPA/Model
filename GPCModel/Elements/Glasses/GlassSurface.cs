using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    public class GlassSurface : Element
    {
        #region VARIABLES

        protected Glass _glass;
        protected Shape2d _shape;

        #endregion VARIABLES

        #region PROPERTIES

        public Glass Glass => _glass;
        public Shape2d Shape => _shape;

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(Glass glass, Shape2d shape, Guid guid)
            : base(guid)
        {
            if (shape.HasHoles())
                throw new ArgumentException("Shape cannot have holes");

            this._glass = glass;
            this._shape = shape;
        }

        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _glass = (Glass)info.GetValue("Glass", typeof(Glass));
            _shape = (Shape2d)info.GetValue("Shape2d", typeof(Shape2d));
        }

        #endregion PUBLIC CONSTRUCTORS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Glass", _glass);
            info.AddValue("Shape2d", _shape);
        }
    }
}