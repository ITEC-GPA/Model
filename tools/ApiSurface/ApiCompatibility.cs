using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Tools;

internal static class ApiCompatibility
{
    // Implementing another interface on a class does not remove any of its existing contracts.
    // All other type metadata must match. Interface inheritance changes remain strict.
    internal static bool IsAdditiveInterfaceImplementation(string previous, string current)
    {
        string[] old = previous.Split('|'), now = current.Split('|');
        if (old.Length != 6 || now.Length != 6 || old[0] != "T" || old[2].Split(',').Any(a => a.Trim() == "Interface")) return false;
        for (int i = 0; i < old.Length; i++) if (i != 4 && old[i] != now[i]) return false;
        const string prefix = "interfaces=";
        if (!old[4].StartsWith(prefix, StringComparison.Ordinal) || !now[4].StartsWith(prefix, StringComparison.Ordinal)) return false;
        var actual = new HashSet<string>(Interfaces(now[4].Substring(prefix.Length)), StringComparer.Ordinal);
        return Interfaces(old[4].Substring(prefix.Length)).All(actual.Contains);
    }

    private static IEnumerable<string> Interfaces(string value)
    {
        int depth = 0, start = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '<' || value[i] == '[') depth++;
            else if (value[i] == '>' || value[i] == ']') depth--;
            else if (value[i] == ',' && depth == 0) { yield return value.Substring(start, i - start); start = i + 1; }
        }
        if (start < value.Length) yield return value.Substring(start);
    }
}
