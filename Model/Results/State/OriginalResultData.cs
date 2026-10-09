using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class OriginalResultData
    {
        public double?[] Values { get; set; }
        public GPC.Geometry.CoordinateSystem Axes { get; set; }
        public string ComponentOrder { get; set; }
        public string Convention { get; set; }
        public string ReaderVersion { get; set; }
        public double ForceToN { get; set; }
        public double LengthToMm { get; set; }
        public double MomentToNmm { get; set; }
        public double DenominatorLengthToMm { get; set; }
        public double AngleToRad { get; set; }
        public OriginalResultData Copy()
        {
            var copy = (OriginalResultData)MemberwiseClone();
            copy.Values = Values == null ? null : (double?[])Values.Clone();
            copy.Axes = Axes == null ? null : ActionTransformations.AtPoint(Axes, Axes.Origin);
            return copy;
        }
    }
}
