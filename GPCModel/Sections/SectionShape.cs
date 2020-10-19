using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionShape : Section
    {
        protected Shape2d _shape;

        public Shape2d Shape => _shape;

        public SectionShape(Shape2d shape)
            : base()
        {
            _shape = shape;            
        }

        public override void Calculate()
        {
            base.Calculate();
            // Calcolo Area, ecc a partire dalla shape
        }

        public override Shape2d GetShape()
        {
            return _shape;
        }
    }
}
