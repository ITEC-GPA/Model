using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionCHS : SectionCHS, ISteelSection
    {
        #region Variables 

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;

        #endregion


        #region Properties

        public SectionTypes SectionType => _sectionType;

        public FormedTypes FormedType => _formedType;

        public bool IsColdFormed => _formedType == FormedTypes.ColdFormed;

        public bool IsHotFinished => _formedType == FormedTypes.HotFinished;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        public double Height => Diameter;

        #endregion


        #region Public Constructors

        public SteelSectionCHS(double diameter, double thickness, SteelMaterial material, string name, FormedTypes type = FormedTypes.ColdFormed)
            : base(diameter, thickness, material, name)
        {
            _formedType = type;
        }

        public SteelSectionCHS(SectionCHS section, FormedTypes type = FormedTypes.ColdFormed)
            : this(section.Diameter, section.Thickness, (SteelMaterial)section.Material, section.Name, type)
        {

        }

        public SteelSectionCHS(SteelSectionCHS section)
            : this(section.Diameter, section.Thickness, (SteelMaterial)section.Material, section.Name, section.FormedType)
        {

        }

        #endregion

    }
}
