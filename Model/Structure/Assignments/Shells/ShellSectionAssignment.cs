using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.ElementProperties;

namespace GPC.Model.PostProcessing
{
    /// <summary>A reusable plate section placed in a physical coordinate system. Null Region covers the whole element.</summary>
    [Serializable]
    public sealed class ShellSectionAssignment
    {
        public string Id { get; set; }
        public PlateProperty Property { get; set; }
        public CoordinateSystem Axes { get; set; }
        public ShellSectionRegion Region { get; set; }
        internal Point2d Project(Point3d point)
        {
            PostProcessing.Axes.Validate(Axes);
            var delta=point-Axes.Origin;
            if (Math.Abs(PostProcessing.Axes.Dot(delta,Axes.V3))>ShellSectionRegion.Tolerance) throw new ArgumentException("ShellSectionPlaneMismatch");
            return new Point2d(PostProcessing.Axes.Dot(delta,Axes.V1),PostProcessing.Axes.Dot(delta,Axes.V2));
        }
        internal ShellRegionLocation Locate(Point3d point)
        { var local=Project(point); return Region==null ? ShellRegionLocation.Inside : Region.Locate(local); }
        internal Point3d[] GlobalVertices() => Region.Vertices.Select(p=>Axes.Origin+Axes.V1*p.X+Axes.V2*p.Y).ToArray();
    }
}
