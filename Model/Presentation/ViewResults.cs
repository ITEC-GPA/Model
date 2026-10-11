using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;

namespace GPC.Model.Presentation
{
    /// <summary>
    /// One value of a field. Beams: <see cref="Station"/> in [0, 1] from node I and the side at a discontinuity. Plates: the point kind and, for a
    /// value extrapolated by the solver at an element node, the <see cref="Node"/>. Node fields: <see cref="Element"/> is the node.
    /// </summary>
    public readonly struct ViewSample
    {
        internal ViewSample(int element, double value, double? station, SectionSide side, int? node, ShellResultPointKind pointKind)
        { Element = element; Value = value; Station = station; Side = side; Node = node; PointKind = pointKind; }
        public int Element { get; }
        public double Value { get; }
        public double? Station { get; }
        public SectionSide Side { get; }
        public int? Node { get; }
        public ShellResultPointKind PointKind { get; }
    }

    /// <summary>The (case, family, component) of an available field.</summary>
    public readonly struct ViewFieldKey : IEquatable<ViewFieldKey>
    {
        public ViewFieldKey(string caseName, ViewFamily family, string component) { Case = caseName; Family = family; Component = component; }
        public string Case { get; }
        public ViewFamily Family { get; }
        public string Component { get; }
        public bool Equals(ViewFieldKey other) => Case == other.Case && Family == other.Family && Component == other.Component;
        public override bool Equals(object obj) => obj is ViewFieldKey other && Equals(other);
        public override int GetHashCode() { unchecked { return ((Case?.GetHashCode() ?? 0) * 397 ^ (int)Family) * 397 ^ (Component?.GetHashCode() ?? 0); } }
        public override string ToString() => Case + " · " + Family + " · " + Component;
    }

    /// <summary>The values of one component of one case, with its unit and axes.</summary>
    public sealed class ViewField
    {
        internal ViewField(string dataset, ViewFieldKey key, string unit, string axes, ViewSample[] samples)
        { Dataset = dataset; Case = key.Case; Family = key.Family; Component = key.Component; Unit = unit; Axes = axes; Samples = Array.AsReadOnly(samples); }
        public string Dataset { get; }
        public string Case { get; }
        public ViewFamily Family { get; }
        public string Component { get; }
        /// <summary>The unit of the values, in the units of the model: N, N·mm, N/mm, N·mm/mm, mm or rad.</summary>
        public string Unit { get; }
        /// <summary>The axes of the components: "section" (beam section axes), "element" (plate element axes), "global" or "local" (other
        /// node axes, given by the source program).</summary>
        public string Axes { get; }
        public IReadOnlyList<ViewSample> Samples { get; }
    }

    /// <summary>
    /// The results of one dataset of the model, read once (<see cref="ModelView.Results"/>). Supported values: beam internal forces N, V1, V2,
    /// T, M1, M2 at the stations, in the section axes, on the positive section face; plate forces per unit width Fxx, Fyy, Fxy, Fxz, Fyz, Mxx, Myy,
    /// Mxy in the element axes; node displacements D1, D2, D3, R1, R2, R3 and support reactions Fx, Fy, Fz, Mx, My, Mz. The values are those of
    /// the model, in the GPC conventions applied by the Converter: nothing is averaged, interpolated, combined or transformed. Values of construction
    /// stages, steps, modes, moving loads and of the other result types are not read: <see cref="Skipped"/> counts them.
    /// </summary>
    public sealed class ViewResults
    {
        private static readonly string[] BeamComponents = { "N", "V1", "V2", "T", "M1", "M2" };
        private static readonly string[] BeamUnits = { "N", "N", "N", "N·mm", "N·mm", "N·mm" };
        private static readonly string[] PlateComponents = { "Fxx", "Fyy", "Fxy", "Fxz", "Fyz", "Mxx", "Myy", "Mxy" };
        private static readonly string[] PlateUnits = { "N/mm", "N/mm", "N/mm", "N/mm", "N/mm", "N·mm/mm", "N·mm/mm", "N·mm/mm" };
        private static readonly string[] DisplacementComponents = { "D1", "D2", "D3", "R1", "R2", "R3" };
        private static readonly string[] DisplacementUnits = { "mm", "mm", "mm", "rad", "rad", "rad" };
        private static readonly string[] ReactionComponents = { "Fx", "Fy", "Fz", "Mx", "My", "Mz" };
        private static readonly string[] ReactionUnits = { "N", "N", "N", "N·mm", "N·mm", "N·mm" };

        // Columns of one (case, kind of value): the locations and the components of every sample.
        private sealed class Table
        {
            internal readonly List<int> Elements = new List<int>();
            internal readonly List<double?> Stations = new List<double?>();
            internal readonly List<SectionSide> Sides = new List<SectionSide>();
            internal readonly List<int?> Nodes = new List<int?>();
            internal readonly List<ShellResultPointKind> Kinds = new List<ShellResultPointKind>();
            internal readonly List<double>[] Values;
            internal bool Global = true;
            internal Table(int components) { Values = Enumerable.Range(0, components).Select(_ => new List<double>()).ToArray(); }
            internal void Add(int element, double? station, SectionSide side, int? node, ShellResultPointKind kind, double[] values)
            {
                Elements.Add(element); Stations.Add(station); Sides.Add(side); Nodes.Add(node); Kinds.Add(kind);
                for (int i = 0; i < values.Length; i++) Values[i].Add(values[i]);
            }
        }

        private enum Kind { Beam, Plate, Displacement, Reaction }
        private readonly Dictionary<Tuple<string, Kind>, Table> _tables = new Dictionary<Tuple<string, Kind>, Table>();

        private ViewResults(string dataset) { Dataset = dataset; }

        public string Dataset { get; }
        /// <summary>The cases with values, in the order of the model.</summary>
        public IReadOnlyList<string> Cases { get; private set; }
        /// <summary>The fields with values.</summary>
        public IReadOnlyList<ViewFieldKey> Fields { get; private set; }
        /// <summary>The values of the dataset not read (stages, steps, modes, moving loads, other result types), by result type.</summary>
        public IReadOnlyDictionary<string, int> Skipped { get; private set; }

        /// <summary>The values of a field; null if the dataset has none.</summary>
        public ViewField Field(string caseName, ViewFamily family, string component)
        {
            var key = new ViewFieldKey(caseName, family, component);
            foreach (Kind kind in Enum.GetValues(typeof(Kind)))
            {
                if (FamilyOf(kind) != family) continue;
                int column = Array.IndexOf(Components(kind), component);
                if (column < 0 || !_tables.TryGetValue(Tuple.Create(caseName, kind), out var table)) continue;
                var samples = new ViewSample[table.Elements.Count];
                for (int i = 0; i < samples.Length; i++)
                    samples[i] = new ViewSample(table.Elements[i], table.Values[column][i], table.Stations[i], table.Sides[i], table.Nodes[i], table.Kinds[i]);
                string axes = kind == Kind.Beam ? "section" : kind == Kind.Plate ? "element" : table.Global ? "global" : "local";
                return new ViewField(Dataset, key, Units(kind)[column], axes, samples);
            }
            return null;
        }

        private static ViewFamily FamilyOf(Kind kind) => kind == Kind.Beam ? ViewFamily.Beam : kind == Kind.Plate ? ViewFamily.Plate : ViewFamily.Node;
        private static string[] Components(Kind kind) => kind == Kind.Beam ? BeamComponents : kind == Kind.Plate ? PlateComponents
            : kind == Kind.Displacement ? DisplacementComponents : ReactionComponents;
        private static string[] Units(Kind kind) => kind == Kind.Beam ? BeamUnits : kind == Kind.Plate ? PlateUnits
            : kind == Kind.Displacement ? DisplacementUnits : ReactionUnits;

        // One value read from an element, before it enters the columns. Skipped holds the result type of a value not read.
        private sealed class Row
        {
            internal string Case; internal Kind Kind; internal double? Station; internal SectionSide Side; internal int? Node;
            internal ShellResultPointKind PointKind; internal double[] Values; internal bool Global = true; internal string Skipped;
        }

        // The values of one element: decoded in parallel, without keeping the decoded objects.
        private static List<Row> Rows(Element element, string dataset)
        {
            var rows = new List<Row>();
            foreach (var result in element.PeekResults(dataset))
            {
                if (result?.Results == null) continue;
                foreach (var r in result.Results)
                {
                    var state = r?.State;
                    if (state == null || state.DatasetId != dataset) continue;
                    if (state.Phase != null || state.Step != null || state.Mode != null || state.MovingLoadPosition != null || r.Case == null)
                    { rows.Add(new Row { Skipped = r.GetType().Name }); continue; }
                    var row = new Row { Case = r.Case.Name, Side = SectionSide.Unspecified, PointKind = ShellResultPointKind.Unknown };
                    if (r is StationResultBeamForces beam && element is BeamElement)
                    {
                        var f = beam.ResultBeamForces;
                        row.Kind = Kind.Beam; row.Station = beam.ParametricDistance; row.Side = beam.Side; row.Values = new[] { f.N, f.V1, f.V2, f.T, f.M1, f.M2 };
                    }
                    else if (r is PointResultPlateForces plate && element is AreaElement)
                    {
                        var f = plate.Forces;
                        row.Kind = Kind.Plate; row.Node = plate.SourceNodeId; row.PointKind = plate.PointKind;
                        row.Values = new[] { f.Fxx, f.Fyy, f.Fxy, f.Fxz, f.Fyz, f.Mxx, f.Myy, f.Mxy };
                    }
                    else if (r is NodeResultDisplacement displacement && element is NodeElement)
                    {
                        var d = displacement.ResultDisplacement;
                        row.Kind = Kind.Displacement; row.Global = IsGlobal(d.CoordinateSystem); row.Values = new[] { d.D1, d.D2, d.D3, d.R1, d.R2, d.R3 };
                    }
                    else if (r is NodeResultForces force && element is NodeElement && force.Kind == NodalForceKind.SupportReaction)
                    {
                        row.Kind = Kind.Reaction; row.Global = IsGlobal(force.ResultBeamForces.CoordinateSystem);
                        row.Values = new[] { force.Fx, force.Fy, force.Fz, force.Mx, force.My, force.Mz };
                    }
                    else row = new Row { Skipped = r.GetType().Name };
                    rows.Add(row);
                }
            }
            return rows;
        }

        internal static ViewResults Read(Models.Model model, string dataset, CancellationToken token)
        {
            var view = new ViewResults(dataset); var skipped = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var elements = model.AllElements.Where(e => e.ContainsResultsOf(dataset)).ToArray();
            var rows = new List<Row>[elements.Length];
            System.Threading.Tasks.Parallel.For(0, elements.Length, new System.Threading.Tasks.ParallelOptions { CancellationToken = token },
                i => rows[i] = Rows(elements[i], dataset));
            // The columns in the order of the elements and of their results, whatever the order of the parallel decoding.
            for (int i = 0; i < elements.Length; i++)
                foreach (var row in rows[i])
                {
                    if (row.Skipped != null) { skipped[row.Skipped] = skipped.TryGetValue(row.Skipped, out var n) ? n + 1 : 1; continue; }
                    var key = Tuple.Create(row.Case, row.Kind);
                    if (!view._tables.TryGetValue(key, out var table)) view._tables.Add(key, table = new Table(Components(row.Kind).Length));
                    table.Global &= row.Global;
                    table.Add(elements[i].Id, row.Station, row.Side, row.Node, row.PointKind, row.Values);
                }

            var order = model.LoadCases.Keys.Concat(model.Combinations.Keys).Select((name, i) => new { name, i }).ToDictionary(x => x.name, x => x.i, StringComparer.Ordinal);
            int Order(string name) => order.TryGetValue(name, out var i) ? i : int.MaxValue;
            view.Cases = view._tables.Keys.Select(k => k.Item1).Distinct().OrderBy(Order).ThenBy(n => n, StringComparer.Ordinal).ToList().AsReadOnly();
            view.Fields = view._tables.Keys.OrderBy(k => Order(k.Item1)).ThenBy(k => k.Item1, StringComparer.Ordinal).ThenBy(k => k.Item2)
                .SelectMany(k => Components(k.Item2).Select(c => new ViewFieldKey(k.Item1, FamilyOf(k.Item2), c))).ToList().AsReadOnly();
            view.Skipped = new Dictionary<string, int>(skipped);
            return view;
        }

        private static bool IsGlobal(Geometry.CoordinateSystem axes)
        {
            if (axes == null) return true;
            const double tolerance = 1e-9;
            return Math.Abs(axes.V1.X - 1) <= tolerance && Math.Abs(axes.V1.Y) <= tolerance && Math.Abs(axes.V1.Z) <= tolerance
                && Math.Abs(axes.V2.X) <= tolerance && Math.Abs(axes.V2.Y - 1) <= tolerance && Math.Abs(axes.V2.Z) <= tolerance
                && Math.Abs(axes.V3.X) <= tolerance && Math.Abs(axes.V3.Y) <= tolerance && Math.Abs(axes.V3.Z - 1) <= tolerance;
        }
    }
}
