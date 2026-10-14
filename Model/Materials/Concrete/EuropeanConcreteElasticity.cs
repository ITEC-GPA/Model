using System;
namespace GPC.Model.Materials
{
    /// <summary>Shared European concrete secant modulus. Mean compressive strength in MPa; either sign is accepted.</summary>
    public static class EuropeanConcreteElasticity
    {
        /// <summary>Secant modulus in MPa, evaluated in the requested unit.</summary>
        public static double SecantModulusMpa(double meanStrengthMpa) => InUnit(meanStrengthMpa, 1);
        /// <summary>Secant modulus in GPa. Existing material properties retain their GPa-to-MPa evaluation order.</summary>
        public static double SecantModulusGpa(double meanStrengthMpa) => InUnit(meanStrengthMpa, 1000);
        private static double InUnit(double meanStrengthMpa, double megapascalsPerUnit)
            => Math.Abs((22000 / megapascalsPerUnit) * Math.Pow(Math.Abs(meanStrengthMpa) / 10, .30));
    }
}
