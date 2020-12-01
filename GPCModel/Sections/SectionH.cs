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
            IsSymmetricAlongYLocalAxis = true;
            if (_btop == _bbottom && _tbottom == _ttop)
            {
                IsSymmetricAlongZLocalAxis = true;
            }
            else
            {
                IsSymmetricAlongZLocalAxis = false;
            }
        }

        #region Properties
        public double LenghtBottomFlange => _bbottom;
        public double LenghtTopFlange => _btop;
        public double ThicknessTopFlange => _ttop;
        public double ThicknessBottomFlange => _tbottom;
        public double ThicknessWeb => _tw;
        public double HeightWeb => _hw;
        public double H => _hTot;
        public double B
        {
            get
            {
                if (_bbottom == _btop)
                {
                    return _btop;
                } else
                {
                    throw new Exception("different B");
                }
            }
        }

        public bool IsRolled { get; set; }
        #endregion
    }
}
