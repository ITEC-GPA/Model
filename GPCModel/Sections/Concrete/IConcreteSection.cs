using GPC.Geometry;
using GPC.Model.Materials;


namespace GPC.Model.Sections.Concrete
{
    public interface IConcreteSection
    {
        string Name { get; }

        Shape2d Shape { get; }

        ConcreteMaterial ConcreteMaterial { get; }

        double Area { get; }

        double R11 { get; }

        double R22 { get; }

        Point2d Centroid { get; }

        Point2d ShearCenter { get; }

        double J11 { get; }

        double J22 { get; }

        double Jxx { get; }

        double Jyy { get; }

        double Wpl1 { get; }

        double Wpl2 { get; }

        double Wel1 { get; }

        double Wel2 { get; }

        bool IsSymmetricAlongXLocalAxis { get; }

        bool IsSymmetricAlongYLocalAxis { get; }

        bool IsDoubleSymmetric { get; }

        ReinforcedConcreteRebar[] Rebars { get; }

        double GetHomogenizedArea(double n);

        double GetHomogenizedArea();

        double GetHomogeneizedJ11();

        double GetHomogeneizedJ11(double n);

        double GetHomogeneizedJ22();

        double GetHomogeneizedJ22(double n);

        Geometry.Meshes.Mesh Mesh { get; }

        double CalculateN(ReinforcedConcreteRebar rebar);

        double CalculateN(int rebar);

        void GetHomogeneizedMechanicalProperties(out double areaH, out double SxH, out double SyH, out Point3d centroidH,
            out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX);

        void GetHomogeneizedMechanicalProperties(double n, out double areaH, out double SxH, out double SyH, out Point3d centroidH,
            out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX);
    }
}
