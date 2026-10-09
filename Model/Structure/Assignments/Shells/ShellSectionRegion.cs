using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;

namespace GPC.Model.PostProcessing
{
    public enum ShellRegionLocation { Outside, Boundary, Inside }

    /// <summary>A convex polygon in assignment-local x/y, in mm. Concave zones must be partitioned.
    /// Boundary ownership is explicit at resolution time, never inferred from list order.</summary>
    [Serializable]
    public sealed class ShellSectionRegion : ISerializable
    {
        public const double Tolerance = 1e-7;
        private readonly Point2d[] _vertices;
        public IReadOnlyList<Point2d> Vertices => Array.AsReadOnly(_vertices.Select(p => new Point2d(p.X, p.Y)).ToArray());
        public ShellSectionRegion(IEnumerable<Point2d> vertices)
        {
            _vertices = vertices?.Select(p => p == null ? throw new ArgumentException("NullRegionVertex") : new Point2d(p.X, p.Y)).ToArray()
                ?? throw new ArgumentNullException(nameof(vertices));
            Validate();
        }
        private static double Cross(Point2d a, Point2d b, Point2d p) => (b.X-a.X)*(p.Y-a.Y)-(b.Y-a.Y)*(p.X-a.X);
        private static double Distance(Point2d a, Point2d b) => Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y));
        private void Validate()
        {
            if (_vertices.Length < 3) throw new ArgumentException("InvalidShellRegion");
            foreach (var p in _vertices) { NumericGuard.Finite(p.X, "region x"); NumericGuard.Finite(p.Y, "region y"); }
            double winding = 0;
            for (int i=0; i<_vertices.Length; i++)
            {
                var a=_vertices[i]; var b=_vertices[(i+1)%_vertices.Length]; double length=Distance(a,b);
                if (length <= Tolerance) throw new ArgumentException("DegenerateShellRegionEdge");
                for (int j=0; j<_vertices.Length; j++)
                {
                    if (j==i || j==(i+1)%_vertices.Length) continue;
                    double c=Cross(a,b,_vertices[j])/length;
                    if (Math.Abs(c)<=Tolerance) continue;
                    if (winding==0) winding=Math.Sign(c);
                    if (c*winding < -Tolerance) throw new ArgumentException("ConvexShellRegionRequired");
                }
            }
            if (winding==0) throw new ArgumentException("DegenerateShellRegion");
        }
        public ShellRegionLocation Locate(Point2d point)
        {
            if (point==null) throw new ArgumentNullException(nameof(point));
            NumericGuard.Finite(point.X,"region x"); NumericGuard.Finite(point.Y,"region y");
            bool positive=false, negative=false, boundary=false;
            for(int i=0;i<_vertices.Length;i++)
            {
                var a=_vertices[i]; var b=_vertices[(i+1)%_vertices.Length];
                double d=Cross(a,b,point)/Distance(a,b);
                positive |= d>Tolerance; negative |= d < -Tolerance; boundary |= Math.Abs(d)<=Tolerance;
            }
            return positive && negative ? ShellRegionLocation.Outside : boundary ? ShellRegionLocation.Boundary : ShellRegionLocation.Inside;
        }
        internal static bool Overlap(Point2d[] a, Point2d[] b)
        {
            foreach(var polygon in new[]{a,b})
                for(int i=0;i<polygon.Length;i++)
                {
                    var p=polygon[i]; var q=polygon[(i+1)%polygon.Length]; double length=Distance(p,q);
                    var x=a.Select(v=>Cross(p,q,v)/length).ToArray(); var y=b.Select(v=>Cross(p,q,v)/length).ToArray();
                    if (Math.Min(x.Max(),y.Max())-Math.Max(x.Min(),y.Min())<=Tolerance) return false;
                }
            return true;
        }
        private ShellSectionRegion(SerializationInfo info, StreamingContext context)
            : this(ReadVertices((double[])info.GetValue("Vertices",typeof(double[])))) { }
        private static IEnumerable<Point2d> ReadVertices(double[] values)
        {
            if (values==null || values.Length%2!=0) throw new SerializationException("InvalidShellRegionVertices");
            for(int i=0;i<values.Length;i+=2) yield return new Point2d(values[i],values[i+1]);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context) => info.AddValue("Vertices",_vertices.SelectMany(p=>new[]{p.X,p.Y}).ToArray());
    }
}
