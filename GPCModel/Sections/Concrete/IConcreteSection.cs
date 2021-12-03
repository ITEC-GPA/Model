using System.Collections.Generic;
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

        double AreaRebars { get; }
        int RebarsCount { get; }

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

        IEnumerable<ReinforcedConcreteRebar> Rebars { get; }


        bool AddRebar(ReinforcedConcreteRebar rebar);
        bool AddRebar (ReinforcedConcreteRebar rebar, out int id);

        bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars, out int[] ids);
        bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars);

        bool RemoveRebar(ReinforcedConcreteRebar rebar);

        bool RemoveRebar(int rebarId);

        bool RemoveRebars(IEnumerable<ReinforcedConcreteRebar> rebars);

        bool ClearRebars();

        ReinforcedConcreteRebar GetRebarById(int rebarId);
        ReinforcedConcreteRebar[] GetRebarById(IEnumerable<int> rebarIds);

        ReinforcedConcreteRebar[] GetRebars();

        double GetHomogenizedArea(double n);

        double GetHomogenizedArea();

        double GetHomogeneizedJ11();

        double GetHomogeneizedJ11(double n);

        double GetHomogeneizedJ22();

        double GetHomogeneizedJ22(double n);

        Point2d GetHomogenizedCentroid(out double SxHomog, out double SyHomog);

        Point2d GetHomogenizedCentroid(double n, out double SxHomog, out double SyHomog);

        Geometry.Meshes.Mesh Mesh { get; }

        double CalculateN(ReinforcedConcreteRebar rebar);

        double CalculateN(int rebar);

        (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties();

        (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties(double n);

        ReinforcedConcreteSection ToReinforcedConcreteSection();
    }
}
