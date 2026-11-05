using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;

namespace GPC.Model.Attributes
{
    /// <summary>
    /// A rigid link from the node that carries the attribute to the node at <see cref="TargetMm"/> (a full spatial kinematic link, e.g.
    /// the BridGHe "Rigid Connection"), before the model exists: an application lifts it into a <see cref="Constraints.RigidLink"/> between
    /// the node and the one node found at the target within its tolerance. The point is in mm in a model (in the units of the definition in
    /// the drafts of an application, which normalize it with the node). Descriptive: not written to the solvers and not part of the
    /// analysis fingerprint. A node may carry several, never two with the same target (<see cref="Models.ModelPreflight.ValidateDomain"/>).
    /// </summary>
    [Serializable]
    public sealed class RigidConnectionAttribute : Attribute
    {
        private readonly Point3d _target;

        /// <summary>The target point (a copy: the attribute is immutable).</summary>
        public Point3d TargetMm => new Point3d(_target.X, _target.Y, _target.Z);

        public RigidConnectionAttribute(Point3d targetMm)
        {
            if (targetMm is null || !Finite(targetMm)) throw new ArgumentException("InvalidRigidConnectionTarget", nameof(targetMm));
            _target = new Point3d(targetMm.X, targetMm.Y, targetMm.Z);
        }

        private RigidConnectionAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "RigidConnectionAttributeVersion", 1);
            var target = (Point3d)info.GetValue("TargetMm", typeof(Point3d));
            if (target is null || !Finite(target)) throw new SerializationException("InvalidRigidConnectionTarget");
            _target = new Point3d(target.X, target.Y, target.Z);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("RigidConnectionAttributeVersion", 1);
            // The held point, not a new copy: a GPC.Geometry point carries its own GUID, so a copy per save made two saves of one model differ.
            info.AddValue("TargetMm", _target);
        }

        private static bool Finite(Point3d p) => !double.IsNaN(p.X) && !double.IsInfinity(p.X) && !double.IsNaN(p.Y) && !double.IsInfinity(p.Y) && !double.IsNaN(p.Z) && !double.IsInfinity(p.Z);
        internal bool SameTarget(RigidConnectionAttribute other) => _target.X == other._target.X && _target.Y == other._target.Y && _target.Z == other._target.Z;
    }

    /// <summary>The rigid connections of the elements: the analysis fingerprint leaves them out, the assembly keeps them, the domain
    /// preflight checks where they are.</summary>
    internal static class RigidConnections
    {
        /// <summary>The attributes of an element as the analysis fingerprint sees them: without the rigid connections, the same collection
        /// object when it has none (the fingerprints of the other models do not change).</summary>
        internal static object AnalysisView(Element element)
        {
            var attributes = element.Attributes;
            if (attributes == null || attributes.Count == 0 || !attributes.Values.Any(a => a is RigidConnectionAttribute)) return attributes;
            // Same collection type and the same id-attribute pairs (the fingerprint walks the pairs, never the counter); the ids of the
            // attributes are not touched.
            var kept = new Collections.UniqueIdCollection<Attribute>();
            foreach (var pair in attributes) if (!(pair.Value is RigidConnectionAttribute)) kept[pair.Key] = pair.Value;
            return kept;
        }

        /// <summary>The rigid connections of a node, as a merge of coincident nodes compares them: none gives no entry, so the other
        /// nodes merge as before.</summary>
        internal static object[] MergeTargets(NodeElement node)
        {
            var targets = node.Attributes?.Values.OfType<RigidConnectionAttribute>().Select(a => a.TargetMm)
                .OrderBy(p => p.X).ThenBy(p => p.Y).ThenBy(p => p.Z).Select(p => (object)new[] { p.X, p.Y, p.Z }).ToArray();
            return targets == null || targets.Length == 0 ? null : targets;
        }

        /// <summary>Errors of the rigid connections: on an element that is not a node, or two with the same target.</summary>
        internal static IEnumerable<ModelDiagnostic> Diagnose(Models.Model model)
        {
            foreach (var element in model.AllElements)
            {
                var attributes = element.Attributes;
                if (attributes == null || attributes.Count == 0) continue;
                var links = attributes.OrderBy(p => p.Key).Select(p => p.Value).OfType<RigidConnectionAttribute>().ToList();
                if (links.Count == 0) continue;
                if (!(element is NodeElement))
                    yield return ModelDiagnostic.Error("RigidConnectionOnWrongElement", element, "RigidConnectionOnWrongElement: rigid connections belong to nodes");
                for (int i = 1; i < links.Count; i++)
                    if (links.Take(i).Any(previous => previous.SameTarget(links[i])))
                    { yield return ModelDiagnostic.Error("DuplicateRigidConnection", element, "DuplicateRigidConnection: two rigid connections with the same target"); break; }
            }
        }
    }
}
