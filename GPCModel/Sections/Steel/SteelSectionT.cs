using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionT : SectionT, ISteelSection
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

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion


        #region Public Constructors

        public SteelSectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, SteelMaterial material, string name,
            double radius = 0, FormedTypes formedType = FormedTypes.HotFinished, SectionTypes sectionType = SectionTypes.Rolled)
            : base(height, flangeLength, thicknessWeb, thicknessFlange, material, name)
        {
            _sectionType = sectionType;
            _formedType = formedType;
            _r = radius;        // raggio di curvatura o altezza di gola
        }

        #endregion


        #region Public override method

        public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + base.Height + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }



        #endregion
    }
}
