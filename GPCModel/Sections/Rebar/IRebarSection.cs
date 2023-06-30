using GPC.Model.Materials;


namespace GPC.Model.Sections.Rebar
{
    public interface IRebarSection
    {
        string Name { get; }

        SteelMaterial RebarMaterial { get; }

        double Diameter { get; }

        double Area { get; }

        int Id { get; }

        double Jxx { get; }

        double Jyy { get; }

        double Jxy { get; }
    }
}
