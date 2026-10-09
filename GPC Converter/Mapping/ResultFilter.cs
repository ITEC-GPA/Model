using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.PostProcessing;

namespace GPC.Converter
{
    /// <summary>The results to read for a model already imported from a solver, chosen by the user after the import: elements by group,
    /// explicit element or property (section, thickness), result families, static cases and combinations. Resolved on the model into a
    /// <see cref="ResultReadPlan"/>, which the solver readers request element by element and case by case.</summary>
    public sealed class ResultFilter
    {
        /// <summary>Groups (with their subgroups when IncludeDescendants), explicit elements and the result families: Beam = beam forces,
        /// Shell = plate forces, Node = displacements and support reactions. No groups and no elements select every element of the families.</summary>
        public ElementSelection Elements { get; set; } = new ElementSelection { Families = new[] { EntityFamily.Node, EntityFamily.Beam, EntityFamily.Shell } };
        /// <summary>With Node results, the nodes of the selected beams and plates are read as well as the selected nodes.</summary>
        public bool IncludeElementNodes { get; set; } = true;
        /// <summary>Names of beam and plate properties: when given, only elements with one of them are read, and only nodes connected to them.</summary>
        public string[] Properties { get; set; } = new string[0];
        /// <summary>Static load cases by Model name.</summary>
        public string[] StaticCases { get; set; } = new string[0];
        /// <summary>Combinations (source definitions by name or id, or Model combinations by name): their static cases are read, and the
        /// combinations are rebuilt from them (<see cref="CombinationExpansion"/>, ResultAlgebra). No cases and no combinations read every static case.</summary>
        public string[] Combinations { get; set; } = new string[0];

        public ResultReadPlan Resolve(GPC.Model.Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (Elements == null || Properties == null || StaticCases == null || Combinations == null) throw new ArgumentException("InvalidResultFilter");
            var selected = Elements.Resolve(model);
            var properties = new HashSet<string>(Properties, StringComparer.Ordinal);
            foreach (var name in properties)
                if (!model.BeamProperties.Values.Any(p => p.Name == name) && !model.PlateProperties.Values.Any(p => p.Name == name)) throw new ArgumentException("UnknownProperty: " + name);
            bool Match(Element e) => properties.Count == 0 || e is BeamElement b && b.BeamProperty != null && properties.Contains(b.BeamProperty.Name)
                || e is AreaElement a && a.PlateProperty != null && properties.Contains(a.PlateProperty.Name);
            IEnumerable<NodeElement> NodesOf(Element e) => e is BeamElement b ? new[] { b.NodeI, b.NodeJ } : e is AreaElement a ? a.Nodes : Enumerable.Empty<NodeElement>();

            var beams = selected.OfType<BeamElement>().Where(Match).ToArray(); var shells = selected.OfType<AreaElement>().Where(Match).ToArray();
            var nodes = new Dictionary<int, NodeElement>();
            if (Elements.Families.Contains(EntityFamily.Node))
            {
                foreach (var n in selected.OfType<NodeElement>()) nodes[n.Id] = n;
                if (properties.Count != 0)
                {
                    var connected = new HashSet<int>(model.AllElements.Where(e => (e is BeamElement || e is AreaElement) && Match(e)).SelectMany(NodesOf).Select(n => n.Id));
                    foreach (var id in nodes.Keys.Where(id => !connected.Contains(id)).ToArray()) nodes.Remove(id);
                }
                if (IncludeElementNodes) foreach (var n in beams.Concat<Element>(shells).SelectMany(NodesOf)) nodes[n.Id] = n;
            }

            var cases = new List<string>(); var names = new HashSet<string>(StringComparer.Ordinal);
            void Add(string name, string reason)
            {
                if (!model.LoadCases.ContainsKey(name)) throw new ArgumentException("UnknownStaticCase: " + name + reason);
                if (names.Add(name)) cases.Add(name);
            }
            foreach (var name in StaticCases) Add(name, "");
            var definitions = CombinationExpansion.Definitions(model); var combinations = new List<string>();
            foreach (var name in Combinations.Distinct(StringComparer.Ordinal))
            {
                var definition = CombinationResults.Definition(model, definitions, name);
                if (definition != null) foreach (var c in CombinationExpansion.StaticCases(definitions, definition.Id)) Add(c, " (combination " + name + ")");
                else if (model.Combinations.TryGetValue(name, out var combination)) foreach (var c in combination.GetLoadCases()) Add(c.Name, " (combination " + name + ")");
                else throw new ArgumentException("UnknownCombination: " + name);
                combinations.Add(name);
            }
            if (StaticCases.Length == 0 && Combinations.Length == 0) foreach (var name in model.LoadCases.Keys) Add(name, "");

            var ordered = nodes.Values.OrderBy(n => n.Id).ToArray();
            return new ResultReadPlan { Beams = beams.OrderBy(b => b.Id).ToArray(), Shells = shells.OrderBy(s => s.Id).ToArray(), Nodes = ordered,
                Supports = ordered.Where(n => n.Assignments.Restrains.Count != 0).ToArray(), StaticCases = cases, Combinations = combinations };
        }

        /// <summary>Every beam, plate and node with every static case of the model, or of the given cases.</summary>
        public static ResultReadPlan All(GPC.Model.Models.Model model, IEnumerable<string> staticCases = null) =>
            new ResultFilter { StaticCases = staticCases?.ToArray() ?? new string[0] }.Resolve(model);
    }

    /// <summary>Elements and static cases whose results are read. The readers bind the results to the model with <see cref="SourceBinding"/>.</summary>
    public sealed class ResultReadPlan
    {
        public IReadOnlyList<BeamElement> Beams { get; internal set; }
        public IReadOnlyList<AreaElement> Shells { get; internal set; }
        /// <summary>Nodes whose displacements are read.</summary>
        public IReadOnlyList<NodeElement> Nodes { get; internal set; }
        /// <summary>The restrained nodes among <see cref="Nodes"/>, whose reactions are read.</summary>
        public IReadOnlyList<NodeElement> Supports { get; internal set; }
        public IReadOnlyList<string> StaticCases { get; internal set; }
        /// <summary>The selected combinations, to rebuild from the static cases.</summary>
        public IReadOnlyList<string> Combinations { get; internal set; }
    }
}
