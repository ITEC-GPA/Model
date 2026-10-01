using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;

namespace GPC.Model.Sections
{
    /// <summary>
    /// The groups of properties of a section whose availability is declared by <see cref="Section.GetAvailability"/>
    /// </summary>
    public enum SectionProperty
    {
        /// <summary>The area</summary>
        Area,
        /// <summary>The centroid</summary>
        Centroid,
        /// <summary>The moments of inertia (about X and Y, product, principal) and the principal axes</summary>
        MomentsOfInertia,
        /// <summary>The elastic moduli</summary>
        ElasticModuli,
        /// <summary>The plastic moduli</summary>
        PlasticModuli,
        /// <summary>The Saint-Venant torsion constant</summary>
        TorsionConstant,
        /// <summary>The warping constant</summary>
        WarpingConstant,
        /// <summary>The shear centre</summary>
        ShearCenter,
    }

    /// <summary>
    /// How a property of a section is obtained (see <see cref="Section.GetAvailability"/>)
    /// </summary>
    public enum PropertyAvailability
    {
        /// <summary>
        /// Not available: the value is not defined (NaN) or is a placeholder (e.g. 0, the centroid) that must not be used
        /// </summary>
        NotAvailable,
        /// <summary>
        /// Given with the section (the section created from its values)
        /// </summary>
        Given,
        /// <summary>
        /// Exact for the geometry of the section: closed formula or integration on its boundary (the arcs are polygons, relative error about 1e-4)
        /// </summary>
        Exact,
        /// <summary>
        /// Solved with the finite elements on the region of the section (relative error about 1e-4, see <see cref="SectionTorsionProperties"/>)
        /// </summary>
        Numerical,
        /// <summary>
        /// From an approximate formula (thin-walled theory, formulas of the producers): see the class of the section for its deviation
        /// </summary>
        Approximate,
    }

    /// <summary>
    /// The availability of the properties of the section shapes that are not a <see cref="Section"/>
    /// </summary>
    public static class SectionAvailability
    {
        /// <summary>
        /// How a property of a section shape is obtained: the one of the <see cref="Section"/>, of the shape of a <see cref="SteelSection"/>, of
        /// the concrete of an <see cref="IConcreteSection"/> (the properties of the concrete only: the homogenized ones are computed by the
        /// concrete section)
        /// </summary>
        /// <param name="shape">The section shape</param>
        /// <param name="property">The property</param>
        /// <returns>The availability; <see cref="PropertyAvailability.NotAvailable"/> for the other shapes or null</returns>
        public static PropertyAvailability GetAvailability(this ISectionShape shape, SectionProperty property)
        {
            switch (shape)
            {
                case Section section:
                    return section.GetAvailability(property);
                case SteelSection steel:
                    return GetAvailability(steel.SectionShape, property);
                case IConcreteSection concrete:
                    return GetAvailability(concrete.SectionShape, property);
                default:
                    return PropertyAvailability.NotAvailable;
            }
        }
    }
}
