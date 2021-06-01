using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionCHS : SectionCHS, ISteelSection
    {
        #region Variables 

        private readonly FormedTypes _profileType;

        #endregion


        #region Properties

        public FormedTypes ProductionType => _profileType;

        public bool IsColdFormed => ProductionType == FormedTypes.ColdFormed;

        public bool IsHotFinished => ProductionType == FormedTypes.HotFinished;

        Material ISteelSection.Material()
        {
            return Material;
        }

        public double Height()
        {
            return D;
        }

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
