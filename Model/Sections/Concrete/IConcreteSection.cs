using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Steel;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// A concrete section with rebars and steel sections (composite): the homogenized properties use n = Es / Ec, or Es / (Ec / (1 + phi)) with the creep coefficient phi
    /// </summary>
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

        /// <summary>
        /// The steel sections (composite section)
        /// </summary>
        IList<SteelSectionPosition> SteelSections { get; }

        /// <summary>
        /// Returns if the section is composite (mixed) concrete with steel sections.
        /// </summary>
        bool IsCompositeSteelConcrete { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a rebar
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <returns>True if the rebar has been added</returns>
        bool AddRebar(ReinforcedConcreteRebar rebar);

        /// <summary>
        /// Adds a rebar
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <param name="id">The id of the rebar</param>
        /// <returns>True if the rebar has been added</returns>
        bool AddRebar (ReinforcedConcreteRebar rebar, out int id);

        /// <summary>
        /// Adds rebars
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <param name="ids">The ids of the rebars</param>
        /// <returns>For each rebar, true if it has been added</returns>
        bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars, out int[] ids);

        /// <summary>
        /// Adds rebars
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <returns>For each rebar, true if it has been added</returns>
        bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars);

        /// <summary>
        /// Removes a rebar
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <returns>True if the rebar has been removed</returns>
        bool RemoveRebar(ReinforcedConcreteRebar rebar);

        /// <summary>
        /// Removes the rebar with an id
        /// </summary>
        /// <param name="rebarId">The id</param>
        /// <returns>True if the rebar has been removed</returns>
        bool RemoveRebar(int rebarId);

        /// <summary>
        /// Removes rebars
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <returns>True if all the rebars have been removed</returns>
        bool RemoveRebars(IEnumerable<ReinforcedConcreteRebar> rebars);

        /// <summary>
        /// Removes all the rebars
        /// </summary>
        /// <returns>True if the rebars have been removed</returns>
        bool ClearRebars();

        /// <summary>
        /// The rebar with an id
        /// </summary>
        /// <param name="rebarId">The id</param>
        /// <returns>The rebar</returns>
        ReinforcedConcreteRebar GetRebarById(int rebarId);

        /// <summary>
        /// The rebars with the given ids
        /// </summary>
        /// <param name="rebarIds">The ids</param>
        /// <returns>The rebars</returns>
        ReinforcedConcreteRebar[] GetRebarById(IEnumerable<int> rebarIds);

        /// <summary>
        /// The rebars
        /// </summary>
        /// <returns>A new array with the rebars</returns>
        ReinforcedConcreteRebar[] GetRebars();

        /// <summary>
        /// For each rebar, if it is inside the concrete
        /// </summary>
        /// <returns>The map between the INDEX of the rebar (not its id) and true if it is inside</returns>
        Dictionary<int, bool> GetRebarIsInsideAssociation();

        #endregion

        #region Homogenized Properties

        /// <summary>
        /// The homogenized area with the creep coefficient
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>The homogenized area</returns>
        double GetHomogenizedArea(double phi);

        /// <summary>
        /// The homogenized area (n = Es / Ec)
        /// </summary>
        /// <returns>The homogenized area</returns>
        double GetHomogenizedArea();

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 1 (n = Es / Ec)
        /// </summary>
        /// <returns>The moment of inertia</returns>
        double GetHomogeneizedJ11();

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 1 with the creep coefficient
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>The moment of inertia</returns>
        double GetHomogeneizedJ11(double phi);

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 2 (n = Es / Ec)
        /// </summary>
        /// <returns>The moment of inertia</returns>
        double GetHomogeneizedJ22();

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 2 with the creep coefficient
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>The moment of inertia</returns>
        double GetHomogeneizedJ22(double phi);

        /// <summary>
        /// The centroid of the homogenized section (n = Es / Ec)
        /// </summary>
        /// <param name="SxHomog">The homogenized static moment respect to X</param>
        /// <param name="SyHomog">The homogenized static moment respect to Y</param>
        /// <returns>The centroid</returns>
        Point2d GetHomogenizedCentroid(out double SxHomog, out double SyHomog);

        /// <summary>
        /// The centroid of the homogenized section with a given factor
        /// </summary>
        /// <param name="n">The homogenization factor</param>
        /// <param name="SxHomog">The homogenized static moment respect to X</param>
        /// <param name="SyHomog">The homogenized static moment respect to Y</param>
        /// <returns>The centroid</returns>
        Point2d GetHomogenizedCentroid(double n, out double SxHomog, out double SyHomog);

        /// <summary>
        /// The homogenization factor of a rebar: Es / Ec
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <returns>The factor</returns>
        double CalculateN(ReinforcedConcreteRebar rebar);

        /// <summary>
        /// The homogenization factor of the rebar with an id: Es / Ec
        /// </summary>
        /// <param name="rebar">The id of the rebar</param>
        /// <returns>The factor</returns>
        double CalculateN(int rebar);

        /// <summary>
        /// All the homogenized properties (n = Es / Ec)
        /// </summary>
        /// <returns>Area, static moments, centroid, moments of inertia about X, Y and principal, product of inertia, polar moment and angle of the axis 1</returns>
        (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties();

        /// <summary>
        /// All the homogenized properties with the creep coefficient
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>Area, static moments, centroid, moments of inertia about X, Y and principal, product of inertia, polar moment and angle of the axis 1</returns>
        (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties(double phi);

        #endregion
    }
}
