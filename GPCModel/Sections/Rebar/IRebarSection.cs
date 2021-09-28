using GPC.Model.Materials;


namespace GPC.Model.Sections.Rebar
{
    public interface IRebarSection
    {
        string Name { get; }

        RebarMaterial RebarMaterial { get; }

        double Area { get; }
        
        int Id { get; }
    }
}
