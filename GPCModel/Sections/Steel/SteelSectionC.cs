using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionC : SectionC, ISteelSection
    {
        #region Variables

        private readonly double _r;                // raggio di curvatura o altezza di gola

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;

        #endregion


        #region Properties

        public SectionTypes Type => _sectionType;

        public double R => _r;

        public bool IsRolled => Type == SectionTypes.Rolled;

        public bool IsWelded => Type == SectionTypes.Welded;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion


        #region Public Constructors

        public SteelSectionC(double height, double thicknessWeb, double lengthTop, double thicknessTop, double lengthBottom, 
                            double thicknessBottom, SteelMaterial material, string name, 
                            SectionTypes type = SectionTypes.Rolled,
                            FormedTypes formedType = FormedTypes.ColdFormed, 
                            double radius = 0)
            : base(height, thicknessWeb, lengthTop, thicknessTop, lengthBottom, thicknessBottom, material, name)
        {
            _r = radius < 0 ? 0 : radius;         // raggio di curvatura o altezza di gola
            _sectionType = type;
            _formedType = formedType;
        }

        #endregion
                
        
    }
}
