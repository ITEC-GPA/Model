using GPC.Model.Structure.Assignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;

namespace GPC.Model.Checking.Preparation
{
    // Synchronous read-only checkpoints only. Never span a checker, progress callback, await or user code.
    // No public scope: legacy mutable setters cannot notify a long-lived revision cache reliably.
    // ThreadStatic on purpose (architecture review R7): the scope is only a cache with a writable dictionary; work on another thread
    // does not see it and reads directly, which costs time but cannot race on the dictionary.
    // Everything memoized here is derived from the model as it is during the scope and dies with it: the next scope reads the model again,
    // so any change made between two calls (model, datasets, results, properties, settings, prepared copies) is seen by the next check.
    internal sealed class ValidationReadScope : IDisposable
    {
        [ThreadStatic] private static ValidationReadScope _current;
        private readonly ValidationReadScope _previous;
        private readonly Models.Model _model;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.Ordinal);
        // Values derived from one object of the model (a beam, a source property, a result owner), keyed by reference.
        private Dictionary<ObjectKey, object> _objects;
        private FingerprintMemo _memo;
        private ValidationReadScope(Models.Model model) { _model = model; _previous = _current; _current = this; }
        internal static IDisposable Enter(Models.Model model) => ReferenceEquals(_current?._model, model) ? (IDisposable)new NestedScope() : new ValidationReadScope(model);
        private static ValidationReadScope For(Models.Model model)
        {
            var scope = _current;
            return scope != null && ReferenceEquals(scope._model, model) ? scope : null;
        }
        internal static T Read<T>(Models.Model model, string key, Func<T> read)
        {
            var scope = For(model);
            if (scope == null) return read();
            if (!scope._values.TryGetValue(key, out var value)) scope._values.Add(key, value = read());
            return (T)value;
        }
        /// <summary>As <see cref="Read{T}(Models.Model, string, Func{T})"/>, for a value derived from one object of the model.</summary>
        internal static T Read<T>(Models.Model model, object owner, string key, Func<T> read)
        {
            var scope = For(model);
            if (scope == null) return read();
            if (scope._objects == null) scope._objects = new Dictionary<ObjectKey, object>();
            var id = new ObjectKey(owner, key);
            if (!scope._objects.TryGetValue(id, out var value)) scope._objects.Add(id, value = read());
            return (T)value;
        }
        internal static ModelDiagnostic[] Errors(Models.Model model) => Read(model, "validation-errors", () =>
            model.ValidateTopology().Concat(model.ValidateAssignments()).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray());

        /// <summary>The canonical bytes of the model samples, properties and datasets of this scope; null outside it.</summary>
        internal static FingerprintMemo Memo(Models.Model model)
        {
            var scope = For(model);
            return scope == null ? null : scope._memo ?? (scope._memo = new FingerprintMemo());
        }
        /// <summary><see cref="ModelValues.Fingerprint"/> of values of the model, walking each sample, property or dataset once per scope.</summary>
        internal static string Fingerprint(Models.Model model, IEnumerable<object> inputs) => InputFingerprint.Compute(inputs, Memo(model));
        /// <summary><c>ModelValues.Fingerprint(new object[] { value })</c> of a value of the model, computed once per scope.</summary>
        internal static string FingerprintOf(Models.Model model, object value)
        {
            var memo = Memo(model);
            return memo == null ? ModelValues.Fingerprint(new[] { value }) : memo.Digest(value);
        }
        /// <summary>Whether the sample is one of the results of the owner, as a reference.</summary>
        internal static bool IsRegistered(Models.Model model, Element owner, object sample)
        {
            if (For(model) == null) return owner.Results.SelectMany(r => r.Results).Any(s => ReferenceEquals(s, sample));
            var registered = Read(model, owner, "registered-results", () => new HashSet<object>(owner.Results.SelectMany(r => r.Results), InputFingerprint.ReferenceComparer.Instance));
            return registered.Contains(sample);
        }
        /// <summary>The section fingerprint of a beam cut: its property and its section assignments.</summary>
        internal static string SectionFingerprint(Models.Model model, BeamElement beam)
            => Read(model, beam, "section-fingerprint", () => Fingerprint(model, new object[] { beam.BeamProperty, beam.Assignments.Sections.ToArray() }));

        /// <summary><see cref="ModelValues.CopyValue{T}"/> of a value of the model. In a scope the XML is written once per value and each call
        /// reads it again: every caller still owns a new detached copy, equal to the one CopyValue would return.</summary>
        internal static T CopyValue<T>(Models.Model model, T value) where T : class
        {
            if (value == null || For(model) == null) return ModelValues.CopyValue(value);
            return (T)Read(model, value, "detached-copy", () => ModelValues.Detach(value)).Read();
        }
        /// <summary>The fingerprint of <paramref name="copy"/>, just returned by <see cref="CopyValue{T}"/> for <paramref name="source"/> and not
        /// changed since. The copies of one XML are equal, so the fingerprint is computed once per source and scope.</summary>
        internal static string CopyFingerprint(Models.Model model, object source, object copy)
        {
            var scope = For(model);
            if (scope == null || source == null || FingerprintContracts.UsesLegacyNames || scope._objects == null
                || !scope._objects.TryGetValue(new ObjectKey(source, "detached-copy"), out var stored))
                return ModelValues.Fingerprint(new[] { copy });
            var detached = (ModelValues.DetachedValue)stored;
            return detached.CopyFingerprint ?? (detached.CopyFingerprint = ModelValues.Fingerprint(new[] { copy }));
        }

        public void Dispose() { _current = _previous; }
        private sealed class NestedScope : IDisposable { public void Dispose() { } }
        private struct ObjectKey : IEquatable<ObjectKey>
        {
            private readonly object _owner;
            private readonly string _key;
            internal ObjectKey(object owner, string key) { _owner = owner; _key = key; }
            public bool Equals(ObjectKey other) => ReferenceEquals(_owner, other._owner) && string.Equals(_key, other._key, StringComparison.Ordinal);
            public override bool Equals(object obj) => obj is ObjectKey other && Equals(other);
            public override int GetHashCode() => RuntimeHelpers.GetHashCode(_owner) * 31 + StringComparer.Ordinal.GetHashCode(_key);
        }
    }
}
