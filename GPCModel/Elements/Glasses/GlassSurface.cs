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

        protected int _index;

        protected Glass _glass;

        protected Shape _shape;

        protected List<Load> _loads;

        protected List<LineRestrain> _lineRestrains;

        protected List<PointRestrain> _pointRestrains;

        #endregion

        #region PROPERTIES

        public int Index => _index;
        
        public Glass Glass => _glass;

        public Shape Shape => _shape;

        public List<Load> Loads => _loads;

        public List<LineRestrain> LineRestrain => _lineRestrains;

        public List<PointRestrain> PointRestrain => _pointRestrains;

        #endregion

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(Glass glass, Shape shape, List<Load> loads, List<LineRestrain> lineRestrain, List<PointRestrain> pointRestrain, int index, Guid guid)
            : base(guid)
        {
            this._glass = glass;
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

        public GlassSurface(Glass glass, Shape shape, int index, Guid guid)
            : this(glass, shape, null, null, null, index, guid)
        {

        }

        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _glass = (Glass)info.GetValue("GlassProperty", typeof(Glass));
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
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
            info.AddValue("Glass", _glass);
            info.AddValue("Shape", _shape);
            info.AddValue("Index", _index);
        } 

        #endregion
    }
}