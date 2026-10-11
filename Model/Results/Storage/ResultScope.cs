using System;
using System.Collections.Generic;
using System.Threading;
using GPC.Model.Elements;

namespace GPC.Model.Results.Storage
{
    /// <summary>While an image of the model is written, the registered elements leave their results out of the graph:
    /// the image carries them apart, in compact form. The set is read only and AsyncLocal (architecture review R7): a graph
    /// written inside the scope on another thread still leaves the results out.</summary>
    internal static class ResultScope
    {
        private static readonly AsyncLocal<HashSet<object>> _externalized = new AsyncLocal<HashSet<object>>();

        internal static bool Skips(Element element) => _externalized.Value is HashSet<object> set && set.Contains(element);

        internal static IDisposable Externalize(IEnumerable<Element> elements)
        {
            var previous = _externalized.Value;
            _externalized.Value = new HashSet<object>(elements, ResultIdentity.Instance);
            return new Restore(previous);
        }

        private sealed class Restore : IDisposable
        {
            private readonly HashSet<object> _previous;
            private bool _done;
            internal Restore(HashSet<object> previous) { _previous = previous; }
            public void Dispose() { if (_done) return; _done = true; _externalized.Value = _previous; }
        }
    }
}
