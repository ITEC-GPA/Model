using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionRHS : SectionRHS, ISteelSection
    {
        #region Varibles

        private readonly double _r;                // raggio di curvatura o altezza di gola

        #endregion


        #region Properties

        public double R => _r;

        public bool IsRolled => SectionType == SectionTypes.Rolled;

        public bool IsWelded => SectionType == SectionTypes.Welded;

        public bool IsHotFinished => FormedType == FormedTypes.HotFinished;

        public bool IsColdFormed => FormedType == FormedTypes.ColdFormed;

        #region Interface

        Material ISteelSection.Material => Material;

        double ISteelSection.Area => Area;

        double ISteelSection.Height => Height;

        double ISteelSection.InertiaRadiusY => InertiaRadiusY;

        double ISteelSection.InertiaRadiusX => InertiaRadiusX;

        Point2d ISteelSection.Centroid => Centroid;

        double ISteelSection.J11 => J11;

        double ISteelSection.J22 => J22;

        double ISteelSection.Jxx => Jxx;

        double ISteelSection.Jyy => Jyy;

        double ISteelSection.Jt => Jt;

        double ISteelSection.Jw => Jw;

        double ISteelSection.Sx => Sx;

        double ISteelSection.Wpl1 => Wpl1;

        double ISteelSection.Wpl2 => Wpl2;

        double ISteelSection.Wel1 => Wel1;

        double ISteelSection.Wel2 => Wel2;

        #endregion

#endregion


        #region Public Constructors

        public SteelSectionRHS(double height, double width, double thicknessTopFlange, double thicknessBottomFlange, double thicknessWebLeft, double thickenssWebRight, SteelMaterial material, string name, double r = 0,
                            FormedTypes formed = FormedTypes.ColdFormed, SectionTypes sectionType = SectionTypes.Rolled) 
            : base(height, width, thicknessTopFlange, thicknessBottomFlange, thicknessWebLeft, thickenssWebRight, material, name)
        {
            _formedType = formed;
            _sectionType = sectionType;
            _r = r;
        }

        #endregion

    }
}
