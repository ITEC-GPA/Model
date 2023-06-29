using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Concrete
{
    public interface IConcreteSection : ISectionShape
    {
		#region Section Properties

        ConcreteMaterial ConcreteMaterial { get; }

        double AreaRebars { get; }

        int RebarsCount { get; }

        Geometry.Meshes.Mesh Mesh { get; }

        ISectionShape SectionShape { get; }

        IEnumerable<ReinforcedConcreteRebar> Rebars { get; }

        #endregion

        #region Methods

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

		#endregion

		#region Homogenized Properties

		double GetHomogenizedArea(double phi);

        double GetHomogenizedArea();

        double GetHomogeneizedJ11();

        double GetHomogeneizedJ11(double phi);

        double GetHomogeneizedJ22();

        double GetHomogeneizedJ22(double phi);

        Point2d GetHomogenizedCentroid(out double SxHomog, out double SyHomog);

        Point2d GetHomogenizedCentroid(double n, out double SxHomog, out double SyHomog);

        double CalculateN(ReinforcedConcreteRebar rebar);

        double CalculateN(int rebar);

        (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties();

        (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties(double phi);

        #endregion
    }
}
