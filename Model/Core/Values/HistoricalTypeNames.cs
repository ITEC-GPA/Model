using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GPC.Model.Core
{
    // Explicit CLR-to-persisted identities. Type names in generic/array fingerprints are data, not deployment names.
    internal static partial class HistoricalTypeNames
    {
        private static readonly Regex Pattern;
        static HistoricalTypeNames()
        {
            Pattern = Names.Count == 0 ? null : new Regex(
                "(?:" + string.Join("|", Names.Keys.OrderByDescending(n => n.Length).Select(Regex.Escape)) + @")(?=\+|\[|,|$)", RegexOptions.CultureInvariant);
        }
        internal static string For(Type type) => Rewrite(type.FullName);
        internal static string Rewrite(string name) => Pattern == null ? name : Pattern.Replace(name, m => Names[m.Value]);
    }
}
