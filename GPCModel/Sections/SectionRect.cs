using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionRect : Section
    {
        #region Variables

        protected double _d;
        protected double _b;

        #endregion

        #region Properties

        public double D => _d;

        public double B => _b;

        #endregion

        #region Public Constructors

        public SectionRect(double b, double d)
            : base()
        {
            _b = b;
            _d = d;
        }

        public SectionRect(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _b = info.GetDouble("B");
            _d = info.GetDouble("D");
        }

        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("B", _d);
            info.AddValue("D", _d);
        }

        public override void Calculate()
        {
            base.Calculate();
            _area = _b * _d;
        }

        #endregion
    }
}