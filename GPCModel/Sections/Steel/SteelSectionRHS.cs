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

        Material ISteelSection.Material()
        {
            return Material;
        }


        #endregion


        #region Public Constructors

        public SteelSectionRHS(double h, double b, double tf_top, double tf_bottom, double tw1, double tw2, SteelMaterial material, string name, double r = 0,
                            FormedTypes formed = FormedTypes.ColdFormed, SectionTypes sectionType = SectionTypes.Rolled) 
            : base(h, b, tf_top, tf_bottom, tw1, tw2, material, name)
        {
            _formedType = formed;
            _sectionType = sectionType;
            _r = r;
        }

        #endregion

        //public double MinSigma(double N, double M2, double M1)
        //{
        //    double sigma1 = N / _area - M2 / Jyy * (_h - _centroid.Y) + M1 / Jxx * (_centroid.X);
        //    double sigma2 = N / _area - M2 / Jyy * (_h - _centroid.Y) - M1 / Jxx * (_b - _centroid.X);
        //    double sigma3 = N / _area + M2 / Jyy * (_centroid.Y) + M1 / Jxx * (_centroid.X);
        //    double sigma4 = N / _area + M2 / Jyy * (_centroid.Y) - M1 / Jxx * (_b - _centroid.X);

        //    double sigmaMin = Math.Min(sigma1, sigma2);
        //    sigmaMin = Math.Min(sigmaMin, sigma3);
        //    sigmaMin = Math.Min(sigmaMin, sigma4);
        //    return sigmaMin;
        //}

    }
}
