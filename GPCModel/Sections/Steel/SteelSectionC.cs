using GPC.Geometry;
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

        #region Interface

        Material ISteelSection.Material => Material;

        double ISteelSection.Height => Height;

        double ISteelSection.Area => Area;

        double ISteelSection.InertiaRadiusY => InertiaRadiusY;

        double ISteelSection.InertiaRadiusX => InertiaRadiusX;

        Point2d ISteelSection.Centroid => Centroid;

        Point2d ISteelSection.ShearCenter => ShearCenter;

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

        public SteelSectionC(double height, double thicknessWeb, double lengthTop, double thicknessTop, double lengthBottom, double thicknessBottom, SteelMaterial material, string name, 
                            SectionTypes type = SectionTypes.Rolled, FormedTypes formedType = FormedTypes.ColdFormed, double radius = 0)
            : base(height, thicknessWeb, lengthTop, thicknessTop, lengthBottom, thicknessBottom, material, name)
        {
            _sectionType = type;
            _formedType = formedType;
            _r = radius;        // raggio di curvatura o altezza di gola
        }

        #endregion
                
        
    }
}
