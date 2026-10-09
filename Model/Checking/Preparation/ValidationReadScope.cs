using GPC.Model.Structure.Assignments;
using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Core.Diagnostics;

namespace GPC.Model.Checking.Preparation
{
    // Synchronous read-only checkpoints only. Never span a checker, progress callback, await or user code.
    // No public scope: legacy mutable setters cannot notify a long-lived revision cache reliably.
    internal sealed class ValidationReadScope : IDisposable
    {
        [ThreadStatic] private static ValidationReadScope _current;
        private readonly ValidationReadScope _previous;
        private readonly Models.Model _model;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.Ordinal);
        private ValidationReadScope(Models.Model model) { _model = model; _previous = _current; _current = this; }
        internal static IDisposable Enter(Models.Model model) => ReferenceEquals(_current?._model, model) ? (IDisposable)new NestedScope() : new ValidationReadScope(model);
        internal static T Read<T>(Models.Model model, string key, Func<T> read)
        {
            var scope = _current;
            if (scope == null || !ReferenceEquals(scope._model, model)) return read();
            if (!scope._values.TryGetValue(key, out var value)) scope._values.Add(key, value = read());
            return (T)value;
        }
        internal static ModelDiagnostic[] Errors(Models.Model model) => Read(model, "validation-errors", () =>
            model.ValidateTopology().Concat(model.ValidateAssignments()).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray());
        public void Dispose() { _current = _previous; }
        private sealed class NestedScope : IDisposable { public void Dispose() { } }
    }
}
