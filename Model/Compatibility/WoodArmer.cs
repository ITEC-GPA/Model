using System;
using GPC.Model.Results.Locations;

namespace GPC.Model.Compatibility
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

    /// <summary>Compatibility facade. Wood-Armer is a design policy, never part of physical result normalization.</summary>
    public static class WoodArmer
    {
        public static WoodArmerMoments Compute(double mxx, double myy, double mxy) => Design.Plate.WoodArmerDesign.Compute(mxx, myy, mxy);
        public static WoodArmerMoments Compute(PointResultPlateForces sample) => Design.Plate.WoodArmerDesign.Compute(sample);
    }
}