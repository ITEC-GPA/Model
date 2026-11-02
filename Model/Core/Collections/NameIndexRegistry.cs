using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Collections
{
    internal sealed class NameIndexChange
    {
        internal Action Apply { get; set; }
        internal Action Notify { get; set; }
    }

    internal interface INameIndex
    {
        /// <summary>The type of the items: the index holds only objects of this type or of derived types.</summary>
        Type ItemType { get; }
        NameIndexChange PrepareRename(ModelObject item, string name);
    }

    /// <summary>
    /// Compatibility bridge for dictionary-derived registries. Inspect actual entries on rename so edits made through
    /// Dictionary/IDictionary (including already compiled clients) are observed too. Only renaming scans registries, and only
    /// those whose item type the renamed object has: every element keeps one for its groups, which renaming a node, an element
    /// or a property does not scan. Ordinary lookups retain their dictionary cost. Weak references never extend a model's lifetime.
    /// Like the model collections themselves, mutations must be serialized by the caller.
    /// </summary>
    internal static class NameIndexRegistry
    {
        private struct Entry
        {
            internal long Order;
            internal WeakReference<INameIndex> Index;
        }

        private sealed class Registrations
        {
            internal readonly List<Entry> Entries = new List<Entry>();
            /// <summary>Size at which the collected indexes are dropped: registering costs O(1) amortized, not a scan of every
            /// index created before (an element creates one, so building or copying a model was quadratic).</summary>
            internal int PurgeAt = MinimumPurge;

            internal void Purge()
            {
                Entries.RemoveAll(e => !e.Index.TryGetTarget(out _));
                PurgeAt = Math.Max(MinimumPurge, 2 * Entries.Count);
            }
        }

        private const int MinimumPurge = 64;
        private static readonly Dictionary<Type, Registrations> ByItemType = new Dictionary<Type, Registrations>();
        private static long _registered;

        internal static void Register(INameIndex index)
        {
            lock (ByItemType)
            {
                if (!ByItemType.TryGetValue(index.ItemType, out var registrations)) ByItemType.Add(index.ItemType, registrations = new Registrations());
                if (registrations.Entries.Count >= registrations.PurgeAt) registrations.Purge();
                registrations.Entries.Add(new Entry { Order = ++_registered, Index = new WeakReference<INameIndex>(index) });
            }
        }

        internal static void Rename(ModelObject item, string name, Action assignName)
        {
            INameIndex[] indexes;
            lock (ByItemType)
            {
                // The indexes of the other item types cannot hold the item. The rest in registration order, across the types.
                var type = item.GetType();
                var live = new List<Entry>();
                foreach (var pair in ByItemType)
                {
                    if (!pair.Key.IsAssignableFrom(type)) continue;
                    pair.Value.Purge();
                    live.AddRange(pair.Value.Entries);
                }
                indexes = live.OrderBy(e => e.Order).Select(e => e.Index.TryGetTarget(out var index) ? index : null).Where(i => i != null).ToArray();
            }
            // No name or index changes until every registry has accepted the rename.
            var changes = indexes.Select(i => i.PrepareRename(item, name)).Where(c => c != null).ToArray();
            assignName();
            foreach (var change in changes) change.Apply();
            foreach (var change in changes) change.Notify();
        }
    }
}
