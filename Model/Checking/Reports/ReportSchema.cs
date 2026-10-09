using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.PostProcessing;

namespace GPC.Model.Checking
{
    internal static class ReportSchema
    {
        internal static bool RequiresVersion2(IEnumerable<CheckReport> reports) => reports.Any(r => r.BeamScope != null || r.Results.Any(v => v.SchemaVersion >= 2));
        internal static bool RequiresVersion3(IEnumerable<CheckReport> reports) => reports.Any(r => r.Results.Any(v => v.Provenance != null));
        internal static bool RequiresVersion4(IEnumerable<CheckReport> reports) => reports.Any(r => r.Results.Any(v => v.SchemaVersion >= 3));
        internal static bool RequiresVersion5(IEnumerable<CheckReport> reports) => reports.Any(r => r.Results.Any(v => v.SchemaVersion >= 4));
        internal static void Validate(CheckReport[] reports)
        {
            if (reports == null || reports.Any(r => r == null || r.Required < 0 || r.Results == null || r.Results.Any(v => v == null || v.SchemaVersion < 0 || v.SchemaVersion > 4
                || (v.SchemaVersion < 4 && v.ShellCheck != null)
                || (v.SchemaVersion < 3 && (v.ShellInput != null || v.Scope == CheckScope.ShellPoint || v.Target?.Family != null || v.Target?.ElementId != null))
                || (v.SchemaVersion < 2 && (v.Target != null || v.MemberInput != null || v.PlanItemId != null || v.CoverageAssessment != null
                    || v.MemberLocation != null || v.MethodId != null || v.Scope != CheckScope.SectionSample)))))
                throw new SerializationException("Invalid report or unsupported result schema.");
            // Historical failures, missing rows and changed evidence are retained, never rewritten as successes.
        }
    }
}
