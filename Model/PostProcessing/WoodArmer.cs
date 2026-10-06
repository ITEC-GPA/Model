using System;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>Design moments for orthogonal reinforcement along the plate x, y axes, per face, in N mm/mm: each value is the moment the
    /// reinforcement of that face must resist (positive, stretching that face); zero when the face needs no reinforcement.</summary>
    public sealed class WoodArmerMoments
    {
        public double MxPositiveFace { get; internal set; }
        public double MyPositiveFace { get; internal set; }
        public double MxNegativeFace { get; internal set; }
        public double MyNegativeFace { get; internal set; }
    }

    /// <summary>Wood–Armer moments (Wood 1968, Armer 1968) for reinforcement parallel to the plate axes, from the GPC resultants:
    /// Mxx, Myy, Mxy are first moments of the stresses through the thickness, so a positive Mxx stretches the +z face.
    /// For each face, with m the moments stretching that face: Mx* = mx + |mxy|, My* = my + |mxy|; when Mx* &lt; 0, Mx* = 0 and
    /// My* = my + mxy²/|mx|; when My* &lt; 0, My* = 0 and Mx* = mx + mxy²/|my|; negative results are zero (no reinforcement).
    /// Membrane forces and skew reinforcement are not included.</summary>
    public static class WoodArmer
    {
        public static WoodArmerMoments Compute(double mxx, double myy, double mxy)
        {
            NumericGuard.Finite(mxx, nameof(mxx)); NumericGuard.Finite(myy, nameof(myy)); NumericGuard.Finite(mxy, nameof(mxy));
            var positive = Face(mxx, myy, mxy); var negative = Face(-mxx, -myy, mxy);
            return new WoodArmerMoments { MxPositiveFace = positive.Mx, MyPositiveFace = positive.My, MxNegativeFace = negative.Mx, MyNegativeFace = negative.My };
        }

        /// <summary>The moments of a plate result in its own axes.</summary>
        public static WoodArmerMoments Compute(PointResultPlateForces sample)
        {
            if (sample?.Forces == null) throw new ArgumentNullException(nameof(sample));
            return Compute(sample.Forces.Mxx, sample.Forces.Myy, sample.Forces.Mxy);
        }

        private static (double Mx, double My) Face(double mx, double my, double mxy)
        {
            double twist = Math.Abs(mxy), x = mx + twist, y = my + twist;
            if (x < 0 && y < 0) return (0, 0);
            if (x < 0) return (0, Math.Max(0, my + mxy * mxy / Math.Abs(mx)));
            if (y < 0) return (Math.Max(0, mx + mxy * mxy / Math.Abs(my)), 0);
            return (x, y);
        }
    }
}
