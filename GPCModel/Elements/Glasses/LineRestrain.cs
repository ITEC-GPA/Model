using System;
using System.Runtime.Serialization;
using GPC.Geometry;

namespace GPC.Model.Elements.Glasses
{
    public class LineRestrain : Element
    {
        #region Variables

        private Line3d _line;
        private bool _d1;
        private bool _d2;
        private bool _d3;
        private bool _r1;
        private bool _r2;
        private bool _r3;

        #endregion

        #region Properties

        public bool D1 => _d1;
        public bool D2 => _d2;
        public bool D3 => _d3;
        public bool R1 => _r1;
        public bool R2 => _r2;
        public bool R3 => _r3;

        public Line3d Line => _line; 

        #endregion

        public LineRestrain(Guid guid) 
            : base(guid)
        {

        }

        public LineRestrain(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _d1 = info.GetBoolean("D1");
            _d2 = info.GetBoolean("D2");
            _d3 = info.GetBoolean("D3");
            _r1 = info.GetBoolean("R1");
            _r2 = info.GetBoolean("R2");
            _r3 = info.GetBoolean("R3");
            _line = (Line3d)info.GetValue("Line", typeof(Line3d));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("D1", _d1);
            info.AddValue("D2", _d2);
            info.AddValue("D3", _d3);
            info.AddValue("R1", _r1);
            info.AddValue("R2", _r2);
            info.AddValue("R3", _r3);
            info.AddValue("Line", _line);
        }
    }
}
