using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Maths.GaussIntegrations
{
    public static class HexahedroGaussPoints
    {

        public enum GaussPointNumber
        {
            Hexa1 = 1,
            Hexa8 = 8,
        }


        public static readonly GaussPoint[] Hexa1 = new GaussPoint[] { new GaussPoint(0.0, 0.0, 0.0, 8.0, 1) };

        public static readonly GaussPoint[] Hexa8 = new GaussPoint[] { new GaussPoint(-1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 1.0, 1),
                                                                      new GaussPoint(+1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 1.0, 2),
                                                                      new GaussPoint(-1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 1.0, 3),
                                                                      new GaussPoint(+1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 1.0, 4),
                                                                      new GaussPoint(-1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 1.0, 5),
                                                                      new GaussPoint(+1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 1.0, 6),
                                                                      new GaussPoint(-1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 1.0, 7),
                                                                      new GaussPoint(+1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 1.0, 8) };
    }
}
