using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionL : Section
    {
        #region Variables
        double _l1;
        double _t1;
        double _l2;
        double _t2;
        #endregion

        #region Properties
        public double L1 => _l1;
        public double T1 => _t1;
        public double L2 => _l2;
        public double T2 => _t2;
        #endregion

        public SectionL(double l1, double t1, double l2, double t2, Material material) : base(material)
        {
            _l1 = l1;
            _t1 = t1;
            _l2 = l2;
            _t2 = t2;
        }
    }
}
