using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.FiniteElements
{
    public class Quad8Element
    {
        #region ShapeFunction
        public static double dNdCsi(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (2.0 * csi + eta) * (1.0 - eta);
                case 2:
                    return 1.0 / 4.0 * (2.0 * csi - eta) * (1.0 - eta);
                case 3:
                    return 1.0 / 4.0 * (2.0 * csi + eta) * (1.0 + eta);
                case 4:
                    return 1.0 / 4.0 * (2.0 * csi - eta) * (1.0 + eta);
                case 5:
                    return -csi * (1.0 - eta);
                case 6:
                    return 1.0 / 2.0 * (1.0 - eta * eta);
                case 7:
                    return -csi * (1.0 + eta);
                case 8:
                    return -1.0 / 2.0 * (1.0 - eta * eta);
                default:
                    throw new Exception();
            }
        }

        public static double dNdEta(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (2.0 * eta + csi) * (1.0 - csi);
                case 2:
                    return 1.0 / 4.0 * (2.0 * eta - csi) * (1.0 + csi);
                case 3:
                    return 1.0 / 4.0 * (2.0 * eta + csi) * (1.0 + csi);
                case 4:
                    return 1.0 / 4.0 * (2.0 * eta - csi) * (1.0 - csi);
                case 5:
                    return -1.0 / 2.0 * (1.0 - csi * csi);
                case 6:
                    return -eta * (1.0 + csi);
                case 7:
                    return 1.0 / 2.0 * (1.0 - csi * csi);
                case 8:
                    return -eta * (1.0 - csi);
                default:
                    throw new Exception();
            }
        }
        #endregion
    }
}
