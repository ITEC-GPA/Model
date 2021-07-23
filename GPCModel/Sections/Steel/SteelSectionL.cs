using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    
    public class SteelSectionL : SectionL, ISteelSection
    {
        #region Variables

        private readonly double _r;                // raggio di curvatura o altezza di gola

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;
        #endregion


        #region Properties

        public SectionTypes SectionType => _sectionType;
        public FormedTypes FormedType => _formedType;

        public double R => _r;

        public bool IsRolled => _sectionType == SectionTypes.Rolled;

        public bool IsWelded => _sectionType == SectionTypes.Welded;

        public double Height => LengthVert;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion


        #region Constructor

        public SteelSectionL(double lHor, double tHor, double lVert, double tVert, SteelMaterial material, 
                            string name, SectionTypes sectionTypes = SectionTypes.Rolled, 
                            FormedTypes formedType = FormedTypes.ColdFormed, double radius = 0) 
            : base(lHor, tHor, lVert, tVert, material, name)
        {
            _sectionType = sectionTypes;
            _formedType = formedType;
            _r = radius < 0 ? 0 : radius;        // raggio di curvatura o altezza di gola
        }

        #endregion

    }
}
