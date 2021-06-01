using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionC : SectionC, ISteelSection
    {
        #region Variables

        private readonly double _r;                // raggio di curvatura o altezza di gola

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
        public double Height()
        {
            return H;
        }


        #endregion


        #region Public Constructors

        public SteelSectionC(double h, double tw, double lTop, double tTop, double lBottom, double tBottom, SteelMaterial material, string name, 
                            SectionTypes type = SectionTypes.Rolled, double radius = 0)
            : base(h, tw, lTop, tTop, lBottom, tBottom, material, name)
        {
            if (radius != 0)
            {
                if (type == SectionTypes.Rolled)
                    _r = radius;        // raggio di curvatura

                else if (type == SectionTypes.Welded)
                    _r = radius;        // altezza di gola
            }
        }

        #endregion
                
        
    }
}
