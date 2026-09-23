using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Steel;

namespace GPC.Model.Sections.Concrete
{
    public interface IConcreteSection : ISectionShape
    {
        #region Section Properties

        /// <summary>
        /// Concrete material.
        /// </summary>
        ConcreteMaterial ConcreteMaterial { get; }

        /// <summary>
        /// Total rebars area.
        /// </summary>
        double AreaRebars { get; }

        /// <summary>
        /// Number of rebars.
        /// </summary>
        int RebarsCount { get; }

        /// <summary>
        /// Cross section-shape of concrete, without material.
        /// </summary>
        ISectionShape SectionShape { get; }

        /// <summary>
        /// Rebar list.
        /// </summary>
        IEnumerable<ReinforcedConcreteRebar> Rebars { get; }

        IList<SteelSectionPosition> SteelSections { get; }

        /// <summary>
        /// Returns if the section is composite (mixed) concrete with steel sections.
        /// </summary>
        bool IsCompositeSteelConcrete { get; }

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

        Dictionary<int, bool> GetRebarIsInsideAssociation();

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
