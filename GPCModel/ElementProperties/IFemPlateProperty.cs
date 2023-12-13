using GPC.Model.Materials;

namespace GPC.Model.ElementProperties
{
    public interface IFemPlateProperty
    {
        double BendingThickness { get; }

        double MembraneThickness { get; }

        Material Material { get; }
    }
}