using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionH : Section
    {
        #region Variables
        double _hTot;
        double _hw;
        double _tw;
        double _ttop;
        double _tbottom;
        double _btop;
        double _bbottom;
        #endregion

        public SectionH(double hTot, double tw, double btop, double ttop, double bbottom, double tbottom, Materials.Material material) : base(material)
        {
            _hTot = hTot;
            _tw = tw;
            _btop = btop;
            _bbottom = bbottom;
            _ttop = ttop;
            _tbottom = tbottom;

            _hw = _hTot - _tbottom - _ttop;
        }        

        #region Properties
        public double LenghtBottomFlange => _bbottom;
        public double LenghtTopFlange => _btop;
        public double ThicknessTopFlange => _ttop;
        public double ThicknessBottomFlange => _tbottom;
        public double ThicknessWeb => _tw;
        public double HeightWeb => _hw;
        #endregion
    }
}
