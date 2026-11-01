using System;
using System.Collections.Generic;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;

namespace GPC.Model.Results.Processing
{
    /// <summary>
    /// Principal components of the plate actions, in the GPC convention of the samples (positive section face, shell tensor; N, mm):
    /// Nmax and Nmin of the membrane forces (Nxx, Nyy, Nxy), Mmax and Mmin of the moments (Mxx, Myy, Mxy), with
    /// max, min = (xx + yy) / 2 ± √(((xx − yy) / 2)² + xy²). Invariant under a rotation of the in-plane axes. They are always taken from one
    /// state: never summed over load cases and never taken from extremes of the local components that do not occur together.
    /// </summary>
    public static class PlatePrincipals
    {
        /// <summary>The order of the components of <see cref="PlatePrincipalResult.Components"/>.</summary>
        public static IReadOnlyList<string> Components { get; } = new[] { "Nmax", "Nmin", "Mmax", "Mmin" };

        /// <summary>The larger and the smaller principal value of the symmetric tensor (xx, yy, xy).</summary>
        public static void Of(double xx, double yy, double xy, out double max, out double min)
        {
            double centre = (xx + yy) / 2, half = (xx - yy) / 2, radius = Math.Sqrt(half * half + xy * xy);
            max = centre + radius; min = centre - radius;
        }

        /// <summary>Nmax, Nmin, Mmax, Mmin of a plate vector in the order of the combination (Nxx, Nyy, Nxy, Qx, Qy, Mxx, Myy, Mxy).</summary>
        internal static double[] Of(double[] vector)
        {
            var p = new double[4];
            Of(vector[0], vector[1], vector[2], out p[0], out p[1]);
            Of(vector[5], vector[6], vector[7], out p[2], out p[3]);
            return p;
        }
    }

    /// <summary>One principal component at a plate location of a combination: its smallest and largest value over the alternatives (the same
    /// value for a linear combination), each with the id of its concomitant state (as the <c>ConcomitantStateId</c> of the samples) and the
    /// complete local vector of that state (Nxx, Nyy, Nxy, Qx, Qy, Mxx, Myy, Mxy, in the frame of the location). Ties keep the first alternative.</summary>
    public sealed class PlatePrincipalExtreme
    {
        internal PlatePrincipalExtreme(string component, double minimum, string minimumState, double[] minimumVector, double maximum, string maximumState, double[] maximumVector)
        {
            Component = component; Minimum = minimum; MinimumStateId = minimumState; MinimumState = minimumVector;
            Maximum = maximum; MaximumStateId = maximumState; MaximumState = maximumVector;
        }
        public string Component { get; }
        public double Minimum { get; }
        public string MinimumStateId { get; }
        public IReadOnlyList<double> MinimumState { get; }
        public double Maximum { get; }
        public string MaximumStateId { get; }
        public IReadOnlyList<double> MaximumState { get; }
    }

    /// <summary>The principal components of one plate location of a combination (<see cref="CombinationRebuildCache.PlatePrincipals"/>): the
    /// element, the static sample that gives the location (its <c>SourceNodeId</c> and point) and the extremes in the order of
    /// <see cref="PlatePrincipals.Components"/>.</summary>
    public sealed class PlatePrincipalResult
    {
        internal PlatePrincipalResult(Element owner, ResultLocation location, PlatePrincipalExtreme[] components)
        {
            Owner = owner; Location = location; Components = components;
        }
        public Element Owner { get; }
        public ResultLocation Location { get; }
        public IReadOnlyList<PlatePrincipalExtreme> Components { get; }
    }
}
