using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionC : Section
    {
        #region Variables
        protected double _h;
        protected double _hw;
        protected double _tw;
        protected double _lengthBottom;
        protected double _tBottom;
        protected double _lengthTop;
        protected double _ttop;
        #endregion

        #region Properties
        public double H => _h;
        public double Hw => _hw;
        public double Tw => _tw;
        public double Lbottom => _lengthBottom;
        public double ThicknessBottom => _tBottom;
        public double Ltop => _lengthTop;
        public double ThicknessTop => _ttop;
        #endregion

        public SectionC(double h, double tw, double LTop, double tTop, double LBottom, double tBottom, Material material) : base(material)
        {
            _hw = h - tBottom - tTop;
        }
    }
}
