using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Steel
{
    class SteelSectionCHS : SectionCHS
    {
        #region Variables 

        private FormedTypes _profileType;

        #endregion


        #region Properties

        public FormedTypes ProductionType => _profileType;

        public bool IsColdFormed => ProductionType == FormedTypes.ColdFormed;

        public bool IsHotFinished => ProductionType == FormedTypes.HotFinished;

        #endregion


        #region Public Constructors

        public SteelSectionCHS(double diameter, double t, SteelMaterial material, string name, FormedTypes type = FormedTypes.ColdFormed)
            : base(diameter, t, material, name)
        {
            _profileType = type;
        }

        #endregion
                
    }
}
