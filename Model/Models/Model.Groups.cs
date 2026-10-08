using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Attributes;
using GPC.Model.Elements;
using GPC.Model.PostProcessing;

namespace GPC.Model.Models
{
    public enum GroupRemovalPolicy { RequireEmpty, DetachMemberships }

    public partial class Model
    {
        private Group RegisteredGroup(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name required.", nameof(name));
            if (!Groups.TryGetValue(name, out var group) || group.Name != name) throw new InvalidOperationException("MissingOrRenamedGroup: " + name);
            return group;
        }

        public IReadOnlyList<Element> GetGroupElements(string name, bool includeChildren = false, EntityFamily? family = null)
        {
            if (ValidateGroups().Count != 0) throw new InvalidOperationException("InvalidGroupGraph: inspect ValidateGroups before querying.");
            var selected = new HashSet<Group>(ReferenceComparer<Group>.Instance);
            var pending = new Stack<Group>(); pending.Push(RegisteredGroup(name));
            while (pending.Count > 0)
            {
                var group = pending.Pop();
                if (!selected.Add(group)) continue;
                if (includeChildren) foreach (var child in group.Childs) pending.Push(RegisteredGroup(child.Name));
            }
            return AllElements.Where(e => (!family.HasValue || FamilyOf(e) == family)
                && e.Groups.Values.Any(selected.Contains)).ToArray();
        }

        public static EntityFamily FamilyOf(Element element) => element is NodeElement ? EntityFamily.Node : element is BeamElement ? EntityFamily.Beam
            : element is AreaElement ? EntityFamily.Shell : element is VolumeElement ? EntityFamily.Solid : throw new NotSupportedException("Unsupported element family.");

        private Element[] RegisteredElements(IEnumerable<Element> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            var values = elements.Distinct(ReferenceComparer<Element>.Instance).ToArray();
            var registry = new HashSet<Element>(AllElements, ReferenceComparer<Element>.Instance);
            if (values.Any(e => e == null || !registry.Contains(e))) throw new InvalidOperationException("UnregisteredGroupElement");
            return values;
        }

        /// <summary>Atomic validation and idempotent addition; elements may belong to several groups.</summary>
        public void AssignGroup(string name, IEnumerable<Element> elements)
        {
            var group = RegisteredGroup(name); var values = RegisteredElements(elements);
            if (values.Any(e => e.Groups.TryGetValue(name, out var existing) && !ReferenceEquals(group, existing)))
                throw new InvalidOperationException("GroupIdentityConflict");
            foreach (var element in values) element.Groups[name] = group;
        }

        public void UnassignGroup(string name, IEnumerable<Element> elements)
        {
            RegisteredGroup(name); var values = RegisteredElements(elements);
            foreach (var element in values) element.Groups.Remove(name);
        }

        public void RenameGroup(string name, string newName)
        {
            var group = RegisteredGroup(name);
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Group name required.", nameof(newName));
            if (name == newName) return;
            if (Groups.ContainsKey(newName) || AllElements.Any(e => e.Groups.ContainsKey(newName))) throw new InvalidOperationException("GroupNameConflict");
            GetGroupElements(name); // Validate the entire group graph before changing any index.
            if (AllElements.Any(e => e.Groups.TryGetValue(name, out var old) && !ReferenceEquals(old, group))) throw new InvalidOperationException("GroupIdentityConflict");
            group.Name = newName;
        }

        private void OnGroupRenamed(Group group, string oldName, string newName)
        {
            foreach (var shell in AreaElements.Values.Where(a => a.Assignments.ReinforcementZone == oldName))
                shell.Assignments.ReinforcementZone = newName;
        }

        private void OnGroupRenaming(Group group, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Group name required.", nameof(newName));
        }

        public void ReparentGroup(string name, string parentName)
        {
            if (ValidateGroups().Count != 0) throw new InvalidOperationException("InvalidGroupGraph");
            var group = RegisteredGroup(name); var parent = parentName == null ? null : RegisteredGroup(parentName);
            if (ReferenceEquals(parent, group.Parent)) return;
            parent?.ValidateChild(group, true);
            group.Parent?.RemoveChild(group); parent?.AddChild(group);
        }

        /// <summary>Never removes FEM elements or children. Detaching memberships requires an explicit policy.</summary>
        public void RemoveGroup(string name, GroupRemovalPolicy policy = GroupRemovalPolicy.RequireEmpty)
        {
            var group = RegisteredGroup(name);
            if (!Enum.IsDefined(typeof(GroupRemovalPolicy), policy)) throw new ArgumentOutOfRangeException(nameof(policy));
            if (group.Childs.Count != 0 || AreaElements.Values.Any(a => a.Assignments.ReinforcementZone == name)) throw new InvalidOperationException("ReferencedGroup");
            var members = GetGroupElements(name);
            if (members.Count != 0 && policy == GroupRemovalPolicy.RequireEmpty) throw new InvalidOperationException("GroupHasMembers");
            foreach (var element in members) element.Groups.Remove(name);
            group.Parent?.RemoveChild(group); Groups.Remove(name);
        }

        public IReadOnlyList<ModelDiagnostic> ValidateGroups()
        {
            var errors = new List<ModelDiagnostic>();
            bool Registered(Group group) => group != null && group.Name != null && Groups.TryGetValue(group.Name, out var value) && ReferenceEquals(value, group);
            foreach (var pair in Groups)
            {
                var group = pair.Value;
                if (!Registered(group) || pair.Key != group.Name) { errors.Add(ModelDiagnostic.Error("GroupRegistryMismatch")); continue; }
                if (group.Childs == null) { errors.Add(ModelDiagnostic.Error("MissingGroupChildren")); continue; }
                if (group.Childs.Distinct(ReferenceComparer<Group>.Instance).Count() != group.Childs.Count) errors.Add(ModelDiagnostic.Error("DuplicateGroupChild"));
                foreach (var child in group.Childs)
                    if (!Registered(child) || !ReferenceEquals(child.Parent, group)) errors.Add(ModelDiagnostic.Error("GroupHierarchyMismatch"));
                if (group.Parent != null && (!Registered(group.Parent) || group.Parent.Childs == null || !group.Parent.Childs.Any(c => ReferenceEquals(c, group))))
                    errors.Add(ModelDiagnostic.Error("GroupHierarchyMismatch"));
                var path = new HashSet<Group>(ReferenceComparer<Group>.Instance);
                for (var current = group; current != null; current = current.Parent)
                    if (!path.Add(current)) { errors.Add(ModelDiagnostic.Error("GroupCycle")); break; }
            }
            foreach (var element in AllElements)
                foreach (var pair in element.Groups)
                    if (!Registered(pair.Value) || pair.Key != pair.Value.Name) errors.Add(ModelDiagnostic.Error("DanglingGroupMembership", element));
            return errors;
        }
    }
}
