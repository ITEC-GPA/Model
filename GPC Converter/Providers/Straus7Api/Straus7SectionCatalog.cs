// Source: installed Straus7 R3 API Manual, printed pp. 1003-1009.
using System;
using System.Linq;
namespace GPC.Converter.Straus7
{
    public static class Straus7SectionCatalog
    {
        public static readonly System.Collections.Generic.IReadOnlyList<Straus7SectionShape> All = Array.AsReadOnly(new[]
        {
            new Straus7SectionShape("CircularSolid", "Circular Solid", 1, false, new[] { "D" }, new[] { 0 }),
            new Straus7SectionShape("CircularHollow", "Circular Hollow", 2, false, new[] { "D", "T" }, new[] { 0, 3 }),
            new Straus7SectionShape("RectangleSolid", "Rectangle Solid", 3, false, new[] { "B", "D" }, new[] { 0, 1 }),
            new Straus7SectionShape("RectangleHollow", "Rectangle Hollow", 4, false, new[] { "B", "D", "T1", "T2" }, new[] { 0, 1, 3, 4 }),
            new Straus7SectionShape("LipChannel", "Lipped Channel", 5, false, new[] { "B", "D", "L", "T1", "T2", "T3" }, new[] { 0, 1, 2, 3, 4, 5 }),
            new Straus7SectionShape("TopHat", "Top Hat", 6, false, new[] { "B", "D", "L", "T1", "T2", "T3" }, new[] { 0, 1, 2, 3, 4, 5 }),
            new Straus7SectionShape("I", "I", 7, false, new[] { "B1", "B2", "D", "T1", "T2", "T3" }, new[] { 0, 1, 2, 3, 4, 5 }),
            new Straus7SectionShape("T", "T", 8, false, new[] { "B", "D", "L", "T1", "T2", "T3" }, new[] { 0, 1, 2, 3, 4, 5 }),
            new Straus7SectionShape("Angle", "Angle", 9, false, new[] { "B", "D", "T1", "T2" }, new[] { 0, 1, 3, 4 }),
            new Straus7SectionShape("Z", "Z", 10, false, new[] { "B1", "B2", "D", "T1", "T2", "T3" }, new[] { 0, 1, 2, 3, 4, 5 }),
            new Straus7SectionShape("TrapezoidSolid", "Trapezoid Solid", 12, false, new[] { "B1", "B2", "D" }, new[] { 0, 1, 2 }),
            new Straus7SectionShape("TrapezoidHollow", "Trapezoid Hollow", 13, false, new[] { "B1", "B2", "D", "T1", "T2" }, new[] { 0, 1, 2, 3, 4 }),
            new Straus7SectionShape("TriangleSolid", "Triangle Solid", 14, false, new[] { "B", "D" }, new[] { 0, 1 }),
            new Straus7SectionShape("TriangleHollow", "Triangle Hollow", 15, false, new[] { "B", "D", "T1", "T2" }, new[] { 0, 1, 3, 4 }),
            new Straus7SectionShape("Cruciform", "Cruciform", 16, false, new[] { "B", "D", "T1", "T2" }, new[] { 0, 1, 3, 4 }),
            new Straus7SectionShape("BglRectangle", "BGL Hollow Rectangle", 1, true, new[] { "D", "B", "tw", "tf", "Ri", "Ro" }, new[] { 0, 1, 2, 3, 4, 5 }),
            new Straus7SectionShape("BglI", "BGL I", 2, true, new[] { "D", "B1", "B2", "tw", "tf1", "tf2", "Rr1", "Rr2", "Rt1", "Rt2", "Rt3", "Rt4", "alpha1", "alpha2" }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }),
            new Straus7SectionShape("BglChannel", "BGL Channel", 3, true, new[] { "D", "B1", "B2", "tw", "tf1", "tf2", "Rr1", "Rr2", "Rh1", "Rh2", "Rt1", "Rt2", "Rt3", "Rt4", "alpha1", "alpha2" }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }),
            new Straus7SectionShape("BglT", "BGL T", 4, true, new[] { "D", "B", "tw", "tf", "Rr", "Rt1", "Rt2", "Rt3", "alpha1", "alpha2" }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
            new Straus7SectionShape("BglAngle", "BGL Angle", 5, true, new[] { "D", "B", "tw", "tf", "Rr", "Rh", "Rt1", "Rt2", "Rt3", "Rt4", "alpha1", "alpha2" }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }),
            new Straus7SectionShape("BglBulb", "BGL Bulb Flat", 6, true, new[] { "D", "B", "t", "Rr", "Rh", "Rt1", "Rt2", "Rt3", "alpha" }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }),
        });
        public static Straus7SectionShape Get(string key) => All.FirstOrDefault(s => s.Key == key) ?? throw new ArgumentException("Sezione Straus sconosciuta: " + key);
    }
    public sealed class Straus7SectionShape
    {
        public string Key { get; }
        public string Name { get; }
        public int NativeId { get; }
        public bool Bgl { get; }
        private readonly string[] dimensions;
        private readonly int[] slots;
        public string[] Dimensions => (string[])dimensions.Clone();
        internal Straus7SectionShape(string key,string name,int id,bool bgl,string[] dimensions,int[] slots)
        {Key=key;Name=name;NativeId=id;Bgl=bgl;this.dimensions=dimensions;this.slots=slots;}
        public Straus7SectionDefinition Create(double[] values)
        {
            if(values==null||values.Length!=dimensions.Length)throw new ArgumentException("InvalidNamedSectionDimensions");
            var native=new double[Bgl?16:6];
            for(int i=0;i<values.Length;i++)
            {
                if(double.IsNaN(values[i])||double.IsInfinity(values[i])||!dimensions[i].StartsWith("alpha",StringComparison.Ordinal)&&values[i]<0)
                    throw new ArgumentException("InvalidNamedSectionDimension: "+dimensions[i]);
                native[slots[i]]=values[i];
            }
            return Bgl?Straus7SectionDefinition.Bgl(NativeId,native):Straus7SectionDefinition.Standard(NativeId,native);
        }
    }

}
