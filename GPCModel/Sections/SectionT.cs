using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionT : Section
    {
        #region Variables
        double _h;
        double _tw;
        double _tf;
        double _b;
        #endregion

        #region Properties
        public double H => _h;
        public double Tw => _tw;
        public double Tf => _tf;
        public double B => _b;
        #endregion

        public SectionT(double h, double b, double tw, double tf, Material material) : base(material)
        {
            _h = h;
            _b = b;
            _tw = tw;
            _tf = tf;
        }
    }
}
