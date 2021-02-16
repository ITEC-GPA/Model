using GPC.Geometry;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GPC.Model.Loads;

namespace GPC.Model.Elements.Glasses
{
    public class GlassSurface : Element, IEquatable<GlassSurface>
    {
        #region VARIABLES

        protected Glass _glass;

        protected Shape _shape;

        protected List<Load> _loads;

        protected List<LineRestrain> _lineRestrains;

        protected List<PointRestrain> _pointRestrains;

        #endregion

        #region PROPERTIES
                
        public Glass Glass => _glass;

        public Shape Shape => _shape;

        public List<Load> Loads => _loads;

        public List<LineRestrain> LineRestrain => _lineRestrains;

        public List<PointRestrain> PointRestrain => _pointRestrains;

        #endregion

        #region PUBLIC CONSTRUCTORS

        public GlassSurface(Glass glass, Shape shape, List<Load> loads, List<LineRestrain> lineRestrain, List<PointRestrain> pointRestrain, Guid guid)
            : base(guid)
        {
            this._glass = glass;
            this._shape = shape;
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

        public GlassSurface(Glass glass, Shape shape, Guid guid)
            : this(glass, shape, null, null, null, guid)
        {

        }

        public GlassSurface(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _glass = (Glass)info.GetValue("GlassProperty", typeof(Glass));
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
            throw new NotSupportedException();
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
            throw new NotSupportedException();
        }

        public bool Equals(GlassSurface other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._glass.Equals(_glass)
                                    && other._lineRestrains.Equals(_lineRestrains)
                                    && other._loads.Equals(_loads)
                                    && other._shape.Equals(_shape)
                                    && other._pointRestrains.Equals(_pointRestrains)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as GlassSurface);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<Glass>.Default.GetHashCode(_glass);
            hashCode = hashCode * -17 + EqualityComparer<Shape>.Default.GetHashCode(_shape);
            hashCode = hashCode * -17 + EqualityComparer<List<Load>>.Default.GetHashCode(_loads);
            hashCode = hashCode * -17 + EqualityComparer<List<LineRestrain>>.Default.GetHashCode(_lineRestrains);
            hashCode = hashCode * -17 + EqualityComparer<List<PointRestrain>>.Default.GetHashCode(_pointRestrains);
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