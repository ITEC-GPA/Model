using System;
using System.Collections.Generic;

namespace GPC.Model.Geotechnics.Seepage
{
    /// <summary>Stationary plane seepage. Coordinates and total head mm, z upwards;
    /// conductivity mm/s, flux mm/s, discharges per out-of-plane length mm²/s.
    /// Triangles form a conforming, non-overlapping domain. Unspecified boundaries are impermeable.</summary>
    public sealed class SeepageModel
    {
        public string CaseId { get; set; } = "";
        public IReadOnlyList<SeepageNode> Nodes { get; set; } = Array.Empty<SeepageNode>();
        public IReadOnlyList<SeepageTriangle> Triangles { get; set; } = Array.Empty<SeepageTriangle>();
        public IReadOnlyList<SeepageMaterial> Materials { get; set; } = Array.Empty<SeepageMaterial>();
        public IReadOnlyList<SeepageHead> Heads { get; set; } = Array.Empty<SeepageHead>();
        public IReadOnlyList<SeepageFlux> Fluxes { get; set; } = Array.Empty<SeepageFlux>();
        /// <summary>Boundary node indices: atmospheric discharge only, h≤z, outward flux≥0.
        /// Not a prescribed water table. Each connected domain also requires a permanent head.</summary>
        public IReadOnlyList<int> SeepageFaceNodes { get; set; } = Array.Empty<int>();
        public IReadOnlyList<double> InitialHeads { get; set; } = Array.Empty<double>();
    }
    public readonly struct SeepageNode
    {
        public double X { get; }
        public double Z { get; }
        public SeepageNode(double x, double z) { X = x; Z = z; }
    }
    public readonly struct SeepageTriangle
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
        public int Material { get; }
        public SeepageTriangle(int a, int b, int c, int material = 0) { A = a; B = b; C = c; Material = material; }
    }
    /// <summary>Symmetric positive definite saturated conductivity tensor. A null curve means
    /// constant conductivity (confined saturated analysis); a curve enables variably saturated flow.</summary>
    public sealed class SeepageMaterial
    {
        public string Name { get; }
        public double Kxx { get; }
        public double Kxz { get; }
        public double Kzz { get; }
        public VanGenuchtenCurve Unsaturated { get; }
        public string Source { get; }
        public SeepageMaterial(string name, double kxx, double kzz, double kxz, string source, VanGenuchtenCurve unsaturated = null)
        { Name = name; Kxx = kxx; Kxz = kxz; Kzz = kzz; Source = source; Unsaturated = unsaturated; }
    }
    /// <summary>Van Genuchten–Mualem: α in 1/mm, n&gt;1, m=1−1/n; pore connectivity exponent 0.5.
    /// Effective saturation only; water content is unnecessary for stationary flow. No hysteresis.</summary>
    public sealed class VanGenuchtenCurve
    {
        public double Alpha { get; }
        public double N { get; }
        public VanGenuchtenCurve(double alpha, double n) { Alpha = alpha; N = n; }
    }
    public readonly struct SeepageHead
    {
        public int Node { get; }
        public double Head { get; }
        public SeepageHead(int node, double head) { Node = node; Head = head; }
    }
    /// <summary>Uniform inflow on an exterior edge, mm/s, positive into the domain.
    /// A negative value extracts water. Do not duplicate an edge.</summary>
    public readonly struct SeepageFlux
    {
        public int A { get; }
        public int B { get; }
        public double Inflow { get; }
        public SeepageFlux(int a, int b, double inflow) { A = a; B = b; Inflow = inflow; }
    }
}
