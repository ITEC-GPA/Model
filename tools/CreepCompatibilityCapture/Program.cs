using GPC.Model.Materials;
using System.Globalization;
// Before/after record of the historical strain API, including its documented limitations.
// Not an independent engineering oracle; output is created only if the target does not exist.
using var writer = new StreamWriter(new FileStream(args.Single(), FileMode.CreateNew));
writer.WriteLine("fck;cement;RH;t0;sigma;temperature;strain-bits");
foreach (double fck in new[] { 12d, 27, 30, 90 })
foreach (ConcreteMaterialEuropeanCommon.CementTypes cement in Enum.GetValues<ConcreteMaterialEuropeanCommon.CementTypes>())
foreach (double rh in new[] { 40d, 70, 100 })
foreach (double t0 in new[] { .5, 7, 28 })
foreach (double sigma in new[] { -1d, 0, 20 })
foreach (double temperature in new[] { 0d, 20 })
{
    var material = new ConcreteMaterialEN1992("Capture", fck, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle, cementType: cement);
    double strain = material.GetEpsilonCCInfiniteTime(sigma, rh, 280000, 1000, t0, temperature, 7);
    writer.WriteLine(FormattableString.Invariant($"{fck};{cement};{rh};{t0};{sigma};{temperature};{BitConverter.DoubleToInt64Bits(strain):X16}"));
}
