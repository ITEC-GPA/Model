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
        NameIndexChange PrepareRename(ModelObject item, string name);
    }

    /// <summary>
    /// Compatibility bridge for dictionary-derived registries. Inspect actual entries on rename so edits made through
    /// Dictionary/IDictionary (including already compiled clients) are observed too. Only renaming scans registries;
    /// ordinary lookups retain their dictionary cost. Weak references never extend a model's lifetime.
    /// Like the model collections themselves, mutations must be serialized by the caller.
    /// </summary>
    internal static class NameIndexRegistry
    {
        private static readonly List<WeakReference<INameIndex>> Indexes = new List<WeakReference<INameIndex>>();

        internal static void Register(INameIndex index)
        {
            lock (Indexes)
            {
                Indexes.RemoveAll(w => !w.TryGetTarget(out _));
                Indexes.Add(new WeakReference<INameIndex>(index));
            }
        }

        internal static void Rename(ModelObject item, string name, Action assignName)
        {
            INameIndex[] indexes;
            lock (Indexes)
            {
                Indexes.RemoveAll(w => !w.TryGetTarget(out _));
                indexes = Indexes.Select(w => w.TryGetTarget(out var index) ? index : null).Where(i => i != null).ToArray();
            }
            // No name or index changes until every registry has accepted the rename.
            var changes = indexes.Select(i => i.PrepareRename(item, name)).Where(c => c != null).ToArray();
            assignName();
            foreach (var change in changes) change.Apply();
            foreach (var change in changes) change.Notify();
        }
    }
}
