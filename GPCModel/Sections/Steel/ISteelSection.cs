using GPC.Model.Materials;


namespace GPC.Model.Sections.Steel
{
    public interface ISteelSection
    {
        string Name { get; }

        SteelMaterial SteelMaterial { get; }

        double Height { get; }

        double Area { get; }

        double R11 { get; }

        double R22 { get; }

        Geometry.Point2d Centroid { get; }

        Geometry.Point2d ShearCenter { get; }

        double J11 { get; }

        double J22 { get; }

        double Jxx { get; }

        double Jyy { get; }

        double Jt { get; }

        double Jw { get; }

        double Wpl1 { get; }

        double Wpl2 { get; }

        double Wel1 { get; }

        double Wel2 { get; }

    }
}
