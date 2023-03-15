using System;
using System.Collections.Generic;
using System.Text;

namespace GPC.Model.Sections.Bolt
{
    public class Hole
    {
        public double Diameter { get; set; }

        /// <summary>
        /// Rotation in radian, counterclockwise and zero in positive x-axis.
        /// </summary>
        public double Rotation { get; set; }

        /// <summary>
        /// For slotted holes, distance between centers. For circular hole this is equal to zero.
        /// </summary>
        public double SlotLength { get; set; }

        /// <summary>
        /// Adimensional position of bolt center, -1 to 1, 0 to use middle point.
        /// </summary>
        public double PosBolt { get; set; }
    }
}
