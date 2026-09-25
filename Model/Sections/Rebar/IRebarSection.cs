using GPC.Model.Materials;


namespace GPC.Model.Sections.Rebar
{
    /// <summary>
    /// The section of a rebar
    /// </summary>
    public interface IRebarSection
    {
        /// <summary>
        /// The name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The material
        /// </summary>
        SteelMaterial RebarMaterial { get; set; }

        /// <summary>
        /// The diameter
        /// </summary>
        double Diameter { get; }

        /// <summary>
        /// The area
        /// </summary>
        double Area { get; }

        /// <summary>
        /// The id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The moment of inertia about X through the center
        /// </summary>
        double Jxx { get; }

        /// <summary>
        /// The moment of inertia about Y through the center
        /// </summary>
        double Jyy { get; }

        /// <summary>
        /// The product of inertia (0 for a circle)
        /// </summary>
        double Jxy { get; }
    }
}
