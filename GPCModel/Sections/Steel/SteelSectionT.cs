using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionT : SectionT, ISteelSection
    {
        public enum ProfileType
        {
            Rolled,
            Welded,
        }

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


        #region Public Constructors

        public SteelSectionT(SectionTypes type, double hw, double b, double tw, double tf, SteelMaterial material, string name, double radius) 
            : base(hw, b, tw, tf, material, name)
        {
            if (type == SectionTypes.Rolled)
                _r = radius;        // raggio di curvatura

            else if (type == SectionTypes.Welded)
                _r = radius;        // altezza di gola
        }

        #endregion


        #region Public override method

        public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + H + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }

        #endregion
    }
}
