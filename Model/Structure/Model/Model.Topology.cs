using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Checking.Reports;
using GPC.Model.Constraints;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Structure.Assignments;

namespace GPC.Model.Models
{
    public partial class Model
    {
        public List<CheckReport> CheckReports { get; private set; } = new List<CheckReport>();
        public List<PreservedAssignment> PreservedSourceData { get; private set; } = new List<PreservedAssignment>();
        public IEnumerable<Element> AllElements => NodesElements.Values.Cast<Element>()
            .Concat(BeamElements.Values).Concat(AreaElements.Values)
            .Concat(VolumeElements == null ? Enumerable.Empty<Element>() : VolumeElements.Values.Cast<Element>());

        public Element FindBySource(SourceIdentity source)
        {
            return AllElements.SingleOrDefault(e => source.Equals(e.Source));
        }

        public void ConnectBeam(int beamId, int nodeI, int nodeJ)
        {
            var i = NodesElements[nodeI]; var j = NodesElements[nodeJ];
            if (ReferenceEquals(i, j)) throw new ArgumentException("A beam requires distinct nodes.");
            BeamElements[beamId].ConnectNodes(i, j);
        }

        public void ConnectShell(int shellId, params int[] nodeIds)
        {
            if (nodeIds.Distinct().Count() != nodeIds.Length) throw new ArgumentException("Repeated shell node.");
            var nodes = nodeIds.Select(id => NodesElements[id]).ToArray();
            AreaElements[shellId].ConnectNodes(nodes);
        }

        public IReadOnlyList<NodeElement> GetConnectedNodes(Element element)
        {
            if (element is BeamElement beam) return beam.NodeI is null || beam.NodeJ is null
                ? new NodeElement[0] : new[] { beam.NodeI, beam.NodeJ };
            if (element is AreaElement area) return area.Nodes;
            if (element is VolumeElement volume) return volume.Nodes ?? new NodeElement[0];
            return new NodeElement[0];
        }

        /// <summary>Snapshot index, rebuilt to observe direct legacy collection edits.</summary>
        public IReadOnlyDictionary<int, IReadOnlyList<Element>> GetAdjacency()
        {
            var index = NodesElements.Keys.ToDictionary(id => id, id => new List<Element>());
            foreach (var element in AllElements.Where(e => !(e is NodeElement)))
                foreach (var node in GetConnectedNodes(element))
                    if (node != null && index.TryGetValue(node.Id, out var list)) list.Add(element);
            return index.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<Element>)kv.Value.AsReadOnly());
        }

        public void RemoveNodeChecked(int nodeId)
        {
            if (GetAdjacency().TryGetValue(nodeId, out var incident) && incident.Count != 0)
                throw new InvalidOperationException("ReferencedNode: disconnect incident elements explicitly before removal.");
            if (NodesElements.Values.Any(n => n.Assignments.Links.Any(l => l.OtherNodeId == nodeId)))
                throw new InvalidOperationException("ReferencedNode: a nodal link references this node.");
            if (BeamElements.Values.Any(b => b.Assignments.OrientationNodeId == nodeId))
                throw new InvalidOperationException("ReferencedNode: a beam orientation references this node.");
            if (Costrains.Values.Any(c => ConstraintNodes(c).Any(n => n != null && n.Id == nodeId)))
                throw new InvalidOperationException("ReferencedNode: a constraint references this node.");
            NodesElements.RemoveById(nodeId);
        }

        private static IEnumerable<NodeElement> ConstraintNodes(global::GPC.Model.Constraints.Costrain constraint)
        {
            yield return constraint.StartNode;
            foreach (var node in constraint.EndNodes ?? new NodeElement[0]) yield return node;
            foreach (var link in constraint.Links ?? new global::GPC.Model.Constraints.MultiPointsCostrain[0])
                foreach (var term in link.Equations ?? new global::GPC.Model.Constraints.MultiPointsCostrain.Equation[0]) yield return term.NodeSlave;
        }

        public IReadOnlyList<ModelDiagnostic> ValidateTopology()
        {
            var issues = new List<ModelDiagnostic>();
            issues.AddRange(ValidateGroups());
            if (NodesElements.MissingLegacyPayload || BeamElements.MissingLegacyPayload || AreaElements.MissingLegacyPayload || VolumeElements.MissingLegacyPayload)
                issues.Add(ModelDiagnostic.Error("MissingLegacyCollectionPayload", message: "The legacy writer did not store collection entries; reimport the model from an authoritative source."));
            foreach (var node in NodesElements)
            {
                if (node.Key != node.Value.Id) issues.Add(ModelDiagnostic.Error("RegistryKeyMismatch", node.Value));
                if (!Axes.IsFinite(node.Value.Position)) issues.Add(ModelDiagnostic.Error("NonFiniteCoordinate", node.Value));
            }
            var sources = new HashSet<SourceIdentity>();
            foreach (var constraint in Costrains.Values)
                foreach (var node in ConstraintNodes(constraint))
                    if (node == null || !NodesElements.TryGetValue(node.Id, out var registered) || !ReferenceEquals(node, registered))
                        issues.Add(ModelDiagnostic.Error("DanglingConstraintNode", message: "Constraint " + constraint.Id + " contains an unregistered node."));
            foreach (var element in AllElements)
            {
                if (element.Source != null && !sources.Add(element.Source)) issues.Add(ModelDiagnostic.Error("SourceCollision", element));
                if (element is BeamElement || element is AreaElement || element is VolumeElement)
                {
                    var nodes = GetConnectedNodes(element);
                    if (nodes.Count == 0) issues.Add(ModelDiagnostic.Error("UnresolvedConnectivity", element));
                    foreach (var node in nodes)
                        if (node == null || !NodesElements.TryGetValue(node.Id, out var registered) || !ReferenceEquals(node, registered))
                            issues.Add(ModelDiagnostic.Error("DanglingNodeReference", element));
                }
                if (element is BeamElement beam)
                {
                    if (beam.Assignments.OrientationNodeId.HasValue && !NodesElements.ContainsKey(beam.Assignments.OrientationNodeId.Value))
                        issues.Add(ModelDiagnostic.Error("MissingOrientationNode", beam));
                    if (!Axes.IsFinite(beam.StartPoint) || !Axes.IsFinite(beam.EndPoint) || beam.Length <= 1e-9)
                        issues.Add(ModelDiagnostic.Error("DegenerateBeam", element));
                    if (beam.Assignments.Formulation != BeamFormulation.StraightTwoNode)
                        issues.Add(ModelDiagnostic.Error("UnsupportedBeamFormulation", element));
                }
                if (element is AreaElement area && area.Nodes.Count > 0)
                {
                    var p = area.Points;
                    Vector3d edge = p[1] - p[0];
                    var normal = edge.CrossProduct(p[2] - p[0]);
                    double length = Axes.Length(normal);
                    if (length < 1e-9) issues.Add(ModelDiagnostic.Error("DegenerateShell", element));
                    else if (p.Length == 4 && Math.Abs(Axes.Dot(p[3] - p[0], normal)) / length > 1e-6)
                        issues.Add(ModelDiagnostic.Error("NonPlanarShell", element));
                    else if (p.Length == 4)
                    {
                        // Supported quads are convex with a consistent winding; refuse bow-ties and collapsed edges.
                        for (int k = 0; k < 4; k++)
                        {
                            Vector3d a = p[(k + 1) % 4] - p[k], b = p[(k + 2) % 4] - p[(k + 1) % 4];
                            if (Axes.Dot(a.CrossProduct(b), normal) <= 1e-12 * length * length)
                            { issues.Add(ModelDiagnostic.Error("UnsupportedShellWinding", element)); break; }
                        }
                    }
                }
            }
            return issues.AsReadOnly();
        }
    }
}
