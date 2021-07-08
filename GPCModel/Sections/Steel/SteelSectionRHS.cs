using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionRHS : SectionRHS, ISteelSection
    {
        #region Varibles

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

        public bool IsHotFinished => _formedType == FormedTypes.HotFinished;

        public bool IsColdFormed => _formedType == FormedTypes.ColdFormed;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion


        #region Public Constructors

        public SteelSectionRHS(double height, double width, double thicknessTopFlange, double thicknessBottomFlange, 
                               double thicknessWebLeft, double thickenssWebRight, SteelMaterial material, string name, double radius = 0,
                               FormedTypes formed = FormedTypes.ColdFormed, SectionTypes sectionType = SectionTypes.Rolled) 
            : base(height, width, thicknessTopFlange, thicknessBottomFlange, thicknessWebLeft, thickenssWebRight, material, name)
        {
            _formedType = formed;
            _sectionType = sectionType;
            _r = radius < 0 ? 0 : radius;
        }

        #endregion

    }
}
