using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;
using GPC.Model.Structure.Members;

namespace GPC.Model.Structure.Bridges
{
    /// <summary>
    /// The behaviour of a bridge section in an action: steel only, long term (modular ratio n∞), short term (n0), shrinkage (n,sh).
    /// Same names and order as GPC.Checkers.CompositeBridge.BridgeActionBehavior, which Model cannot reference.
    /// </summary>
    public enum BridgeSectionBehavior { Steel, LongTerm, ShortTerm, Shrinkage }

    /// <summary>
    /// The physical bridge member of a beam (PhysicalId, family, group, segment, span, stations in mm, SectionUp, GeometryId) before the
    /// model exists: an application (BridGHe Build Model) lifts the attributes of the beams into <see cref="Models.Model.PhysicalMembers"/>
    /// (<see cref="BeamMemberPart"/> with these metadata). Descriptive: not written to the solvers and not part of the analysis
    /// fingerprint. One per beam (<see cref="Models.ModelPreflight.ValidateDomain"/>).
    /// </summary>
    [Serializable]
    public sealed class BridgeMemberAttribute : Attributes.Attribute
    {
        /// <summary>The metadata (not null, validated by <see cref="BridgeMemberMetadata"/>).</summary>
        public BridgeMemberMetadata Metadata { get; private set; }

        public BridgeMemberAttribute(BridgeMemberMetadata metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        private BridgeMemberAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "BridgeMemberAttributeVersion", 1);
            Metadata = (BridgeMemberMetadata)info.GetValue("Metadata", typeof(BridgeMemberMetadata)) ?? throw new SerializationException("MissingBridgeMemberMetadata");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BridgeMemberAttributeVersion", 1);
            info.AddValue("Metadata", Metadata);
        }
    }

    /// <summary>
    /// The super element of a beam (BridGHe "Super Element"), carried as the <see cref="ModelObject.Name"/> of the attribute: not empty,
    /// without leading or trailing spaces. An application lifts it into a physical member (BridGHe: "SuperElement:&lt;name&gt;").
    /// Descriptive: not written to the solvers and not part of the analysis fingerprint. One per beam.
    /// </summary>
    [Serializable]
    public sealed class SuperElementAttribute : Attributes.Attribute
    {
        public SuperElementAttribute(string name) : base(Checked(name)) { }

        private SuperElementAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "SuperElementAttributeVersion", 1);
            if (!BridgeElementAttributes.ValidName(Name)) throw new SerializationException("InvalidSuperElementName");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SuperElementAttributeVersion", 1);
        }

        private static string Checked(string name)
            => BridgeElementAttributes.ValidName(name) ? name : throw new ArgumentException("InvalidSuperElementName", nameof(name));
    }

    /// <summary>
    /// The local state of the composite section of a beam in the actions of one behaviour (BridGHe "section state": the moment sign of a
    /// reference case decides whether the slab is cracked). An application lifts it into the section states of the verification recipe
    /// (GPC.Checkers.CompositeBridge.BridgeBeamSectionState) and its concrete-excluded beams. Descriptive: not written to the solvers and
    /// not part of the analysis fingerprint. One per beam.
    /// </summary>
    [Serializable]
    public sealed class BridgeSectionStateAttribute : Attributes.Attribute
    {
        /// <summary>The behaviour of the actions the state applies to: long term, short term or shrinkage (never steel only).</summary>
        public BridgeSectionBehavior Behavior { get; private set; }
        /// <summary>True when the concrete of the slab is excluded (cracked section).</summary>
        public bool ConcreteExcluded { get; private set; }
        /// <summary>The modular ratio of the state (dimensionless, finite, greater than 0).</summary>
        public double ModularRatio { get; private set; }
        /// <summary>The name of the load case whose results classified the state; null when none.</summary>
        public string ClassificationCase { get; private set; }
        /// <summary>The fingerprint of the beam property the state was computed for (not empty), so a later edit of the property is
        /// detected as a stale state.</summary>
        public string PropertyFingerprint { get; private set; }

        public BridgeSectionStateAttribute(BridgeSectionBehavior behavior, bool concreteExcluded, double modularRatio, string classificationCase, string propertyFingerprint)
        {
            Behavior = behavior; ConcreteExcluded = concreteExcluded; ModularRatio = modularRatio;
            ClassificationCase = classificationCase; PropertyFingerprint = propertyFingerprint;
            string error = Error();
            if (error != null) throw new ArgumentException(error);
        }

        private BridgeSectionStateAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "BridgeSectionStateAttributeVersion", 1);
            Behavior = (BridgeSectionBehavior)info.GetValue("Behavior", typeof(BridgeSectionBehavior));
            ConcreteExcluded = info.GetBoolean("ConcreteExcluded");
            ModularRatio = info.GetDouble("ModularRatio");
            ClassificationCase = info.GetString("ClassificationCase");
            PropertyFingerprint = info.GetString("PropertyFingerprint");
            string error = Error();
            if (error != null) throw new SerializationException(error);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BridgeSectionStateAttributeVersion", 1);
            info.AddValue("Behavior", Behavior);
            info.AddValue("ConcreteExcluded", ConcreteExcluded);
            info.AddValue("ModularRatio", ModularRatio);
            info.AddValue("ClassificationCase", ClassificationCase);
            info.AddValue("PropertyFingerprint", PropertyFingerprint);
        }

        private string Error()
        {
            if (!Enum.IsDefined(typeof(BridgeSectionBehavior), Behavior) || Behavior == BridgeSectionBehavior.Steel) return "InvalidBridgeSectionBehavior";
            if (double.IsNaN(ModularRatio) || double.IsInfinity(ModularRatio) || ModularRatio <= 0) return "InvalidBridgeSectionModularRatio";
            if (ClassificationCase != null && string.IsNullOrWhiteSpace(ClassificationCase)) return "InvalidBridgeSectionClassificationCase";
            if (string.IsNullOrWhiteSpace(PropertyFingerprint)) return "MissingBridgeSectionPropertyFingerprint";
            return null;
        }
    }

    /// <summary>
    /// A rigid link from the node that carries the attribute to the node at <see cref="TargetMm"/> (BridGHe "Rigid Connection", a full
    /// spatial kinematic link), before the model exists: an application lifts it into a <see cref="Constraints.RigidLink"/> between the
    /// node and the one node found at the target within its tolerance. The point is in mm in a model (in the units of the definition in
    /// the drafts of an application, which normalize it with the node). Descriptive: not written to the solvers and not part of the
    /// analysis fingerprint. A node may carry several, never two with the same target.
    /// </summary>
    [Serializable]
    public sealed class RigidConnectionAttribute : Attributes.Attribute
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
            info.AddValue("TargetMm", TargetMm);
        }

        private static bool Finite(Point3d p) => !double.IsNaN(p.X) && !double.IsInfinity(p.X) && !double.IsNaN(p.Y) && !double.IsInfinity(p.Y) && !double.IsNaN(p.Z) && !double.IsInfinity(p.Z);
        internal bool SameTarget(RigidConnectionAttribute other) => _target.X == other._target.X && _target.Y == other._target.Y && _target.Z == other._target.Z;
    }

    /// <summary>The descriptive bridge attributes of the elements: the analysis fingerprint leaves them out, the assembly keeps them, the
    /// domain preflight checks where they are.</summary>
    internal static class BridgeElementAttributes
    {
        internal static bool Is(Attributes.Attribute value) => value is BridgeMemberAttribute || value is SuperElementAttribute
            || value is BridgeSectionStateAttribute || value is RigidConnectionAttribute;

        internal static bool ValidName(string name) => !string.IsNullOrWhiteSpace(name) && name.Trim() == name;

        /// <summary>The attributes of an element as the analysis fingerprint sees them: without the bridge attributes, the same collection
        /// object when it has none (the fingerprints of the other models do not change).</summary>
        internal static object AnalysisView(Element element)
        {
            var attributes = element.Attributes;
            if (attributes == null || attributes.Count == 0 || !attributes.Values.Any(Is)) return attributes;
            // Same collection type and the same id-attribute pairs (the fingerprint walks the pairs, never the counter); the ids of the
            // attributes are not touched.
            var kept = new Collections.UniqueIdCollection<Attributes.Attribute>();
            foreach (var pair in attributes) if (!Is(pair.Value)) kept[pair.Key] = pair.Value;
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

        /// <summary>Errors of the bridge attributes: a member, super element or section state on an element that is not a beam, or more
        /// than one of a kind on a beam; a rigid connection on an element that is not a node, or two with the same target; a super element
        /// renamed to an invalid name.</summary>
        internal static IEnumerable<ModelDiagnostic> Diagnose(Models.Model model)
        {
            foreach (var element in model.AllElements)
            {
                var attributes = element.Attributes;
                if (attributes == null || attributes.Count == 0) continue;
                int members = 0, superElements = 0, states = 0;
                var links = new List<RigidConnectionAttribute>();
                foreach (var pair in attributes.OrderBy(p => p.Key))
                {
                    var value = pair.Value;
                    if (value is BridgeMemberAttribute) members++;
                    else if (value is SuperElementAttribute super)
                    {
                        superElements++;
                        if (!ValidName(super.Name)) yield return ModelDiagnostic.Error("InvalidSuperElementName", element, "InvalidSuperElementName: \"" + super.Name + "\"");
                    }
                    else if (value is BridgeSectionStateAttribute) states++;
                    else if (value is RigidConnectionAttribute link) links.Add(link);
                }
                if (members + superElements + states != 0 && !(element is BeamElement))
                    yield return ModelDiagnostic.Error("BridgeBeamAttributeOnWrongElement", element,
                        "BridgeBeamAttributeOnWrongElement: bridge member, super element and section state attributes belong to beams");
                if (element is BeamElement && (members > 1 || superElements > 1 || states > 1))
                    yield return ModelDiagnostic.Error("DuplicateBridgeBeamAttribute", element,
                        "DuplicateBridgeBeamAttribute: at most one bridge member, one super element and one section state per beam");
                if (links.Count != 0 && !(element is NodeElement))
                    yield return ModelDiagnostic.Error("RigidConnectionOnWrongElement", element, "RigidConnectionOnWrongElement: rigid connections belong to nodes");
                for (int i = 1; i < links.Count; i++)
                    if (links.Take(i).Any(previous => previous.SameTarget(links[i])))
                    { yield return ModelDiagnostic.Error("DuplicateRigidConnection", element, "DuplicateRigidConnection: two rigid connections with the same target"); break; }
            }
        }
    }
}
