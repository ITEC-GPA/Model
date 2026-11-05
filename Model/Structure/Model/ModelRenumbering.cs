using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Collections;
using GPC.Model.Core;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;

namespace GPC.Model.Models
{
    /// <summary>The ids of the elements of an input model numbered again (<see cref="ModelValues.Renumber"/>): nodes, beams, shells and
    /// solids 1..n per family in the order of a sequence, on a copy with the same identities; every reference by id remapped.
    /// <para>References by id (audit of the vocabulary, every int field that names an element; the other references are object
    /// references or GUIDs and need nothing):</para>
    /// <list type="bullet">
    /// <item>the registries themselves (<see cref="Model.NodesElements"/>, <see cref="Model.BeamElements"/>, <see cref="Model.AreaElements"/>,
    /// <see cref="Model.VolumeElements"/>), keyed by id;</item>
    /// <item><see cref="Structure.Assignments.BeamAssignments.OrientationNodeId"/> (node);</item>
    /// <item><see cref="Structure.Assignments.NodalLink.OtherNodeId"/> (node);</item>
    /// <item><see cref="Structure.Members.BeamMemberPart.BeamId"/> of the parts of <see cref="Model.PhysicalMembers"/> (beam);</item>
    /// <item><see cref="Structure.Members.PhysicalSurfaceDefinition.ElementIds"/> and the ids of its zones (<see cref="Model.PhysicalSurfaces"/>, shell);</item>
    /// <item><see cref="AssemblyIdentityEntry.ModelId"/> of the assembly map (the canonical element; <see cref="AssemblyIdentityEntry.OriginalModelId"/>
    /// is the id the input had when it was assembled, a record of the input, and stays).</item>
    /// </list>
    /// <para>Not remapped, because a model with them is refused: results and their datasets, the analysis snapshot and source, the
    /// verification context and the check reports (their contracts name elements by id: <c>MemberLocation.BeamId</c>, the beam and shell
    /// check contracts). Unchanged by design: <see cref="SourceIdentity"/> (the identifier of the application), the verification scenarios
    /// (keyed by GUID), the constraints (they cite node objects; their own ids are not element ids), the moving paths (target element GUIDs),
    /// group, property, case and stage ids (not element ids).</para></summary>
    internal static class ModelRenumbering
    {
        internal static Model Renumber(Model model, Func<Element, long> sequence)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            if (model.Datasets.Count != 0 || model.Analysis != null || model.AnalysisSource != null || model.VerificationContext != null || model.CheckReports.Count != 0
                || model.AllElements.Any(e => e.HasResults))
                throw new ArgumentException("RenumberRequiresInputModel: results, datasets, an analysis snapshot, a verification context or check reports name the elements by id.", nameof(model));
            var map = model.AssemblyMap;
            if (map != null)
            {
                // A map of another element set (elements added or removed after the assembly) cannot be renumbered: refresh it first.
                var current = model.AllElements.ToDictionary(e => e.Guid);
                var canonical = map.Entries.Where(e => e.InputGuid == e.CanonicalGuid).ToArray();
                if (map.ModelGuid != model.Guid || canonical.Length != current.Count
                    || canonical.Any(e => !current.TryGetValue(e.CanonicalGuid, out var element) || element.Id != e.ModelId || Model.FamilyOf(element) != e.Family))
                    throw new ArgumentException("RenumberRequiresCurrentAssemblyMap: refresh it (ModelAssembly.RefreshIdentityMap) after adding or removing elements.", nameof(model));
            }

            // The sequence is read on the input elements; ties keep the current order (ids).
            var ids = new Dictionary<EntityFamily, Dictionary<int, int>>();
            Dictionary<int, int> Order<T>(IEnumerable<T> elements) where T : Element
            {
                var keyed = elements.Select(e => (Element: e, Key: sequence(e))).ToArray();
                var result = new Dictionary<int, int>(); int next = 0;
                foreach (var item in keyed.OrderBy(x => x.Key).ThenBy(x => x.Element.Id)) result.Add(item.Element.Id, ++next);
                return result;
            }
            ids[EntityFamily.Node] = Order(model.NodesElements.Values);
            ids[EntityFamily.Beam] = Order(model.BeamElements.Values);
            ids[EntityFamily.Shell] = Order(model.AreaElements.Values);
            ids[EntityFamily.Solid] = Order(model.VolumeElements?.Values ?? Enumerable.Empty<VolumeElement>());
            int Node(int id) => ids[EntityFamily.Node].TryGetValue(id, out int n) ? n : throw new ArgumentException("RenumberUnknownNodeReference: " + id, nameof(model));
            int Beam(int id) => ids[EntityFamily.Beam].TryGetValue(id, out int n) ? n : throw new ArgumentException("RenumberUnknownBeamReference: " + id, nameof(model));
            int Shell(int id) => ids[EntityFamily.Shell].TryGetValue(id, out int n) ? n : throw new ArgumentException("RenumberUnknownShellReference: " + id, nameof(model));

            // The references are resolved on the input first, so a dangling one refuses before anything is copied.
            foreach (var node in model.NodesElements.Values) foreach (var link in node.Assignments.Links) Node(link.OtherNodeId);
            foreach (var beam in model.BeamElements.Values) if (beam.Assignments.OrientationNodeId.HasValue) Node(beam.Assignments.OrientationNodeId.Value);
            foreach (var member in model.PhysicalMembers.Values) foreach (var part in member.Parts) Beam(part.BeamId);
            foreach (var surface in model.PhysicalSurfaces.Values) foreach (int id in surface.ElementIds.Concat(surface.Zones.SelectMany(z => z.ElementIds))) Shell(id);

            var copy = ModelValues.Copy(model);
            void Registry<T>(SortedCollection<T> registry, Dictionary<int, int> order) where T : Element
            {
                if (registry == null) return;
                var elements = registry.Values.ToArray();
                registry.Clear();
                foreach (var element in elements) element.Id = order[element.Id];
                foreach (var element in elements.OrderBy(e => e.Id)) registry.Add(element);
            }
            Registry(copy.NodesElements, ids[EntityFamily.Node]);
            Registry(copy.BeamElements, ids[EntityFamily.Beam]);
            Registry(copy.AreaElements, ids[EntityFamily.Shell]);
            Registry(copy.VolumeElements, ids[EntityFamily.Solid]);
            foreach (var node in copy.NodesElements.Values) foreach (var link in node.Assignments.Links) link.OtherNodeId = Node(link.OtherNodeId);
            foreach (var beam in copy.BeamElements.Values) if (beam.Assignments.OrientationNodeId.HasValue) beam.Assignments.OrientationNodeId = Node(beam.Assignments.OrientationNodeId.Value);
            foreach (var key in copy.PhysicalMembers.Keys.ToArray()) copy.PhysicalMembers[key] = copy.PhysicalMembers[key].Renumbered(Beam);
            foreach (var key in copy.PhysicalSurfaces.Keys.ToArray()) copy.PhysicalSurfaces[key] = copy.PhysicalSurfaces[key].Renumbered(Shell);
            if (copy.AssemblyMap != null) copy.AssemblyMap = copy.AssemblyMap.Renumbered((family, id) => ids[family][id]);
            return copy;
        }
    }
}
