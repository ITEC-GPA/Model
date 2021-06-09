
namespace GPC.Model.Glasses
{
    public interface IGlassPanel
    {
        double TotalThickness { get; }

        /// <returns>Total thickness of the glass package included interlayer</returns>
        double GetTotalThickness();

        /// <returns>The mininum Elastic modulus of the glass panels</returns>
        double GetElasticModulus();

        /// <returns>The mininum poissonRatio of the glass panels</returns>
        double GetPoissonRatios();

        /// <returns>The self weight per unit area</returns>
        double GetSelfWeightPerUnitArea();

        IGlassPackage[] GetGlassPackage();
    }
}