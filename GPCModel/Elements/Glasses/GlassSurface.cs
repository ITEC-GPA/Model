using GPC.Geometry;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GPC.Model.Loads;

namespace GPC.Model.Elements.Glasses
{
    public class GlassSurface : Element
    {
        #region VARIABLES

        protected GlassProperty _glassProperty;

        protected Shape _shape;

        protected int _index;

        protected List<Load> _loads;

        #endregion

        #region PROPERTIES

        public GlassProperty GlassProperty => _glassProperty;

        public Shape Shape => _shape;

        public List<Load> Loads => _loads;

        #endregion

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(GlassProperty glassProperty, Shape shape, List<Load> loads, int index, Guid guid)
            : base(guid)
        {
            this._glassProperty = glassProperty;
            this._shape = shape;
            this._index = index;
            this._loads = new List<Load>();
            if (loads != null)
                _loads.AddRange(loads);
        }

        public GlassSurface(GlassProperty glassProperty, Shape shape, int index, Guid guid)
            : this(glassProperty, shape, null, index, guid)
        {

        }

        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _glassProperty = (GlassProperty)info.GetValue("GlassProperty", typeof(GlassProperty));
            _shape = (Shape)info.GetValue("Shape2d", typeof(Shape));
            _index = info.GetInt32("Index");
        }

        #endregion

        #region Public methods

        public void AddLoad(Load load)
        {
            this._loads.Add(load);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassProperty", _glassProperty);
            info.AddValue("Shape2d", _shape);
            info.AddValue("Index", _index);
        } 

        #endregion
    }
}