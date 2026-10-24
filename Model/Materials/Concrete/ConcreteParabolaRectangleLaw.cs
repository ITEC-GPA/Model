using System;

namespace GPC.Model.Materials
{
    /// <summary>The evaluation of a standard European n=2 parabola-rectangle diagram.</summary>
    public enum ConcreteParabolaRectangleMethod
    {
        /// <summary>Analytic parabola, the default for current calculations.</summary>
        Analytic = 0,
        /// <summary>Historical piecewise-linear approximation, retained only for reproducing earlier calculations.</summary>
        [Obsolete("Historical method. Use ConcreteParabolaRectangleMethod.Analytic for current calculations.")]
        TabulatedLegacy = 1
    }

    /// <summary>Characteristic n=2 parabola-rectangle law, shared by material evaluation and section integration.</summary>
    public readonly struct ConcreteParabolaRectangleLaw
    {
        public double Ultimate { get; }
        public double Peak { get; }
        public double Stress { get; }

        private ConcreteParabolaRectangleLaw(double ultimate, double peak, double stress)
        { Ultimate = ultimate; Peak = peak; Stress = stress; }

        /// <summary>Characteristic stress, negative in compression; zero outside the compressive diagram.</summary>
        public double GetStress(double strain)
        {
            if (strain >= 0 || strain < Ultimate) return 0;
            if (strain <= Peak) return Stress;
            double r = strain / Peak;
            return Stress * r * (2 - r);
        }

        /// <summary>Recognizes the current, unmodified n=2 table. Custom diagrams, high strengths and the historical mode are excluded.</summary>
        public static bool TryCreate(ConcreteMaterial material, out ConcreteParabolaRectangleLaw law)
        {
            law = default;
            if (!(material is ConcreteMaterialEuropeanCommon european) ||
                european.ParabolaRectangleMethod != ConcreteParabolaRectangleMethod.Analytic ||
                material.CompressionStressStrainDiagram != ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle ||
                !(european.Fck < 0 && european.Fck >= -50)) return false;
            var e = material.StressStrainTableCompression.Strains;
            var s = material.StressStrainTableCompression.Stresses;
            double peak = material.StrainYCompression, ultimate = material.StrainUCompression;
            if (e.Length != 10 || s.Length != 10 || !(ultimate < peak && peak < 0) ||
                double.IsInfinity(ultimate) || e[0] != 0 || s[0] != 0 || e[9] != ultimate || s[9] != european.Fck) return false;
            // Inspect contents on each call: SetStress/SetStrain can mutate a table in place.
            for (int i = 1; i <= 8; i++)
            {
                double r = i / 8.0;
                if (Math.Abs(e[i] - peak * r) > 1e-13 * Math.Abs(peak) ||
                    Math.Abs(s[i] - european.Fck * r * (2 - r)) > 1e-13 * Math.Abs(european.Fck) ||
                    double.IsNaN(e[i]) || double.IsNaN(s[i])) return false;
            }
            law = new ConcreteParabolaRectangleLaw(ultimate, peak, european.Fck);
            return true;
        }
    }
}
