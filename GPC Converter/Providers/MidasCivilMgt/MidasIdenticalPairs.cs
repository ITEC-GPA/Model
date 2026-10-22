using System;
using System.Linq;

namespace GPC.Converter.MidasCivil
{
    /// <summary>Native Civil NX 9.5.5 DBUSER 2C/2L dimensions, shared by JSON and MCT.
    /// H,B,tw,tf,C describe ONE component and the clear back-to-back gap. No principal-axis sorting.</summary>
    internal static class MidasIdenticalPairs
    {
        internal static IdenticalSectionPair Definition(string shape, double[] size, double lengthToMm)
        {
            if (shape != "2L" && shape != "2C") throw new ArgumentException("Not an identical C/L pair.");
            if (size == null || size.Length < 5 || size.Length > 10 || size.Any(v => double.IsNaN(v) || double.IsInfinity(v))
                || size.Take(4).Any(v => v <= 0) || size[4] < 0 || size.Skip(5).Any(v => v != 0)
                || !(lengthToMm > 0) || double.IsInfinity(lengthToMm))
                throw new ArgumentException("Explicit finite H,B,tw,tf,C are required; extra dimensions must be zero.");
            var d = size.Select(v => v * lengthToMm).ToArray();
            if (d.Any(double.IsInfinity) || d[1] <= d[2] || d[0] <= (shape == "2C" ? 2 : 1) * d[3])
                throw new ArgumentException("Invalid component plate dimensions.");
            return new IdenticalSectionPair { ComponentShape = shape == "2L" ? SectionShapeKind.Angle : SectionShapeKind.Channel,
                ComponentDimensions = shape == "2L" ? new[] { d[0], d[1], d[2], d[3] } : new[] { d[0], d[1], d[2], d[3], d[1], d[3] },
                Mirror = SectionPairMirror.Left, Gap = d[4] };
        }
    }
}
