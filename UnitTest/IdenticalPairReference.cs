namespace UnitTest;

// Independent rectangular-area integrals in the SOURCE geometric section x/y axes.
// No GPC section property is used to construct an expected number.
internal static class IdenticalPairReference
{
    internal record Rectangle(double X, double Y, double B, double H);
    internal record Properties(double Area, double X, double Y, double Ixx, double Iyy, double Ixy);
    internal static Properties Integrate(bool channel, double h, double b, double tw, double tf, double gap, int mirror)
    {
        var single = new List<Rectangle> { new(0, 0, b, tf), new(0, tf, tw, h - (channel ? 2 : 1) * tf) };
        if (channel) single.Add(new(0, h - tf, b, tf));
        var rectangles = new List<Rectangle>(single);
        foreach (var r in single)
            rectangles.Add(mirror switch
            {
                1 => r with { Y = 2 * h + gap - r.Y - r.H },
                2 => r with { Y = -gap - r.Y - r.H },
                3 => r with { X = -gap - r.X - r.B },
                4 => r with { X = 2 * b + gap - r.X - r.B },
                _ => throw new ArgumentOutOfRangeException(nameof(mirror))
            });
        double a = rectangles.Sum(r => r.B * r.H);
        double x = rectangles.Sum(r => r.B * r.H * (r.X + r.B / 2)) / a;
        double y = rectangles.Sum(r => r.B * r.H * (r.Y + r.H / 2)) / a;
        return new(a, x, y,
            rectangles.Sum(r => r.B * r.H * (r.H * r.H / 12 + Math.Pow(r.Y + r.H / 2 - y, 2))),
            rectangles.Sum(r => r.B * r.H * (r.B * r.B / 12 + Math.Pow(r.X + r.B / 2 - x, 2))),
            rectangles.Sum(r => r.B * r.H * (r.X + r.B / 2 - x) * (r.Y + r.H / 2 - y)));
    }
}
