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

        protected List<LineRestrain> _lineRestrains;

        protected List<PointRestrain> _pointRestrains;

        #endregion

        #region PROPERTIES

        public GlassProperty GlassProperty => _glassProperty;

        public Shape Shape => _shape;

        public List<Load> Loads => _loads;

        public List<LineRestrain> LineRestrain => _lineRestrains;

        public List<PointRestrain> PointRestrain => _pointRestrains;

        public int Index => _index;

        #endregion

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(GlassProperty glassProperty, Shape shape, List<Load> loads, List<LineRestrain> lineRestrain, List<PointRestrain> pointRestrain, int index, Guid guid)
            : base(guid)
        {
            this._glassProperty = glassProperty;
            this._shape = shape;
            this._index = index;
            this._loads = new List<Load>();
            if (loads != null)
                _loads.AddRange(loads);

            this._lineRestrains = new List<LineRestrain>();
            if (lineRestrain != null)
                _lineRestrains.AddRange(lineRestrain);

            this._pointRestrains = new List<PointRestrain>();
            if (pointRestrain != null)
                _pointRestrains.AddRange(pointRestrain);
        }

        public GlassSurface(GlassProperty glassProperty, Shape shape, int index, Guid guid)
            : this(glassProperty, shape, null, null, null, index, guid)
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
        
        public void AddLineRestrain(LineRestrain restrain)
        {
            this._lineRestrains.Add(restrain);
        }

        public void AddPointRestrain(PointRestrain restrain)
        {
            this._pointRestrains.Add(restrain);
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