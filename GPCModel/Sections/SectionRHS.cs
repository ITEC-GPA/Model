using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionRHS : Section
    {
        #region Varibles
        double _h;
        double _b;
        double _tf_top;
        double _tf_bottom;
        double _tw1;
        double _tw2;
        #endregion

        #region Properties
        public double B => _b;
        public double H => _h;
        public double ThicknessFlange {
            get {
                if(_tf_bottom == _tf_top) {
                    return _tf_top;
                } else
                {
                    throw new Exception("not yet supported");
                }
            }
        }
        public double ThicknessWeb
        {
            get
            {
                if (_tw1 == _tw2)
                {
                    return _tw1;
                }
                else
                {
                    throw new Exception("not yet supported");
                }
            }
        }
        #endregion

        public SectionRHS(double h, double b, double tf_top, double tf_bottom, double tw1, double tw2, Materials.Material material) : base(material)
        {
            _h = h;
            _b = b;
            _tf_top = tf_top;
            _tf_bottom = tf_bottom;
            _tw1 = tw1;
            _tw2 = tw2;
        }
    }
}
