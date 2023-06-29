using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    public interface ISteelSection : ISectionShape
    {
        SteelMaterial SteelMaterial { get; }

        Section.SectionTypes SectionType { get; }

        Section.FormedTypes FormedType { get; }

        bool IsRolled { get; }

        bool IsWelded { get; }

        ISectionShape SectionShape { get; }

        double GetMinSigma(double N, double M1, double M2);

        double GetMaxSigma(double N, double M1, double M2);
    }
}
