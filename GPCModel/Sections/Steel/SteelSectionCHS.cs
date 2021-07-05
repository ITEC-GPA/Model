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

        #endregion


        #region Properties

        public FormedTypes ProductionType => _profileType;

        public bool IsColdFormed => ProductionType == FormedTypes.ColdFormed;

        public bool IsHotFinished => ProductionType == FormedTypes.HotFinished;

        #region Interface

        Material ISteelSection.Material => Material;

        public double Height => Diameter;

        double ISteelSection.Area => Area;

        #region Interface

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
