using GPC.Geometry;
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

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;
        #endregion


        #region Properties

        public FormedTypes ProductionType => _profileType;

        public bool IsColdFormed => ProductionType == FormedTypes.ColdFormed;

        public bool IsHotFinished => ProductionType == FormedTypes.HotFinished;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        public double Height => Diameter;


        #endregion


        #region Public Constructors

        public SteelSectionCHS(double diameter, double thickness, SteelMaterial material, string name, FormedTypes type = FormedTypes.ColdFormed)
            : base(diameter, thickness, material, name)
        {
            _profileType = type;
        }

        #endregion

    }
}
