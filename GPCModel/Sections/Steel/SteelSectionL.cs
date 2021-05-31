using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    
    public class SteelSectionL : SectionL, ISteelSection
    {
        #region Variables

        private double _r;                // raggio di curvatura o altezza di gola

        #endregion


        #region Properties

        public SectionTypes Type => _sectionType;

        public double R => _r;

        public bool IsRolled => Type == SectionTypes.Rolled;

        public bool IsWelded => Type == SectionTypes.Welded;

        Material ISteelSection.Material()
        {
            return Material;
        }


        #endregion


        #region Constructor

        public SteelSectionL(double lHor, double tHor, double lVert, double tVert, SteelMaterial material, string name, SectionTypes sectionTypes = SectionTypes.Rolled, double radius = 0) 
            : base(lHor, tHor, lVert, tVert, material, name)
        {
            if (sectionTypes == SectionTypes.Rolled)
                _r = radius;        // raggio di curvatura

            else if (sectionTypes == SectionTypes.Welded)
                _r = radius;        // altezza di gola
        }

        #endregion

    }
}
