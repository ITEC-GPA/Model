using GPC.Model.Materials;


namespace GPC.Model.Sections.Steel
{
    public interface ISteelSection : ISection
    {
        SteelMaterial SteelMaterial { get; }

        Section.SectionTypes SectionType { get; }

        Section.FormedTypes FormedType { get; }

        ISection SectionShape { get; }
    }
}
