using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.Results.Storage
{
    /// <summary>While an image of the model is written, the registered elements leave their results out of the graph:
    /// the image carries them apart, in compact form.</summary>
    internal static class ResultScope
    {
        [ThreadStatic] private static HashSet<object> _externalized;

        internal static bool Skips(Element element) => _externalized != null && _externalized.Contains(element);

        internal static IDisposable Externalize(IEnumerable<Element> elements)
        {
            var previous = _externalized;
            _externalized = new HashSet<object>(elements, ResultIdentity.Instance);
            return new Restore(previous);
        }

        private sealed class Restore : IDisposable
        {
            private readonly HashSet<object> _previous;
            private bool _done;
            internal Restore(HashSet<object> previous) { _previous = previous; }
            public void Dispose() { if (_done) return; _done = true; _externalized = _previous; }
        }
    }
}
