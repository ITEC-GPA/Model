using GPC.Model.Materials;
using static GPC.Model.Sections.Section;


namespace GPC.Model.Sections.Steel
{
    public interface ISteelSection : ISection
    {
        SteelMaterial SteelMaterial { get; }

        Section.SectionTypes SectionType { get; }

        Section.FormedTypes FormedType { get; }

        bool IsRolled { get; }

        bool IsWelded { get; }

        ISection SectionShape { get; }

        double GetMinSigma(double N, double M1, double M2);

        double GetMaxSigma(double N, double M1, double M2);
    }
}
