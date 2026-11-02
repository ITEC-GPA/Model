using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;

namespace GPC.Model.Models
{
    /// <summary>Deterministic assembly of standalone nodes, two-node beams and linear shells.
    /// The caller owns a stable model GUID (e.g. the assembly component GUID). Objects are copied
    /// as one graph before registry changes. Null merge tolerance preserves coincident identities.
    /// Merge conflicts fail: loads are never silently added or discarded.</summary>
    public static class ModelAssembly
    {
        public static Model Assemble(IEnumerable<Element> input, Guid modelGuid, string name = "",
            double? mergeToleranceMm = null, IEnumerable<Guid> keepSeparate = null, bool supportsWarpedQuads = false)
        {
            if (modelGuid == Guid.Empty) throw new ArgumentException("A stable model identity is required.", nameof(modelGuid));
            if (mergeToleranceMm.HasValue && NumericGuard.Finite(mergeToleranceMm.Value, nameof(mergeToleranceMm)) < 0) throw new ArgumentOutOfRangeException(nameof(mergeToleranceMm));
            var roots = (input ?? throw new ArgumentNullException(nameof(input))).ToArray();
            var originals = Gather(roots);
            var distinct = originals.GroupBy(e => e.Guid).Select(group =>
            {
                var first = group.First(); var token = Content(first);
                if (group.Any(e => e.GetType() != first.GetType() || e.Id != first.Id || Content(e) != token)) throw new ArgumentException("ConflictingEntityGuid: " + group.Key);
                return first;
            }).OrderBy(e => e.Guid).ToArray();
            var protectedNodes = new HashSet<Guid>(keepSeparate ?? Enumerable.Empty<Guid>());
            if (protectedNodes.Except(distinct.OfType<NodeElement>().Select(n => n.Guid)).Any()) throw new ArgumentException("UnknownKeepSeparateNode");
            var copies = ModelValues.CopyValue(distinct.Cast<object>().ToArray()).Cast<Element>().ToDictionary(e => e.Guid);
            var nodes = copies.Values.OfType<NodeElement>().OrderBy(n => n.Guid).ToArray();
            var canonical = nodes.ToDictionary(n => n.Guid, n => n);
            if (mergeToleranceMm.HasValue) Merge(nodes, canonical, protectedNodes, mergeToleranceMm.Value);

            // Integer-only references must identify exactly one original node. Reused IDs in
            // independent GH branches are legal until such an ambiguous reference is requested.
            var oldIds = distinct.OfType<NodeElement>().GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.Select(n => canonical[n.Guid]).Distinct(ReferenceComparer<NodeElement>.Instance).ToArray());
            NodeElement ByOldId(int id)
            {
                if (id <= 0 || !oldIds.TryGetValue(id, out var matches) || matches.Length != 1) throw new ArgumentException("MissingOrAmbiguousNodeId: " + id);
                return matches[0];
            }
            var links = nodes.SelectMany(n => n.Assignments.Links).Distinct(ReferenceComparer<Structure.Assignments.NodalLink>.Instance)
                .ToDictionary(l => l, l => ByOldId(l.OtherNodeId), ReferenceComparer<Structure.Assignments.NodalLink>.Instance);
            var orientations = copies.Values.OfType<BeamElement>().Where(b => b.Assignments.OrientationNodeId.HasValue)
                .ToDictionary(b => b.Guid, b => ByOldId(b.Assignments.OrientationNodeId.Value));
            var model = new Model(modelGuid, name);
            int next = 0;
            foreach (var node in canonical.Values.Distinct(ReferenceComparer<NodeElement>.Instance).OrderBy(n => n.Guid))
            { node.Id = ++next; model.NodesElements.Add(node); }
            foreach (var node in model.NodesElements.Values)
            {
                foreach (var link in node.Assignments.Links)
                {
                    link.OtherNodeId = links[link].Id;
                    if (link.OtherNodeId == node.Id) throw new ArgumentException("MergeCollapsedNodalLink");
                }
                foreach (var restraint in node.Assignments.Restrains)
                {
                    if (restraint.Restrain == null || restraint.Restrain.Point == null || !ReferenceEquals(canonical[restraint.Restrain.Point.Guid], node))
                        throw new ArgumentException("RestraintNodeMismatch");
                    restraint.Restrain.Point = node;
                    if (restraint.Case != null) restraint.Case = CanonicalCase(model, restraint.Case as LoadCaseBase ?? throw new NotSupportedException("AssemblyRequiresLoadCaseBase"));
                }
            }
            next = 0;
            foreach (var beam in copies.Values.OfType<BeamElement>().OrderBy(b => b.Guid))
            {
                var a = canonical[beam.NodeI.Guid]; var b = canonical[beam.NodeJ.Guid];
                if (ReferenceEquals(a, b)) throw new ArgumentException("MergeCollapsedBeam");
                beam.ConnectNodes(a, b); beam.Id = ++next;
                if (orientations.TryGetValue(beam.Guid, out var orientation)) beam.Assignments.OrientationNodeId = orientation.Id;
                beam.BeamProperty = (BeamProperty)CanonicalProperty(model, beam.BeamProperty);
                foreach (var section in beam.Assignments.Sections)
                {
                    section.Property = (BeamProperty)CanonicalProperty(model, section.Property);
                    section.EndProperty = (BeamProperty)CanonicalProperty(model, section.EndProperty);
                    foreach (var station in section.Stations) station.Property = (BeamProperty)CanonicalProperty(model, station.Property);
                }
                foreach (var load in beam.Assignments.Loads)
                    foreach (Load value in new Load[] { load.Concentrated, load.StartIntensity, load.EndIntensity })
                        if (value != null) value.LoadCase = CanonicalCase(model, value.LoadCase);
                model.BeamElements.Add(beam);
            }
            next = 0;
            foreach (var shell in copies.Values.OfType<AreaElement>().OrderBy(s => s.Guid))
            {
                var connected = shell.Nodes.Select(n => canonical[n.Guid]).ToArray();
                if (connected.Select(n => n.Guid).Distinct().Count() != connected.Length) throw new ArgumentException("MergeCollapsedShell");
                shell.ConnectNodes(connected); shell.Id = ++next;
                shell.PlateProperty = (PlateProperty)CanonicalProperty(model, shell.PlateProperty); model.AreaElements.Add(shell);
            }
            foreach (var element in model.AllElements)
                foreach (var load in element.Loads.Values) load.LoadCase = CanonicalCase(model, load.LoadCase);
            RegisterGroups(model);
            model.AssemblyMap = new AssemblyIdentityMap(modelGuid, mergeToleranceMm, distinct.Select(original =>
                new AssemblyIdentityEntry(original, original is NodeElement ? canonical[original.Guid] : copies[original.Guid], Family(original))));
            var errors = model.ValidateTopology(supportsWarpedQuads, GeometryBase.Tolerance).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            if (errors.Length != 0) throw new ArgumentException("InvalidAssembledTopology: " + string.Join(", ", errors.Select(e => e.Code)));
            return model;
        }

        private static List<Element> Gather(Element[] roots)
        {
            var found = new List<Element>(); var seen = new HashSet<Element>(ReferenceComparer<Element>.Instance); var todo = new Stack<Element>(roots);
            while (todo.Count > 0)
            {
                var element = todo.Pop();
                if (element == null) throw new ArgumentException("NullAssemblyElement");
                if (!seen.Add(element)) continue;
                if (element.GetType() != typeof(NodeElement) && element.GetType() != typeof(BeamElement) && element.GetType() != typeof(AreaElement)) throw new NotSupportedException("UnsupportedAssemblyElement: " + element.GetType().FullName);
                if (element.Guid == Guid.Empty || element.Results.Count != 0) throw new ArgumentException("AssemblyRequiresIdentifiedInputsWithoutResults");
                // Legacy generic attributes can contain constraints and arbitrary element references.
                // Explicit assignments are supported; an unimplemented remap is never copied silently.
                if (element.Attributes.Count != 0) throw new NotSupportedException("AssemblyRequiresExplicitAssignments: legacy Attributes require an adapter.");
                found.Add(element);
                if (element is NodeElement node)
                {
                    if (!Axes.IsFinite(node.Position) || node.Assignments == null) throw new ArgumentException("InvalidAssemblyNode");
                    foreach (var restraint in node.Assignments.Restrains) if (restraint?.Restrain?.Point != null) todo.Push(restraint.Restrain.Point);
                }
                else if (element is BeamElement beam)
                { if (beam.NodeI == null || beam.NodeJ == null) throw new ArgumentException("ExplicitBeamConnectivityRequired"); todo.Push(beam.NodeI); todo.Push(beam.NodeJ); }
                else if (element is AreaElement shell)
                { if (shell.Nodes.Count != 3 && shell.Nodes.Count != 4) throw new ArgumentException("ExplicitShellConnectivityRequired"); foreach (var n in shell.Nodes) todo.Push(n); }
            }
            return found;
        }
        private static EntityFamily Family(Element value) => value is NodeElement ? EntityFamily.Node : value is BeamElement ? EntityFamily.Beam : EntityFamily.Shell;
        private static string Content(Element element)
        {
            var info = new SerializationInfo(element.GetType(), new FormatterConverter()); element.GetObjectData(info, new StreamingContext());
            var fields = new SortedDictionary<string, object>(StringComparer.Ordinal);
            foreach (SerializationEntry entry in info) if (entry.Name != "Guid" && entry.Name != "Id") fields.Add(entry.Name, entry.Value);
            return ModelValues.Fingerprint(new object[] { fields });
        }
        private static string MergeContent(NodeElement node) => ModelValues.Fingerprint(new object[] {
            node.Name, node.CoordinateSystem, node.Groups, node.Assignments.Dofs, node.Assignments.Active, node.Assignments.Mass,
            node.Assignments.GroundSprings, node.Assignments.Links,
            node.Assignments.Restrains.Select(r => new object[] { r.Case, r.Phase, r.SourceRecord, r.Restrain?.Name, r.Restrain?.CoordinateSystem, r.Restrain?.Restrains }).ToArray() });

        private static void Merge(NodeElement[] nodes, Dictionary<Guid, NodeElement> map, HashSet<Guid> separate, double tolerance)
        {
            var cells = new Dictionary<(double, double, double), List<NodeElement>>();
            var tokens = new Dictionary<Guid, string>();
            (double, double, double) Cell(Point3d p)
            {
                if (tolerance == 0) return (p.X, p.Y, p.Z);
                double x = Math.Floor(p.X / tolerance), y = Math.Floor(p.Y / tolerance), z = Math.Floor(p.Z / tolerance);
                if (Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z))) > 4503599627370495.0) throw new ArgumentOutOfRangeException(nameof(tolerance), "Merge grid cannot resolve this coordinate/tolerance ratio.");
                return (x, y, z);
            }
            foreach (var node in nodes)
            {
                if (separate.Contains(node.Guid)) continue;
                var key = Cell(node.Position); var candidates = new List<NodeElement>(); int span = tolerance == 0 ? 0 : 1;
                for (int x = -span; x <= span; x++) for (int y = -span; y <= span; y++) for (int z = -span; z <= span; z++)
                    if (cells.TryGetValue((key.Item1 + x, key.Item2 + y, key.Item3 + z), out var nearby))
                        candidates.AddRange(nearby.Where(n => Within(n.Position, node.Position, tolerance)));
                var selected = candidates.OrderBy(n => n.Guid).FirstOrDefault();
                if (selected == null)
                { if (!cells.TryGetValue(key, out var list)) cells.Add(key, list = new List<NodeElement>()); list.Add(node); }
                else
                {
                    if (node.Loads.Count != 0 || selected.Loads.Count != 0) throw new ArgumentException("NodeMergeLoadConflict: resolve loads explicitly before merging.");
                    if (!tokens.TryGetValue(selected.Guid, out var token)) tokens.Add(selected.Guid, token = MergeContent(selected));
                    if (token != MergeContent(node)) throw new ArgumentException("NodeMergeAssignmentConflict: " + node.Guid + " / " + selected.Guid);
                    map[node.Guid] = selected;
                }
            }
        }
        private static bool Within(Point3d a, Point3d b, double tolerance)
        {
            if (tolerance == 0) return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
            double x = Math.Abs(a.X - b.X) / tolerance, y = Math.Abs(a.Y - b.Y) / tolerance, z = Math.Abs(a.Z - b.Z) / tolerance;
            return x <= 1 && y <= 1 && z <= 1 && x * x + y * y + z * z <= 1;
        }
        private static LoadCaseBase CanonicalCase(Model model, LoadCaseBase value)
        {
            if (value == null) throw new ArgumentException("MissingAssemblyLoadCase");
            if (model.LoadCases.TryGetValue(value.Name, out var previous))
            { SameValue(previous, value, "LoadCaseConflict"); return previous; }
            model.AddLoadCase(value); return value;
        }
        private static ElementProperty CanonicalProperty(Model model, ElementProperty value)
        {
            if (value == null) return null;
            ElementProperty previous = value is BeamProperty ? (ElementProperty)model.GetBeamProperty(value.Name) : model.GetPlateProperty(value.Name);
            if (previous != null) { SameValue(previous, value, "PropertyConflict"); return previous; }
            if (!model.AddProperty(value)) throw new ArgumentException("UnsupportedAssemblyProperty"); return value;
        }
        private static void SameValue(object a, object b, string code)
        { if (!ReferenceEquals(a, b) && (a.GetType() != b.GetType() || ModelValues.Fingerprint(new[] { a }) != ModelValues.Fingerprint(new[] { b }))) throw new ArgumentException(code); }
        private static void RegisterGroups(Model model)
        {
            var visited = new HashSet<Group>(ReferenceComparer<Group>.Instance); var stack = new Stack<Group>(model.AllElements.SelectMany(e => e.Groups.Values));
            while (stack.Count > 0)
            {
                var group = stack.Pop(); if (group == null || group.Childs == null) throw new ArgumentException("InvalidAssemblyGroup");
                if (!visited.Add(group)) continue;
                if (model.Groups.TryGetValue(group.Name, out var previous))
                { if (previous.Guid != group.Guid) throw new ArgumentException("GroupIdentityConflict: " + group.Name); SameValue(previous, group, "GroupContentConflict"); }
                else model.Groups.Add(group.Name, group);
                if (group.Parent != null) stack.Push(group.Parent); foreach (var child in group.Childs) stack.Push(child);
            }
            var parents = visited.GroupBy(g => g.Name).ToDictionary(g => g.Key, g => g.First().Parent?.Name);
            foreach (var group in visited) foreach (var old in group.Childs.ToArray()) group.RemoveChild(old);
            foreach (var parent in parents) if (parent.Value != null) model.Groups[parent.Value].AddChild(model.Groups[parent.Key]);
            foreach (var element in model.AllElements) foreach (var name in element.Groups.Keys.ToArray()) element.Groups[name] = model.Groups[name];
        }
    }
}
