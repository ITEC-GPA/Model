
namespace GPC.Model.Glasses
{
    public interface IGlassPanel
    {
        /// <returns>Total _thickness of the glass package included interlayer</returns>
        double TotalThickness { get; }

        /// <returns>The mininum Elastic modulus of the glass panels</returns>
        double GetElasticModulus();

        /// <returns>The mininum poissonRatio of the glass panels</returns>
        double GetPoissonRatios();

        /// <returns>The self weight per unit area</returns>
        double GetSelfWeightPerUnitArea();

        /// <returns>The equivalent density</returns>
        double GetDensity();


        IGlassPackage[] GetGlassPackage();
    }
}