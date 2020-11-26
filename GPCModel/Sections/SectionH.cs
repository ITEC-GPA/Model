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
        double _h_tot;
        double _tw;
        double _ttop;
        double _tbottom;
        double _btop;
        double _bbottom;
        #endregion

        public SectionH(double h_tot, double tw, double btop, double ttop, double bbottom, double tbottom, Materials.Material material) : base(material)
        {
            _h_tot = h_tot;

            _tw = tw;
            _btop = btop;
            _bbottom = bbottom;

            _ttop = ttop;
            _tbottom = tbottom;
        }        

        #region Properties
        public double LenghtBottomFlange => _bbottom;
        public double LenghtTopFlange => _btop;
        public double ThicknessTopFlange => _ttop;
        public double ThicknessBottomFlange => _tbottom;
        #endregion
    }
}
