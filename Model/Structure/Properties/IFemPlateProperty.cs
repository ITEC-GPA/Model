using GPC.Model.Materials;

namespace GPC.Model.ElementProperties
{
    /// <summary>
    /// A property of the plate elements for the finite element analysis: thicknesses and material
    /// </summary>
    public interface IFemPlateProperty
    {
        /// <summary>
        /// The thickness for the bending stiffness
        /// </summary>
        double BendingThickness { get; }

        /// <summary>
        /// The thickness for the membrane stiffness
        /// </summary>
        double MembraneThickness { get; }

        /// <summary>
        /// The material
        /// </summary>
        Material Material { get; }
    }
}